namespace NTTaxi.Libraries.Models.Vetcs
{
    public class GetsPayload
    {
        public int accountid { get; set; }
        public string fromdate { get; set; } = string.Empty;
        public string toDate { get; set; } = string.Empty;
    }
}
