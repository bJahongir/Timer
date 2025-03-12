
using Microsoft.EntityFrameworkCore;
using Timer;
using ActualLab.Fusion.UI;
using ActualLab.Fusion;
using ActualLab.Fusion.Blazor;
using ActualLab.Fusion.Extensions;

var builder = WebApplication.CreateBuilder(args);

var fusion = builder.Services.AddFusion();
fusion.AddBlazor();
fusion.AddFusionTime();
builder.Services.AddTransient<IUpdateDelayer>(c => new UpdateDelayer(c.UIActionTracker(), 0.1));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<TimerRepostory>();
builder.Services.AddSingleton<TimerService>();



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
