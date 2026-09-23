using Corteos_Test_Task_Zuev.Domain.Entities;
using Corteos_Test_Task_Zuev.Domain.Interfaces;
using Corteos_Test_Task_Zuev.Infrastructure.ExternalServices.Models;
using System.Text;
using System.Xml.Serialization;

namespace Corteos_Test_Task_Zuev.Infrastructure.ExternalServices;

/// <summary>
/// Реализация клиента для работы с XML API Центрального Банка РФ.
/// </summary>
public class CbrClient : ICbrClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly XmlSerializer _xmlSerializer;

    public CbrClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _xmlSerializer = new XmlSerializer(typeof(ValCursXmlDto));

        // Регистрируем провайдер кодировок для поддержки windows-1251
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public async Task<IEnumerable<CurrencyRate>> GetRatesByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        // Формат запроса к ЦБ: dd/mm/yyyy
        var url = $"http://cbr.ru{date:dd/MM/yyyy}";

        try
        {
            using var client = _httpClientFactory.CreateClient("CbrClient");
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.GetEncoding("windows-1251"));

            if (_xmlSerializer.Deserialize(reader) is not ValCursXmlDto dto || dto.Valutes == null)
            {
                return Enumerable.Empty<CurrencyRate>();
            }

            // Маппинг DTO из инфраструктурного XML во внутренние доменные сущности
            return dto.Valutes.Select(v => new CurrencyRate(
                currencyId: v.Id,
                charCode: v.CharCode,
                numCode: v.NumCode,
                name: v.Name,
                date: date,
                nominal: v.Nominal,
                value: v.GetValue()
            ));
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public Task<IEnumerable<CurrencyRate>> GetRatesForRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}