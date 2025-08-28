using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Vetcs
{
    public class VetcItem
    {
        public string TransportTransId { get; set; } = string.Empty;
        public string Plate { get; set; } = string.Empty;
        public string CheckInName { get; set; } = string.Empty;
        public decimal? Amount { get; set; } 
        public DateTime? CheckerOutDateTime { get; set; } 
        public string Pass { get; set; } = string.Empty; // SCC, ETC, VETC
        public string PriceTicketType { get; set; } = string.Empty;
    }
}
