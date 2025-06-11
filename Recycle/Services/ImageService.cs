using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Recycle.Api.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string folderName);
        Task<bool> DeleteImageAsync(string filePath);
    }

    public class ImageService : IImageService
    {
        private readonly string _baseUploadsFolder;

        public ImageService(IWebHostEnvironment env)
        {
            _baseUploadsFolder = Path.Combine(env.WebRootPath, "Uploads");
        }

        public async Task<string> SaveImageAsync(IFormFile image, string folderName)
        {
            if (image == null || image.Length == 0)
                return null;

            var folderPath = Path.Combine(_baseUploadsFolder, folderName);
            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(image.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return web-safe relative path
            return $"/Uploads/{folderName}/{uniqueFileName}";
        }

        public async Task<bool> DeleteImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var relativePath = filePath.Replace("/Uploads/", "");
            var fullFilePath = Path.Combine(_baseUploadsFolder, relativePath);

            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                return true;
            }

            return false;
        }
    }
}
