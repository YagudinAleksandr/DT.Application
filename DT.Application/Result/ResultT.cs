using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции с возвращаемым значением типа <typeparamref name="T"/>.
    /// Может находиться в состоянии успеха (содержит значение) или неудачи (содержит ошибку).
    /// Обеспечивает безопасный доступ к значению только в случае успеха.
    /// Поддерживает неявное преобразование из <see cref="Error"/> и в небинарный <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения (может быть как ссылочным, так и значимым типом).</typeparam>
    public readonly struct Result<T>
    {
        /// <summary>
        /// Тело возврата данных
        /// </summary>
        private readonly T? _value;

        /// <summary>
        /// Ошибка
        /// </summary>
        private readonly Error? _error;

        /// <summary>
        /// Успешное выполнение
        /// </summary>
        public bool IsSuccess => _error is null;

        /// <summary>
        /// Неудачное выполнение
        /// </summary>
        public bool IsFailure => _error != null;

        /// <summary>
        /// Тело
        /// </summary>
        public T Value => _error is null
            ? _value!
            : throw new InvalidOperationException("Cannot access value of a failure result.");

        /// <summary>
        /// Ошибка
        /// </summary>
        public Error Error => _error ?? throw new InvalidOperationException("No error in success result.");

        /// <summary>
        /// Результат выполнения
        /// </summary>
        /// <param name="value">Тело</param>
        /// <param name="error">Ошибка</param>
        private Result(T value, Error? error = null)
        {
            _value = value;
            _error = error;
        }

        /// <summary>
        /// Успешное выполнение
        /// </summary>
        /// <param name="value">Тело ответа</param>
        /// <returns><see cref="Result{T}"/></returns>
        public static Result<T> Success(T value) => new Result<T>(value);

        /// <summary>
        /// Неудачное выполнение
        /// </summary>
        /// <param name="error">Ошибка</param>
        /// <returns><see cref="Result{T}"/></returns>
        public static Result<T> Failure(Error error) => new Result<T>(default!, error);

        // Удобные фабрики
        public static Result<T> Failure(string code, string message, ErrorTypeEnum type = ErrorTypeEnum.Failure)
            => Failure(new Error(code, message, type));

        // Implicit conversion from Error
        public static implicit operator Result<T>(Error error) => Failure(error);

        // Преобразование в небинарный Result
        public static implicit operator Result(Result<T> result)
            => result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}
