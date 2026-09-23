using System.Globalization;
using System.Xml.Serialization;

namespace Corteos_Test_Task_Zuev.Infrastructure.ExternalServices.Models;

public class ValuteXmlDto
{
    [XmlAttribute("ID")]
    public string Id { get; set; } = null!;

    [XmlElement("NumCode")]
    public string NumCode { get; set; } = null!;

    [XmlElement("CharCode")]
    public string CharCode { get; set; } = null!;

    [XmlElement("Nominal")]
    public int Nominal { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; } = null!;

    [XmlElement("Value")]
    public string ValueString { get; set; } = null!;

    /// <summary>
    /// Безопасное приведение строки с запятой к типу decimal.
    /// </summary>
    public decimal GetValue()
    {
        if (string.IsNullOrWhiteSpace(ValueString)) return 0;

        // ЦБ РФ использует запятую в качестве разделителя
        var culture = new CultureInfo("ru-RU");
        return decimal.Parse(ValueString, culture);
    }
}