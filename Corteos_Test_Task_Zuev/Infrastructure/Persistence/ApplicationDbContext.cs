using Corteos_Test_Task_Zuev.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Corteos_Test_Task_Zuev.Infrastructure.Persistence
{
    /// <summary>
    /// Контекст EF Core для работы с PostgreSQL.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CurrencyRate>(entity =>
            {
                // Указываем имя таблицы
                entity.ToTable("currency_rates");

                // Настройка составного первичного ключа (3НФ: Валюта + Дата)
                entity.HasKey(e => new { e.CurrencyId, e.Date });

                // Явное указание типов данных под PostgreSQL
                entity.Property(e => e.CurrencyId).HasColumnName("currency_id").HasMaxLength(10);
                entity.Property(e => e.CharCode).HasColumnName("char_code").HasMaxLength(5).IsRequired();
                entity.Property(e => e.NumCode).HasColumnName("num_code").HasMaxLength(5).IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();

                // Используем тип date в PostgreSQL вместо timestamp, так как время нам не нужно
                entity.Property(e => e.Date).HasColumnName("date").HasColumnType("date");
                entity.Property(e => e.Nominal).HasColumnName("nominal").IsRequired();

                // Точность для финансовых данных (например: 123.4567)
                entity.Property(e => e.Value).HasColumnName("value").HasColumnType("numeric(18,4)").IsRequired();
            });
        }
    }
}
