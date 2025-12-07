using System;
using System.Collections.Generic;
using System.Reflection;

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

            // Случай: Result (без <T>)
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }

            // Случай: Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Получаем метод Failure(IEnumerable<Error>) напрямую из TResponse (закрытого типа)
                var method = typeof(TResponse).GetMethod(
                    "Failure",
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(IEnumerable<Error>) },
                    modifiers: null
                );

                if (method == null)
                    throw new InvalidOperationException($"Метод Failure(IEnumerable<Error>) не найден в {typeof(TResponse)}.");

                var result = method.Invoke(null, new object[] { errors });
                return (TResponse)result!;
            }

            throw new InvalidOperationException($"Не поддерживаемый тип результата: {typeof(TResponse)}");
        }
    }
}
