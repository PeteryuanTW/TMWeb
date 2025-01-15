using TMWeb.Components;
using TMWeb.Services;
using TMWeb.Client.Pages;
using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using CommonLibrary.Auth;
using CommonLibrary.Auth.EFModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using BitzArt.Blazor.Cookies;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using System.Data.Common;
using CommonLibrary.MachinePKG;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Collections.ObjectModel;
using System.Data;
using TMWeb.Data;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDevExpressBlazor(options =>
{
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Large;
});

//builder.AddBlazorCookies();


//https://github.com/nethawkChen/dotnet8-Serilog
var setting = builder.Configuration;
var logPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : Directory.GetCurrentDirectory();

builder.Host.UseSerilog(
    (context, configuration) =>
    {
        configuration
        .ReadFrom.Configuration(context.Configuration)
        //.ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With<LogLevelAsIntEnricher>()
        //.WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("Controller"))
        //.WriteTo.File($"{logPath}\\logs\\controller\\log_.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 720))
        //.WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("Service"))
        //.WriteTo.File($"{logPath}\\logs\\service\\log_.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 720))
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .WriteTo.MSSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DbConnection"),
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "SerilogDatas",
                AutoCreateSqlTable = true,

            },
            columnOptions: new ColumnOptions
            {
                Store = new Collection<StandardColumn>
                {
                    StandardColumn.Id,
                    StandardColumn.Message,
                    StandardColumn.TimeStamp,
                },
                Message = new MessageColumnOptions
                {
                    ColumnName = "Msg",
                    DataType = SqlDbType.NVarChar,
                    DataLength = -1,
                },
                AdditionalColumns = new Collection<SqlColumn>
                {
                    new SqlColumn { ColumnName = "Severity", DataType = SqlDbType.Int},
                    new SqlColumn { ColumnName = "Caller", DataType = SqlDbType.NVarChar},
                    new SqlColumn { ColumnName = "Method", DataType = SqlDbType.NVarChar},
                    new SqlColumn { ColumnName = "Row", DataType = SqlDbType.Int},
                    new SqlColumn { ColumnName = "Col", DataType = SqlDbType.Int},
                }
            }
            );
    });

builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddMvc();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopfloor API", Version = "v0" });
});

builder.Services.AddDbContextFactory<TmwebContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")/*, o =>o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)*/);
});
//builder.Services.AddDbContextFactory<UserDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
//});

builder.AddUserCookieService("TMWeb", "DbConnection");
builder.AddMachineService("DbConnection");


builder.Services.AddSingleton<ApplicationLifetimeService>();
builder.Services.AddSingleton<TMWebShopfloorService>();

builder.Services.AddSingleton<ScriptService>();
builder.Services.AddScoped<UIService>();
builder.Services.AddScoped<ScriptLoaderService>();

builder.Services.AddHostedService<HostedService>();

builder.Services.Configure<SerilogCleanupSetting>(builder.Configuration.GetSection("SerilogCleanupSetting"));
builder.Services.AddSingleton<SerilogService>();
builder.Services.AddHostedService<SerilogDBCleanupService>();




builder.Services.AddLocalization();
var supportedCultures = new[] { "zh-TW", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

builder.Services.AddControllers();

builder.Services.AddCascadingAuthenticationState();

builder.Host.UseWindowsService();

var app = builder.Build();
var lifetimeEvents = app.Services.GetRequiredService<ApplicationLifetimeService>();
app.UseRequestLocalization(localizationOptions);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.RunMachineService();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Shopfloor API V0");
});

app.Run();


