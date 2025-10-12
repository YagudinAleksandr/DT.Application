namespace DT.Application.Extensions
{
    /// <summary>
    /// Дескриптор сортировки
    /// </summary>
    public class SortDescriptor
    {
        /// <summary>
        /// Свойство сортировки
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Направление сортировки обратное
        /// </summary>
        public bool IsDescending { get; }

        public SortDescriptor(string propertyName, bool isDescending)
        {
            PropertyName = propertyName;
            IsDescending = isDescending;
        }
    }
}
