namespace Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Processing = 3,
    AssignedToRider = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Completed = 7,
    Cancelled = 8,
    Returned = 9,
    FailedDelivery = 10
}
