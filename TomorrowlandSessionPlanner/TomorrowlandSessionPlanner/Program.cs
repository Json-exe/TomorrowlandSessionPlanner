using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.DBContext;
using TomorrowlandSessionPlanner.Core.ViewModel;

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
builder.Services.AddDbContextFactory<TmldbContext>(optionsBuilder => 
    optionsBuilder.UseSqlite("Data Source=data/tmldata.db"));
builder.Services.AddScoped<PlannerManager>();
builder.Services.AddViewModel();

var app = builder.Build();

var dbContext = await app.Services.GetRequiredService<IDbContextFactory<TmldbContext>>().CreateDbContextAsync();
await dbContext.Database.MigrateAsync();

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