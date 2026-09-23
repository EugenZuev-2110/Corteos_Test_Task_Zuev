namespace Corteos_Test_Task_Zuev.Domain.Entities;

/// <summary>
/// Сущность курса валюты, оптимизированная под 3НФ.
/// </summary>
public class CurrencyRate
{
    /// <summary>
    /// Внутренний код валюты ЦБ РФ (например, "R01235" для USD).
    /// </summary>
    public string CurrencyId { get; private set; } = null!;

    /// <summary>
    /// Буквенный код валюты (ISO) (например, "USD").
    /// </summary>
    public string CharCode { get; private set; } = null!;

    /// <summary>
    /// Цифровой код валюты (например, "840").
    /// </summary>
    public string NumCode { get; private set; } = null!;

    /// <summary>
    /// Полное наименование валюты (например, "Доллар США").
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Дата, к которой относится курс.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    /// Номинал (количество единиц валюты, для которых указан курс, например, 1 или 100).
    /// </summary>
    public int Nominal { get; private set; }

    /// <summary>
    /// Значение курса к рублю.
    /// </summary>
    public decimal Value { get; private set; }
}