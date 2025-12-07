using System;
using System.Collections.Generic;
using System.Linq;

namespace DT.Application.Result
{
    /// <summary>
    /// Фабрика результатов
    /// </summary>
    public static class ResultFactory
    {
        /// <summary>
        /// Создание списка ошибок для результата
        /// </summary>
        /// <typeparam name="TResponse">Тип ответа</typeparam>
        /// <param name="errors">Список ошибок</param>
        /// <returns>Ответ типа <typeparamref name="TResponse"/></returns>
        /// <exception cref="InvalidOperationException">Не поддерживаемый тип ответа</exception>
        public static TResponse CreateFailure<TResponse>(Error[] errors)
            where TResponse : IResult
        {
            if (errors == null || errors.Length == 0)
                throw new ArgumentException("Ошибки не могут быть пустыми.", nameof(errors));

            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var openType = typeof(Result<>);
                var method = openType.GetMethod("Failure", new[] { typeof(IEnumerable<Error>) })
                    ?? throw new InvalidOperationException("Метод Result<T>.Failure(IEnumerable<Error>) не найден.");

                var innerType = typeof(TResponse).GetGenericArguments()[0];
                var closedMethod = method.MakeGenericMethod(innerType);
                var result = closedMethod.Invoke(null, new object[] { errors });
                return (TResponse)result!;
            }

            throw new InvalidOperationException($"Не поддерживаемый тип результата: {typeof(TResponse)}");
        }
    }
}
