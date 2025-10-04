namespace DT.Application.Result
{
    /// <summary>
    /// Обхект ошибки
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Код ошибки
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Параметры
        /// </summary>
        public object[]? Parameters { get; }

        /// <summary>
        /// Представляет отсутствие ошибки.
        /// </summary>
        public static readonly Error None = new Error(string.Empty);

        /// <summary>
        /// Указывает, что ошибка отсутствует.
        /// </summary>
        public bool IsNone => this == None;

        /// <summary>
        /// Указывает, что у ошибки есть параметры для подстановки.
        /// </summary>
        public bool HasParameters => Parameters != null && Parameters.Length > 0;
        public Error(string code, object[]? parameters = null)
        {
            Code = code;
            Parameters = parameters;
        }

    }
}
