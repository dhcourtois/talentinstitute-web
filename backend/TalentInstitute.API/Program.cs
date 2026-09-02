using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TalentInstitute.Application;
using TalentInstitute.Infrastructure;
using TalentInstitute.Infrastructure.Authentication;
using TalentInstitute.Infrastructure.Data;
using TalentInstitute.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(connectionString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
            ?? throw new InvalidOperationException("JwtOptions not found.");
            
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT de la siguiente manera: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Migraciones automáticas en todos los entornos ─────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TalentInstituteDbContext>();
    await db.Database.MigrateAsync();
}

// Semilla de datos de prueba solo en Development
if (app.Environment.IsDevelopment())
{
    await TalentInstitute.API.Data.DbInitializer.SeedAsync(app.Services);
}
else
{
    // Fuera de Development no se siembran datos de prueba, pero sí hace falta
    // poder crear la primera cuenta: sin ella nadie puede entrar y StaffController
    // exige rol Principal para dar de alta usuarios.
    await TalentInstitute.API.Data.DbInitializer.BootstrapAdminAsync(app.Services, app.Configuration);
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Servir SPA de Angular desde wwwroot ───────────────────────────────────
// index.html nunca se cachea: es el índice que apunta a los bundles con hash.
// Si el navegador lo conserva, sigue pidiendo chunks de un deploy anterior y
// la app queda corriendo código viejo contra el backend nuevo.
// Los .js/.css sí llevan hash en el nombre (outputHashing: "all"), así que son
// inmutables y se pueden cachear indefinidamente.
var staticFileOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var headers = ctx.Context.Response.Headers;
        var name = ctx.File.Name;

        if (name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            headers.CacheControl = "no-cache, no-store, must-revalidate";
            headers.Pragma = "no-cache";
            headers.Expires = "0";
        }
        else if (name.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
              || name.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
        {
            headers.CacheControl = "public, max-age=31536000, immutable";
        }
    }
};

app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);

app.UseHttpsRedirection();

app.UseCors("SpaPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Todas las rutas no-API redirigen a index.html para el router de Angular.
// Se pasan las mismas opciones para que el index servido por el fallback (que
// es el de /login y cualquier deep link) también salga con no-cache.
app.MapFallbackToFile("index.html", staticFileOptions);

app.Run();
