namespace DT.Application.Result
{
    /// <summary>
    /// Типы ошибок
    /// </summary>
    public enum ErrorTypeEnum
    {
        /// <summary>
        /// Ошибка
        /// </summary>
        Failure = 0,

        /// <summary>
        /// Валидация
        /// </summary>
        Validation = 1,

        /// <summary>
        /// Ресурс не найден
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Конфликт
        /// </summary>
        Conflict = 3,

        /// <summary>
        /// Не авторизирован
        /// </summary>
        Unauthorized = 4,

        /// <summary>
        /// Доступ ограничен
        /// </summary>
        Forbidden = 5,
    }
}
