using Microsoft.Data.Sqlite;
using System.Data;
using System.Reflection;

namespace JFA.Database.Sqlite;

public abstract class DbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    protected DbContext(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    protected DbSet<T> Configure<T>() where T : class
    {
        CreateTableIfNotExists<T>();
        return GetData<T>();
    }

    protected DbSet<T> GetData<T>() where T : class => Read<T>().ToList();

    private IEnumerable<T> Read<T>(T? @type = null) where T : class
    {
        CheckConnection();
        using var commond = _connection.CreateCommand();
        commond.CommandText = $"SELECT * FROM {GetTableName<T>()}";

        var data = commond.ExecuteReader();
        var properties = typeof(T).GetProperties();

        while (data.Read())
        {
            var obj = (T)Activator.CreateInstance(typeof(T))!;

            for (var i = 0; i < properties.Length; i++)
            {
                properties[i].SetValue(obj, data.IsDBNull(i) ? null : data.GetValue(i, properties[i]));
            }

            yield return obj;
        }

        _connection.Close();
    }


    public abstract void Save();

    public void SaveTable<T>(IEnumerable<T>? entities) where T : class
    {
        if (entities is null)
            return;

        CheckConnection();

        using var commond = _connection.CreateCommand();

        foreach (var entity in entities)
        {
            var propertyValue = typeof(T).GetProperties().First(prop => prop.Name.ToLower() == "id").GetValue(entity);

            commond.CommandText = propertyValue is null or 0
                ? $"INSERT INTO {GetTableName<T>()}({GetProperties<T>(true, EQueryType.Select)}) VALUES({GetProperties<T>(true, EQueryType.Insert)})"
                : $"UPDATE {GetTableName<T>()} SET {GetProperties<T>(true, EQueryType.Update)} WHERE id = @id";

            foreach (var info in typeof(T).GetProperties().Where(FilterProperties))
            {
                var value = info.GetValue(entity) ?? DBNull.Value;
                commond.Parameters.AddWithValue($"@{info.Name.ToLower()}", value);
            }

            commond.ExecuteNonQuery();
            commond.Parameters.Clear();
        }
    }

    public void Insert<T>(T entity) where T : class
    {
        CheckConnection();

        using var commond = _connection.CreateCommand();
        commond.CommandText =
            $"INSERT INTO {GetTableName<T>()}({GetProperties<T>(true, EQueryType.Select)}) VALUES({GetProperties<T>(true, EQueryType.Insert)})";

        foreach (var info in typeof(T).GetProperties().Where(FilterProperties))
        {
            var value = info.GetValue(entity) ?? DBNull.Value;
            commond.Parameters.AddWithValue($"@{info.Name.ToLower()}", value);
        }

        commond.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    #region Private methods

    private void CheckConnection()
    {
        if (_connection.State == ConnectionState.Closed) _connection.Open();
    }

    private void CreateTableIfNotExists<T>() where T : class
    {
        CheckConnection();
        using var commond = _connection.CreateCommand();
        commond.CommandText = $"CREATE TABLE IF NOT EXISTS {GetTableName<T>()}({GetProperties<T>(false, EQueryType.Create)})";
        commond.ExecuteNonQuery();
        _connection.Close();
    }

    private string GetTableName<T>() => $"{typeof(T).Name.ToLower()}s";

    private string GetProperties<T>(bool excludeId, EQueryType queryType) where T : class =>
        typeof(T).GetProperties()
            .Where(FilterProperties)
            .Where(info => !excludeId || (excludeId && info.Name.ToLower() != "id"))
            .Aggregate("", (current, prop) => CombineQuery(current, prop, queryType)).Remove(0, 1);

    private string CombineQuery(string current, PropertyInfo prop, EQueryType queryType)
    {
        return queryType switch
        {
            EQueryType.Create => $"{current}, {GetColumnName(prop, out _)}",
            EQueryType.Select => $"{current}, {prop.Name.ToLower()}",
            EQueryType.Insert => $"{current}, @{prop.Name.ToLower()}",
            EQueryType.Update => $"{current}, {prop.Name.ToLower()}=@{prop.Name.ToLower()}",
            _ => $"{current}, {prop.Name.ToLower()}"
        };
    }

    private Func<PropertyInfo, bool> FilterProperties => info =>
    {
        GetColumnName(info, out var isTypeSupported);
        return isTypeSupported;
    };

    private string? GetColumnName(PropertyInfo info, out bool isTypeSupported)
    {
        isTypeSupported = true;
        string? columnName = null;
        if (info.PropertyType == typeof(int))
        {
            columnName = "INTEGER NOT NULL";

            if (info.Name == "Id") columnName += " PRIMARY KEY AUTOINCREMENT";
        }
        else if (info.PropertyType == typeof(int?)) columnName = "INTEGER";
        else if (info.PropertyType == typeof(string)) columnName = "TEXT";
        else if (info.PropertyType == typeof(bool)) columnName = "BOOLEAN NOT NULL";
        else if (info.PropertyType == typeof(bool?)) columnName = "BOOLEAN";

        isTypeSupported = columnName != null;
        return columnName == null ? null : $"{info.Name.ToLower()} {columnName}";
    }

    #endregion
}


