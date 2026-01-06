using Apps.AdminPanel.Models;
using Apps.AdminPanel.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace Apps.AdminPanel.DBconection
{
    class AppDbContext : DbContext
    {
        // تعريف الجداول
        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Course> Course { get; set; }

        // إعداد الاتصال
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // هذا السطر يحدد اسم ملف قاعدة البيانات
            // سيتم إنشاؤه بجانب ملف التشغيل exe
            optionsBuilder.UseSqlite("Data Source=SystemDB.db");
        }
    }
}