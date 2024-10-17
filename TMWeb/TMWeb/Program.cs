using TMWeb.Components;
using TMWeb.Services;
using TMWeb.Client.Pages;
using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.LogBranch.Extensions;
using CommonLibrary.Auth;
using CommonLibrary.Auth.EFModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using BitzArt.Blazor.Cookies;
using Microsoft.Extensions.Hosting;

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

builder.AddBlazorCookies();



//https://github.com/nethawkChen/dotnet8-Serilog
var setting = builder.Configuration;
builder.Host.UseSerilog(
    (context, services, configuration) =>
    {
        configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("Controller")).WriteTo.File(setting["Serilog:WriteTo:1:Args:Path"],
            rollingInterval: Enum.Parse<RollingInterval>(setting["Serilog:WriteTo:1:Args:rollingInterval"]),
            retainedFileCountLimit: int.Parse(setting["Serilog:WriteTo:1:Args:retainedFileCountLimit"])))
         .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("Service")).WriteTo.File(setting["Serilog:WriteTo:2:Args:Path"],
            rollingInterval: Enum.Parse<RollingInterval>(setting["Serilog:WriteTo:2:Args:rollingInterval"]),
            retainedFileCountLimit: int.Parse(setting["Serilog:WriteTo:2:Args:retainedFileCountLimit"])));
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
builder.Services.AddDbContextFactory<UserDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
});

builder.Services.AddSingleton<TMWebShopfloorService>();
builder.Services.AddSingleton<EventLogService>();
builder.Services.AddSingleton<ScriptService>();
builder.Services.AddScoped<UIService>();
builder.Services.AddScoped<ScriptLoaderService>();
builder.Services.AddScoped<AuthService>(p =>
{
    return new AuthService("TMWeb", p.GetRequiredService<IServiceScopeFactory>(), p.GetRequiredService<ICookieService>());
});

builder.Services.AddHostedService<HostedService>();

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

//var tmWebShopfloorService = app.Services.GetRequiredService<TMWebShopfloorService>();
//await tmWebShopfloorService.InitAll();

