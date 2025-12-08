using System;
using System.Collections.Generic;
using System.Linq;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции без возвращаемого значения.
    /// Может быть успешным или содержать одну или несколько ошибок.
    /// Неизменяемая структура для потокобезопасности и производительности.
    /// </summary>
    public readonly struct Result
    {
        private readonly Error[] _errors;

        private Result(Error[] errors)
        {
            _errors = errors;
            IsSuccess = _errors.Length == 0;
        }

        /// <summary>
        /// Возвращает <see langword="true"/>, если операция завершилась успешно.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Возвращает <see langword="true"/>, если операция завершилась с ошибкой.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Возвращает список всех ошибок. Никогда не возвращает <see langword="null"/>.
        /// </summary>
        public IReadOnlyList<Error> Errors => _errors;

        /// <summary>
        /// Создаёт успешный результат без ошибок.
        /// </summary>
        public static Result Success() => new(Array.Empty<Error>());

        /// <summary>
        /// Создаёт неуспешный результат с одной ошибкой.
        /// </summary>
        public static Result Failure(Error error) => new(new[] { error });

        /// <summary>
        /// Создаёт неуспешный результат с несколькими ошибками.
        /// </summary>
        public static Result Failure(IEnumerable<Error> errors)
        {
            var array = errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors));
            if (array.Length == 0)
                throw new ArgumentException("Должна быть хотя бы одна ошибка.", nameof(errors));
            return new Result(array);
        }

        /// <summary>
        /// Создаёт неуспешный результат с переменным числом ошибок.
        /// </summary>
        public static Result Failure(params Error[] errors)
        {
            if (errors == null || errors.Length == 0)
                throw new ArgumentException("Должна быть хотя бы одна ошибка.", nameof(errors));
            return new Result(errors);
        }
    }
}