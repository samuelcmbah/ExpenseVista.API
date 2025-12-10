using ExpenseVista.API.Configurations;
using ExpenseVista.API.Data;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.Background;
using ExpenseVista.API.Services.IServices;
using ExpenseVista.API.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Resend;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.Local.json", optional: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddTransient<GlobalExceptionMiddleware>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPeriodicSummaryService, PeriodicSummaryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<JwtService>();
// Register the background worker
builder.Services.AddHostedService<TokenCleanupService>();

// Register EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendEmailSettings>(
    builder.Configuration.GetSection("ResendEmailSettings"));
builder.Services.AddOptions(); // Required for options pattern
builder.Services.AddHttpClient<ResendClient>(); 
// Set the ApiToken for the ResendClientOptions from configuration
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["ResendEmailSettings:ApiKey"]!;
});

// register wrapper interface used by the library
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
// Register the output caching services
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("ShortCache", builder =>
        builder.Expire(TimeSpan.FromSeconds(30)));

    options.AddPolicy("LongCache", builder =>
        builder.Expire(TimeSpan.FromMinutes(5)));
});

// Load serilog.json
builder.Configuration.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddHttpContextAccessor();//Http context used in classes. By default, only controllers and middleware have access to HttpContext
// Add CORS with a default policy that allows predefined origin for api consumption
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "";
var origins = allowedOrigins
    .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(o => o.Trim())
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientPolicy", policy =>
    {
        if (origins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = string.IsNullOrWhiteSpace(jwtSettings["Key"])
    ? "Expen$eVista_Temporary_Key_For_Migrations_Only_123!"
    : jwtSettings["Key"];
var key = Encoding.UTF8.GetBytes(jwtKey!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromSeconds(30)
    };

});

builder.Services.AddAuthorization();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add Identity
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddHttpContextAccessor();


// Add AutoMapper 
builder.Services.AddAutoMapper(typeof(AutoMappingConfiguration).Assembly);

builder.Services.AddControllers(config =>
{
    // Global authorization policy
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    config.Filters.Add(new AuthorizeFilter(policy));

})
.AddJsonOptions(options =>
{
    // CamelCase for JSON
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    // Custom Decimal converter
    options.JsonSerializerOptions.Converters.Add(new DecimalTwoPlacesConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                        "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                        "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
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
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
     });//to use bearer token in swagger

});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
// optional but recommended for clarity in minimal hosting
//app.UseRouting();
app.UseSerilogRequestLogging();

app.UseCors("ClientPolicy");
app.UseStaticFiles();

app.UseExceptionHandler("/error"); // fallback built-in handler
app.UseMiddleware<GlobalExceptionMiddleware>(); //custom handler
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


await CategorySeeder.EnsurePopulatedAsync(app);



app.Run();
