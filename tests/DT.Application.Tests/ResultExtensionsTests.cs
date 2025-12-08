using DT.Application.Result;
using Microsoft.AspNetCore.Mvc;

namespace DT.Application.Tests
{
    /// <summary>
    /// Фейковая реализация ILocalizationService для тестов.
    /// </summary>
    public class FakeLocalizationService : ILocalizationService
    {
        public string GetLocalizedString(string errorCode, params object?[] args)
        {
            // Простая подстановка: "Код: {errorCode}, Арг: {args}"
            var argsStr = args.Length == 0 ? "нет" : string.Join(", ", args);
            return $"Ошибка [{errorCode}]: аргументы = ({argsStr})";
        }
    }

    /// <summary>
    /// Тесты на <see cref="ResultExtensions"/>
    /// </summary>
    public class ResultExtensionsTests
    {
        private readonly ILocalizationService _localization = new FakeLocalizationService();

        [Fact(DisplayName = "Успешный Result возвращает OkResult")]
        public void Success_Result_Should_Return__OkResult()
        {
            // Arrange
            var result = Result.Result.Success();

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            Assert.IsType<OkResult>(actionResult);
        }

        [Fact(DisplayName = "Успешный Result{T} возвращает OkObjectResult со значением")]
        public void Success_ResultT_Should_Return_OkObjectResult_With_Value()
        {
            // Arrange
            var result = Result<int>.Success(42);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            Assert.Equal(42, okResult.Value);
        }

        [Fact(DisplayName = "Глобальная ошибка возвращает BadRequest с Detail")]
        public void Global_Error_Should_Return_BadRequest_With_Detail()
        {
            // Arrange
            var error = Error.Global("User.Blocked", ["user123"]);
            var result = Result.Result.Failure(error);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            var problem = Assert.IsType<ProblemDetails>(badRequest.Value);

            // Убеждаемся, что "errors" отсутствует
            Assert.False(problem.Extensions.ContainsKey("errors"));
            Assert.NotNull(problem.Detail);
            Assert.Equal("User.Blocked", problem.Title);
        }

        [Fact(DisplayName = "Ошибка поля добавляется в Extensions_errors")]
        public void Field_Error_Should_Add_Filed_In_Extensions_Errors()
        {
            // Arrange
            var error = Error.WithField("Email", "Validation.Email.Invalid");
            var result = Result.Result.Failure(error);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
            Assert.Equal("Произошли ошибки проверки.", problem.Title);
            Assert.Null(problem.Detail); // глобальной ошибки нет

            Assert.True(problem.Extensions.TryGetValue("errors", out var errorsObj));
            var errorsDict = Assert.IsType<Dictionary<string, string>>(errorsObj);
            Assert.Single(errorsDict);
            Assert.Contains("Email", errorsDict.Keys);
            Assert.Contains("Validation.Email.Invalid", errorsDict["Email"]);
        }

        [Fact(DisplayName = "Смешанные ошибки глобальная и поле используют глобальную в Detail")]
        public void Mix_Errors_Global_And_Field_Should_Use_Global_In_Detail()
        {
            // Arrange
            var errors = new[]
            {
                Error.WithField("Name", "Required"),
                Error.Global("Organization.Closed")
            };
            var result = Result.Result.Failure(errors);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
            Assert.Equal("Organization.Closed", problem.Title);
            Assert.Contains("Organization.Closed", problem.Detail!);

            var errorsDict = Assert.IsType<Dictionary<string, string>>(problem.Extensions["errors"]);
            Assert.Contains("Name", errorsDict.Keys);
        }

        [Theory(DisplayName = "Тип ошибки определяет тип результата")]
        [InlineData(ErrorType.NotFound, typeof(NotFoundObjectResult))]
        [InlineData(ErrorType.Conflict, typeof(ConflictObjectResult))]
        [InlineData(ErrorType.Unauthorized, typeof(UnauthorizedObjectResult))]
        [InlineData(ErrorType.Forbidden, typeof(ForbidResult))]
        [InlineData(ErrorType.Failure, typeof(BadRequestObjectResult))]
        public void Error_Type_Should_Select_Result_Type(ErrorType errorType, Type expectedType)
        {
            // Arrange
            var error = Error.Global("Test", type: errorType);
            var result = Result.Result.Failure(error);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            Assert.IsType(expectedType, actionResult);
        }

        [Fact(DisplayName = "Ошибка Validation возвращает BadRequest")]
        public void Error_Validation_Should_Return_BadRequest()
        {
            // Arrange
            var error = Error.WithField("X", "Invalid");
            var result = Result<int>.Failure(error);

            // Act
            var actionResult = result.ToActionResult(_localization);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult);
        }
    }
}
