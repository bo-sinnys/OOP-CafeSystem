using System.Text.Json;
using System.Text.Json.Serialization;
using CafeSystem.Domain.Entities;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Serialization;

/// <summary>
/// Серіалізатор стану через System.Text.Json (ПЗ 13).
/// </summary>
public class JsonStateSerializer<T> : IStateSerializer<T>
{
    private readonly JsonSerializerOptions _options;

    public JsonStateSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public string Serialize(T obj) => JsonSerializer.Serialize(obj, _options);

    public T? Deserialize(string data) => JsonSerializer.Deserialize<T>(data, _options);
}

// ─── DTO (СР 13) ─────────────────────────────────────────────────────────────

/// <summary>
/// Data Transfer Object для Order — відокремлює доменну модель від серіалізованого формату.
/// Запобігає витоку внутрішніх деталей домену (циклічні посилання, приватні поля).
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();

    // Маппінг Domain → DTO (СР 13)
    public static OrderDto From(Order order) => new()
    {
        Id          = order.Id,
        TableNumber = order.TableNumber,
        Status      = order.Status.ToString(),
        CreatedAt   = order.CreatedAt,
        CompletedAt = order.CompletedAt,
        TotalAmount = order.TotalAmount,
        Notes       = order.Notes,
        Items       = order.Items.Select(OrderItemDto.From).ToList()
    };
}

public class OrderItemDto
{
    public string MenuItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    public static OrderItemDto From(OrderItem item) => new()
    {
        MenuItemName = item.MenuItem.Name,
        Category     = item.MenuItem.Category,
        UnitPrice    = item.MenuItem.Price,
        Quantity     = item.Quantity,
        LineTotal    = item.LineTotal
    };
}

/// <summary>DTO знімку стану меню.</summary>
public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public int PreparationTimeMinutes { get; set; }

    public static MenuItemDto From(MenuItem item) => new()
    {
        Id                    = item.Id,
        Name                  = item.Name,
        Category              = item.Category,
        Price                 = item.Price,
        IsAvailable           = item.IsAvailable,
        PreparationTimeMinutes = item.PreparationTimeMinutes
    };
}
