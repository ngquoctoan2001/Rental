namespace RentalOS.Application.Common.Models;

public class NotificationInvoiceDto
{
    public string InvoiceCode { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public decimal RoomRent { get; set; }
    public decimal ElectricityAmount { get; set; }
    public int ElectricityUsed { get; set; }
    public decimal WaterAmount { get; set; }
    public int WaterUsed { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
}

public class NotificationCustomerDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
 Eskom more DTOs can be added as needed.
 Eskom changed INotificationService to use these DTOs.
