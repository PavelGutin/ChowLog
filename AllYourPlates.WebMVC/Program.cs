using AllYourPlates.Hubs;
using AllYourPlates.Services;
using AllYourPlates.Utilities;
using AllYourPlates.WebMVC.DataAccess;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//TODO remove hardcoded variable name
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSignalR();

//TODO this is very dirty, need to figure out a cleaner way to do this, but that's a front end problem which isn't what I am trying to learn
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500 MB
});

builder.Services.Configure<ApplicationOptions>(
    builder.Configuration.GetSection(nameof(ApplicationOptions)));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();


//TODO really dig into this to understand how it's working
builder.Services.AddSingleton<ThumbnailProcessingService>();
builder.Services.AddSingleton<ImageDescriptionService>();
builder.Services.AddSingleton<PlateMetadataService>();

builder.Services.AddScoped<IPlateService, PlateService>();
builder.Services.AddScoped<IPlateRepsitory, PlateLocalDBRepository>();
builder.Services.AddScoped<IPlateImageStorage, PlateLocalImageStorage>();



builder.Services.AddHostedService(provider => provider.GetService<ThumbnailProcessingService>());
builder.Services.AddHostedService(provider => provider.GetService<ImageDescriptionService>());
builder.Services.AddHostedService(provider => provider.GetService<PlateMetadataService>());

//THIS MAY NOT WORK <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
//builder.Services.AddHostedService<ThumbnailProcessingService>();
//builder.Services.AddHostedService<ImageDescriptionService>();
//builder.Services.AddHostedService<PlateMetadataService>();


builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "AllYourPlates")
    .WriteTo.File("Logs/applogZZZ-.txt");
    //.WriteTo.File("Logs/applogZZZ-.txt", outputTemplate: mt);
});



var app = builder.Build();


// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        //dbContext.Database.EnsureCreated(); // Ensures the database exists
        dbContext.Database.Migrate();      // Applies any pending migrations
    }
    catch (Exception ex)
    {
        // Log the exception (use your preferred logging framework)
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
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

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
});

app.Run();