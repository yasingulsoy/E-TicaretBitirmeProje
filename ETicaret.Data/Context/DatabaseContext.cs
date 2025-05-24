 using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ETicaret.Core.Entities;
using ETicaret.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Data.Context
{
   public  class DatabaseContext:DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Adress> Adresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)//Veri tabanı bağlantısı yaptığımız yer
        {
            optionsBuilder.UseSqlServer(@"Server=YASIN\SQLEXPRESS;Database=ETicaretProje;Trusted_Connection=True;TrustServerCertificate=True;");
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tüm config class'larını yüklemeye devam
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Burada decimal alanlara precision veriyoruz
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasPrecision(18, 4);

            modelBuilder.Entity<OrderLine>()
                .Property(ol => ol.UnitPrice)
                .HasPrecision(18, 4);

            base.OnModelCreating(modelBuilder);
        }

    }
}
