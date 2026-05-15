namespace CafeSystem.Domain.Exceptions;

/// <summary>Базовий клас для всіх виключень предметної області кафе.</summary>
public class CafeDomainException : Exception
{
    public CafeDomainException(string message) : base(message) { }
    public CafeDomainException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Кидається, коли замовлення не знайдено.</summary>
public class OrderNotFoundException : CafeDomainException
{
    public int OrderId { get; }
    public OrderNotFoundException(int orderId)
        : base($"Замовлення з ID={orderId} не знайдено.") => OrderId = orderId;
}

/// <summary>Кидається при некоректному переході між статусами замовлення.</summary>
public class InvalidOrderTransitionException : CafeDomainException
{
    public InvalidOrderTransitionException(string from, string to)
        : base($"Неможливо перейти зі стану '{from}' до '{to}'.") { }
}

/// <summary>Кидається при спробі додати позицію з негативною ціною або кількістю.</summary>
public class InvalidMenuItemException : CafeDomainException
{
    public InvalidMenuItemException(string reason)
        : base($"Некоректна позиція меню: {reason}") { }
}

/// <summary>Кидається при переповненні черги на кухні.</summary>
public class KitchenQueueOverflowException : CafeDomainException
{
    public KitchenQueueOverflowException(int maxCapacity)
        : base($"Черга на кухні переповнена. Максимум: {maxCapacity} замовлень.") { }
}
