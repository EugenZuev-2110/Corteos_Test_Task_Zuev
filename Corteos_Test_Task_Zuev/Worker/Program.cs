using Corteos_Test_Task_Zuev.Domain.Interfaces;
using Corteos_Test_Task_Zuev.Domain.Services;
using Corteos_Test_Task_Zuev.Infrastructure.ExternalServices;
using Corteos_Test_Task_Zuev.Infrastructure.Persistence;
using Corteos_Test_Task_Zuev.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corteos_Test_Task_Zuev.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Явно задаем рабочую папку, чтобы программа гарантированно увидела appsettings.json
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        // Сборка хоста приложения (конфигурация, логирование, DI)
        var host = CreateHostBuilder(args).Build();

        // Создаем Scope для безопасного извлечения Scoped-зависимостей (DbContext)
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                // Автоматическое применение миграций при старте (Middle-стандарт для контейнеризации)
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();

                // Извлекаем оркестратор бизнес-логики и запускаем синхронизацию
                var syncService = services.GetRequiredService<RateSyncService>();
                await syncService.SyncAsync();
            }
            catch (Exception ex)
            {
                // Добавляем задержку, чтобы успеть прочитать ошибку в консоли
                Console.WriteLine("\nНажмите ENTER для закрытия приложения...");
                Console.ReadLine();
                Environment.ExitCode = 1; // Возвращаем код ошибки для внешней вызывающей среды (CI/CD, OS Scheduler)
            }
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.Sources.Clear(); // Очищаем стандартные пути
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            // Здесь ваш текущий код (строка подключения, DbContext, HttpClient и т.д.)
            var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Критическая ошибка: Строка подключения 'DefaultConnection' не найдена или равна null! " +
                    "Проверьте, что внутри appsettings.json точно есть секция ConnectionStrings.");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // 2. Регистрация фабрики HttpClient с таймаутом и обходом проблем с SSL-сертификатами ЦБ
            services.AddHttpClient("CbrClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "CbrRateExporter-App");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true, // Автоматически переходить по редиректам сервера ЦБ
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true // Игнорируем локальные проблемы с SSL-сертификатами РФ
            });

            services.AddScoped<ICbrClient, CbrClient>();
            services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();
            services.AddScoped<RateSyncService>();
        });
}