#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace CoolifiCli.Data;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Retrieves an entity by its integer primary key.</summary>
    Task<T?> GetByIdAsync(int id);
    /// <summary>Returns all entities from the data source.</summary>
    Task<IEnumerable<T>> GetAllAsync();
    /// <summary>Adds a new entity to the data source.</summary>
    Task<T> AddAsync(T entity);
    /// <summary>Updates an existing entity in the data source.</summary>
    Task<T> UpdateAsync(T entity);
    /// <summary>Deletes an entity by ID. Returns <c>false</c> if not found.</summary>
    Task<bool> DeleteAsync(int id);
    /// <summary>Persists pending changes. Returns the number of affected entities.</summary>
    Task<int> SaveChangesAsync();
}

/// <summary>
/// Base repository providing common CRUD operations with caching support.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly Dictionary<int, T> _cache = new();
    protected readonly List<T> _all = new();
    protected bool _isLoaded = false;

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        if (!_isLoaded)
            await LoadAsync();

        return _cache.TryGetValue(id, out var entity) ? entity : null;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        if (!_isLoaded)
            await LoadAsync();

        return _all.AsReadOnly();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        if (!_isLoaded)
            await LoadAsync();

        _all.Add(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        if (!_isLoaded)
            await LoadAsync();

        var index = _all.FindIndex(e => e == entity);
        if (index >= 0)
        {
            _all[index] = entity;
        }

        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        if (!_isLoaded)
            await LoadAsync();

        var entity = _cache.Values.FirstOrDefault(e => GetId(e) == id);
        if (entity is not null)
        {
            _all.Remove(entity);
            _cache.Remove(id);
            return true;
        }

        return false;
    }

    public virtual Task<int> SaveChangesAsync()
    {
        return Task.FromResult(_all.Count);
    }

    protected abstract Task LoadAsync();
    protected abstract int GetId(T entity);
}
