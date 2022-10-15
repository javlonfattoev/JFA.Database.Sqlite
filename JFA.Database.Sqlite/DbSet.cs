using System.Collections;

namespace JFA.Database.Sqlite;

public sealed class DbSet<T> : IEnumerable<T> where T : class
{
    private List<T> _list = new();

    public DbSet(IEnumerable<T> list) => Init(list);

    public void Add(T entity) => _list.Add(entity);

    public void Init(IEnumerable<T> list) => _list = list.ToList();

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator DbSet<T>(List<T> value) => new(value);
}