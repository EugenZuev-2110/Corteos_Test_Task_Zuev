using Corteos_Test_Task_Zuev.Domain.Entities;
using Corteos_Test_Task_Zuev.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Corteos_Test_Task_Zuev.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий для выполнения операций с курсами валют в БД PostgreSQL.
/// </summary>
public class CurrencyRateRepository : ICurrencyRateRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyRateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasAnyDataAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyRates.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasDataForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        return await _context.CurrencyRates.AnyAsync(r => r.Date == targetDate, cancellationToken);
    }

    public async Task SaveRatesAsync(IEnumerable<CurrencyRate> rates, CancellationToken cancellationToken = default)
    {
        if (rates == null || !rates.Any()) return;

        // Чтобы не упасть по Unique Constraint, если за этот день данные частично были,
        // мы можем использовать стратегию: сначала обновить существующие/добавить новые.

        var targetDates = rates.Select(r => r.Date.Date).Distinct().ToList();

        // Извлекаем то, что уже есть в БД на эти даты, чтобы сделать merge в памяти
        var existingRates = await _context.CurrencyRates
            .Where(r => targetDates.Contains(r.Date))
            .ToListAsync(cancellationToken);

        foreach (var rate in rates)
        {
            var existing = existingRates.FirstOrDefault(x => x.CurrencyId == rate.CurrencyId && x.Date == rate.Date.Date);
            if (existing != null)
            {
                // Если запись есть — обновляем ее поля (на случай изменения курса или исправления данных ЦБ)
                _context.Entry(existing).CurrentValues.SetValues(rate);
            }
            else
            {
                // Если записи нет — добавляем
                await _context.CurrencyRates.AddAsync(rate, cancellationToken);
            }
        }

        // Сохраняем пачкой в одной транзакции
        await _context.SaveChangesAsync(cancellationToken);
    }
}