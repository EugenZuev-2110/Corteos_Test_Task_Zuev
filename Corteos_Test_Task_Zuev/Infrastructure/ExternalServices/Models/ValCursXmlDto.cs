using System.Xml.Serialization;

namespace Corteos_Test_Task_Zuev.Infrastructure.ExternalServices.Models;

[XmlRoot("ValCurs")]
public class ValCursXmlDto
{
    [XmlAttribute("Date")]
    public string DateString { get; set; } = null!;

    [XmlElement("Valute")]
    public List<ValuteXmlDto> Valutes { get; set; } = [];
}