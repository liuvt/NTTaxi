using System.Text.Json.Serialization;

namespace NTTaxi.Libraries.Models.Skysofts
{
    public class ReportPayload
    {
        public int ReportID { get; set; }
        public string Description { get; set; } = string.Empty;
        public string FromDate { get; set; } = string.Empty;
        public string FromTime { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string ToTime { get; set; } = string.Empty;
        public string RegisterNo { get; set; } = string.Empty;
        public string Interval { get; set; } = string.Empty;
        public int MaxReportDay { get; set; }
        public bool Granted { get; set; }
        public string TravelLine { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string VehicleGroup { get; set; } = string.Empty;
        public string PlaceGroup { get; set; } = string.Empty;
        public bool DrivingOver4Hours { get; set; }
        public int ReportType { get; set; }
        public string User { get; set; } = string.Empty;
        public string LineNo { get; set; } = string.Empty;
        public bool IgnoreTest { get; set; }
        public string Department { get; set; } = string.Empty;
        public bool XlsxFormat { get; set; }
        public bool InternalUse { get; set; }
        public int ShowOnWeb { get; set; }
        public bool FuelReport { get; set; }
        public bool AutoGrant { get; set; }
    }

    public class ReportCategory
    {
        public int ReportType { get; set; } 
        public string ReportCate { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class PlaceMarkDTO
    {
        // ID & Loại
        public int PlaceMarkID { get; set; }
        public int PlaceType { get; set; }
        public int UserID { get; set; }

        // Vị trí
        public double X { get; set; }
        public double Y { get; set; }
        public int Direction { get; set; }

        // Mô tả & ghi chú
        public string Description { get; set; }
        public string TextToSpeak { get; set; }
        public string Note { get; set; }
        public string VoiceNote { get; set; }
        public byte[] VoiceData { get; set; }
        public string VoiceMd5 { get; set; }

        // Hiển thị
        public string TextColor { get; set; }
        public bool ShowInfo { get; set; }
        public int IconID { get; set; }

        // Trạng thái & kiểm soát
        public bool IsDeleted { get; set; }
        public bool IsMonitoringMark { get; set; }
        public bool MarkMatrixFound { get; set; }
        public bool HavingStop { get; set; }
        public bool IsPublicView { get; set; }
        public bool SpeakBothWays { get; set; }
        public int SpeakInDistance { get; set; }
        public bool AllowUnlock { get; set; }
        public bool IsTempPlace { get; set; }
        public bool IsLineBoundary { get; set; }
        public bool BackWay { get; set; }

        // Thời gian
        public DateTime? CreateDate { get; set; }
        public DateTime? MatrixFoundDate { get; set; }
        public DateTime? MatrixLeaveDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? FromDate { get; set; }

        // Khoảng cách & tính toán
        public int DiffDistance { get; set; }
        public double DistanceFromHere { get; set; }
        public double DistanceToHere { get; set; }
        public int Duration { get; set; }
        public int DurationFromHere { get; set; }
        public int DurationToHere { get; set; }
        public int TollFee { get; set; }

        // Vận tải
        public int VehicleID { get; set; }
        public int TicketFare { get; set; }
        public double LoadHeight { get; set; }

        // Boundary & Queue
        public string Boundary { get; set; }
        public string QueueBoundary { get; set; }
        public string PriceBoundary { get; set; }

        // Tolls
        public List<PlaceMarkDTO> TollFromHere { get; set; }
        public List<PlaceMarkDTO> TollToHere { get; set; }

        // Queue vehicles
        public List<int> QueueVehicles { get; set; }

        // Token & retries
        public string Token { get; set; }
        public int Retries { get; set; }

        public PlaceMarkDTO()
        {
            Description = string.Empty;
            TextToSpeak = string.Empty;
            Note = string.Empty;
            VoiceNote = string.Empty;
            VoiceData = Array.Empty<byte>();
            TextColor = "Color.Black";
            TollFromHere = new List<PlaceMarkDTO>();
            TollToHere = new List<PlaceMarkDTO>();
            QueueVehicles = new List<int>();
        }
    }
    public class ReportParamDTO1
    {
        [JsonPropertyName("vehicleID")]
        public int VehicleID { get; set; }

        [JsonPropertyName("registerNo")]
        public string RegisterNo { get; set; }

        [JsonPropertyName("fromDate")]
        public DateTime? FromDate { get; set; }

        [JsonPropertyName("toDate")]
        public DateTime? ToDate { get; set; }

        [JsonPropertyName("reportCate")]
        public string ReportCate { get; set; }

        [JsonPropertyName("placeMark")]
        public PlaceMarkDTO PlaceMark { get; set; }

        [JsonPropertyName("diffTime")]
        public int DiffTime { get; set; }

        [JsonPropertyName("diffDistance")]
        public int DiffDistance { get; set; }

        [JsonPropertyName("customerID")]
        public int CustomerID { get; set; }

        [JsonPropertyName("countByStop")]
        public bool CountByStop { get; set; }

        [JsonPropertyName("reportID")]
        public int ReportID { get; set; }

        [JsonPropertyName("interval")]
        public int Interval { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("fileData")]
        public string FileData { get; set; }

        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        [JsonPropertyName("groupID")]
        public int GroupID { get; set; }

        [JsonPropertyName("alarmType")]
        public string AlarmType { get; set; }

        [JsonPropertyName("placeGroupID")]
        public int PlaceGroupID { get; set; }

        [JsonPropertyName("countBySpeed")]
        public bool CountBySpeed { get; set; }

        [JsonPropertyName("recognizePlace")]
        public bool RecognizePlace { get; set; }

        [JsonPropertyName("summaryType")]
        public string SummaryType { get; set; }

        [JsonPropertyName("pickupPointID")]
        public int PickupPointID { get; set; }

        [JsonPropertyName("updateDistance")]
        public bool UpdateDistance { get; set; }

        [JsonPropertyName("excavatorID")]
        public int ExcavatorID { get; set; }

        [JsonPropertyName("materialID")]
        public int MaterialID { get; set; }

        [JsonPropertyName("releasePointID")]
        public int ReleasePointID { get; set; }

        [JsonPropertyName("shiftID")]
        public int ShiftID { get; set; }

        [JsonPropertyName("filterByDB9")]
        public int FilterByDB9 { get; set; }

        [JsonPropertyName("filterByCard")]
        public int FilterByCard { get; set; }

        [JsonPropertyName("filterByWorking")]
        public int FilterByWorking { get; set; }

        [JsonPropertyName("filterByCamera")]
        public int FilterByCamera { get; set; }

        [JsonPropertyName("filterBySimType")]
        public int FilterBySimType { get; set; }

        [JsonPropertyName("filterBySkysoftSIM")]
        public int FilterBySkysoftSIM { get; set; }

        [JsonPropertyName("filterNoOpenDoor")]
        public bool FilterNoOpenDoor { get; set; }

        [JsonPropertyName("filterOutOfCredit")]
        public bool FilterOutOfCredit { get; set; }

        [JsonPropertyName("notAllSkysoftSIM")]
        public bool NotAllSkysoftSIM { get; set; }

        [JsonPropertyName("ignoreSkysoftDevice")]
        public bool IgnoreSkysoftDevice { get; set; }

        [JsonPropertyName("filterEmptySIMNo")]
        public bool FilterEmptySIMNo { get; set; }

        [JsonPropertyName("vehicleCode")]
        public string VehicleCode { get; set; }

        [JsonPropertyName("phoneNo")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("versionNo")]
        public string VersionNo { get; set; }

        [JsonPropertyName("filterByDeviceType")]
        public string FilterByDeviceType { get; set; }

        [JsonPropertyName("profileID")]
        public int ProfileID { get; set; }

        [JsonPropertyName("fromTime")]
        public string FromTime { get; set; }

        [JsonPropertyName("toTime")]
        public string ToTime { get; set; }

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }

        [JsonPropertyName("countByArrived")]
        public bool CountByArrived { get; set; }

        [JsonPropertyName("tripCountType")]
        public int TripCountType { get; set; }

        [JsonPropertyName("superUser")]
        public bool SuperUser { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("privateKey")]
        public string PrivateKey { get; set; }

        [JsonPropertyName("travelLineID")]
        public int TravelLineID { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("drivingOver4Hours")]
        public bool DrivingOver4Hours { get; set; }

        [JsonPropertyName("countByRelease")]
        public bool CountByRelease { get; set; }

        [JsonPropertyName("noteType")]
        public string NoteType { get; set; }

        [JsonPropertyName("filterByExpire")]
        public bool FilterByExpire { get; set; }

        [JsonPropertyName("inDuration")]
        public int InDuration { get; set; }

        [JsonPropertyName("generateHtml")]
        public bool GenerateHtml { get; set; }

        [JsonPropertyName("having2Points")]
        public bool Having2Points { get; set; }

        [JsonPropertyName("filterByFuelSensorType")]
        public string FilterByFuelSensorType { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("forMobile")]
        public bool ForMobile { get; set; }

        [JsonPropertyName("fromECheck")]
        public bool FromECheck { get; set; }

        [JsonPropertyName("partnerID")]
        public int PartnerID { get; set; }

        [JsonPropertyName("creatorID")]
        public int CreatorID { get; set; }

        [JsonPropertyName("deptID")]
        public int DeptID { get; set; }

        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("orderMethod")]
        public string OrderMethod { get; set; }

        [JsonPropertyName("line")]
        public string Line { get; set; }

        [JsonPropertyName("containerNo")]
        public string ContainerNo { get; set; }

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; }

        [JsonPropertyName("sdCardError")]
        public bool SdCardError { get; set; }

        [JsonPropertyName("reportType")]
        public string ReportType { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("lineNo")]
        public string LineNo { get; set; }

        [JsonPropertyName("lineID")]
        public int LineID { get; set; }

        [JsonPropertyName("modelID")]
        public int ModelID { get; set; }

        [JsonPropertyName("ignoreTest")]
        public bool IgnoreTest { get; set; }

        [JsonPropertyName("showVehicleNo")]
        public bool ShowVehicleNo { get; set; }

        [JsonPropertyName("taxiPaymentType")]
        public int TaxiPaymentType { get; set; }
    }

    public class ReportParamDTO
    {
        public int VehicleID { get; set; }
        public string RegisterNo { get; set; } = string.Empty;
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string ReportCate { get; set; } = string.Empty;
        public int PlaceMarkID { get; set; }
        public int DiffTime { get; set; }
        public int DiffDistance { get; set; }
        public int CustomerID { get; set; }
        public bool CountByStop { get; set; }
        public int ReportID { get; set; }
        public int GroupID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileData { get; set; } = string.Empty;
        public double Speed { get; set; }
        public int PlaceGroupID { get; set; }
        public bool CountBySpeed { get; set; }
        public bool RecognizePlace { get; set; }
        public bool CountByRelease { get; set; }
        public int MaterialID { get; set; }
        public int ReleasePointID { get; set; }
        public int ShiftID { get; set; }
        public string FromDateStr { get; set; } = string.Empty;
        public string ToDateStr { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string SummaryType { get; set; } = string.Empty;
        public string AlarmType { get; set; } = string.Empty;
        public int State { get; set; }
        public string OrderMethod { get; set; } = string.Empty;
        public string Line { get; set; } = string.Empty;
        public string ContainerNo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public string LineNo { get; set; } = string.Empty;
        public int TaxiPaymentType { get; set; }
        public string Language { get; set; } = "vi";
        public string PrivateKey { get; set; } = string.Empty;

        // … có thể bổ sung thêm các field ít dùng khác (Camera, SIM, Credit, Fuel, …)
    }
}
