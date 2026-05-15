using CafeSystem.Domain.Interfaces;

namespace CafeSystem.App.Repositories;

/// <summary>
/// Узагальнений репозиторій у пам'яті — демонструє Generics з where-обмеженням (ПЗ 5).
/// </summary>
public abstract class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly Dictionary<int, T> _store = new();
    protected abstract int GetId(T entity);

    public virtual T? GetById(int id) =>
        _store.TryGetValue(id, out var entity) ? entity : null;

    public virtual IEnumerable<T> GetAll() => _store.Values.ToList();

    public virtual void Add(T entity)
    {
        var id = GetId(entity);
        if (_store.ContainsKey(id))
            throw new InvalidOperationException($"Запис з ID={id} вже існує.");
        _store[id] = entity;
    }

    public virtual void Update(T entity)
    {
        var id = GetId(entity);
        if (!_store.ContainsKey(id))
            throw new InvalidOperationException($"Запис з ID={id} не знайдено.");
        _store[id] = entity;
    }

    public virtual void Delete(int id)
    {
        if (!_store.Remove(id))
            throw new InvalidOperationException($"Запис з ID={id} не знайдено.");
    }
}
