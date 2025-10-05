# DT.Application - библиотека содержащая инструментарий для работы сборки Application

## Описание
Идеально подходит для:

Валидации входных данных
Обработки ошибок в бизнес-логике
Построения отказоустойчивых цепочек вызовов
Подготовки ответов API с кодами ошибок

## Установка

Через NuGet
```bash
dotnet add package DT.Application
```

Добавление ссылки на проект
```xml
<ProjectReference Include="..\DT.Application\DT.Application.csproj" />
```

## Примеры использования
1. Базовая проверка
```csharp
public Result<string> GetUserName(int id)
{
    if (id <= 0)
        return ("INVALID_ID", "ID должен быть положительным");

    var user = _userRepository.FindById(id);
    if (user == null)
        return ("USER_NOT_FOUND", "Пользователь не найден", ErrorType.NotFound);

    return user.Name; // неявное преобразование в Result<string>.Success
}
```

2. Цепочка операций (функциональный стиль)
```csharp
var result = GetUserById(123)
    .Ensure(u => u.IsActive, ("USER_INACTIVE", "Пользователь неактивен"))
    .Map(u => u.Email)
    .Ensure(email => email.Contains("@"), ("INVALID_EMAIL", "Email некорректен"));

if (result.IsSuccess)
{
    Console.WriteLine($"Email: {result.Value}");
}
else
{
    Console.WriteLine($"Ошибка [{result.Error.Code}]: {result.Error.Message}");
}
```

3. Обработка обоих исходов
```csharp
var response = result.Match(
    onSuccess: email => Ok(email),
    onFailure: error => error.Type switch
    {
        ErrorType.NotFound => NotFound(error.Message),
        ErrorType.Validation => BadRequest(error.Message),
        _ => StatusCode(500, error.Message)
    }
);
```
