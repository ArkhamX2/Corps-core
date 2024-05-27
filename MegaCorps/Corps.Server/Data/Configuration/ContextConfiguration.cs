
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Corps.Server.Data.Configuration
{
    public abstract class ContextConfiguration(string connectionString, bool isDebugMode)
    {
        /// <summary>
        ///     Строка подключения к базе данных.
        /// </summary>
        protected string ConnectionString { get; } = connectionString;

        /// <summary>
        ///     Статус конфигурации для разработки.
        /// </summary>
        private bool IsDebugMode { get; } = isDebugMode;

        /// <summary>
        ///     Тип полей даты и времени в базе данных.
        /// </summary>
        internal abstract string DateTimeType { get; }

        /// <summary>
        ///     Указатель использования текущих даты и времени
        ///     для полей типа <see cref="DateTimeType" /> в базе данных.
        /// </summary>
        internal abstract string DateTimeValueCurrent { get; }

        /// <summary>
        ///     Применить настройки к сессии.
        /// </summary>
        /// <param name="optionsBuilder">Набор интерфейсов настройки сессии.</param>
        public virtual void ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        {
            if (!IsDebugMode) return;

            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.ConfigureWarnings(builder => builder.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }
}
