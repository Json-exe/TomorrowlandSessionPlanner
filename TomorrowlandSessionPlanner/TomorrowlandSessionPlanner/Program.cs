using MudBlazor;
using MudBlazor.Services;
using TomorrowlandSessionPlanner.Code;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();