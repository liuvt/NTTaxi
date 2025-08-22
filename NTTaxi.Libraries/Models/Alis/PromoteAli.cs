namespace NTTaxi.Libraries.Models.Alis
{
    public class PromoteAli
    {
        public string ID { get; set; } = string.Empty; // ID cuốc xe
        public string PartnerCode { get; set; } = string.Empty; // Mã đối tác
        public string DriverPhoneNumber { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty; // Tiền cước cuốc xe
        public string PromotionPrice { get; set; } = string.Empty;
        public string ReturnDiscount { get; set; } = string.Empty; // Giảm giá chiều về
        public string CustomerPay { get; set; } = string.Empty; // Khách hàng phải trả 
        public string ExtraFee { get; set; } = string.Empty; // Phụ phí
        public string Discount { get; set; } = string.Empty; // Chiết khấu  
        public string Revenue { get; set; } = string.Empty;
        public string DepositRemaining { get; set; } = string.Empty; // Tiền ký quỹ còn lại 
        public string PaymentMethod { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
