using System;
using System.Collections.Generic;
using System.Linq;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет результат операции без возвращаемого значения (аналог void).
    /// Может находиться в состоянии успеха (<see cref="IsSuccess"/>) или неудачи (<see cref="IsFailure"/>).
    /// В случае неудачи может содержать одну или несколько ошибок через свойство <see cref="Errors"/>.
    /// Реализован как неизменяемая структура для эффективности и потокобезопасности.
    /// </summary>
    public readonly struct Result : IResult
    {
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
        /// Возвращает первую ошибку из списка.
        /// Выбрасывает <see cref="InvalidOperationException"/>, если результат успешен.
        /// Предназначен для обратной совместимости с кодом, ожидающим одну ошибку.
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

        private Result(List<Error>? errors)
        {
            _errors = errors?.Count > 0 ? errors : null;
        }

        /// <summary>
        /// Создаёт успешный результат без ошибок.
        /// </summary>
        /// <returns>Успешный экземпляр <see cref="Result"/>.</returns>
        public static Result Success() => new Result(null);

        /// <summary>
        /// Создаёт неуспешный результат с одной ошибкой.
        /// </summary>
        /// <param name="error">Ошибка, описывающая причину неудачи.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result"/>.</returns>
        public static Result Failure(Error error) => new Result(new List<Error> { error });

        /// <summary>
        /// Создаёт неуспешный результат с несколькими ошибками.
        /// </summary>
        /// <param name="errors">Список ошибок. Не может быть <see langword="null"/> или пустым.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если список ошибок пуст или равен <see langword="null"/>.</exception>
        public static Result Failure(IEnumerable<Error> errors)
        {
            var list = errors?.ToList();
            if (list == null || list.Count == 0)
                throw new ArgumentException("Список ошибок не может быть пустым или равным null.", nameof(errors));
            return new Result(list);
        }

        /// <summary>
        /// Создаёт неуспешный результат с ошибкой, заданной по коду, сообщению и типу.
        /// </summary>
        /// <param name="code">Код ошибки.</param>
        /// <param name="message">Описание ошибки.</param>
        /// <param name="type">Тип ошибки. По умолчанию — <see cref="ErrorTypeEnum.Failure"/>.</param>
        /// <returns>Неуспешный экземпляр <see cref="Result"/>.</returns>
        public static Result Failure(string code, string message, ErrorTypeEnum type = ErrorTypeEnum.Failure)
            => Failure(new Error(code, message, type));

        /// <summary>
        /// Неявное преобразование из <see cref="Error"/> в <see cref="Result"/>.
        /// Позволяет писать: <c>return new Error("...", "...");</c> в методах, возвращающих <see cref="Result"/>.
        /// </summary>
        /// <param name="error">Ошибка.</param>
        public static implicit operator Result(Error error) => Failure(error);
    }
}
