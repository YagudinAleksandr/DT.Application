using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DT.Application.Extensions
{
    public static class SortExtensions
    {
        /// <summary>
        /// Преобразует массив строк вида "field,dir" в список дескрипторов сортировки.
        /// Пример: ["status,asc", "createdat,desc"] → [ { Status, false }, { CreatedAt, true } ]
        /// </summary>
        /// <param name="allowedSortFields">Допустимые поля сортировки</param>
        /// <param name="sortParams">Параметры сортировки</param>
        public static List<SortDescriptor> ParseSortParameters(Dictionary<string, string> allowedSortFields, string[]? sortParams)
        {
            if (sortParams == null) return new();

            var descriptors = new List<SortDescriptor>();
            foreach (var param in sortParams)
            {
                var parts = param.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                var inputName = parts[0].Trim();
                var isDesc = parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);

                if (allowedSortFields.TryGetValue(inputName, out var actualName))
                {
                    descriptors.Add(new SortDescriptor(actualName, isDesc));
                }
            }
            return descriptors;
        }

        /// <summary>
        /// Применяет множественную сортировку к IQueryable.
        /// Первое поле — OrderBy/OrderByDescending, остальные — ThenBy/ThenByDescending.
        /// </summary>
        /// <param name="source">Источник сортировки</param>
        /// <param name="sortDescriptors">Описание запроса</param>
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, IReadOnlyList<SortDescriptor> sortDescriptors)
        {
            if (sortDescriptors == null || sortDescriptors.Count == 0)
                return source;

            var type = typeof(T);
            var parameter = Expression.Parameter(type, "x");
            IQueryable<T> query = source;
            bool isFirst = true;

            foreach (var desc in sortDescriptors)
            {
                Expression propertyAccess = parameter;
                // Разбиваем имя свойства по точке, чтобы поддерживать вложенные свойства
                var parts = desc.PropertyName.Split('.');
                foreach (var part in parts)
                {
                    var propInfo = propertyAccess.Type.GetProperty(part)
                        ?? throw new InvalidOperationException($"Свойство '{part}' не найдено в типе '{propertyAccess.Type.Name}'");

                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, propInfo);
                }

                var lambda = Expression.Lambda(propertyAccess, parameter);

                string methodName;
                if (isFirst)
                {
                    methodName = desc.IsDescending ? "OrderByDescending" : "OrderBy";
                    isFirst = false;
                }
                else
                {
                    methodName = desc.IsDescending ? "ThenByDescending" : "ThenBy";
                }

                var method = typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(type, propertyAccess.Type);

                query = (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
            }

            return query;
        }
    }
}
