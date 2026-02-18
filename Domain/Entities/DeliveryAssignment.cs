using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class DeliveryAssignment
{
    [Key]
    public long Id { get; set; }
    public long OrderId { get; set; }
    public string DeliveryBoyName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredDate { get; set; }

    public Order Order { get; set; } = null!;
}
