using dprmitory.Data;
using dprmitory.Services;
using Serilog;

// ── Serilog: write to console + rolling daily log file ──────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/dormitory-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── MVC + Session ────────────────────────────────────────────────────────
    builder.Services.AddControllersWithViews(options =>
    {
        // Global anti-forgery filter — all POST actions require a valid token
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    });

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(24);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = TimeSpan.FromHours(24);
    });

    // ── Database services ────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddSingleton(new DatabaseHelper(connectionString));
    builder.Services.AddScoped<StudentRepository>();
    builder.Services.AddScoped<RoomRepository>();
    builder.Services.AddScoped<UserRepository>();
    builder.Services.AddScoped<DailyReportRepository>();

    // ── Background services ──────────────────────────────────────────────────
    builder.Services.AddHostedService<DailyResetService>();

    var app = builder.Build();

    // ── Initialize database ──────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var dbHelper = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();
        dbHelper.InitializeDatabase();
    }

    app.UseSession();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    Log.Information("Dormitory Management System starting up.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
