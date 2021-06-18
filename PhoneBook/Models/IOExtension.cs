using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Models
{
    public class IOExtension
    {
        public static void CopyFileToPath(string path, IFormFile file)
        {
            if (Directory.Exists(path))
            {
                string filePath = Path.Combine(path, file.FileName);
                file.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            else
            {
                Directory.CreateDirectory(path);
                string filePath = Path.Combine(path, file.FileName);
                file.CopyTo(new FileStream(filePath, FileMode.Create));
            }
        }


        public static void ConvertBase64ToImageAndWriteToPath(string base64, string fileName, string path)
        {
            Byte[] bytes = Convert.FromBase64String(base64);
            System.IO.File.WriteAllBytes(path + "\\" + fileName, bytes);
        }

        public static string ConvertToBase64(string fileName, string path)
        {
            string fullPath = path+"\\" + fileName;
            byte[] b = System.IO.File.ReadAllBytes(fullPath);
            return  Convert.ToBase64String(b);
        }

    }
}
