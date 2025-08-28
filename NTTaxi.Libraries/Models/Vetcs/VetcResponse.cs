using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Vetcs
{
    public class VetcResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<VetcItemResponse> data { get; set; } = new List<VetcItemResponse>();
    }

    public class VetcItemResponse
    {

        [JsonPropertyName("transportTransId")]
        public string TransportTransId { get; set; } = string.Empty;
        [JsonPropertyName("plate")]
        public string Plate { get; set; } = string.Empty;
        [JsonPropertyName("checkinTollName")]
        public string CheckInName { get; set; } = string.Empty;
        [JsonPropertyName("totalAmount")]
        public string Amount { get; set; } = string.Empty;
        [JsonPropertyName("checkoutDatetime")]
        public string CheckerOutDateTime { get; set; } = string.Empty;
        [JsonPropertyName("pass")]
        public string Pass { get; set; } = string.Empty; // SCC, ETC, VETC
        [JsonPropertyName("priceTicketType")]
        public string PriceTicketType { get; set; } = string.Empty;
    }
}
