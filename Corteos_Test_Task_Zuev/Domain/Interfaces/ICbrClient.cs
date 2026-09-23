using Corteos_Test_Task_Zuev.Domain.Entities;

namespace Corteos_Test_Task_Zuev.Domain.Interfaces;

public interface ICbrClient
{
    /// <summary>
    /// Получает курсы валют от ЦБ РФ за конкретную дату.
    /// </summary>
    Task<IEnumerable<CurrencyRate>> GetRatesByDateAsync(DateTime date, CancellationToken cancellationToken = default);
}