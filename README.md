# 🍽 CafeSystem — Навчальний проєкт ООП

Повноцінна система управління кафе, розроблена в рамках навчальної практики з **об'єктно-орієнтованого програмування** на C# / .NET 8.

---

## Архітектура рішення

```
CafeSystem/
├── CafeSystem.Domain/          ← Доменний шар (сутності, інтерфейси, виключення)
│   ├── Entities/
│   │   ├── MenuItem.cs         — позиція меню (інкапсуляція, оператори)
│   │   ├── Menu.cs             — агрегат меню (індексатор)
│   │   ├── Order.cs            — замовлення (State-машина, оператор +)
│   │   ├── OrderItem.cs        — рядок замовлення
│   │   └── StaffMember.cs      — ієрархія персоналу (Waiter, Chef, Cashier, Manager)
│   ├── Interfaces/
│   │   └── IInterfaces.cs      — IRepository<T>, IOrderService, ILogger,
│   │                             IDiscountStrategy, IOrderObserver, IMenuComponent
│   ├── Exceptions/
│   │   └── CafeExceptions.cs   — CafeDomainException та підкласи
│   └── Enums/
│       ├── OrderStatus.cs
│       └── StaffRole.cs
│
├── CafeSystem.App/             ← Шар застосунку (сервіси, патерни, серіалізація)
│   ├── Repositories/
│   │   ├── InMemoryRepository.cs   — Generics Repository<T>
│   │   └── ConcreteRepositories.cs — OrderRepository, MenuRepository, StaffRepository
│   ├── Logging/
│   │   └── Loggers.cs          — ConsoleLogger, FileLogger, CompositeLogger (DIP)
│   ├── Factories/
│   │   └── Factories.cs        — StaffFactory, MenuItemFactory,
│   │                             DiscountStrategyFactory, CafeQueueManager (Singleton)
│   ├── Patterns/
│   │   ├── Strategy/           — IDiscountStrategy + 4 реалізації + DiscountContext
│   │   ├── Observer/           — OrderEventPublisher + KitchenDisplay, WaiterNotifier, CashierNotifier
│   │   ├── Decorator/          — LoggingDecorator, MinimumAmountDecorator, StatisticsDecorator
│   │   ├── Composite/          — MenuLeaf, MenuGroup, ComboMeal
│   │   └── Facade/             — CafeSystemFacade (єдина точка входу)
│   ├── Serialization/
│   │   └── Serialization.cs    — JsonStateSerializer<T>, OrderDto, MenuItemDto
│   ├── Services/
│   │   ├── CollectionAlgorithms.cs — ForEach/Map/Reduce/Filter + Extension Methods
│   │   └── RetryPolicy.cs          — Retry з експоненційною затримкою
│   └── Program.cs              ← Точка входу + демонстрація всіх концепцій
│
└── CafeSystem.Tests/           ← Тести (xUnit + Moq)
    ├── Domain/
    │   └── DomainTests.cs      — MenuItemTests, OrderTests, StaffTests
    └── Services/
        └── PatternTests.cs     — Strategy, Decorator, Composite, Repository,
                                  Serialization, Algorithms, Mock-тести
```

---

## UML — Основні зв'язки

```
+------------------+       1   *  +------------------+
|     Order        |<>------------|    OrderItem     |
|------------------|              |------------------|
| -Id: int         |              | -MenuItem        |
| -TableNumber     |              | -Quantity: int   |
| -Status          |              | +LineTotal       |
| +TransitionTo()  |              +------------------+
| +AddItem()       |                      |
+------------------+                      |
         |                                |
         v                         +------+------+
+------------------+               |  MenuItem   |
| IOrderObserver   |               |-------------|
|------------------|               | -Name       |
| +OnStatusChanged |               | -Price      |
+------------------+               | -Category   |
    ^     ^    ^                   +-------------+
    |     |    |
Kitchen Waiter Cashier  (Observer)

+------------------+    implements    +-------------------+
| IDiscountStrategy|<----------------| PercentDiscount   |
|------------------|                  | FixedDiscount     |
| +Apply(amount)   |                  | HappyHourDiscount |
+------------------+                  | NoDiscount        |
         ^                            +-------------------+
         |  wraps
+------------------------+
| DiscountStrategyDecorator|  (Decorator)
| LoggingDecorator         |
| MinimumAmountDecorator   |
| StatisticsDecorator      |
+------------------------+

+-------------------+
| IMenuComponent    |  (Composite)
|-------------------|
| +GetTotalPrice()  |
| +Display()        |
+-------------------+
    ^          ^
MenuLeaf    MenuGroup
            ComboMeal

StaffMember (abstract)
    ├── Waiter
    ├── Chef
    ├── Cashier
    └── Manager
```

---

## Застосовані принципи та патерни

### SOLID
| Принцип | Де застосовано |
|---------|----------------|
| **SRP** | `Order` — лише агрегат даних; `CafeSystemFacade` — координація; `Loggers` — лише логування |
| **OCP** | `IDiscountStrategy` — нові знижки без зміни `DiscountContext` |
| **LSP** | `Waiter`, `Chef`, `Cashier`, `Manager` — замінні скрізь де `StaffMember` |
| **ISP** | `IRepository<T>`, `ILogger`, `IDiscountStrategy` — вузькі ізольовані контракти |
| **DIP** | `CafeSystemFacade` залежить від `ILogger`, а не від `ConsoleLogger` |

### Патерни проєктування
| Категорія | Патерн | Файл |
|-----------|--------|------|
| Породжувальні | **Factory Method** | `Factories.cs` — `StaffFactory`, `MenuItemFactory` |
| Породжувальні | **Singleton** | `Factories.cs` — `CafeQueueManager` |
| Поведінкові | **Strategy** | `DiscountStrategies.cs` |
| Поведінкові | **Observer** | `OrderObservers.cs` |
| Структурні | **Decorator** | `DiscountDecorators.cs` |
| Структурні | **Composite** | `MenuComposite.cs` |
| Структурні | **Facade** | `CafeSystemFacade.cs` |

---

## Запуск проєкту

### Передумови
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Клонування та запуск

```bash
git clone <url>
cd CafeSystem

# Запуск демонстрації
dotnet run --project CafeSystem.App

# Запуск тестів
dotnet test CafeSystem.Tests

# Запуск тестів з детальним виводом
dotnet test CafeSystem.Tests --logger "console;verbosity=detailed"
```

---

## Приклади використання

### Створення замовлення та просування статусу

```csharp
var logger = new ConsoleLogger("КАФЕ");
var cafe = new CafeSystemFacade(logger);

var item = MenuItemFactory.Create("Борщ", 120m, "Перші страви", 15);
cafe.AddMenuItem(item);

var order = cafe.CreateOrder(tableNumber: 3);
cafe.AddItemToOrder(order.Id, item.Id, 2);
cafe.AdvanceOrder(order.Id); // New → Accepted (KitchenDisplay повідомляє)
cafe.AdvanceOrder(order.Id); // → InProgress
cafe.AdvanceOrder(order.Id); // → Ready (WaiterNotifier повідомляє)
cafe.AdvanceOrder(order.Id); // → Delivered (CashierNotifier повідомляє)
```

### Динамічна знижка з конфігурації

```csharp
// З рядка конфігурації (JSON / CLI)
var strategy = DiscountStrategyFactory.Create("percent:15");
cafe.SetDiscountStrategy(strategy);

decimal bill = cafe.GetBill(orderId);
```

### Composite меню

```csharp
var menu = new MenuGroup("Меню дня");
var combo = new ComboMeal("Бізнес-ланч", discountPercent: 15);
combo.AddItem(new MenuLeaf("Борщ", 120m));
combo.AddItem(new MenuLeaf("Стейк", 280m));
menu.Add(combo);
menu.Display(); // рекурсивний вивід дерева
```

---

## Покриття тестами

| Клас / компонент | Тип тестів |
|-----------------|------------|
| `MenuItem` | Unit (конструктори, оператори, інваріанти) |
| `Order` | Unit (переходи стану, агрегація, копіювання) |
| `StaffMember` hierarchy | Unit (поліморфізм, бонуси) |
| `IDiscountStrategy` + фабрика | Unit + параметризовані |
| `Decorator` | Unit + Mock (Moq) для ILogger |
| `Composite` | Unit (рекурсивна ціна) |
| `InMemoryRepository<T>` | Unit (CRUD, фільтрація) |
| `JsonStateSerializer<T>` | Unit (round-trip) |
| `CollectionAlgorithms` | Unit (Map/Reduce/Filter) |
| `RetryPolicy` | Unit + Mock (Moq) |

---

## CHANGELOG

### v1.0.0 (2026-05)
- Розділ I: UML-модель, ієрархія сутностей, інкапсуляція, наслідування, інтерфейси
- Розділ II: Generics Repository, LINQ, Extension Methods, Custom Exceptions, RetryPolicy
- Розділ III: SOLID-рефакторинг, Strategy, Observer, Decorator, Composite, Facade, Factory, Singleton
- Розділ IV: JSON-серіалізація з DTO, xUnit тести, Moq ізоляція, документація
