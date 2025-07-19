using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AccountingApi.Infrastructure;
using AccountingApi.Infrastructure.Seeds;
using AccountingApi.Middleware;
using AccountingApi.Mappings;
using AccountingApi.Services.NumberGenerationService;
using AccountingApi.Services.AutomaticJournalEntryService;
using AccountingApi.Services.AccountConfigurationService;
using AccountingApi.Services.JwtService;
using AccountingApi.Services.CurrentUserService;
using AccountingApi.Models;
using MyMediator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Serilog.Enrichers.Span;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithSpan());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Entity Framework with Aspire SQL Server integration
builder.AddSqlServerDbContext<AccountingDbContext>("accountingdb");

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AccountingDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSecretKeyHere123456789MustBe32CharactersOrMore!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "Authentication failed." });
            return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized." });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

// Health checks are automatically added by AddSqlServerDbContext
// No need to manually add them again

// Add MediatR - Register all handlers from the current assembly
// Add MyMediator and register all handlers
builder.Services.AddMediator();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Mapping Services
builder.Services.AddMappingServices();

// Add Number Generation Service
builder.Services.AddScoped<INumberGenerationService, NumberGenerationService>();

// Add Automatic Journal Entry Service
builder.Services.AddScoped<IAutomaticJournalEntryService, AutomaticJournalEntryService>();

// Add Account Configuration Service
builder.Services.Configure<AccountMappingOptions>(
    builder.Configuration.GetSection(AccountMappingOptions.SectionName));
builder.Services.AddScoped<IAccountConfigurationService, AccountConfigurationService>();

// Add Authentication Services
builder.Services.AddScoped<IJwtService, JwtService>();

// Add HTTP Context Accessor for CurrentUserService
builder.Services.AddHttpContextAccessor();

// Add Current User Service for audit purposes
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Dashboard Service

// Add CORS for frontend development with Aspire support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // In development, allow localhost with any port (for Aspire dynamic ports)
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                
                var uri = new Uri(origin);
                // Allow localhost with any port for development
                return uri.Host == "localhost" || uri.Host == "127.0.0.1";
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // Production: Use specific origins from configuration
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                ?? new[] { "https://yourdomain.com" };
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Add logging
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();
// builder.Logging.AddDebug();

var app = builder.Build();

app.MapDefaultEndpoints();

// Apply database migrations automatically on startup
// This ensures the database schema is always up to date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AccountingDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        
        // Ensure the database is created and apply all pending migrations
        await context.Database.MigrateAsync();
        
        logger.LogInformation("Database migrations applied successfully.");

        // Seed roles and default admin user
        logger.LogInformation("Seeding roles and default users...");
        await RoleSeeder.SeedRolesAsync(roleManager, userManager);
        logger.LogInformation("Roles and users seeded successfully.");

        // Seed basic chart of accounts
        logger.LogInformation("Seeding chart of accounts...");
        await AccountSeeder.SeedAccountsAsync(context);
        logger.LogInformation("Chart of accounts seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations or seeding data.");
        
        // In development, we might want to continue anyway
        // In production, you might want to fail fast
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
        
        logger.LogWarning("Continuing startup despite migration errors in development environment.");
    }
}

// Map health checks for Aspire integration
app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Scalar API documentation
}

app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
