using System;

namespace DT.Application.Result
{
    /// <summary>
    /// Представляет неизменяемую ошибку с поддержкой кода, аргументов локализации,
    /// типа ошибки и опциональной привязки к полю модели.
    /// </summary>
    public readonly struct Error : IEquatable<Error>
    {
        /// <summary>
        /// Уникальный код ошибки (например, "User.Email.Invalid").
        /// Используется как ключ для локализованных строк.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Аргументы для подстановки в локализованное сообщение.
        /// Может быть пустым массивом.
        /// </summary>
        public object?[] Arguments { get; }

        /// <summary>
        /// Тип ошибки, определяющий HTTP-статус при API-ответе.
        /// </summary>
        public ErrorType Type { get; }

        /// <summary>
        /// Название поля, к которому относится ошибка.
        /// Если значение <see langword="null"/> — ошибка глобальная (не привязана к полю).
        /// </summary>
        public string? FieldName { get; }

        /// <summary>
        /// Создаёт экземпляр ошибки без привязки к полю (глобальная ошибка).
        /// </summary>
        public Error(string code, object?[]? arguments = null, ErrorType type = ErrorType.Failure)
            : this(code, arguments, type, fieldName: null)
        {
        }

        /// <summary>
        /// Создаёт экземпляр ошибки с привязкой к полю (например, при валидации).
        /// Параметр <paramref name="fieldName"/> может быть <see langword="null"/> только
        /// если вызывается из другого конструктора. При явном вызове обычно передаётся непустое имя.
        /// </summary>
        public Error(string code, object?[]? arguments, ErrorType type, string? fieldName)
        {
            this = default;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Arguments = arguments ?? Array.Empty<object?>();
            Type = type;
            FieldName = fieldName; // ← null допустим!
        }

        public bool Equals(Error other) =>
            Code == other.Code &&
            FieldName == other.FieldName &&
            Type == other.Type;

        public override bool Equals(object? obj) => obj is Error other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Code, FieldName, Type);

        public override string ToString() =>
            FieldName != null ? $"{FieldName}: {Code}" : Code;

        /// <summary>
        /// Создаёт глобальную ошибку (без привязки к полю).
        /// </summary>
        public static Error Global(string code, object?[]? arguments = null, ErrorType type = ErrorType.Failure)
            => new(code, arguments, type, fieldName: null);

        /// <summary>
        /// Создаёт ошибку, привязанную к конкретному полю модели.
        /// </summary>
        public static Error WithField(string fieldName, string code, object?[]? arguments = null)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Название поля не может быть пустым.", nameof(fieldName));
            return new Error(code, arguments, ErrorType.Validation, fieldName);
        }
    }
}