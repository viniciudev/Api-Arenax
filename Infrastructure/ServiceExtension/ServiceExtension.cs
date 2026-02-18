using Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.ServiceExtension
{
    public static class ServiceExtension
    {
        public static IServiceCollection 
            AddDIServices(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            string connectionString;

            // Verifica se está em produção
            if (environment.IsProduction())
            {
                // Em produção, usa a DATABASE_URL do Digital Ocean App Platform
                connectionString = GetProductionConnectionStringSafely();
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Se não tem DATABASE_URL, usa a connection string do appsettings como fallback
                    connectionString = configuration.GetConnectionString("DefaultConnection");
                    Console.WriteLine("WARNING: DATABASE_URL not found, using fallback connection string");
                }
                else
                {
                    Console.WriteLine("Using production database connection from DATABASE_URL");
                }
                Console.WriteLine("Using production database connection from DATABASE_URL");
            }
            else
            {
                // Em desenvolvimento, usa a connection string do appsettings.json
                connectionString = configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine("Using development database connection from appsettings.json");
            }

            services.AddDbContext<DbContextClass>(options =>
            {
                //options.UseNpgsql("host=localhost;user id=postgres;password=123456789;database=Comercial3irmaos;Pooling=false;Timeout=300;CommandTimeout=300;");
                //options.UseNpgsql("host=89.117.146.50;user id=postgres;password=7A24Jdp1Rcyv;database=ComercialHomolog;Pooling=false;Timeout=300;CommandTimeout=300;");
                //options.UseNpgsql("host=localhost;user id=postgres;password=admin;database=4Axon;Pooling=false;Timeout=300;CommandTimeout=300;");
                options.UseNpgsql(connectionString,
                    b =>
                    {
                        b.UseNodaTime();
                        b.MigrationsAssembly("Infrastructure");
                        b.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                    });

            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISportsCategoryRepository, SportsCategoryRepository>();
            services.AddScoped<ISportsCourtRepository, SportsCourtRepository>();
            services.AddScoped<ISportsCourtOperationRepository, SportsCourtOperationRepository>();
            services.AddScoped<ICourtEvaluationsRepository, CourtEvaluationsRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISportsCourtAppointmentsRepository, SportsCourtAppointmentsRepository>();
            services.AddScoped<ISportsCenterRepository, SportsCenterRepository>();
            services.AddScoped<ISportsCenterUsersRepository, SportsCenterUsersRepository>();
            services.AddScoped<ISportsCourtCategoryRepository, SportsCourtCategoryRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ISportsCourtImageRepository, SportsCourtImageRepository>();
            services.AddScoped<ISportsRegistrationRepository, SportsRegistrationRepository>();
            services.AddScoped<INotificationsRepository, NotificationsRepository>();

            services.AddScoped<IImageUrlRepository, ImageUrlRepository>();
            services.AddScoped<IClientEvaluationRepository, ClientEvaluationRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            return services;
        }

        private static string GetProductionConnectionStringSafely()
        {
            try
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                // Verifica se é uma variável não substituída (começa com ${)
                if (string.IsNullOrEmpty(databaseUrl) || databaseUrl.StartsWith("${"))
                {
                    Console.WriteLine($"DATABASE_URL not available or not substituted: {databaseUrl ?? "null"}");
                    return null;
                }

                return ConvertDatabaseUrlToNpgsql(databaseUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting production connection string: {ex.Message}");
                return null; // Retorna null em vez de lançar exceção
            }
        }
        private static string GetProductionConnectionString()
        {
            // DATABASE_URL é injetado automaticamente pelo Digital Ocean App Platform
            // Formato: postgresql://user:password@host:port/database
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrEmpty(databaseUrl))
            {
                throw new InvalidOperationException("DATABASE_URL environment variable not found. This is required in production.");
            }

            return ConvertDatabaseUrlToNpgsql(databaseUrl);
        }

        private static string ConvertDatabaseUrlToNpgsql(string databaseUrl)
        {
            try
            {
                var uri = new Uri(databaseUrl);
                var userInfo = uri.UserInfo.Split(':');

                var username = userInfo[0];
                var password = userInfo[1];
                var database = uri.AbsolutePath.TrimStart('/');

                // Configurações específicas para Digital Ocean Managed Database
                return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Lifetime=300;";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse DATABASE_URL: {databaseUrl}", ex);
            }
        }
    }
}
