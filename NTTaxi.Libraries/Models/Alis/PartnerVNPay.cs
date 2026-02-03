namespace NTTaxi.Libraries.Models.Alis;

public class PartnerVNPay
{
    public string IdDoiTac { get; set; } = string.Empty;           // ID ĐỐI TÁC
    public string IdCongTy { get; set; } = string.Empty;             // ID CÔNG TY
    public string IdHeThong { get; set; } = string.Empty;             // ID HỆ THỐNG
    public string ThoiDiemPhatSinhCuocDi { get; set; } = string.Empty; // Thời điểm phát sinh cuốc đi

    public string SdtKhachHang { get; set; } = string.Empty;         // SĐT khách hàng
    public string TenKhachHang { get; set; } = string.Empty;         // Tên khách hàng

    public string QuangDuong { get; set; } = string.Empty;          // Quãng đường (km) - nếu bạn lưu mét thì đổi ghi chú
    public string TienCuoc { get; set; } = string.Empty;            // Tiền cước

    public string HinhThucThanhToan { get; set; } = string.Empty;    // Hình thức Thanh toán (Cash/Banking/VNPay/...)
    public string TrangThaiThanhToan { get; set; } = string.Empty;   // Trạng thái thanh toán

    public string DienThoaiLaiXe { get; set; } = string.Empty;      // Điện thoại Lái xe
    public string MaLaiXe { get; set; } = string.Empty;       // Mã Lái xe
    public string SoTai { get; set; } = string.Empty;       // Số tài

    public string BienSoXe { get; set; } = string.Empty;     // Biển số xe
    public string TrangThaiChuyenDi { get; set; } = string.Empty; // Trạng thái chuyến đi

    public string DiemDon { get; set; } = string.Empty;       // Điểm đón
    public string DiemTra { get; set; } = string.Empty;       // Điểm trả

    public string DichVu { get; set; } = string.Empty;     // Dịch vụ
}
