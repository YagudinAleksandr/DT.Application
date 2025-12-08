namespace DT.Application.Result
{
    /// <summary>
    /// Перечисление типов ошибок для корректного сопоставления с HTTP-статусами.
    /// Используется при генерации ответа API.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Общая ошибка выполнения операции.
        /// </summary>
        Failure = 0,

        /// <summary>
        /// Ошибка валидации входных данных.
        /// </summary>
        Validation = 1,

        /// <summary>
        /// Запрашиваемый ресурс не найден.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Конфликтующее состояние (например, дубликат).
        /// </summary>
        Conflict = 3,

        /// <summary>
        /// Требуется аутентификация.
        /// </summary>
        Unauthorized = 4,

        /// <summary>
        /// Доступ запрещён.
        /// </summary>
        Forbidden = 5,
    }
}
