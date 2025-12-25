#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;

namespace DT.Application.Providers
{
    /// <summary>
    /// Предоставляет IP-адрес клиента.
    /// </summary>
    public interface IClientIpProvider
    {
        /// <summary>
        /// Получение IP-адреса из запроса
        /// </summary>
        /// <returns>IP-адрес в виде строки</returns>
        string GetClientIp();
    }

    /// <inheritdoc cref="IClientIpProvider"/>
    internal class ClientIpProvider : IClientIpProvider
    {
        #region CTOR
        /// <inheritdoc cref="IHttpContextAccessor"/>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClientIpProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "0.0.0.0";

            // Поддержка прокси и заголовков X-Forwarded-For
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim() ?? "0.0.0.0";
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
    }
}

#endif