using DT.Application.Result;

namespace DT.Application.Tests
{
    public class ResultTTests
    {
        [Fact(DisplayName = "Успешный Result{T} должен вернуть значение")]
        public void Should_Return_Success_Result_With_Value()
        {
            var result = Result<int>.Success(42);
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
        }

        [Fact(DisplayName = "Попытка получить значение из неуспешного Result{T} выбрасывает исключение")]
        public void Should_Return_Exception_When_Result_Is_Not_Success()
        {
            var result = Result<int>.Failure(Error.Global("Fail"));
            Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        }

        [Fact(DisplayName = "Неуспешный Result{T} сохраняет ошибки")]
        public void Неуспешный_ResultT_сохраняет_ошибки()
        {
            var errors = new[]
            {
            Error.WithField("Name", "Required"),
            Error.Global("Conflict")
        };
            var result = Result<string>.Failure(errors);
            Assert.False(result.IsSuccess);
            Assert.Equal(2, result.Errors.Count);
        }
    }
}
