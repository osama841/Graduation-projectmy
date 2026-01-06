using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Apps.AdminPanel.Helpers
{
    public class FileHelper
    {
        public static  string getNewName()
        {
            return "Course" + Guid.NewGuid().ToString().Substring(0, 8);
        }
        public static  string GetSafeFilename(string filename)
        {
            filename = filename.Replace(" ", "_");
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string cleanName = string.Join("_", filename.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return cleanName;
        }
    }
}
