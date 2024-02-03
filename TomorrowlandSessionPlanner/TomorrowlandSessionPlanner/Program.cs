using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor;
using MudBlazor.Services;
using TomorrowlandSessionPlanner.Code;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.MaxDisplayedSnackbars = 5;
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    configuration.SnackbarConfiguration.PreventDuplicates = true;
    configuration.SnackbarConfiguration.VisibleStateDuration = 5000;
});
builder.Services.AddScoped<PlannerManager>();

// if (!builder.Environment.IsDevelopment())
// {
//     builder.WebHost.ConfigureKestrel(options =>
//     {
//         options.Listen(IPAddress.Loopback, 5002, listenOptions =>
//         {
//             listenOptions.UseHttps(new X509Certificate2("/home/jason/certs/tmlPlanner/certificate.pfx", Environment.GetCommandLineArgs()[0]));
//         });
//     });
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();