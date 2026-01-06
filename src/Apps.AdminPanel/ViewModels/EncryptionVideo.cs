using Apps.AdminPanel.Models;
using Apps.AdminPanel.Repositories;
using Apps.AdminPanel.Services;
using Apps.AdminPanel.Helpers;
using DRM.Shared.Security;
using System.IO;
using System;

namespace Apps.AdminPanel.ViewModels
{
    public class AddVideoViewModel
    {
        private EncryptionService _encryptionService = new EncryptionService();
        private ContentRepository _contentRepository = new ContentRepository();
        private string _sorcebace = "C:\\MyProject\\Content";
        private byte[] _keyAES;
        private int _currentCourseId = 0;
        private string _currentCoursePath = "";

        // المتغيرات العامة كما في كودك
        public bool IsCourse { get; set; }
        public string SelectedFile { get; set; }
        public string[] SelectedFilesList { get; set; }
        public string CourseTitle { get; set; }
       

       

        public AddVideoViewModel()
        {
            _keyAES = getKey();
        }

        private byte[] getKey()
        {
            return SecureVideoEngine.GenerateRandomBytes(32);
        }

        // دالة الزر الرئيسية
        public void Execute()
        {
            if (IsCourse)
            {
                ProcessCourseFolder(SelectedFilesList);
            }
            else
            {
                _currentCourseId = 0;
                _currentCoursePath = "";
                ProcessSingleVideo(SelectedFile);
            }
        }

        private void ProcessCourseFolder(string[] videoFiles=null)
        {
            // 1. ضبط اسم المجلد وإنشاؤه
            safeCourseName();

            // 2. إنشاء الكورس في الداتا بيز
            var newCourse = new Course
            {
                Title = CourseTitle,
                IsPublished = false
            };

            // تخزين الـ ID في المتغير العام ليراه الجميع
            _currentCourseId = _contentRepository.SaveCourse(newCourse);

            // ملاحظة: هنا سنستخدم keyAES الذي تم توليده في الكونستركتور كمفتاح مشترك

            // 3. الدوران على الملفات
            foreach (string file in SelectedFilesList)
            {
                ProcessSingleVideo(file);
            }
        }

        private void ProcessSingleVideo(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string filenameClean = FileHelper.GetSafeFilename(fileName);

            // IV مختلف لكل ملف (ضروري)
            byte[] uniqueIV = SecureVideoEngine.GenerateRandomBytes(16);

            // هنا نستخدم keyAES (المشترك) و uniqueIV (الفريد)
            EncryptAndSaveAsset(filePath, filenameClean, uniqueIV);
        }

        private void EncryptAndSaveAsset(string inputPath, string filename, byte[] iv)
        {
            string finalEncPath = "";

            if (IsCourse)
            {
                // نستخدم المسار الذي تم تحديده في دالة safeCourseName
                finalEncPath = Path.Combine(_currentCoursePath, filename + ".enc");
            }
            else
            {
                // حفظ بجانب الملفات في المجلد الرئيسي
                finalEncPath = Path.Combine(_sorcebace, filename + ".enc");
            }

            // التشفير
         //   _encryptionService.EncryptVideoFile(inputPath, finalEncPath, _keyAES, iv);

            // الحفظ (ستقرأ keyAES و _currentCourseId من الكلاس مباشرة)
            SaveAssetToDatabase(filename, finalEncPath, _keyAES, iv);
        }

        // (إصلاح): الدالة الآن تنشئ المجلد وتحتفظ بمساره في المتغير العام
        public void safeCourseName()
        {
            if (CourseTitle=="")
            {
                CourseTitle = FileHelper.getNewName();
            }
            string safeTitle = FileHelper.GetSafeFilename(CourseTitle);
            int counter = 0;
            string puthDir = Path.Combine(_sorcebace, safeTitle);

            // التأكد من عدم التكرار
            string tempDir = puthDir;
            while (Directory.Exists(tempDir))
            {
                counter++;
                tempDir = $"{puthDir}_{counter}";
            }

            puthDir = tempDir; // الاسم النهائي

            // إنشاء المجلد فعلياً
            Directory.CreateDirectory(puthDir);

            // حفظ المسار لنستخدمه في التشفير
            _currentCoursePath = puthDir;
        }

        private void SaveAssetToDatabase(string title, string encPath, byte[] key, byte[] iv)
        {
            var securityRecord = new AssetSecurity
            {
                MasterKey = key,
                IV = iv,
                EncryptionAlgo = "AES-256-CTR",
                Checksum = "MD5"
            };

            var assetRecord = new Asset
            {
                Title = title,
                EncryptedFilePath = encPath,
                AssetType = "Video",
                // هنا نستخدم المتغير العام الذي ضبطناه سابقاً
                CourseId = _currentCourseId,
                Security = securityRecord
            };

            using (var db = new DBconection.AppDbContext())
            {
                db.Assets.Add(assetRecord);
                db.SaveChanges();
            }
        }

    }
}