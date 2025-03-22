using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Recycle.Api.Services
{
    /// <summary>
    /// Defines operations for saving and deleting uploaded images.
    /// </summary>
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string folderName);
        Task<bool> DeleteImageAsync(string filePath);
    }

    /// <summary>
    /// Provides methods to handle saving and deleting image files on disk.
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly string _baseUploadsFolder = @"C:\Elareinstaluje\repos\RecycleApi\Recycle\Uploads";

        /// <summary>
        /// Saves the uploaded image to a specified folder and returns its relative path.
        /// </summary>
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

        /// <summary>
        /// Deletes an image file based on its relative path.
        /// </summary>
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
