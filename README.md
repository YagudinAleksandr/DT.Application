# DT.Application - библиотека содержащая инструментарий для работы сборки Application

[![NuGet Version](https://img.shields.io/nuget/v/DT.Application.svg?logo=nuget)](https://www.nuget.org/packages/DT.Application)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DT.Application.svg)](https://www.nuget.org/packages/DT.Application)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

## Описание
Набор классов для реализации функционального подхода к обработке ошибок и результатам операций в C# приложениях.

## Установка

Через NuGet
```bash
dotnet add package DT.Application
```

Добавление ссылки на проект
```xml
<ProjectReference Include="..\DT.Application\DT.Application.csproj" />
```

# DT.Application

Набор классов для реализации функционального подхода к обработке ошибок и результатам операций в C# приложениях.

## Содержание

- [DT.Application](#result-pattern-library)
  - [Содержание](#содержание)
  - [Основные компоненты](#основные-компоненты)
  - [Быстрый старт](#быстрый-старт)
    - [Базовое использование](#базовое-использование)
    - [Цепочки операций](#цепочки-операций)
  - [Детальное описание компонентов](#детальное-описание-компонентов)
    - [Error](#error)
    - [Result](#result)
    - [Result\<T\>](#resultt)
    - [ResultExtensions](#resultextensions)
    - [PagedResult\<T\>](#pagedresultt)
    - [Система фильтрации и сортировки](#система-фильтрации-и-сортировки)
    - [Сервис для старта в фоновом режиме сервисов](#сервис-для-старта-в-фоновом-режиме-сервисов)
  - [Примеры использования](#примеры-использования)
    - [Валидация данных](#валидация-данных)
    - [Работа с базой данных](#работа-с-базой-данных)
    - [Пагинация с сортировкой](#пагинация-с-сортировкой)
  - [Лучшие практики](#лучшие-практики)

## Основные компоненты

| Компонент | Назначение |
|-----------|------------|
| `Error` | Представляет ошибку с кодом, сообщением и типом |
| `Result` | Результат операции без возвращаемого значения |
| `Result<T>` | Результат операции с возвращаемым значением |
| `ResultExtensions` | Методы расширения для функциональных операций |
| `PagedResult<T>` | Результат постраничного запроса |
| `IFilter` | Интерфейс для фильтрации данных |
| `SortExtensions` | Утилиты для сортировки данных |

## Быстрый старт

### Базовое использование

```csharp
// Успешная операция
Result success = Result.Success();

// Ошибка с кодом и сообщением
Result failure = Result.Failure("VALIDATION_ERROR", "Invalid input data");

// Результат с значением
Result<User> userResult = Result<User>.Success(user);

// Результат с ошибкой
Result<User> errorResult = Result<User>.Failure("NOT_FOUND", "User not found");
```

### Цепочки операций
```csharp
public Result<UserDto> GetUserProfile(int userId)
{
    return GetUserById(userId)
        .Ensure(user => user.IsActive, new Error("USER_INACTIVE", "User is inactive"))
        .Map(user => new UserDto(user))
        .Ensure(dto => dto.HasRequiredFields(), new Error("INVALID_PROFILE", "Profile incomplete"));
}
```

## Детальное описание компонентов

### Error
Неизменяемая структура, представляющая ошибку:

```csharp
// Создание ошибок
var error = new Error("NOT_FOUND", "Resource not found", ErrorTypeEnum.NotFound);

// Неявное преобразование из кортежей
Error error1 = ("VALIDATION_ERROR", "Invalid email");
Error error2 = ("AUTH_ERROR", "Unauthorized", ErrorTypeEnum.Unauthorized);
```

Типы ошибок

`Failure` - Общая ошибка

`Validation` - Ошибка валидации

`NotFound` - Ресурс не найден

`Conflict` - Конфликт данных

`Unauthorized` - Не авторизован

`Forbidden` - Доступ запрещен

### Result
Результат операции без возвращаемого значения:

```csharp
csharp
public Result ValidateUser(User user)
{
    if (string.IsNullOrEmpty(user.Email))
        return new Error("EMAIL_REQUIRED", "Email is required");
    
    if (user.Age < 18)
        return new Error("AGE_INVALID", "User must be 18+", ErrorTypeEnum.Validation);
    
    return Result.Success();
}
```

### Result<T>
Типизированный результат операции:

```csharp
public Result<User> CreateUser(UserRequest request)
{
    if (await _userRepository.Exists(request.Email))
        return Result<User>.Failure("USER_EXISTS", "User already exists", ErrorTypeEnum.Conflict);
    
    var user = User.Create(request);
    await _userRepository.Add(user);
    
    return Result<User>.Success(user);
}
```

### ResultExtensions
Функциональные методы расширения:

```csharp
// Map - преобразование значения
Result<UserDto> dtoResult = userResult.Map(user => new UserDto(user));

// Bind - последовательное выполнение операций
Result<Order> orderResult = userResult.Bind(user => CreateOrder(user));

// Match - обработка обоих случаев
string message = result.Match(
    onSuccess: user => $"User: {user.Name}",
    onFailure: error => $"Error: {error.Message}"
);

// Ensure - условная проверка
Result<User> activeUser = userResult
    .Ensure(user => user.IsActive, new Error("INACTIVE", "User is inactive"))
    .Ensure(user => user.EmailConfirmed, new Error("UNCONFIRMED", "Email not confirmed"));
```

### PagedResult<T>
Результат постраничного запроса:

```csharp
public PagedResult<User> GetUsers(int pageNumber, int pageSize)
{
    var query = _context.Users.Where(u => u.IsActive);
    return PagedResult<User>.From(query, pageNumber, pageSize);
}

// Использование
var pagedResult = GetUsers(1, 10);
Console.WriteLine($"Page {pagedResult.PageNumber} of {pagedResult.TotalPages}");
Console.WriteLine($"Items: {pagedResult.Items.Count()} of {pagedResult.TotalCount}");
```

### Система фильтрации и сортировки
```csharp
public class UserFilter : IFilter
{
    public int PageSize { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public string[]? Sort { get; set; }
    
    public void Validate()
    {
        if (PageSize > 100) throw new ArgumentException("Page size too large");
        if (PageNumber < 1) throw new ArgumentException("Invalid page number");
    }
}

// Применение сортировки
var allowedFields = new Dictionary<string, string>
{
    ["name"] = "Name",
    ["created"] = "CreatedAt",
    ["status"] = "Status"
};

var sortDescriptors = SortExtensions.ParseSortParameters(allowedFields, filter.Sort);
var sortedUsers = users.ApplySorting(sortDescriptors);
```

### Сервис для старта в фоновом режиме сервисов
```csharp
public class TestService : IStartupRunnerService
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        await Task.ComplitedTask();
    }
}
```

Создать HostedService
```csharp
public class StartupRunnerHostedService : IHostedService
{
    private readonly IEnumerable<IStartupRunnerService> _startupServices;

    public StartupRunnerHostedService(IEnumerable<IStartupRunnerService> startupServices)
    {
        _startupServices = startupServices;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var service in _startupServices)
        {
            await service.ExecuteAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

Зарегистрировать в DI
```chsrp
builder.Services.AddHostedService<StartupRunnerHostedService>();
```

## Примеры использования

### Валидация данных
```csharp
public Result<User> RegisterUser(RegisterRequest request)
{
    return ValidateEmail(request.Email)
        .Bind(_ => ValidatePassword(request.Password))
        .Bind(_ => CheckEmailUnique(request.Email))
        .Map(_ => User.Create(request))
        .Bind(user => SaveUser(user));
}

private Result ValidateEmail(string email)
{
    if (string.IsNullOrEmpty(email))
        return new Error("EMAIL_EMPTY", "Email is required");
    
    if (!email.Contains("@"))
        return new Error("EMAIL_INVALID", "Invalid email format");
    
    return Result.Success();
}
```

### Работа с базой данных
```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _context.Users.FindAsync(id);
    return user != null 
        ? Result<User>.Success(user)
        : Result<User>.Failure("USER_NOT_FOUND", "User not found", ErrorTypeEnum.NotFound);
}

public async Task<Result> UpdateUserAsync(int id, UserUpdate update)
{
    return await GetUserAsync(id)
        .Ensure(user => user.CanBeUpdated(), new Error("UPDATE_FORBIDDEN", "Cannot update user"))
        .Bind(user => ApplyUpdate(user, update))
        .Bind(user => SaveUserAsync(user));
}
```

### Пагинация с сортировкой
```csharp
public PagedResult<UserDto> GetUsers(UserFilter filter)
{
    filter.Validate();
    
    var query = _context.Users.AsQueryable();
    
    // Применяем сортировку
    var sortFields = new Dictionary<string, string>
    {
        ["name"] = "FullName",
        ["email"] = "Email",
        ["created"] = "CreatedDate"
    };
    
    var sortDescriptors = SortExtensions.ParseSortParameters(sortFields, filter.Sort);
    query = query.ApplySorting(sortDescriptors);
    
    // Применяем пагинацию
    var pagedResult = PagedResult<User>.From(query, filter.PageNumber, filter.PageSize);
    
    // Преобразуем в DTO
    return pagedResult.Map(users => users.Select(u => new UserDto(u)));
}
```

## Лучшие практики
1. Всегда проверяйте IsSuccess/IsFailure перед доступом к Value

2. Используйте Ensure для валидации вместо if-else блоков

3. Применяйте Bind для последовательных операций, которые могут завершиться ошибкой

4. Используйте Map для преобразования успешных результатов

5. Группируйте связанные ошибки в одном Result вместо исключений

6. Определите доменные ошибки как константы для повторного использования
```csharp
public static class DomainErrors
{
    public static Error UserNotFound => new("USER_NOT_FOUND", "User not found", ErrorTypeEnum.NotFound);
    public static Error EmailAlreadyExists => new("EMAIL_EXISTS", "Email already registered", ErrorTypeEnum.Conflict);
    public static Error InvalidCredentials => new("INVALID_CREDENTIALS", "Invalid credentials", ErrorTypeEnum.Unauthorized);
}

// Использование
return Result<User>.Failure(DomainErrors.UserNotFound);
```