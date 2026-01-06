using Apps.AdminPanel.DBconection;
using Apps.AdminPanel.Models;
using System.Linq;

namespace Apps.AdminPanel.Repositories
{
    public class ContentRepository
    {
        // دالة لحفظ الكورس
        public int SaveCourse(Course course)
        {
            using (var db = new AppDbContext())
            {
                db.Course.Add(course);
                db.SaveChanges();
                return course.Id;
            }
        }

        // دالة لحفظ الفيديو
        public void SaveAsset(Asset asset)
        {
            using (var db = new AppDbContext())
            {
                db.Assets.Add(asset);
                db.SaveChanges();
            }
        }

        // مستقبلاً يمكنك إضافة دوال مثل:
        // GetAllCourses()
        // DeleteVideo(int id)
    }
}