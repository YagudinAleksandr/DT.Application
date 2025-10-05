using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции без возвращаемого значения (аналог void).
    /// Может находиться в состоянии успеха (<see cref="IsSuccess"/>) или неудачи (<see cref="IsFailure"/>).
    /// В случае неудачи содержит информацию об ошибке через свойство <see cref="Error"/>.
    /// Реализован как неизменяемая структура для эффективности и безопасности.
    /// </summary>
    public readonly struct Result
    {
        /// <summary>
        /// Ошибка
        /// </summary>
        private readonly Error? _error;

        /// <summary>
        /// Статус успешного выполнения
        /// </summary>
        public bool IsSuccess => _error is null;

        /// <summary>
        /// Статус неудачного выполнения
        /// </summary>
        public bool IsFailure => _error != null;

        /// <summary>
        /// Ошибка
        /// </summary>
        public Error Error => _error ?? throw new InvalidOperationException("No error in success result.");

        /// <summary>
        /// Результат
        /// </summary>
        /// <param name="error">Ошибка</param>
        private Result(Error? error)
        {
            _error = error;
        }

        /// <summary>
        /// Успешный результат
        /// </summary>
        /// <returns><see cref="Result"/></returns>
        public static Result Success() => new Result(null);

        /// <summary>
        /// Не успешное выполнение
        /// </summary>
        /// <param name="error">Ошибка</param>
        /// <returns><see cref="Result"/></returns>
        public static Result Failure(Error error) => new Result(error);

        public static Result Failure(string code, string message, ErrorTypeEnum type = ErrorTypeEnum.Failure)
            => Failure(new Error(code, message, type));

        public static implicit operator Result(Error error) => Failure(error);
    }
}
