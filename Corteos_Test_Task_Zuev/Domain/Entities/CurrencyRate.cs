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

    private CurrencyRate() { }

    public CurrencyRate(string currencyId, string charCode, string numCode, string name, DateTime date, int nominal, decimal value)
    {
        if (string.IsNullOrWhiteSpace(currencyId)) 
            throw new ArgumentException("CurrencyId cannot be empty", nameof(currencyId));

        if (string.IsNullOrWhiteSpace(charCode)) 
            throw new ArgumentException("CharCode cannot be empty", nameof(charCode));

        if (nominal <= 0) 
            throw new ArgumentException("Nominal must be greater than zero", nameof(nominal));

        if (value <= 0) 
            throw new ArgumentException("Value must be greater than zero", nameof(value));

        CurrencyId = currencyId;
        CharCode = charCode;
        NumCode = numCode;
        Name = name;
        Date = date.Date;
        Nominal = nominal;
        Value = value;
    }
}