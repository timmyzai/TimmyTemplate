
using System.Reflection;
using Parquet.Schema;
using Parquet.Serialization;

namespace ByteAwesome
{
    public class ParquetSchemaHelper
    {
        public static ParquetSchema CreateSchemaFromDto<T>()
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = properties.Select(p => CreateDataField(p)).ToArray();
            return new ParquetSchema(fields);
        }

        private static DataField CreateDataField(PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            string propertyName = property.Name;

            if (propertyType == typeof(string))
                return new DataField<string>(propertyName);
            if (propertyType == typeof(int))
                return new DataField<int>(propertyName);
            if (propertyType == typeof(long))
                return new DataField<long>(propertyName);
            if (propertyType == typeof(float))
                return new DataField<float>(propertyName);
            if (propertyType == typeof(double))
                return new DataField<double>(propertyName);
            if (propertyType == typeof(bool))
                return new DataField<bool>(propertyName);

            throw new NotSupportedException($"Type {propertyType} is not supported for Parquet schema.");
        }
    }
    public class ParquetDataService<T> where T : class, new()
    {
        public static async Task<IList<T>> DeserializeAsync(string filePath, Func<T, bool> predicate = null)
        {
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            using (Stream fileStream = File.OpenRead(filePath))
            {
                var data = await ParquetSerializer.DeserializeAsync<T>(fileStream);
                return predicate is null ? data : data.Where(predicate).ToList();
            }
        }

        public static async Task SerializeAsync(string filePath, IList<T> data)
        {
            using (Stream fileStream = File.Open(filePath, FileMode.Create))
            {
                await ParquetSerializer.SerializeAsync(data, fileStream);
            }
        }
    }
}