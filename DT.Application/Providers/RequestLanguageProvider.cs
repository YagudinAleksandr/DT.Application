#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;

namespace DT.Application.Providers
{
    /// <summary>
    /// Предоставляет язык, определённый из запроса.
    /// </summary>
    public interface IRequestLanguageProvider
    {
        /// <summary>
        /// Получение языка из запроса
        /// </summary>
        /// <returns>Язык</returns>
        string GetLanguage();
    }

    public class RequestLanguageProvider : IRequestLanguageProvider
    {
        #region CTOR
        /// <inheritdoc cref="IHttpContextAccessor"/>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestLanguageProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        /// <inheritdoc/>
        public string GetLanguage()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "ru"; // значение по умолчанию

            // Пример: сначала Accept-Language, затем кастомный заголовок X-Language
            var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();
            var customLang = context.Request.Headers["X-Language"].FirstOrDefault();

            // Простая логика — можно сделать через parser или библиотеку
            var lang = customLang?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()
                      ?? acceptLanguage?.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()
                      ?? "ru";

            // Нормализация: оставить только код языка (например, "ru", "en")
            return lang?.Split('-').FirstOrDefault()?.ToLowerInvariant() ?? "ru";
        }
    }
}
#endif