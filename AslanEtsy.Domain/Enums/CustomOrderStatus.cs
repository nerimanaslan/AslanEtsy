namespace AslanEtsy.Domain.Enums;

public enum CustomOrderStatus
{
    New = 1,
    PendingProduction = 2,
    InProduction = 3,
    QualityControl = 4,
    ReadyToShip = 5,
    Shipped = 6,
    Delivered = 7,
    OnHold = 8,
    ActionRequired = 9
}
