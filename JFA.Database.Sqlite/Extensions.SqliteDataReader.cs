using Microsoft.Data.Sqlite;
using System.Reflection;

namespace JFA.Database.Sqlite;

internal static partial class Extensions
{
    internal static object? GetValue(this SqliteDataReader reader, int ordinal, PropertyInfo propertyInfo)
    {
        if (reader.IsDBNull(ordinal))
            return null;

        object? value = null;
        var type = propertyInfo.PropertyType;

        if (type == typeof(int)) value = reader.GetInt32(ordinal);
        else if (type == typeof(int?)) value = reader.GetInt32(ordinal);
        else if (type == typeof(string)) value = reader.GetString(ordinal);
        else if (type == typeof(bool)) value = reader.GetBoolean(ordinal);
        else if (type == typeof(bool?)) value = reader.GetBoolean(ordinal);

        return value;
    }
}
