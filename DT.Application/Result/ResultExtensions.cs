#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DT.Application.Result;

/// <summary>
/// Вспомогательные методы для преобразования <see cref="Result"/> и <see cref="Result{T}"/>
/// в объекты ответа ASP.NET Core с поддержкой локализации.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Преобразует <see cref="Result"/> в <see cref="IActionResult"/>.
    /// Локализация выполняется через переданный сервис.
    /// </summary>
    public static IActionResult ToActionResult(
        this Result result,
        ILocalizationService localization)
    {
        if (result.IsSuccess)
            return new OkResult();

        var details = CreateProblemDetails(result.Errors, localization);
        return MapToActionResult(details, result.Errors[0].Type);
    }

    /// <summary>
    /// Преобразует <see cref="Result{T}"/> в <see cref="IActionResult"/>.
    /// </summary>
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ILocalizationService localization)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        var details = CreateProblemDetails(result.Errors, localization);
        return MapToActionResult(details, result.Errors[0].Type);
    }

    private static ProblemDetails CreateProblemDetails(
        IReadOnlyList<Error> errors,
        ILocalizationService localization)
    {
        var globalErrors = errors.Where(e => e.FieldName == null).ToList();
        var fieldErrors = errors.Where(e => e.FieldName != null)
            .ToDictionary(e => e.FieldName!, e => localization.GetLocalizedString(e.Code, e.Arguments));

        string title;
        string? detail = null;

        if (globalErrors.Count > 0)
        {
            var first = globalErrors[0];
            title = first.Code;
            detail = localization.GetLocalizedString(first.Code, first.Arguments);
        }
        else
        {
            title = "Произошли ошибки проверки.";
        }

        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = (int)MapErrorTypeToStatusCode(errors[0].Type),
            Type = errors[0].Code
        };

        if (fieldErrors.Count > 0)
        {
            problem.Extensions["errors"] = fieldErrors;
        }

        return problem;
    }

    private static IActionResult MapToActionResult(ProblemDetails details, ErrorType type)
    {
        return type switch
        {
            ErrorType.Validation => new BadRequestObjectResult(details),
            ErrorType.NotFound => new NotFoundObjectResult(details),
            ErrorType.Conflict => new ConflictObjectResult(details),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(details),
            ErrorType.Forbidden => new ForbidResult(),
            _ => new BadRequestObjectResult(details)
        };
    }

    private static HttpStatusCode MapErrorTypeToStatusCode(ErrorType type)
    {
        return type switch
        {
            ErrorType.Validation => HttpStatusCode.BadRequest,
            ErrorType.NotFound => HttpStatusCode.NotFound,
            ErrorType.Conflict => HttpStatusCode.Conflict,
            ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
            ErrorType.Forbidden => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.BadRequest
        };
    }
}

/// <summary>
/// Интерфейс для сервиса локализации, используемого при преобразовании ошибок в ответ API.
/// Реализуется на уровне Presentation.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Возвращает локализованную строку по коду ошибки и аргументам.
    /// </summary>
    string GetLocalizedString(string errorCode, params object?[] args);
}
#endif

