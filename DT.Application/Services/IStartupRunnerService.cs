using System.Threading;
using System.Threading.Tasks;

namespace DT.Application
{
    /// <summary>
    /// Сервис маркер для запуска сервисов в фоновом запуске
    /// </summary>
    public interface IStartupRunnerService
    {
        /// <summary>
        /// Исполнить
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
