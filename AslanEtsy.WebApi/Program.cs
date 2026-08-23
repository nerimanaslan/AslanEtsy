using System.Text.Json.Serialization;
using AslanEtsy.Infrastructure;
using AslanEtsy.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Dynamic Port Binding: Listen on 5117, 8080 and cloud PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls("http://0.0.0.0:5117", $"http://0.0.0.0:{port}");
builder.WebHost.UseWebRoot("wwwroot");

// Lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Controllers & JSON serializer config
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Infrastructure & Business Layer Services
builder.Services.AddInfrastructure(builder.Configuration);

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger / OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Aslan Etsy Multi-Account & Order Management API",
        Version = "v1",
        Description = "Etsy çoklu mağaza ve sipariş yönetim/takip RESTful API entegrasyonu."
    });
});

var app = builder.Build();

// Auto-create Clean SQLite Database if not exists
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();

    // Ensure CurtainProducts table exists
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS ""CurtainProducts"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_CurtainProducts"" PRIMARY KEY AUTOINCREMENT,
            ""Name"" TEXT NOT NULL,
            ""M2Price"" TEXT NOT NULL,
            ""Fabric"" TEXT NULL,
            ""Note"" TEXT NULL,
            ""ImageUrl"" TEXT NULL,
            ""CreatedAtUtc"" TEXT NOT NULL,
            ""UpdatedAtUtc"" TEXT NULL,
            ""IsDeleted"" INTEGER NOT NULL DEFAULT 0
        );
    ");
}

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aslan Etsy API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// Serve static frontend files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// SPA Fallback for Single Page Application
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"İstenen API adresi bulunamadı.\"}");
        return;
    }

    var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    var indexPath = Path.Combine(webRoot, "index.html");

    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(indexPath);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("index.html dosyası bulunamadı.");
    }
});

app.Run();
