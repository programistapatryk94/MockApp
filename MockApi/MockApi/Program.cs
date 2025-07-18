using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MockApi.Data;
using MockApi.Data.Seed;
using MockApi.Extensions;
using MockApi.Helpers;
using MockApi.Localization;
using MockApi.Localization.RequestCulture;
using MockApi.Runtime.Configuration;
using MockApi.Runtime.DataModels.Auditing;
using MockApi.Runtime.DataModels.UoW;
using MockApi.Runtime.Exceptions.Handling;
using MockApi.Runtime.Features;
using MockApi.Runtime.Session;
using MockApi.Services;
using MockApi.Translations;
using Serilog;
using System.Linq.Expressions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));

builder.Services.AddHttpContextAccessor();

if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(44313, listenOptions =>
        {
            listenOptions.UseHttps();
        });
    });
}

builder.Services.AddCors(options =>
{
    var corsOrigins = builder.Configuration["App:CorsOrigins"];

    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                corsOrigins!.Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.RemovePostFix("/")!)
                .ToArray()
                )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MockAPI", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Wpisz **tylko** JWT token bez 'Bearer'",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {jwtSecurityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddScoped<IAppSession, ClaimsAppSession>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IFeatureChecker, FeatureChecker>();
builder.Services.AddScoped<AppFeatureProvider>();
builder.Services.AddSingleton<ILocalizationConfiguration, LocalizationConfiguration>();
builder.Services.AddScoped<ILanguageManager, LanguageManager>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IAuditSerializer, AuditSerializer>();
builder.Services.AddScoped<IRequestLogHelper, RequestLogHelper>();
builder.Services.AddScoped<RequestResponseLoggingFilter>();
builder.Services.AddScoped<AppExceptionFilter>();
builder.Services.AddScoped<IUserManager, AppUserManager>();
builder.Services.AddSingleton<ITranslationService, XmlTranslationService>();
builder.Services.AddSingleton<IRequestLogConfiguration, RequestLogConfiguration>();
builder.Services.AddSingleton<IUnitOfWorkConfiguration, UnitOfWorkConfiguration>();
builder.Services.AddSingleton<IAppConfiguration, AppConfiguration>();
builder.Services.AddScoped<UowFilter>();
builder.Services.AddSingleton<LocalizationHeaderRequestCultureProvider>();
builder.Services.AddSingleton<DefaultRequestCultureProvider>();
builder.Services.AddSingleton<UserRequestCultureProvider>();

builder.Services.AddSingleton<IFeatureConfiguration, FeatureConfiguration>();
builder.Services.AddSingleton<IFeatureManager, FeatureManager>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.AddService<RequestResponseLoggingFilter>();
    options.Filters.AddService<UowFilter>();
    options.Filters.AddService<AppExceptionFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"]!;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var config = services.GetRequiredService<IConfiguration>();

    context.Database.Migrate();

    bool shouldSeed = config.GetValue<bool>("SeedData");

    if (shouldSeed)
    {
        SeedData.EnsureSeedData(services);
    }
}

app.Services.GetRequiredService<IFeatureConfiguration>().Providers.Add<AppFeatureProvider>();

var localizationConfig = app.Services.GetRequiredService<ILocalizationConfiguration>();
MockApiLocalizationConfigurator.Configure(localizationConfig);

var auditingConfig = app.Services.GetRequiredService<IRequestLogConfiguration>();
auditingConfig.IgnoredTypes.Add(typeof(Stream));
auditingConfig.IgnoredTypes.Add(typeof(Expression));
auditingConfig.IsEnabledForAnonymousUsers = true;

(app.Services.GetRequiredService<IFeatureManager>() as FeatureManager)!.Initialize();
(app.Services.GetRequiredService<ITranslationService>() as XmlTranslationService)!.Initialize();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.UseAppRequestLocalization();

app.MapControllers();

app.MapWhen(context =>
{
    var host = context.Request.Host.Host;
    var isSubdomain = host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) &&
                      host != "localhost";
    var path = context.Request.Path.Value ?? "";

    return isSubdomain && path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
}, builder =>
{
    builder.UseRouting();
    builder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "mock-fallback",
            pattern: "{*path}",
            defaults: new { controller = "PublicMock", action = "HandleRequest" }
            );
    });
});

app.Run();
