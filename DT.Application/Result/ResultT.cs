using System;
using System.Collections.Generic;
using System.Linq;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции с возвращаемым значением типа <typeparamref name="T"/>.
    /// Может находиться в состоянии успеха (содержит значение) или неудачи (содержит одну или несколько ошибок).
    /// Обеспечивает безопасный доступ к значению только в случае успеха.
    /// Поддерживает неявное преобразование из <see cref="Error"/> и в небинарный <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемого значения (может быть как ссылочным, так и значимым типом).</typeparam>
    public readonly struct Result<T> : IResult
    {
        private readonly T? _value;
        private readonly List<Error>? _errors;

        /// <summary>
        /// Возвращает <see langword="true"/>, если операция завершилась успешно (ошибок нет).
        /// </summary>
        public bool IsSuccess => _errors == null || _errors.Count == 0;

        /// <summary>
        /// Возвращает <see langword="true"/>, если операция завершилась неудачно (есть хотя бы одна ошибка).
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
        /// Возвращает первую ошибку из списка.
        /// Выбрасывает <see cref="InvalidOperationException"/>, если результат успешен.
        /// Предназначен для обратной совместимости.
        /// </summary>
        public Error Error
        {
            get
            {
                if (IsSuccess)
                    throw new InvalidOperationException("Невозможно получить ошибку из успешного результата.");
                return _errors![0];
            }
        }

        /// <summary>
        /// Возвращает список всех ошибок. В случае успеха возвращается пустой список.
        /// Никогда не возвращает <see langword="null"/>.
        /// </summary>
        public IReadOnlyList<Error> Errors => _errors != null ? _errors : Array.Empty<Error>();

        private Result(T? value, List<Error>? errors)
        {
            _value = (errors == null || errors.Count == 0) ? value : default;
            _errors = errors?.Count > 0 ? errors : null;
        }

        /// <summary>
        /// Создаёт успешный результат с указанным значением.
        /// </summary>
        /// <param name="value">Значение результата.</param>
        /// <returns>Успешный экземпляр <see cref="Result{T}"/>.</returns>
        public static Result<T> Success(T value) => new Result<T>(value, null);

        /// <summary>
        /// Создаёт неуспешный результат с одной ошибкой.
        /// </summary>
        /// <param name="error">Ошибка, описывающая причину неудачи.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result{T}"/>.</returns>
        public static Result<T> Failure(Error error) => new Result<T>(default, new List<Error> { error });

        /// <summary>
        /// Создаёт неуспешный результат с несколькими ошибками.
        /// </summary>
        /// <param name="errors">Список ошибок. Не может быть <see langword="null"/> или пустым.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result{T}"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если список ошибок пуст или равен <see langword="null"/>.</exception>
        public static Result<T> Failure(IEnumerable<Error> errors)
        {
            var list = errors?.ToList();
            if (list == null || list.Count == 0)
                throw new ArgumentException("Список ошибок не может быть пустым или равным null.", nameof(errors));
            return new Result<T>(default, list);
        }

        /// <summary>
        /// Создаёт неуспешный результат с ошибкой, заданной по коду, сообщению и типу.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Описание ошибки.</param>
        /// <param name="type">Тип ошибки. По умолчанию — <see cref="ErrorTypeEnum.Failure"/>.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result{T}"/>.</returns>
        public static Result<T> Failure(string code, string message, ErrorTypeEnum type = ErrorTypeEnum.Failure)
            => Failure(new Error(code, message, type));

        /// <summary>
        /// Неявное преобразование из <see cref="Error"/> в <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        public static implicit operator Result<T>(Error error) => Failure(error);

        /// <summary>
        /// Неявное преобразование из <see cref="Result{T}"/> в <see cref="Result"/>.
        /// Сохраняет все ошибки (в отличие от старой версии, которая брала только первую).
        /// </summary>
        /// <param name="result">Результат с типизированным значением.</param>
        public static implicit operator Result(Result<T> result)
            => result.IsSuccess ? Result.Success() : Result.Failure(result.Errors);
    }
}
