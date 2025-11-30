using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using API_TI.Data;
using API_TI.Services.Interfaces;
using API_TI.Middlewares;
using API_TI.Swagger;
using Serilog;
using API_TI.Services.Implementations;
using System.Text;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using System.Reflection;
using API_TI.Services.Background;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Stripe;
using API_TI.Services;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TiPcComponentsApi")
    .WriteTo.Console()
    .WriteTo.File("Logs/app_log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/auth_log.txt", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
);

// Culture
var culture = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Validate Key
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
throw new Exception("JWT Key must be at least 32 characters long.");

if (jwtKey.All(c => char.IsLetterOrDigit(c)))
throw new Exception("JWT Key should contain special characters for better security.");

// Get the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext
builder.Services.AddDbContext<TiPcComponentsContext>(option =>
    option.UseSqlServer(connectionString, sql =>
{
sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
}));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TiPcComponentsContext>();

// Access to context out door of controller
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<StockReservationCleanupService>();
builder.Services.AddHostedService<LowStockAlertService>();


// Add services to the container - Dependency Injection.
builder.Services.AddScoped<ITokenService, API_TI.Services.Implementations.TokenService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDireccionService, DireccionService>();
builder.Services.AddScoped<IGooglePlacesService, GooglePlacesService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IMarcaService, MarcaService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IDescuentoService, DescuentoService>();
builder.Services.AddScoped<IImpuestoService, ImpuestoService>();
builder.Services.AddScoped<ICheckoutService, API_TI.Services.Implementations.CheckoutService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IReembolsoService, ReembolsoService>();
builder.Services.AddScoped<ICfdiService, CfdiService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IReviewService, API_TI.Services.Implementations.ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IOrderStatusService, OrderStatusService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IDevolucionService, DevolucionService>();
builder.Services.AddScoped<IProductoImagenService, ProductoImagenService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IDescuentoAdminService, DescuentoAdminService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<LocalImageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IErrorService, ErrorService>();


// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

// Swagger (single registration only)
builder.Services.AddSwaggerGen(c =>
{
    // ===== GENERAL API INFORMATION =====
    c.SwaggerDoc("v1", new OpenApiInfo
    {
    Title = "TI PC Components API",
    Version = "v1.0.0",
    Description = @"
                <h2>REST API for PC Components E-Commerce</h2>
                <p>This API provides complete endpoints for managing a specialized computer components e-commerce system.</p>
            
                <h3>🔐 Authentication</h3>
                <p>Most endpoints require JWT authentication. Get a token using the <code>/api/auth/login</code> endpoint and use it in the <code>Authorization: Bearer {token}</code> header</p>
            
                <h3>📦 Main Features</h3>
                <ul>
                    <li><strong>Product Management:</strong> Full CRUD with categories, brands, and images</li>
                    <li><strong>Shopping Cart:</strong> Item management, coupon application</li>
                    <li><strong>Orders:</strong> Creation, tracking, and status updates</li>
                    <li><strong>Users:</strong> Registration, authentication, profile, and addresses</li>
                    <li><strong>Administration:</strong> Dashboard, reports, and inventory management</li>
                    <li><strong>Payments:</strong> Stripe and PayPal integration</li>
                </ul>
            
                <h3>📊 Error Codes</h3>
                <p>The API uses standardized error codes. Check each endpoint's documentation for specific codes.</p>
            ",
    Contact = new OpenApiContact
    {
        Name = "TI PC Components - Technical Support",
        Email = "support@tipccomponents.com",
        Url = new Uri("https://tipccomponents.com/support")
    },
    License = new OpenApiLicense
    {
        Name = "MIT License",
        Url = new Uri("https://opensource.org/licenses/MIT")
        },
        TermsOfService = new Uri("https://tipccomponents.com/terms")
    });

    // ===== JWT SECURITY =====
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"
                <strong>JWT Authentication</strong><br/>
                Enter your JWT token in the format: <code>Bearer {token}</code><br/><br/>
        
                <strong>Steps to get the token:</strong><br/>
                1. Use the <code>POST /api/auth/login</code> endpoint<br/>
                2. Copy the token from the response<br/>
                3. Click the 'Authorize' button above<br/>
                4. Paste the token (without 'Bearer', just the token)<br/>
                5. Click 'Authorize'<br/><br/>
        
                <strong>Usage example:</strong><br/>
                <code>Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</code>
            "
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    });

    // ===== CUSTOM FILTERS =====
    c.OperationFilter<ErrorResponsesOperationFilter>();
    c.OperationFilter<AuthorizationOperationFilter>();
    c.SchemaFilter<SwaggerExampleFilter>();

    // ===== XML COMMENTS =====
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // ===== TAG ORGANIZATION =====
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        return new[] { api.GroupName };

        var controllerName = api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor descriptor
            ? descriptor.ControllerName
            : "Unknown";

        return new[] { GetTagDisplayName(controllerName) };
    });

    // ===== TAG ORDERING =====
    c.DocInclusionPredicate((name, api) => true);

    // ===== UI CUSTOMIZATION =====
    //c.EnableAnnotations();
    c.OrderActionsBy((apiDesc) => $"{apiDesc.GroupName}_{apiDesc.HttpMethod}_{apiDesc.RelativePath}");
});

// Helper function for more descriptive tag names
string GetTagDisplayName(string controllerName)
{
return controllerName switch
{
"Auth" => "🔐 Authentication",
"User" => "👤 User",
"Admin" => "⚙️ Administration",
"Producto" => "📦 Products",
"Categoria" => "📂 Categories",
"Marca" => "🏷️ Brands",
"Proveedor" => "🚚 Suppliers",
"Carrito" => "🛒 Cart",
"Order" => "📋 Orders",
"Checkout" => "💳 Checkout",
"Payment" => "💰 Payments",
"Direccion" => "📍 Addresses",
"Wishlist" => "❤️ Wishlist",
"Review" => "⭐ Reviews",
"Descuento" => "🎟️ Discounts",
"Notification" => "🔔 Notifications",
"Factura" => "🧾 Invoices",
"Report" => "📊 Reports",
"Contact" => "📧 Contact",
"FAQ" => "❓ FAQs",
_ => controllerName
};
}


// CORS 
builder.Services.AddCors(options =>
{
options.AddPolicy("AllowFrontend", policy =>
{
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
    ?? new[] { "http://127.0.0.1:5500" };

policy.WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
});
});


// Rate limiting 
builder.Services.AddRateLimiter(options =>
{
// Login limiter
options.AddFixedWindowLimiter("LoginLimiter", opt =>
{
opt.PermitLimit = 5;
opt.Window = TimeSpan.FromMinutes(1);
opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
opt.QueueLimit = 0;
});

// ADD Register limiter
options.AddFixedWindowLimiter("RegisterLimiter", opt =>
{
opt.PermitLimit = 3;
opt.Window = TimeSpan.FromMinutes(10);
opt.QueueLimit = 0;
});

// ADD Refresh limiter
options.AddFixedWindowLimiter("RefreshLimiter", opt =>
{
opt.PermitLimit = 10;
opt.Window = TimeSpan.FromMinutes(1);
opt.QueueLimit = 0;
});

// ADD OAuth limiter
options.AddFixedWindowLimiter("OAuthLimiter", opt =>
{
opt.PermitLimit = 5;
opt.Window = TimeSpan.FromMinutes(1);
opt.QueueLimit = 0;
});

options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();


// Jwt authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
// Role-based policy for Admin
options.AddPolicy("RequireAdmin", policy =>
    policy.RequireRole("Admin"));

// Example: policy combining role or custom permission claim
options.AddPolicy("ManageProducts", policy =>
    policy.RequireAssertion(context =>
        context.User.IsInRole("Admin") ||
        (context.User.HasClaim(c => c.Type == "permissions" && c.Value.Split(',').Contains("product.manage")))
    ));
});

// Cookie settings for OAuth flows (if you use cookie during OAuth redirect)
builder.Services.ConfigureApplicationCookie(options =>
{
options.Cookie.SameSite = SameSiteMode.None;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.HttpOnly = true;
});

builder.Services.AddMemoryCache();


builder.Services.Configure<GooglePlacesSettings>(
    builder.Configuration.GetSection("Google:Places"));



// Build App
var app = builder.Build();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
RequestPath = "/uploads"
});

// Serilog - push correlation id property per request
app.Use(async (context, next) =>
{
using (Serilog.Context.LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
{
await next();
}
});

// Use custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseHttpsRedirection();
if (!app.Environment.IsDevelopment())
{
app.UseHsts();
}
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseMiddleware<JwtUserContextMiddleware>();
app.UseAuthorization();


// Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TI PC Components API v1.0");
        c.RoutePrefix = "swagger"; // URL: /swagger

        // ===== UI CUSTOMIZATION =====
        c.DocumentTitle = "TI PC Components API - Documentation";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();

        // ===== RESPONSE CONFIGURATION =====
        c.DisplayOperationId();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse all by default

        // ===== CUSTOM THEME (OPTIONAL) =====
        c.InjectStylesheet("/swagger-ui/custom.css"); // If you want custom CSS

        // ===== SECURITY CONFIGURATION =====
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
    });
//}

// Endpoints
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

// Strongly Typed JWT Settings class
public class JwtSettings
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpireMinutes { get; set; }
}

public class GooglePlacesSettings
{
    public string ApiKey { get; set; }
}