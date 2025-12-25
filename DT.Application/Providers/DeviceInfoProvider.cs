#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Http;

namespace DT.Application.Providers
{
    /// <summary>
    /// Предоставляет информацию об устройстве клиента (User-Agent и т.п.).
    /// </summary>
    public interface IDeviceInfoProvider
    {
        string GetUserAgent();
    }

    /// <inheritdoc/>
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        #region CTOR
        /// <inheritdoc cref="IHttpContextAccessor"/>
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeviceInfoProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
        }
    }
}
#endif