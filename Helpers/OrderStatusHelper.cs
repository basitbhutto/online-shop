using System.Text.RegularExpressions;
using Domain.Enums;

namespace Presentation.Helpers;

public static class OrderStatusHelper
{
    /// <summary>Returns display name for order status (e.g. AssignedToRider -> "Assigned To Rider").</summary>
    public static string DisplayName(OrderStatus status)
    {
        var name = status.ToString();
        return Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
    }

    public static string DisplayName(int statusValue)
    {
        if (Enum.IsDefined(typeof(OrderStatus), statusValue))
            return DisplayName((OrderStatus)statusValue);
        // Legacy DB values (old enum: 0=PendingConfirmation, 7=Cancelled, etc.)
        return statusValue switch
        {
            0 => "Pending",
            7 => "Cancelled",
            _ => statusValue.ToString()
        };
    }

    public static bool CanBuyerCancel(OrderStatus status)
    {
        return status == OrderStatus.Pending || status == OrderStatus.Confirmed || status == OrderStatus.Processing;
    }

    public static bool CanAdminCancel(OrderStatus status)
    {
        return status != OrderStatus.Delivered && status != OrderStatus.Completed && status != OrderStatus.Cancelled;
    }
}
