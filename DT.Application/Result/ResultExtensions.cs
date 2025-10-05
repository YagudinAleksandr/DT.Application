using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Набор функциональных методов-расширений для работы с типами <see cref="Result"/> и <see cref="Result{T}"/>.
    /// Включает такие операции, как сопоставление (<see cref="Map"/>), связывание (<see cref="Bind"/>),
    /// обработка обоих исходов (<see cref="Match"/>) и условная проверка (<see cref="Ensure"/>).
    /// Позволяет писать цепочки безопасных операций без явных проверок на ошибки.
    /// </summary>
    public static class ResultExtensions
    {
        // Map
        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
        {
            return result.IsSuccess
                ? Result<TOut>.Success(mapper(result.Value))
                : Result<TOut>.Failure(result.Error);
        }

        // Bind / FlatMap
        public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
        {
            return result.IsSuccess
                ? binder(result.Value)
                : Result<TOut>.Failure(result.Error);
        }

        // Match (для обработки обоих случаев)
        public static TOut Match<T, TOut>(this Result<T> result, Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
        }

        public static TOut Match<TOut>(this Result result, Func<TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            return result.IsSuccess ? onSuccess() : onFailure(result.Error);
        }

        // Ensure (валидация)
        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error errorIfInvalid)
        {
            if (result.IsFailure) return result;
            return predicate(result.Value) ? result : Result<T>.Failure(errorIfInvalid);
        }
    }
}
