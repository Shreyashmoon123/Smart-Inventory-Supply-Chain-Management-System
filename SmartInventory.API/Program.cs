
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Application.Interfaces;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;
using SmartInventory.Infrastructure.Interceptors;
using SmartInventory.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header. Enter your JWT token below.",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                "Bearer", document)] = []
        });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuditLogInterceptor>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );

    options.AddInterceptors(
        serviceProvider.GetRequiredService<AuditLogInterceptor>()
    );
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordHashingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT Key must contain at least 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Issuer and Audience must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),

                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType =
                    System.Security.Claims.ClaimTypes.Role
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }
        )
    );
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

var initialAdminEmail =
    builder.Configuration["InitialAdmin:Email"];

var initialAdminPassword =
    builder.Configuration["InitialAdmin:Password"];

if (!string.IsNullOrWhiteSpace(initialAdminEmail) &&
    !string.IsNullOrWhiteSpace(initialAdminPassword))
{
    using var scope = app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var passwordService =
        scope.ServiceProvider.GetRequiredService<IPasswordService>();

    var adminRole = await dbContext.Roles
        .FirstOrDefaultAsync(r => r.RoleName == "Admin");

    if (adminRole == null)
    {
        throw new InvalidOperationException(
            "Admin role was not found.");
    }

    var adminAlreadyExists = await dbContext.UserRoles
        .AnyAsync(ur => ur.RoleId == adminRole.RoleId);

    if (!adminAlreadyExists)
    {
        var email = initialAdminEmail.Trim().ToLowerInvariant();

        var adminUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (adminUser == null)
        {
            adminUser = new SmartInventory.Core.Entities.User
            {
                FullName = "System Administrator",
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            adminUser.PasswordHash = passwordService.HashPassword(
                adminUser,
                initialAdminPassword
            );

            dbContext.Users.Add(adminUser);

            await dbContext.SaveChangesAsync();
        }

        dbContext.UserRoles.Add(
            new SmartInventory.Core.Entities.UserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId
            });

        await dbContext.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

if (allowedOrigins.Length > 0)
{
    app.UseCors("FrontendPolicy");
}

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
