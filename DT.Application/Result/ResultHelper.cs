using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Вспомогательный класс для создания экземпляров Result и Result{T} через рефлексию.
    /// Используется в инфраструктурных компонентах (например, MediatR PipelineBehavior).
    /// </summary>
    public static class ResultHelper
    {
        /// <summary>
        /// Создаёт неуспешный результат указанного типа с заданными ошибками.
        /// </summary>
        /// <param name="resultType">Тип результата (должен быть Result или Result{T})</param>
        /// <param name="errors">Массив ошибок</param>
        /// <returns>Экземпляр результата в виде object</returns>
        /// <exception cref="ArgumentException">Если тип не поддерживается</exception>
        public static object CreateFailure(Type resultType, Error[] errors)
        {
            if (resultType == null)
                throw new ArgumentNullException(nameof(resultType));
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));

            if (resultType == typeof(Result))
            {
                return Result.Failure(errors);
            }

            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = resultType.GenericTypeArguments[0];
                var genericResultType = typeof(Result<>).MakeGenericType(innerType);
                var method = genericResultType.GetMethod("Failure", new[] { typeof(Error[]) })
                             ?? throw new InvalidOperationException($"Метод Failure не найден в {genericResultType}.");

                return method.Invoke(null, new object[] { errors })!;
            }

            throw new ArgumentException(
                $"Тип '{resultType}' не поддерживается. Ожидается 'Result' или 'Result<T>'.",
                nameof(resultType));
        }
    }
}
