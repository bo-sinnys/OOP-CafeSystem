namespace CafeSystem.Domain.Enums;

/// <summary>
/// Стан замовлення в системі кафе (використовується патерном State).
/// </summary>
public enum OrderStatus
{
    New,
    Accepted,
    InProgress,
    Ready,
    Delivered,
    Cancelled
}
