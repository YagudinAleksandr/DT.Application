using DT.Application.Result;

namespace DT.Application.Tests
{
    /// <summary>
    /// Тесты на <see cref="Result.Result"/>
    /// </summary>
    public class ResultTests
    {
        [Fact(DisplayName = "Успешный Result имеет статус IsSuccess - true")]
        public void Should_Return_Success_Result_Without_Error()
        {
            var result = Result.Result.Success();
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Empty(result.Errors);
        }

        [Fact(DisplayName = "Неуспешный Result должен содержать ошибку")]
        public void Should_Return_Failure_Result_With_Error()
        {
            var error = Error.Global("Test.Error");
            var result = Result.Result.Failure(error);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Single(result.Errors);
            Assert.Equal("Test.Error", result.Errors[0].Code);
        }
    }
}
