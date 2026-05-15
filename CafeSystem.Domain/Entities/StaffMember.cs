using CafeSystem.Domain.Enums;

namespace CafeSystem.Domain.Entities;

/// <summary>
/// Базовий клас персоналу. ПЗ 3 — наслідування, virtual/override.
/// </summary>
public abstract class StaffMember
{
    private string _name = string.Empty;
    private decimal _salary;

    public int Id { get; }
    public StaffRole Role { get; }
    public bool IsOnShift { get; protected set; }

    public string Name
    {
        get => _name;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Ім'я не може бути порожнім.");
            _name = value.Trim();
        }
    }

    public decimal Salary
    {
        get => _salary;
        protected set
        {
            if (value < 0) throw new ArgumentException("Зарплата не може бути від'ємною.");
            _salary = value;
        }
    }

    protected StaffMember(int id, string name, StaffRole role, decimal salary)
    {
        Id = id;
        Name = name;
        Role = role;
        Salary = salary;
    }

    public virtual void StartShift() => IsOnShift = true;
    public virtual void EndShift() => IsOnShift = false;

    /// <summary>Поліморфна дія — кожна роль реалізує по-своєму.</summary>
    public abstract string PerformDuty();

    /// <summary>Розрахунок бонусу — перевизначається у підкласах (ПЗ 3).</summary>
    public virtual decimal CalculateBonus(int ordersServed) => Salary * 0.05m;

    public override string ToString() =>
        $"{Role} {Name} (ID={Id}) | Зарплата: {Salary:C2} | На зміні: {IsOnShift}";
}

// ─── Конкретні ролі ─────────────────────────────────────────────────────────

public class Waiter : StaffMember
{
    public int TablesAssigned { get; private set; }

    public Waiter(int id, string name, decimal salary)
        : base(id, name, StaffRole.Waiter, salary) { }

    public override string PerformDuty() =>
        $"{Name} обслуговує столик. Поточних столів: {TablesAssigned}.";

    public void AssignTable() => TablesAssigned++;
    public void ReleaseTable() => TablesAssigned = Math.Max(0, TablesAssigned - 1);

    // Більший бонус за кількість обслуговувань
    public override decimal CalculateBonus(int ordersServed) =>
        base.CalculateBonus(ordersServed) + ordersServed * 5m;

    public override void EndShift()
    {
        base.EndShift();
        TablesAssigned = 0;
    }
}

public class Chef : StaffMember
{
    public string Specialization { get; }

    public Chef(int id, string name, decimal salary, string specialization = "Загальна кухня")
        : base(id, name, StaffRole.Chef, salary)
    {
        Specialization = specialization;
    }

    public override string PerformDuty() =>
        $"{Name} готує страву ({Specialization}).";

    public override decimal CalculateBonus(int ordersServed) =>
        base.CalculateBonus(ordersServed) + ordersServed * 10m;
}

public class Cashier : StaffMember
{
    public Cashier(int id, string name, decimal salary)
        : base(id, name, StaffRole.Cashier, salary) { }

    public override string PerformDuty() => $"{Name} проводить оплату.";
}

public class Manager : StaffMember
{
    public Manager(int id, string name, decimal salary)
        : base(id, name, StaffRole.Manager, salary) { }

    public override string PerformDuty() => $"{Name} управляє зміною.";

    public override decimal CalculateBonus(int ordersServed) =>
        Salary * 0.15m; // фіксований %
}
