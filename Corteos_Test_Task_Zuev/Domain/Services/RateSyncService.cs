using Corteos_Test_Task_Zuev.Domain.Interfaces;
namespace Corteos_Test_Task_Zuev.Domain.Services;

/// <summary>
/// Сервис оркестрации бизнес-логики синхронизации курсов валют.
/// Отвечает за координацию между внешним API ЦБ РФ и локальным хранилищем.
/// </summary>
public class RateSyncService
{
    private readonly ICbrClient _cbrClient;
    private readonly ICurrencyRateRepository _repository;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RateSyncService"/>.
    /// </summary>
    /// <param name="cbrClient">Инфраструктурный клиент для работы с API ЦБ РФ.</param>
    /// <param name="repository">Репозиторий для работы с базой данных курсов валют.</param>
    public RateSyncService(
        ICbrClient cbrClient,
        ICurrencyRateRepository repository)
    {
        _cbrClient = cbrClient;
        _repository = repository;
    }

    /// <summary>
    /// Запускает основной процесс синхронизации данных.
    /// Автоматически определяет состояние базы данных и выполняет либо первичную
    /// инициализацию за 30 дней, либо ежедневное обновление текущих курсов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены выполнения асинхронной операции.</param>
    /// <returns>Асинхронная задача, представляющая процесс синхронизации.</returns>
    /// <exception cref="Exception">Бросается в случае критической ошибки и логируется на верхнем уровне.</exception>
    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncDailyDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Выполняет первичное наполнение базы данных историческими данными.
    /// Последовательно запрашивает и сохраняет курсы валют за последние 30 дней от текущей даты.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены выполнения асинхронной операции.</param>
    /// <returns>Асинхронная задача, представляющая процесс выгрузки истории.</returns>
    private async Task SyncHistoricalDataAsync(CancellationToken cancellationToken)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-30);

        // ЦБ РФ API для диапазонов обычно требует выгрузку по конкретной валюте, 
        // либо мы можем последовательно выкачать данные за каждый из 30 дней.
        // Для демонстрации надежности скачиваем по дням циклом (параллельно или последовательно с паузой).
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (await _repository.HasDataForDateAsync(date, cancellationToken)) continue;

            var rates = await _cbrClient.GetRatesByDateAsync(date, cancellationToken);
            await _repository.SaveRatesAsync(rates, cancellationToken);
        }
    }

    /// <summary>
    /// Выполняет ежедневное регламентное обновление курсов валют.
    /// Проверяет наличие данных на текущую дату. Если ЦБ РФ еще не опубликовал курс на сегодня,
    /// плавно смещается на дни назад в поисках последних актуальных данных.
    /// </summary>
    private async Task SyncDailyDataAsync(CancellationToken cancellationToken)
    {
        var targetDate = DateTime.Today;
        const int maxDaysBack = 5; // Защита на случай длинных праздников/выходных

        for (int i = 0; i < maxDaysBack; i++)
        {
            if (await _repository.HasDataForDateAsync(targetDate, cancellationToken))
            {
                return;
            }

            var rates = await _cbrClient.GetRatesByDateAsync(targetDate, cancellationToken);
            var currencyRates = rates.ToList();

            if (currencyRates.Any())
            {
                await _repository.SaveRatesAsync(currencyRates, cancellationToken);
                return; // Успешно сохранили — выходим
            }

            targetDate = targetDate.AddDays(-1);
        }
    }

}