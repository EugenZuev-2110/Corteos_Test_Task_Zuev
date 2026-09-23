namespace Corteos_Test_Task_Zuev.Domain.Interfaces;

public interface ICurrencyRateRepository
{
    /// <summary>
    /// Проверяет наличие записей за конкретную дату.
    /// </summary>
    Task<bool> HasDataForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}