using CafeSystem.Domain.Exceptions;

namespace CafeSystem.Domain.Entities;

/// <summary>
/// Позиція меню. Демонструє інкапсуляцію (private поля + властивості),
/// перевантаження операторів та незмінність інваріантів.
/// ПЗ 2 / СР 2.
/// </summary>
public class MenuItem : IEquatable<MenuItem>
{
    private string _name = string.Empty;
    private decimal _price;
    private int _preparationTimeMinutes;

    public int Id { get; }
    public string Category { get; private set; }

    public string Name
    {
        get => _name;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidMenuItemException("Назва не може бути порожньою.");
            _name = value.Trim();
        }
    }

    public decimal Price
    {
        get => _price;
        private set
        {
            if (value < 0)
                throw new InvalidMenuItemException("Ціна не може бути від'ємною.");
            _price = value;
        }
    }

    public int PreparationTimeMinutes
    {
        get => _preparationTimeMinutes;
        private set
        {
            if (value < 0)
                throw new InvalidMenuItemException("Час приготування не може бути від'ємним.");
            _preparationTimeMinutes = value;
        }
    }

    public bool IsAvailable { get; private set; }

    // Основний конструктор
    public MenuItem(int id, string name, decimal price, string category, int preparationTimeMinutes = 10)
    {
        Id = id;
        Name = name;
        Price = price;
        Category = category;
        PreparationTimeMinutes = preparationTimeMinutes;
        IsAvailable = true;
    }

    // Копіювальний конструктор (СР 1)
    public MenuItem(MenuItem other)
        : this(other.Id, other.Name, other.Price, other.Category, other.PreparationTimeMinutes)
    {
        IsAvailable = other.IsAvailable;
    }

    public void UpdatePrice(decimal newPrice) => Price = newPrice;
    public void SetAvailability(bool available) => IsAvailable = available;
    public void UpdateCategory(string category) => Category = category;

    // Перевантаження операторів (СР 2)
    public static MenuItem operator +(MenuItem item, decimal discount)
    {
        var copy = new MenuItem(item);
        copy.UpdatePrice(Math.Max(0, item.Price - discount));
        return copy;
    }

    public static bool operator ==(MenuItem? left, MenuItem? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(MenuItem? left, MenuItem? right) => !(left == right);

    public bool Equals(MenuItem? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as MenuItem);
    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() =>
        $"[{Id}] {Name} ({Category}) — {Price:C2}, ~{PreparationTimeMinutes} хв";
}
