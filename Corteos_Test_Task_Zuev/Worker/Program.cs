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
                await context.Database.MigrateAsync();

                // Извлекаем оркестратор бизнес-логики и запускаем синхронизацию
                var syncService = services.GetRequiredService<RateSyncService>();
                await syncService.SyncAsync();
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1; // Возвращаем код ошибки для внешней вызывающей среды (CI/CD, OS Scheduler)
            }
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // 1. Настройка базы данных PostgreSQL через EF Core
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(connectionString));

                // 2. Регистрация фабрики HttpClient с таймаутом для защиты от зависания запросов к ЦБ РФ
                services.AddHttpClient("CbrClient", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("User-Agent", "CbrRateExporter-MiddleDeveloper-App");
                });

                // 3. Регистрация компонентов архитектуры (Соблюдение интерфейсов)
                services.AddScoped<ICbrClient, CbrClient>();
                services.AddScoped<ICurrencyRateRepository, CurrencyRateRepository>();

                // Доменный сервис оркестрации
                services.AddScoped<RateSyncService>();
            });
}