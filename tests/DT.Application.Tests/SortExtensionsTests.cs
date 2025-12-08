using DT.Application.Extensions;

namespace DT.Application.Tests
{
    /// <summary>
    /// Тесты на <see cref="SortExtensions"/>
    /// </summary>
    public class SortExtensionsTests
    {
        [Fact(DisplayName = "Parse: null возвращает пустой список")]
        public void ParseSortParameters_Null_Returns_Empty()
        {
            // Arrange
            var allowed = new Dictionary<string, string> { { "age", "Age" } };

            // Act
            var result = SortExtensions.ParseSortParameters(allowed, null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact(DisplayName = "Parse: маппинг полей, регистр и пробелы учитываются корректно")]
        public void ParseSortParameters_Maps_And_Trims()
        {
            // Arrange
            var allowed = new Dictionary<string, string>
            {
                { "status", "Status" },
                { "createdat", "CreatedAt" },
                { "city", "Address.City" }
            };
            var input = new[] { "status,asc", " createdat , DESC ", "unknown,asc", "city,Asc", "age" };

            // Act
            var result = SortExtensions.ParseSortParameters(allowed, input);

            // Assert
            // ожидаем: status asc (= IsDescending false), createdat desc (= true), city asc (= false)
            Assert.Equal(3, result.Count);
            Assert.Collection(
                result,
                d => { Assert.Equal("Status", d.PropertyName); Assert.False(d.IsDescending); },
                d => { Assert.Equal("CreatedAt", d.PropertyName); Assert.True(d.IsDescending); },
                d => { Assert.Equal("Address.City", d.PropertyName); Assert.False(d.IsDescending); }
            );
        }

        [Fact(DisplayName = "Parse: некорректные элементы игнорируются")]
        public void ParseSortParameters_Ignores_Invalid_Items()
        {
            // Arrange
            var allowed = new Dictionary<string, string> { { "age", "Age" } };
            var input = new[] { "age", ",desc", " , ", "age,desc,extra", "unknown,asc", "age,desc" };

            // Act
            var result = SortExtensions.ParseSortParameters(allowed, input);

            // Assert
            Assert.Single(result);
            Assert.Equal("Age", result[0].PropertyName);
            Assert.True(result[0].IsDescending);
        }

        [Fact(DisplayName = "Sort: пустой список дескрипторов возвращает исходный IQueryable")]
        public void ApplySorting_Empty_Returns_Source()
        {
            // Arrange
            var src = People.AsQueryable();
            var descriptors = Array.Empty<SortDescriptor>();

            // Act
            var result = src.ApplySorting(descriptors);

            // Assert
            Assert.True(object.ReferenceEquals(src, result));
            // порядок данных не меняется при материализации
            Assert.Equal(People.Select(p => p.Id), result.Select(p => p.Id));
        }

        [Fact(DisplayName = "Sort: простая сортировка по одному полю ASC (Age)")]
        public void ApplySorting_Single_Ascending()
        {
            // Arrange
            var src = People.AsQueryable();
            var descriptors = new List<SortDescriptor>
            {
                new SortDescriptor("Age", false)
            };

            // Act
            var ordered = src.ApplySorting(descriptors).ToList();

            // Assert -> ожидаемый порядок Id: 2 (25), 1 (30), 3 (30), 4 (40)
            Assert.Equal(new[] { 2, 1, 3, 4 }, ordered.Select(p => p.Id));
        }

        [Fact(DisplayName = "Sort: DESC + ThenBy (CreatedAt desc, Name asc)")]
        public void ApplySorting_Descending_ThenBy()
        {
            // Arrange
            var src = People.AsQueryable();
            var descriptors = new List<SortDescriptor>
            {
                new SortDescriptor("CreatedAt", true),  // DESC
                new SortDescriptor("Name", false)       // ASC (на случай совпадений)
            };

            // Act
            var ordered = src.ApplySorting(descriptors).ToList();

            // Assert -> по CreatedAt desc: Id 4 (2024), 2 (2023-01-02), 1 (2023-01-01), 3 (2022)
            Assert.Equal(new[] { 4, 2, 1, 3 }, ordered.Select(p => p.Id));
        }

        [Fact(DisplayName = "Sort: множественная сортировка ASC/DESC")]
        public void ApplySorting_Multiple_ThenBy_Mixed()
        {
            // Arrange -> Age asc, Name desc (для одинакового Age)
            var src = People.AsQueryable();
            var descriptors = new List<SortDescriptor>
            {
                new SortDescriptor("Age", false),      // ASC
                new SortDescriptor("Name", true)       // DESC внутри равного Age
            };

            // Act
            var ordered = src.ApplySorting(descriptors).ToList();

            // Assert -> Age: 25 [Bob], 30 [Charlie, Alice], 40 [Dave]; внутри 30 по Name desc: Charlie, Alice
            Assert.Equal(new[] { 2, 3, 1, 4 }, ordered.Select(p => p.Id));
        }

        [Fact(DisplayName = "Sort: вложенное свойство (Address.City asc, Address.Zip desc)")]
        public void ApplySorting_Nested_Properties()
        {
            // Arrange
            var src = People.AsQueryable();
            var descriptors = new List<SortDescriptor>
            {
                new SortDescriptor("Address.City", false), // LA перед NY
                new SortDescriptor("Address.Zip", true)     // внутри города по Zip убыв.
            };

            // Act
            var ordered = src.ApplySorting(descriptors).ToList();

            // Assert -> LA: Id 3 (90002), 2 (90001); NY: Id 4 (10002), 1 (10001)
            Assert.Equal(new[] { 3, 2, 4, 1 }, ordered.Select(p => p.Id));
        }

        [Fact(DisplayName = "Sort: неизвестное свойство вызывает InvalidOperationException")]
        public void ApplySorting_Unknown_Property_Throws()
        {
            // Arrange
            var src = People.AsQueryable();
            var descriptors = new List<SortDescriptor>
            {
                new SortDescriptor("Unknown", false)
            };

            // Act + Assert
            var ex = Assert.Throws<InvalidOperationException>(() => src.ApplySorting(descriptors).ToList());
            Assert.Contains("Свойство 'Unknown' не найдено", ex.Message);
        }

        #region Test data

        /// <summary>
        /// Адрес
        /// </summary>
        private class Address
        {
            public string City { get; set; }
            public int Zip { get; set; }
            public Address(string city, int zip)
            {
                City = city;
                Zip = zip;
            }
        }

        /// <summary>
        /// Человек
        /// </summary>
        private class Person
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public DateTime CreatedAt { get; set; }
            public Address Address { get; set; } = new Address("", 0);
        }

        /// <summary>
        /// Список людей
        /// </summary>
        private static List<Person> People => new()
        {
            new Person { Id = 1, Name = "Alice",   Age = 30, CreatedAt = new DateTime(2023, 01, 01), Address = new Address("NY", 10001) },
            new Person { Id = 2, Name = "Bob",     Age = 25, CreatedAt = new DateTime(2023, 01, 02), Address = new Address("LA", 90001) },
            new Person { Id = 3, Name = "Charlie", Age = 30, CreatedAt = new DateTime(2022, 12, 31), Address = new Address("LA", 90002) },
            new Person { Id = 4, Name = "Dave",    Age = 40, CreatedAt = new DateTime(2024, 01, 01), Address = new Address("NY", 10002) },
        };
        #endregion
    }
}
