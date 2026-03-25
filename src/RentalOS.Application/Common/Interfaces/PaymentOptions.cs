namespace RentalOS.Application.Common.Interfaces;

public class MoMoOptions
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool IsProduction { get; set; }
}

public class VNPayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
}

public class BankTransferOptions
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? QrTemplate { get; set; } // e.g. https://img.vietqr.io/image/VCB-12345678-compact2.jpg?amount={amount}&addInfo={info}
}
