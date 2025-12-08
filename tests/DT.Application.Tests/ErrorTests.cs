using DT.Application.Result;

namespace DT.Application.Tests
{
    /// <summary>
    /// Тесты на <see cref="Error"/>
    /// </summary>
    public class ErrorTests
    {
        [Fact(DisplayName ="Глобальная ошибка создается без поля")]
        public void Should_Not_Add_Field_Name_When_Global_Error()
        {
            var error = Error.Global("User.Blocked", ["user123"]);
            Assert.Null(error.FieldName);
            Assert.Equal("User.Blocked", error.Code);
            Assert.Equal(ErrorType.Failure, error.Type);
        }

        [Fact(DisplayName = "Ошибка поля содержит название поля")]
        public void Should_Return_Field_Name_When_Field_Error()
        {
            var error = Error.WithField("Email", "Validation.Email.Invalid");
            Assert.Equal("Email", error.FieldName);
            Assert.Equal("Validation.Email.Invalid", error.Code);
            Assert.Equal(ErrorType.Validation, error.Type);
        }

        [Fact(DisplayName = "Ошибки с одинаковыми параметрами равны")]
        public void Errors_With_The_Same_Params_Are_Equal()
        {
            var e1 = new Error("Code", null, ErrorType.Failure);
            var e2 = new Error("Code", null, ErrorType.Failure);
            Assert.True(e1.Equals(e2));
        }

        [Fact(DisplayName = "Ошибки с разными полями не равны")]
        public void Errors_With_Not_Same_Params_Not_Equal()
        {
            var e1 = Error.WithField("A", "Code");
            var e2 = Error.WithField("B", "Code");
            Assert.False(e1.Equals(e2));
        }
    }
}
