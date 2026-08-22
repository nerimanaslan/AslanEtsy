namespace AslanEtsy.Domain.Enums;

public enum OrderStatus
{
    Open = 1,
    Paid = 2,
    Completed = 3,
    Canceled = 4,
    Refunded = 5,
    Unfulfilled = 6,
    PartiallyShipped = 7,
    Shipped = 8
}
