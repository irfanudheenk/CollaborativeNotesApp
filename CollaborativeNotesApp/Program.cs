using CollaborativeNotesApp.Components;
using CollaborativeNotesApp.Data;
using CollaborativeNotesApp.Hubs;
using CollaborativeNotesApp.Models;  // Add this
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ Register ApiSettings from appsettings.json
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Add HttpClientFactory with configured base address
builder.Services.AddHttpClient("NotesApi", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

// Add database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=notes.db"));

// Add SignalR and controllers
builder.Services.AddSignalR();
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseCors("AllowAll");

app.MapControllers();
app.MapHub<NotesHub>("/noteshub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();