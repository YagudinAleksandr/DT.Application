using DT.Application.Result;

namespace DT.Application.Tests
{
    /// <summary>
    /// Тесты на <see cref="ResultHelper"/>
    /// </summary>
    public class ResultHelperTests
    {
        [Fact(DisplayName = "CreateFailure с типом Result возвращает Result Failure")]
        public void CreateFailure_Should_Return_Result_Failure()
        {
            // Arrange
            var errors = new[] { Error.Global("Test.Error") };

            // Act
            var result = ResultHelper.CreateFailure(typeof(Result.Result), errors);

            // Assert
            Assert.IsType<Result.Result>(result);
            var typedResult = (Result.Result)result;
            Assert.False(typedResult.IsSuccess);
            Assert.Single(typedResult.Errors);
            Assert.Equal("Test.Error", typedResult.Errors[0].Code);
        }

        [Fact(DisplayName = "CreateFailure с типом ResultT возвращает ResultT_Failure")]
        public void CreateFailure_Should_Return_ResultT_With_ResultT_Failure()
        {
            // Arrange
            var errors = new[] { Error.WithField("Name", "Required") };

            // Act
            var result = ResultHelper.CreateFailure(typeof(Result<string>), errors);

            // Assert
            Assert.IsType<Result<string>>(result);
            var typedResult = (Result<string>)result;
            Assert.False(typedResult.IsSuccess);
            Assert.Single(typedResult.Errors);
            Assert.Equal("Name", typedResult.Errors[0].FieldName);
            Assert.Equal("Required", typedResult.Errors[0].Code);
        }

        [Fact(DisplayName = "CreateFailure с null resultType выбрасывает ArgumentNullException(")]
        public void CreateFailure_With_Null_ResultType_Should_Return_ArgumentNullException()
        {
            // Arrange
            var errors = new[] { Error.Global("Test") };

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                ResultHelper.CreateFailure(null!, errors));
            Assert.Equal("resultType", ex.ParamName);
        }

        [Fact(DisplayName = "CreateFailure с null_errors выбрасывает ArgumentNullException")]
        public void CreateFailure_With_Null_errors_Throw_ArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                ResultHelper.CreateFailure(typeof(Result.Result), null!));
            Assert.Equal("errors", ex.ParamName);
        }

        [Fact(DisplayName = "CreateFailure с неподдерживаемым типом выбрасывает ArgumentException")]
        public void CreateFailure_With_Not_Allowed_Type_Throw_ArgumentException()
        {
            // Arrange
            var errors = new[] { Error.Global("Test") };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                ResultHelper.CreateFailure(typeof(string), errors));
            Assert.Contains("не поддерживается", ex.Message);
            Assert.Equal("resultType", ex.ParamName);
        }

        [Fact(DisplayName = "CreateFailure с пустым массивом ошибок всё равно создаёт неуспешный результат")]
        public void CreateFailure_With_Empty_Errors_Array_Return_Success_Result()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                ResultHelper.CreateFailure(typeof(Result.Result), Array.Empty<Error>()));
            Assert.Contains("ошибка", ex.Message);
        }
    }
}
