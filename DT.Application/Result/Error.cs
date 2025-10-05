using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет неизменяемую ошибку с уникальным кодом, описанием и типом.
    /// Код ошибки предназначен для программной обработки и локализации сообщений.
    /// Поддерживает неявное преобразование из кортежей для удобного создания.
    /// </summary>
    public readonly struct Error : IEquatable<Error>
    {
        /// <summary>
        /// Код ошибки
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Тип ошибки
        /// </summary>
        public ErrorTypeEnum Type { get; }

        /// <summary>
        /// Ошибка
        /// </summary>
        /// <param name="code">Код ошибки</param>
        /// <param name="message">Сообщение</param>
        /// <param name="type">Тип</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Error(string code, string message, ErrorTypeEnum type = ErrorTypeEnum.Failure)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Type = type;
        }

        /// <summary>
        /// Равенство
        /// </summary>
        /// <param name="other">Объект для сравнения</param>
        /// <returns>true - равны, false - не равны</returns>
        public bool Equals(Error other) => Code == other.Code;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Error other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);

        /// <inheritdoc/>
        public override string ToString() => $"{Type}: {Code} - {Message}";


        public static implicit operator Error((string Code, string Message, ErrorTypeEnum Type) tuple)
            => new Error(tuple.Code, tuple.Message, tuple.Type);

        public static implicit operator Error((string Code, string Message) tuple)
            => new Error(tuple.Code, tuple.Message, ErrorTypeEnum.Failure);
    }
}
