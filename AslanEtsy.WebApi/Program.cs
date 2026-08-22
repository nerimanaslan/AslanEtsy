using System.Text.Json.Serialization;
using AslanEtsy.Infrastructure;
using AslanEtsy.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Render and other reverse proxies terminate TLS before forwarding the request
// to Kestrel. Trust the forwarded scheme/host so OAuth callback URLs are built
// with the public HTTPS address instead of the internal HTTP address.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;

    // The app can run behind a managed proxy whose IP range is not known in
    // advance. Restricting this to the default localhost entries would make
    // the X-Forwarded-* headers ineffective in production.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Dynamic Port Binding for Render, Cloud & Local
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
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

// Must run before routing/controllers so Request.Scheme and Request.Host use
// the public values supplied by the reverse proxy.
app.UseForwardedHeaders();

// Auto-create Clean SQLite Database if not exists
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();
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
