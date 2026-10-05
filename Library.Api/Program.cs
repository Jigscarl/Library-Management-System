using Library.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------- Services --------

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database — PostgreSQL via Supabase
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// CORS — allow the React frontend (local dev + deployed Vercel)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",          // Vite dev server
                "http://localhost:3000",          // CRA dev server (in case)
                "https://your-app.vercel.app")    // ⚠️ REPLACE with your real Vercel URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------- Pipeline --------

var app = builder.Build();

// Auto-apply pending migrations on startup (useful for Render deployment)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");   // MUST come before MapControllers

app.UseAuthorization();
app.MapControllers();

app.Run();