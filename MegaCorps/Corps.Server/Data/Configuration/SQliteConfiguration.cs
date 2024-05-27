using Microsoft.EntityFrameworkCore;

namespace Corps.Server.Data.Configuration
{
    public class SQliteConfiguration(string connectionString, bool isDebugMode) : ContextConfiguration(connectionString, isDebugMode)
    {
        /// <summary>
        ///     Тип полей даты и времени в базе данных.
        /// </summary>
        internal override string DateTimeType => "TEXT";

        /// <summary>
        ///     Указатель использования текущих даты и времени
        ///     для полей типа <see cref="DateTimeType" /> в базе данных.
        /// </summary>
        internal override string DateTimeValueCurrent => "CURRENT_TIMESTAMP";

        /// <summary>
        ///     Применить настройки к сессии.
        /// </summary>
        /// <param name="optionsBuilder">Набор интерфейсов настройки сессии.</param>
        public override void ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);

            base.ConfigureContext(optionsBuilder);
        }
    }
}
