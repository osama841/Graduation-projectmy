using System;
using System.IO;
using Apps.AdminPanel.Models; // استدعاء المودل

namespace Apps.AdminPanel.Services
{
    public interface IFileHandlingService
    {
        DrmFile CreateFileModel(string filePath);
        string GetReadableFileSize(long bytes);
    }

    public class FileHandlingService : IFileHandlingService
    {
        public DrmFile CreateFileModel(string filePath)
        {
            var info = new FileInfo(filePath);

            return new DrmFile
            {
                Name = info.Name,
                Path = info.FullName,
                Size = info.Length, // الحجم بالبايت، سنحوله لنص لاحقاً
                ModifiedDate = info.LastWriteTime,
                IsEncrypted = false, // افتراضياً غير مشفر
                Icon = DetermineIcon(info.Extension)
            };
        }

        // دالة مساعدة لتحديد الأيقونة بناءً على الامتداد
        private string DetermineIcon(string extension)
        {
            extension = extension.ToLower();
            if (extension.Contains("pdf")) return "FilePdfBox";
            if (extension.Contains("doc")) return "FileWordBox";
            if (extension.Contains("xls")) return "FileExcelBox";
            if (extension.Contains("mp4") || extension.Contains("avi")) return "FileVideo";
            if (extension.Contains("png") || extension.Contains("jpg")) return "FileImage";
            return "FileDocumentOutline"; // أيقونة افتراضية
        }

        // دالة لتحويل الحجم من بايت إلى MB/KB
        public string GetReadableFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (double)bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
