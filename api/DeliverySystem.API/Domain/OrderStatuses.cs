namespace DeliverySystem.API.Domain;

public static class OrderStatuses
{
    public const string Created = "Created";
    public const string Assigned = "Assigned";
    public const string OutForDelivery = "OutForDelivery";
    public const string Delivered = "Delivered";
    public const string Failed = "Failed";
    public const string Returned = "Returned";
    public const string Cancelled = "Cancelled";

    public static bool CanAssign(string status) =>
        status == Created;

    public static bool IsKnown(string status) =>
        status is Created or Assigned or OutForDelivery or Delivered or Failed or Returned or Cancelled;

    public static bool CanTransition(string from, string to)
    {
        if (from == to)
        {
            return false;
        }

        return (from, to) switch
        {
            (Created, Cancelled) => true,
            (Assigned, OutForDelivery) => true,
            (Assigned, Cancelled) => true,
            (OutForDelivery, Delivered) => true,
            (OutForDelivery, Failed) => true,
            (OutForDelivery, Returned) => true,
            (Failed, OutForDelivery) => true,
            (Failed, Returned) => true,
            _ => false
        };
    }

    public static bool RequiresReason(string status) =>
        status is Failed or Returned or Cancelled;

    public static bool RequiresOpenAssignment(string status) =>
        status is OutForDelivery or Delivered or Failed or Returned;
}
