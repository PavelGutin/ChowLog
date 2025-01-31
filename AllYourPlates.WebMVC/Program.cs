using AllYourPlates.Hubs;
using AllYourPlates.Services;
using AllYourPlates.Utilities;
using AllYourPlates.WebMVC.DataAccess;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");


    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration) // Allows configuration from appsettings.json
            .ReadFrom.Services(services) // Captures log context (like HttpContext)
            .WriteTo.Console()
            .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .MinimumLevel.Debug(); // Ensure all levels are logged
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews();


    builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection(nameof(ApplicationOptions)));


    var dbPath = builder.Configuration.GetSection(nameof(ApplicationOptions)).Get<ApplicationOptions>()?.DataPath
             ?? throw new ArgumentException("DB_PATH not defined");

    var envVariables = Environment.GetEnvironmentVariables();
    foreach (DictionaryEntry env in envVariables)
    {
        Console.WriteLine($"{env.Key}: {env.Value}");
    }
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite($"Data Source={dbPath}/AllYourPlates.db;"));
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();


    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    builder.Services.AddSignalR();
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 524288000; // 500 MB
    });



    builder.Services.AddScoped<IPlateService, PlateService>();
    builder.Services.AddScoped<IPlateOrchestrator, PlateOrchestrator>();
    builder.Services.AddScoped<IPlateRepsitory, PlateLocalDBRepository>();
    builder.Services.AddScoped<IPlateImageStorage, PlateLocalImageStorage>();
    builder.Services.AddSingleton<ThumbnailProcessingService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<ThumbnailProcessingService>());
    builder.Services.AddSingleton<ImageDescriptionService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<ImageDescriptionService>());
    builder.Services.AddSingleton<PlateMetadataService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<PlateMetadataService>());


    //builder.Services.AddSingleton<IConfiguration>(provider =>
    //{
    //    var configuration = new ConfigurationBuilder()
    //        .AddEnvironmentVariables()
    //        .Build();

    //    return configuration;
    //});


    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Apply migrations on startup
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
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
            maxRequestBodySizeFeature.MaxRequestBodySize = 524288000;
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
    app.MapHub<NotificationHub>("/notificationHub");

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}