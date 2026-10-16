using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.WebEncoders;
using Nachytej.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
// Česká písmena v HTML vypisovat přímo, ne jako &#233; apod.
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(2);
    o.Cookie.Name = ".Nachytej.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true; // jen technická cookie pro průběh tréninku
});

builder.Services.AddSingleton<ContentStore>();
builder.Services.AddSingleton<ScenarioSelector>();
builder.Services.AddSingleton<StatsStore>();

var app = builder.Build();

// Načíst a zkontrolovat obsah hned při startu – chybný obsah = aplikace nenaběhne.
app.Services.GetRequiredService<ContentStore>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();

app.Run();
