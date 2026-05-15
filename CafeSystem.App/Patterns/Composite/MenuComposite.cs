using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Patterns.Composite;

/// <summary>
/// Патерн Composite: єдине управління деревом меню (СР 12).
/// MenuLeaf — позиція, MenuGroup — категорія або combo.
/// </summary>

public class MenuLeaf : IMenuComponent
{
    public string Name { get; }
    private readonly decimal _price;

    public MenuLeaf(string name, decimal price)
    {
        Name = name;
        _price = price;
    }

    public decimal GetTotalPrice() => _price;

    public void Display(int indent = 0)
        => Console.WriteLine($"{new string(' ', indent * 2)}🍽  {Name} — {_price:C2}");
}

public class MenuGroup : IMenuComponent
{
    public string Name { get; }
    private readonly List<IMenuComponent> _children = new();

    public MenuGroup(string name) => Name = name;

    public void Add(IMenuComponent component) => _children.Add(component);
    public void Remove(IMenuComponent component) => _children.Remove(component);
    public IReadOnlyList<IMenuComponent> Children => _children.AsReadOnly();

    public decimal GetTotalPrice() => _children.Sum(c => c.GetTotalPrice());

    public void Display(int indent = 0)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}📂 {Name} (разом: {GetTotalPrice():C2})");
        foreach (var child in _children)
            child.Display(indent + 1);
    }
}

/// <summary>Комбо-обід — фіксована група з особливою ціною (Composite + Builder-ready).</summary>
public class ComboMeal : IMenuComponent
{
    public string Name { get; }
    private readonly List<MenuLeaf> _items = new();
    private readonly decimal _discount;

    public ComboMeal(string name, decimal discountPercent = 10)
    {
        Name = name;
        _discount = discountPercent;
    }

    public void AddItem(MenuLeaf leaf) => _items.Add(leaf);

    public decimal GetTotalPrice()
    {
        var full = _items.Sum(i => i.GetTotalPrice());
        return Math.Round(full * (1 - _discount / 100), 2);
    }

    public void Display(int indent = 0)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}🎁 {Name} (комбо -{_discount}% = {GetTotalPrice():C2})");
        foreach (var item in _items)
            item.Display(indent + 1);
    }
}
