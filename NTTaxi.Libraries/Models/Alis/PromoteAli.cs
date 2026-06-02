namespace NTTaxi.Libraries.Models.Alis
{
    public class PromoteAli
    {
        public string ID { get; set; } = string.Empty; // ID cuốc xe
        public string PartnerCode { get; set; } = string.Empty; // Mã đối tác
        public string DriverPhoneNumber { get; set; } = string.Empty; //SĐT lái xe
        public string PriceTrip { get; set; } = string.Empty; // Tiền cước cuốc xe
        public string PromotionPrice { get; set; } = string.Empty; //Tiền khuyến mãi
        public string VoucherPrice { get; set; } = string.Empty; //Tiền voucher
        public string ReturnDiscount { get; set; } = string.Empty; // Giảm giá chiều về
        public string ExtraFee { get; set; } = string.Empty; // Phụ phí
        public string CustomerPay { get; set; } = string.Empty; // Khách hàng phải trả 
        public string PaymentMethod { get; set; } = string.Empty; //Phương thức thanh toán
        public string CreatedAt { get; set; } = string.Empty; //Thời gian tạo
    }
}
