using CafeSystem.Domain.Exceptions;

namespace CafeSystem.Domain.Entities;

/// <summary>
/// Меню кафе — агрегат позицій. Демонструє індексатор (СР 2).
/// </summary>
public class Menu
{
    private readonly List<MenuItem> _items = new();

    public string Title { get; }
    public IReadOnlyList<MenuItem> Items => _items.AsReadOnly();

    // Індексатор за ID (СР 2)
    public MenuItem this[int id]
    {
        get => _items.FirstOrDefault(i => i.Id == id)
               ?? throw new InvalidMenuItemException($"Позиція з ID={id} відсутня в меню.");
    }

    // Індексатор за назвою
    public MenuItem this[string name]
    {
        get => _items.FirstOrDefault(i =>
                   i.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidMenuItemException($"Позиція '{name}' відсутня в меню.");
    }

    public Menu(string title) => Title = title;

    public void AddItem(MenuItem item)
    {
        if (_items.Any(i => i.Id == item.Id))
            throw new InvalidMenuItemException($"Позиція з ID={item.Id} вже існує в меню.");
        _items.Add(item);
    }

    public void RemoveItem(int id) =>
        _items.RemoveAll(i => i.Id == id);

    public IEnumerable<MenuItem> GetByCategory(string category) =>
        _items.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                       && i.IsAvailable);

    public IEnumerable<string> GetCategories() =>
        _items.Select(i => i.Category).Distinct();

    public override string ToString() => $"Меню '{Title}': {_items.Count} позицій";
}
