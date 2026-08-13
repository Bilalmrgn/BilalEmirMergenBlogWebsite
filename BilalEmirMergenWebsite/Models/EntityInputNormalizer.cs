using System.Reflection;

namespace BilalEmirMergenWebsite.Models;

public static class EntityInputNormalizer
{
    /// <summary>
    /// Model binding represents an empty form field as null. Portfolio entities keep
    /// optional text as empty strings so SQL Server never receives null for legacy
    /// non-null columns.
    /// </summary>
    public static T NormalizeText<T>(this T entity) where T : class
    {
        foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.PropertyType == typeof(string) && property.CanWrite && property.GetValue(entity) is null)
            {
                property.SetValue(entity, string.Empty);
            }
        }

        return entity;
    }
}
