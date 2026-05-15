using CafeSystem.App.Factories;
using CafeSystem.App.Logging;
using CafeSystem.App.Patterns.Composite;
using CafeSystem.App.Patterns.Decorator;
using CafeSystem.App.Patterns.Facade;
using CafeSystem.App.Patterns.Strategy;
using CafeSystem.App.Services;
using CafeSystem.Domain.Enums;
using CafeSystem.Domain.Exceptions;

// ─── Ініціалізація ────────────────────────────────────────────────────────────

var logger = new ConsoleLogger("КАФЕ");
logger.Log("=== Система управління кафе запускається ===");

var cafe = new CafeSystemFacade(logger);

// ─── Заповнення меню (Factory Method) ────────────────────────────────────────

var espresso   = MenuItemFactory.Create("Еспресо",         45,  "Напої", 3);
var cappuccino = MenuItemFactory.Create("Капучино",         65,  "Напої", 5);
var borsch     = MenuItemFactory.Create("Борщ",            120, "Перші страви", 15);
var steak      = MenuItemFactory.Create("Стейк із рибою",  280, "Другі страви", 20);
var tiramisu   = MenuItemFactory.Create("Тірамісу",         95, "Десерти", 8);
var salad      = MenuItemFactory.Create("Цезар",            90, "Салати", 7);

foreach (var item in new[] { espresso, cappuccino, borsch, steak, tiramisu, salad })
    cafe.AddMenuItem(item);

// ─── Персонал (Factory Method) ───────────────────────────────────────────────

var waiter = StaffFactory.Create(StaffRole.Waiter,  "Олена Коваль",  18_000);
var chef   = StaffFactory.Create(StaffRole.Chef,    "Іван Шевченко", 32_000, "Українська кухня");
cafe.AddStaff(waiter);
cafe.AddStaff(chef);
cafe.StartShift(waiter.Id);
cafe.StartShift(chef.Id);

// ─── Composite меню ──────────────────────────────────────────────────────────

Console.WriteLine("\n--- Структура меню (Composite) ---");
var fullMenu = new MenuGroup("Повне меню");

var drinksGroup = new MenuGroup("Напої");
drinksGroup.Add(new MenuLeaf("Еспресо", 45));
drinksGroup.Add(new MenuLeaf("Капучино", 65));

var combo = new ComboMeal("Бізнес-ланч", discountPercent: 15);
combo.AddItem(new MenuLeaf("Борщ", 120));
combo.AddItem(new MenuLeaf("Стейк із рибою", 280));
combo.AddItem(new MenuLeaf("Еспресо", 45));

fullMenu.Add(drinksGroup);
fullMenu.Add(combo);
fullMenu.Display();

// ─── Замовлення + просування статусу ─────────────────────────────────────────

Console.WriteLine("\n--- Замовлення (Observer сповіщення) ---");
var order1 = cafe.CreateOrder(tableNumber: 3, "Без цибулі");
cafe.AddItemToOrder(order1.Id, borsch.Id, 1);
cafe.AddItemToOrder(order1.Id, steak.Id, 1);
cafe.AddItemToOrder(order1.Id, espresso.Id, 2);

cafe.AdvanceOrder(order1.Id); // New → Accepted
cafe.AdvanceOrder(order1.Id); // Accepted → InProgress
cafe.AdvanceOrder(order1.Id); // InProgress → Ready
cafe.AdvanceOrder(order1.Id); // Ready → Delivered

// ─── Strategy знижок + Decorator ─────────────────────────────────────────────

Console.WriteLine("\n--- Стратегія знижок (Strategy + Decorator) ---");

// Декоратор: логування + статистика
var baseStrategy = new PercentDiscountStrategy(10);
var loggedStrategy = new LoggingDiscountDecorator(baseStrategy, logger);
var statsStrategy = new StatisticsDiscountDecorator(loggedStrategy);

cafe.SetDiscountStrategy(statsStrategy);

var order2 = cafe.CreateOrder(tableNumber: 5);
cafe.AddItemToOrder(order2.Id, tiramisu.Id, 2);
cafe.AddItemToOrder(order2.Id, cappuccino.Id, 2);

var bill = cafe.GetBill(order2.Id);
logger.Log($"Підсумковий рахунок: {bill:C2}");
logger.Log(statsStrategy.GetStats());

// Динамічна заміна стратегії з конфігу (СР 10)
Console.WriteLine("\n--- Фабрика стратегій з конфігу ---");
var configStrategy = DiscountStrategyFactory.Create("fixed:50");
cafe.SetDiscountStrategy(configStrategy);
logger.Log($"Нова стратегія: '{configStrategy.Name}', рахунок: {cafe.GetBill(order2.Id):C2}");

// ─── Retry Policy (СР 8) ──────────────────────────────────────────────────────

Console.WriteLine("\n--- Retry Policy ---");
var retry = new RetryPolicy(logger, maxAttempts: 3, TimeSpan.FromMilliseconds(100));
int attempts = 0;

try
{
    var result = retry.Execute(() =>
    {
        attempts++;
        if (attempts < 3) throw new IOException("Симуляція збою збереження.");
        return "Збережено успішно!";
    }, "SaveOrders");
    logger.Log(result);
}
catch (AggregateException ex)
{
    logger.LogError("Retry вичерпано", ex);
}

// ─── LINQ статистика (ПЗ 7) ──────────────────────────────────────────────────

Console.WriteLine("\n--- LINQ статистика ---");
var revenue = cafe.GetRevenueByCategory();
foreach (var (cat, total) in revenue)
    logger.Log($"  Категорія '{cat}': {total:C2}");

Console.WriteLine("\nТоп позицій:");
foreach (var (name, count) in cafe.GetTopItems(3))
    logger.Log($"  {name}: {count} шт.");

// ─── Серіалізація (ПЗ 13) ────────────────────────────────────────────────────

Console.WriteLine("\n--- Серіалізація стану ---");
var json = cafe.ExportOrders();
logger.Log($"Серіалізовано {json.Length} символів JSON.");
Console.WriteLine(json[..Math.Min(300, json.Length)] + "...");

// ─── Скасування замовлення з обробкою виключень (ПЗ 8) ───────────────────────

Console.WriteLine("\n--- Обробка виключень ---");
try
{
    cafe.CancelOrder(order1.Id); // Вже Delivered — має кинути виключення
}
catch (InvalidOrderTransitionException ex)
{
    logger.LogWarning($"Очікуване виключення: {ex.Message}");
}

try
{
    cafe.GetMenuItem(9999);
}
catch (CafeDomainException ex)
{
    logger.LogWarning($"Очікуване виключення: {ex.Message}");
}

// ─── Extension Methods (СР 7) ────────────────────────────────────────────────

Console.WriteLine("\n--- Extension Methods ---");
var activeOrders = cafe.GetActiveOrders().ToList();
var revenue2 = activeOrders.TotalRevenue();
logger.Log($"Активних замовлень: {activeOrders.Count}, загальна сума: {revenue2:C2}");

logger.Log("\n=== Демонстрацію завершено ===");
