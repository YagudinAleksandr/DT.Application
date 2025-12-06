using System;
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
        public static TResponse CreateFailure<TResponse>(Error[] errors) where TResponse : class, IResult
        {
            // Если TResponse — это Result (без значения)
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }

            // Если TResponse — это Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(Result).GetMethods()
                    .First(m => m.Name == "Failure" && m.IsGenericMethod);

                var innerType = typeof(TResponse).GetGenericArguments()[0];
                var genericFailure = failureMethod.MakeGenericMethod(innerType);
                var result = genericFailure.Invoke(null, new object[] { errors });
                return (TResponse)result!;
            }

            throw new InvalidOperationException($"Not allowed result type: {typeof(TResponse)}");
        }
    }
}
