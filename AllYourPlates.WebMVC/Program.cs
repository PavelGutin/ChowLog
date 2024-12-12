using AllYourPlates.Hubs;
using AllYourPlates.Services;
using AllYourPlates.WebMVC.DataAccess;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//TODO: This doesn't seem right to me. I think it should be in the appsettings.json file that I push to the container. But, I spent two hours getting here, so I am leaving it be 
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSignalR();

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


//Add support to logging with SERILOG
/*
builder.Host.UseSerilog((context, configuration) =>
{
    //configuration.WriteTo.File("Logs/applogXXX-.txt").Enrich.WithEnvironmentName().Enrich.WithProperty("Application", "AllYourPlates");
    configuration
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "AllYourPlates")
    .WriteTo.File("Logs/applogZZZ-.txt");
    //configuration.Enrich.WithEnvironmentName();
    //configuration.Enrich.FromLogContext();
    //configuration.Enrich.WithExceptionStackTraceHash();
    //configuration.Enrich.WithProperty("Application", "AllYourPlates");
    //configuration.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});/*/



//var mt = "{LogLevel:u1}|{SourceContext}|{Message:l}|{Properties}{NewLine}{Exception}";

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "AllYourPlates")
    .WriteTo.File("Logs/applogZZZ-.txt");
    //.WriteTo.File("Logs/applogZZZ-.txt", outputTemplate: mt);
});



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