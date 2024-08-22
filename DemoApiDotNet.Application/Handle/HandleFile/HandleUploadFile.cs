using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Application.Handle.HandleFile
{
    public class HandleUploadFile
    {
        // lưu file vào trong project
        // lưu file trong nguồn thứ 3: lưu trong cloudinary
        public static async Task<string> WriteFile(IFormFile file)
        {
            string fileName = "";
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = "MyBugs_" + DateTime.Now.Ticks + extension;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Files");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                var exactPath = Path.Combine(filePath, fileName);
                using(var stream = new FileStream(exactPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }catch (Exception ex)
            {
                throw;
            }
            return fileName;
        }
    }
}
