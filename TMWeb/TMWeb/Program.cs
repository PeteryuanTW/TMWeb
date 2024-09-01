using TMWeb.Components;
using TMWeb.Services;
using TMWeb.Client.Pages;
using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Large;
});
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddMvc();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopfloor API", Version = "v0" });
});

builder.Services.AddDbContextFactory<TmwebContext>(options =>
{
    var a = builder.Configuration.GetConnectionString("DefaultConnection");

	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<TMWebShopfloorService>();
builder.Services.AddSingleton<UIService>();
builder.Services.AddLocalization();
var supportedCultures = new[] { "zh-TW", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);


//builder.Services.AddControllers();
var app = builder.Build();
app.UseRequestLocalization(localizationOptions);
// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseHttpsRedirection();



//string[] lan = ["zh-TW"];
//app.UseRequestLocalization(x => x.SetDefaultCulture(lan[0]));

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