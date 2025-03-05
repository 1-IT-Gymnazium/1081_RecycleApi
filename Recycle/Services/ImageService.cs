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
        private readonly string _baseUploadsFolder = @"C:\Elareinstaluje\repos\RecycleApi\Recycle\Uploads";

        public async Task<string> SaveImageAsync(IFormFile image, string folderName)
        {
            if (image == null || image.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_baseUploadsFolder, folderName);
            Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

            // Generate a unique file name
            var fileExtension = Path.GetExtension(image.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the image
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // Return the relative path
            return $"/Uploads/{folderName}/{uniqueFileName}";
        }

        public async Task<bool> DeleteImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var fullFilePath = Path.Combine(_baseUploadsFolder, filePath.Replace("/Uploads/", ""));
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                return true;
            }
            return false;
        }
    }
}
