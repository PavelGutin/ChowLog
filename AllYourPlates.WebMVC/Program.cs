using AllYourPlates.Services;
using AllYourPlates.WebMVC.Data;
using AllYourPlates.WebMVC.DataAccess;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//TODO: This doesn't seem right to me. I think it should be in the appsettings.json file that I push to the container. But, I spent two hours getting here, so I am leaving it be 
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500 MB
});


builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ThumbnailProcessingService>();
builder.Services.AddSingleton<ImageDescriptionService>();

builder.Services.AddHostedService(provider => provider.GetService<ThumbnailProcessingService>());
builder.Services.AddHostedService(provider => provider.GetService<ImageDescriptionService>());

var app = builder.Build();

// Apply pending migrations if any, and ensure the database is created.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Get the applied migrations and pending migrations
    var appliedMigrations = dbContext.Database.GetAppliedMigrations();
    var pendingMigrations = dbContext.Database.GetPendingMigrations();

    // Log the number of applied and pending migrations
    Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>Applied Migrations: {appliedMigrations.Count()}");
    Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>Pending Migrations: {pendingMigrations.Count()}");

    // Apply any pending migrations
    dbContext.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (maxRequestBodySizeFeature != null)
    {
        maxRequestBodySizeFeature.MaxRequestBodySize = 524288000; // 500 MB
    }
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();