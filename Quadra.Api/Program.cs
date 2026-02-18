using Core;
using Infrastructure;
using Infrastructure.Authenticate;
using Infrastructure.ServiceExtension;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDIServices(builder.Configuration);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IJWTManager, JWTManager>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddScoped<ISportsCategoryService, SportsCategoryService>();
builder.Services.AddScoped<ISportsCourtOperationService, SportsCourtOperationService>();
builder.Services.AddScoped<ISportsCourtService, SportsCourtService>();
builder.Services.AddScoped<ICourtEvaluationsService, CourtEvaluationsService>();
builder.Services.AddScoped<ISportsCourtAppointmentsService, SportsCourtAppointmentsService>();
builder.Services.AddScoped<ISportsCenterService, SportsCenterService>();
builder.Services.AddScoped<ISportsCenterUsersService, SportsCenterUsersService>();
builder.Services.AddScoped<ISportsCourtCategoryService, SportsCourtCategoryService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ISportsRegistrationService, SportsRegistrationService>();
builder.Services.AddScoped<INotificationsService, NotificationsService>();
builder.Services.AddScoped<IClientEvaluationService, ClientEvaluationService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IImageUrlService, ImageUrlService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IOtpService, OtpService>();


// Registro do serviço de notificação
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configuração do JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
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
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    // Configuração para incluir comentários XML
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    // Verifica se o arquivo existe antes de tentar adicionar
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    else
    {
        // Opcional: Log para ajudar no debug
        Console.WriteLine($"Arquivo XML de documentação não encontrado em: {xmlPath}");
    }
});
builder.WebHost.UseUrls("http://*:80");
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // ? Sua chave secreta DEVE ser a mesma usada para gerar o token!
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidateIssuer = false,  // Ajuste para true se usar issuer
            ValidateAudience = false // Ajuste para true se usar audience
        };
    });
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
});
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var app = builder.Build();
// Configuração do Firebase
FirebaseInitializer.Initialize(builder.Configuration);
app.MigrateDatabase();
app.UseStaticFiles(); // Isso serve arquivos de wwwroot
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Habilitando CORS, redirecionamento HTTPS, autenticação e autorização
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
