namespace Domain.Enums;

public enum OrderStatus
{
    PendingConfirmation = 0,
    AdminReview = 1,
    Confirmed = 2,
    Preparing = 3,
    Shipped = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7
}
