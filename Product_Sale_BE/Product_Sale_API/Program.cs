using BusinessLogic.Hubs;
using BusinessLogic.IServices;
using BusinessLogic.Services;
using DataAccess.Constant;
using DataAccess.Entities;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product_Sale_API.Middleware;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

// Configure environment-specific settings
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true);

// Register services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

// Identity / EF
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
var connectionString = builder.Configuration.GetConnectionString("MyCnn");
builder.Services.AddDbContext<SalesAppDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// Lowercase routes
builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

// Swagger + JWT in Swagger
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Sale API", Version = "v1" });
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// DI: repositories & services
builder.Services.AddScoped<IUOW, UOW>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStoreLocationService, StoreLocationService>();
builder.Services.AddSingleton<IVnpay, Vnpay>();

// JWT Authentication
var jwtCfg = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var signingKey = Encoding.UTF8.GetBytes(jwtCfg.Key);
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = true;
    opts.SaveToken = true;
    opts.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var header = ctx.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header) && !header.StartsWith("Bearer "))
                ctx.Token = header;
            return Task.CompletedTask;
        }
    };
    opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtCfg.Issuer,
        ValidAudience = jwtCfg.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(signingKey),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// **IMPORTANT**: Must come before UseEndpoints(...)
app.UseRouting();

app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

// Custom exception handler
app.UseMiddleware<CustomExceptionHandlerMiddleware>();

// Map controllers & hubs
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

// A simple test endpoint
app.MapGet("/test-token", [Authorize] (HttpContext ctx) =>
{
    var uid = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var name = ctx.User.Identity?.Name;
    var role = ctx.User.FindFirst(ClaimTypes.Role)?.Value;

    if (uid == null) return Results.Unauthorized();
    return Results.Ok(new { message = "Token valid", userId = uid, username = name, role });
});

app.Run();
