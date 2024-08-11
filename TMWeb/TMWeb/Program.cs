using TMWeb.Components;
using TMWeb.Services;
using TMWeb.Client.Pages;
using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;

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

builder.Services.AddDbContextFactory<TmwebContext>(options =>
{
    var a = builder.Configuration.GetConnectionString("DefaultConnection");

	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddSingleton<TMWebShopfloorService>();
builder.Services.AddSingleton<TMWebMachineService>();

//builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

//app.MapControllers();

app.Run();