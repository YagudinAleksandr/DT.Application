using System;
using System.Collections.Generic;
using System.Linq;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции с возвращаемым значением типа <typeparamref name="T"/>.
    /// Может быть успешным (содержит значение) или неуспешным (содержит ошибки).
    /// Неизменяемая структура.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения</typeparam>
    public readonly struct Result<T>
    {
        private readonly T? _value;
        private readonly Error[] _errors;

        private Result(T? value, Error[] errors)
        {
            _value = value;
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
        /// Возвращает значение результата.
        /// Выбрасывает <see cref="InvalidOperationException"/>, если результат неуспешен.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException("Невозможно получить значение из неуспешного результата.");
                return _value!;
            }
        }

        /// <summary>
        /// Возвращает список всех ошибок. Никогда не возвращает <see langword="null"/>.
        /// </summary>
        public IReadOnlyList<Error> Errors => _errors;

        /// <summary>
        /// Создаёт успешный результат с указанным значением.
        /// </summary>
        public static Result<T> Success(T value) => new(value, Array.Empty<Error>());

        /// <summary>
        /// Создаёт неуспешный результат с одной ошибкой.
        /// </summary>
        public static Result<T> Failure(Error error) => new(default, new[] { error });

        /// <summary>
        /// Создаёт неуспешный результат с несколькими ошибками.
        /// </summary>
        public static Result<T> Failure(IEnumerable<Error> errors)
        {
            var array = errors?.ToArray() ?? throw new ArgumentNullException(nameof(errors));
            if (array.Length == 0)
                throw new ArgumentException("Должна быть хотя бы одна ошибка.", nameof(errors));
            return new Result<T>(default, array);
        }

        /// <summary>
        /// Создаёт неуспешный результат с переменным числом ошибок.
        /// </summary>
        public static Result<T> Failure(params Error[] errors)
        {
            if (errors == null || errors.Length == 0)
                throw new ArgumentException("Должна быть хотя бы одна ошибка.", nameof(errors));
            return new Result<T>(default, errors);
        }
    }
}