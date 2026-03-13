using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

public static class FirebaseInitializer
{
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public static void Initialize(IConfiguration configuration)
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            try
            {
                string firebaseConfigPath = configuration["Firebase:ConfigPath"];
                string jsonContent = null;

                // ESTRATÉGIA 1: Tentar ler da variável de ambiente (Render.com)
                string firebaseJsonEnv = Environment.GetEnvironmentVariable("FIREBASE_CONFIG_JSON");
                if (!string.IsNullOrEmpty(firebaseJsonEnv))
                {
                    Console.WriteLine("Using Firebase config from environment variable");
                    jsonContent = firebaseJsonEnv;
                }
                // ESTRATÉGIA 2: Tentar ler do arquivo local
                else if (!string.IsNullOrEmpty(firebaseConfigPath))
                {
                    // Caminhos possíveis para o arquivo
                    string[] possiblePaths = new[]
                    {
                        firebaseConfigPath,
                        Path.Combine(Directory.GetCurrentDirectory(), firebaseConfigPath),
                        Path.Combine(AppContext.BaseDirectory, firebaseConfigPath),
                        Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json"),
                        Path.Combine(AppContext.BaseDirectory, "firebase-service-account.json")
                    };

                    string foundPath = null;
                    foreach (var path in possiblePaths)
                    {
                        if (File.Exists(path))
                        {
                            foundPath = path;
                            Console.WriteLine($"Found Firebase config at: {path}");
                            break;
                        }
                    }

                    if (foundPath != null)
                    {
                        jsonContent = File.ReadAllText(foundPath);
                    }
                }

                if (string.IsNullOrEmpty(jsonContent))
                {
                    Console.WriteLine("WARNING: Firebase configuration not found. Firebase services will be disabled.");
                    return;
                }

                // Parse do JSON
                JObject config = JObject.Parse(jsonContent);

                // Inicializar Firebase App para "arenax"
                if (config["arenax"] != null)
                {
                    try
                    {
                        var app1Config = config["arenax"].ToString();
                        var app1Options = new AppOptions()
                        {
                            Credential = GoogleCredential.FromJson(app1Config)
                        };
                        FirebaseApp.Create(app1Options, "arenax");
                        Console.WriteLine("Firebase App 'arenax' initialized successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing 'arenax' Firebase app: {ex.Message}");
                    }
                }

                // Inicializar Firebase App para "arenaxjogador"
                if (config["arenaxjogador"] != null)
                {
                    try
                    {
                        var app2Config = config["arenaxjogador"].ToString();
                        var app2Options = new AppOptions()
                        {
                            Credential = GoogleCredential.FromJson(app2Config)
                        };
                        FirebaseApp.Create(app2Options, "arenaxjogador");
                        Console.WriteLine("Firebase App 'arenaxjogador' initialized successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error initializing 'arenaxjogador' Firebase app: {ex.Message}");
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR initializing Firebase: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Não relança a exceção para não quebrar o deploy
                // A aplicação continuará sem Firebase se não for crítico
            }
        }
    }
}