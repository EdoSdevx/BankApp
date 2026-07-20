using System.Text;
using System.Text.Json.Serialization;
using BankApp.BankApp.Common.Interfaces.DataAccess;
using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Options;
using BankApp.BankApp.DataAccess;
using BankApp.BankApp.Services;
using BankApp.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

builder.Services.Configure<JwtSettings>(jwtSettingsSection);

var smtpSettingsSection = builder.Configuration.GetSection("SmtpSettings");
builder.Services.Configure<SmtpSettings>(smtpSettingsSection);

var resetTokenSection = builder.Configuration.GetSection("ResetToken");
builder.Services.Configure<ResetTokenOptions>(resetTokenSection);

var groqSection = builder.Configuration.GetSection("Groq");
builder.Services.Configure<GroqOptions>(groqSection);

builder.Services.AddScoped<DatabaseContext>();
builder.Services.AddScoped<IAuthDataAccess,AuthDataAccess>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IJwtTokenService,JwtTokenService>();
builder.Services.AddScoped<IEmailService,EmailService>();
builder.Services.AddScoped<ICustomerDataAccess,CustomerDataAccess>();
builder.Services.AddScoped<ICustomerService,CustomerService>();
builder.Services.AddScoped<IBranchDataAccess,BranchDataAccess>();
builder.Services.AddScoped<IBranchService,BranchService>();
builder.Services.AddScoped<IRoleDataAccess,RoleDataAccess>();
builder.Services.AddScoped<IRoleService,RoleService>();
builder.Services.AddScoped<ICurrencyDataAccess,CurrencyDataAccess>();
builder.Services.AddScoped<ICurrencyService,CurrencyService>();
builder.Services.AddScoped<IAccountDataAccess,AccountDataAccess>();
builder.Services.AddScoped<IAccountService,AccountService>();
builder.Services.AddScoped<IEmployeeDataAccess,EmployeeDataAccess>();
builder.Services.AddScoped<IEmployeeService,EmployeeService>();
builder.Services.AddScoped<IExchangeRateDataAccess,ExchangeRateDataAccess>();
builder.Services.AddScoped<IExchangeRateService,ExchangeRateService>();
builder.Services.AddScoped<ITransactionDataAccess,TransactionDataAccess>();
builder.Services.AddScoped<ITransactionService,TransactionService>();
builder.Services.AddScoped<IBillDataAccess,BillDataAccess>();
builder.Services.AddScoped<IBillService,BillService>();
builder.Services.AddScoped<ICustomerPortalDataAccess,CustomerPortalDataAccess>();
builder.Services.AddScoped<ICustomerPortalService,CustomerPortalService>();
builder.Services.AddScoped<IAdminApprovalDataAccess,AdminApprovalDataAccess>();
builder.Services.AddScoped<IAdminApprovalService,AdminApprovalService>();
builder.Services.AddScoped<ILoanDataAccess,LoanDataAccess>();
builder.Services.AddScoped<ILoanService,LoanService>();
builder.Services.AddScoped<ICustomerLoanDataAccess,CustomerLoanDataAccess>();
builder.Services.AddScoped<ICustomerLoanService,CustomerLoanService>();

builder.Services.AddScoped<ChatService>();

builder.Services.AddHostedService<ExchangeRateUpdaterService>();
builder.Services.AddHostedService<MonthlyLoanProcessor>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste only the JWT token. Swagger will add 'Bearer' automatically."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, externalResource: null)] = []
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();

app.Run();
