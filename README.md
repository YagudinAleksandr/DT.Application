# DT.Application — утилиты для слоя Application

[![NuGet Version](https://img.shields.io/nuget/v/DT.Application.svg?logo=nuget)](https://www.nuget.org/packages/DT.Application)
[![NuGet Downloads](https://img.shields.io/nuget/dt/DT.Application.svg)](https://www.nuget.org/packages/DT.Application)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

Небольшая библиотека для прикладного слоя .NET-приложений: единообразное представление ошибок и результатов, удобная интеграция с ASP.NET Core, пагинация, сортировка, контракт фильтров и вспомогательные сервисы запуска.

Поддерживаемые таргеты: netstandard2.1, net6.0, net7.0, net8.0, net9.0.

- В net6+ доступны интеграционные расширения для ASP.NET Core (ProblemDetails, IActionResult)
- В netstandard2.1 — базовые типы (Result, Error, PagedResult и пр.)

## Установка

Через NuGet:

```bash
dotnet add package DT.Application
```

Либо прямой ProjectReference:

```xml
<ProjectReference Include="..\DT.Application\DT.Application.csproj" />
```

## Что внутри

- `Result`, `Result<T>` — компактные неизменяемые типы результата операции
- `Error` — код ошибки, тип (Validation, NotFound и т.п.), аргументы локализации, опционально поле модели
- `ResultExtensions` (net6+) — преобразование `Result/Result<T>` в `IActionResult` с ProblemDetails и локализацией
- `PagedResult<T>` — пагинация коллекций/запросов
- `SortExtensions`, `SortDescriptor` — безопасная сортировка по разрешённым полям, включая вложенные свойства
- `IFilter` — контракт фильтров (пагинация + сортировка)
- `ResultHelper` — создание неуспешных `Result/Result<T>` по типу во время выполнения (например, в pipeline-ах)
- `IStartupRunnerService` — простая абстракция для запуска фоновых задач при старте приложения

## Быстрый старт

### Результат операции

```csharp
using DT.Application.Result;

Result ok = Result.Success();

var validationError = Error.WithField("Email", "Validation.Email.Invalid");
Result fail = Result.Failure(validationError);

Result<int> valueOk = Result<int>.Success(42);
Result<int> valueFail = Result<int>.Failure(Error.Global("User.NotFound", type: ErrorType.NotFound));

if (valueOk.IsSuccess)
{
    Console.WriteLine(valueOk.Value); // 42
}
```

### Интеграция с ASP.NET Core (net6+)

```csharp
using DT.Application.Result;
using Microsoft.AspNetCore.Mvc;

// Реализация локализации сообщений об ошибках
public class LocalizationService : ILocalizationService
{
    public string GetLocalizedString(string code, params object?[] args)
        => code; // Подставьте вашу логику
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly ILocalizationService _loc = new LocalizationService();

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        Result<UserDto> result = FindUser(id);
        return result.ToActionResult(_loc); // OkObjectResult или ProblemDetails в зависимости от ошибки
    }
}
```

Поведение маппинга ошибок:
- ErrorType.Validation → 400 BadRequest (ProblemDetails + Extensions["errors"] с ошибками полей)
- ErrorType.NotFound → 404 NotFound
- ErrorType.Conflict → 409 Conflict
- ErrorType.Unauthorized → 401 Unauthorized
- ErrorType.Forbidden → 403 (Forbid)
- Иначе → 400 BadRequest

Глобальная ошибка (без FieldName) попадает в Title/Detail ProblemDetails, ошибки полей — в ProblemDetails.Extensions["errors"].

## Пагинация

```csharp
using DT.Application.Result;

IQueryable<User> query = db.Users.Where(u => u.IsActive);
var page = 1;
var size = 20;

// PagedResult.From принимает IEnumerable<T>, но корректно работает и с IQueryable<T>
var pageResult = PagedResult<User>.From(query, page, size);

Console.WriteLine($"Page {pageResult.PageNumber}/{pageResult.TotalPages}. Items: {pageResult.Items.Count()} of {pageResult.TotalCount}");
```

Проекция в DTO:

```csharp
var usersPage = PagedResult<User>.From(query, page, size);
var dtoPage = new PagedResult<UserDto>(
    usersPage.Items.Select(u => new UserDto(u)),
    usersPage.TotalCount,
    usersPage.PageNumber,
    usersPage.PageSize
);
```

## Сортировка

Сортировка строится из массива строк вида "field,dir", где dir ∈ {asc, desc}. Для безопасности используется маппинг разрешённых полей.

```csharp
using DT.Application.Extensions;

var allowed = new Dictionary<string, string>
{
    ["name"] = "FullName",
    ["created"] = "CreatedAt",
    ["city"] = "Address.City" // поддерживаются вложенные свойства
};

string[]? sort = new[] { "created,desc", "name,asc" };
var descriptors = SortExtensions.ParseSortParameters(allowed, sort);

IQueryable<User> query = db.Users;
query = query.ApplySorting(descriptors);
```

Если указано неизвестное свойство, будет выброшено InvalidOperationException.

## Контракт фильтра

```csharp
using DT.Application.Filters;
using DT.Application.Extensions;
using DT.Application.Result;

public class UserFilter : IFilter
{
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public string[]? Sort { get; set; }

    public Dictionary<string, string> SortedFields { get; } = new()
    {
        ["name"] = "FullName",
        ["created"] = "CreatedAt",
        ["status"] = "Status"
    };

    public void Validate()
    {
        if (PageSize < 1 || PageSize > 100) throw new ArgumentOutOfRangeException(nameof(PageSize));
        if (PageNumber < 1) throw new ArgumentOutOfRangeException(nameof(PageNumber));
    }
}

public PagedResult<UserDto> GetUsers(UserFilter f)
{
    f.Validate();

    IQueryable<User> query = db.Users;

    var sortDesc = SortExtensions.ParseSortParameters(f.SortedFields, f.Sort);
    query = query.ApplySorting(sortDesc);

    var page = PagedResult<User>.From(query, f.PageNumber, f.PageSize);
    return new PagedResult<UserDto>(
        page.Items.Select(u => new UserDto(u)),
        page.TotalCount, page.PageNumber, page.PageSize
    );
}
```

## Ошибки (Error)

```csharp
using DT.Application.Result;

// Глобальная ошибка (без привязки к полю)
var e1 = Error.Global("Organization.Closed", type: ErrorType.Failure);

// Ошибка поля (тип автоматически = Validation)
var e2 = Error.WithField("Email", "Validation.Email.Invalid");

// Результаты
Result r1 = Result.Failure(e1);
Result<User> r2 = Result<User>.Failure(e1, e2);
```

Свойства Error:
- Code — ключ локализации/идентификатор
- Arguments — аргументы для локализованного сообщения
- Type — ErrorType (Validation/NotFound/Conflict/Unauthorized/Forbidden/Failure)
- FieldName — имя поля или null для глобальных ошибок

## Вспомогательное: ResultHelper

Создание неуспешного результата по типу во время выполнения (например, в pipeline поведения валидации):

```csharp
using DT.Application.Result;

Type resultType = typeof(Result<MyResponse>);
var errors = new[] { Error.WithField("Name", "Validation.Required") };
object failure = ResultHelper.CreateFailure(resultType, errors);
// failure имеет тип Result<MyResponse>
```

## Сервис запуска задач при старте

```csharp
using DT.Application.Services;
using Microsoft.Extensions.Hosting;

public class SomeStartupTask : IStartupRunnerService
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        // Ваша инициализация
        await Task.CompletedTask;
    }
}

public class StartupRunnerHostedService : IHostedService
{
    private readonly IEnumerable<IStartupRunnerService> _services;
    public StartupRunnerHostedService(IEnumerable<IStartupRunnerService> services) => _services = services;

    public async Task StartAsync(CancellationToken ct)
    {
        foreach (var s in _services)
            await s.ExecuteAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

## Лицензия

MIT — см. LICENSE.md.
