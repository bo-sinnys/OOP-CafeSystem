using CafeSystem.Domain.Exceptions;

namespace CafeSystem.Domain.Entities;

/// <summary>
/// Рядок замовлення: конкретна позиція меню + кількість.
/// </summary>
public class OrderItem
{
    private int _quantity;

    public MenuItem MenuItem { get; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value <= 0)
                throw new InvalidMenuItemException("Кількість має бути більше нуля.");
            _quantity = value;
        }
    }

    public decimal LineTotal => MenuItem.Price * Quantity;
    public int TotalPreparationTime => MenuItem.PreparationTimeMinutes * Quantity;

    public OrderItem(MenuItem menuItem, int quantity)
    {
        MenuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
        Quantity = quantity;
    }

    // Копіювальний конструктор
    public OrderItem(OrderItem other) : this(other.MenuItem, other.Quantity) { }

    public override string ToString() =>
        $"{MenuItem.Name} x{Quantity} = {LineTotal:C2}";
}
