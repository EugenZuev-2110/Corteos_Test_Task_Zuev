using Corteos_Test_Task_Zuev.Domain.Entities;

namespace Corteos_Test_Task_Zuev.Domain.Interfaces;

public interface ICurrencyRateRepository
{
    /// <summary>
    /// Проверяет, есть ли в БД хоть какие-то записи (нужно для первичного заполнения за месяц).
    /// </summary>
    Task<bool> HasAnyDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет наличие записей за конкретную дату.
    /// </summary>
    Task<bool> HasDataForDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет или обновляет пачку курсов валют.
    /// </summary>
    Task SaveRatesAsync(IEnumerable<CurrencyRate> rates, CancellationToken cancellationToken = default);
}