using System.Collections.Generic;

namespace DT.Application.Result
{
    /// <summary>
    /// Итерфейс результата
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Список ошибок
        /// </summary>
        IReadOnlyList<Error> Errors { get; }

        /// <summary>
        /// Статус результата
        /// </summary>
        bool IsSuccess { get; }
    }
}
