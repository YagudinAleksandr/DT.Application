namespace DT.Application.Filters
{
    /// <summary>
    /// Интерфейс фильтра
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Количесвто элементов на странице
        /// </summary>
        int PageSize { get; set;  }

        /// <summary>
        /// Номер страницы
        /// </summary>
        int PageNumber {  get; set; }

        /// <summary>
        /// Массив строк вида "PropertyName,asc" или "PropertyName,desc"
        /// Пример: new[] { "Status,asc", "CreatedAt,desc" }
        /// </summary>
        public string[]? Sort { get; set; }

        /// <summary>
        /// Допустимые поля сортировки
        /// </summary>
        public Dictionary<string, string> SortedFields { get; }

        /// <summary>
        /// Валидатор параметров
        /// </summary>
        void Validate();
    }
}
