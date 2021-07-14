using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Mesawer.InfrastructureLayer.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IStorageLocationService _storageLocation;

        public LocalStorageService(IStorageLocationService storageLocation) => _storageLocation = storageLocation;

        public async Task<string> SaveAsync(string type, byte[] file, string fileName = null)
        {
            var directory = _storageLocation.GetPath(type);

            Directory.CreateDirectory(directory);

            var saveName = fileName ?? Guid.NewGuid().ToString("N") + ".txt";

            var path = Path.Combine(directory, saveName);

            await using var fileStream = File.Create(path);
            await fileStream.WriteAsync(file);

            return saveName;
        }

        public Task<(string saveName, string displayName)> SaveAsync(string type, IFormFile file)
            => SaveAsync(file, _storageLocation.GetPath(type));

        public Task DeleteAsync(string type, string fileName)
            => Task.Run(() => Delete(_storageLocation.GetPath(type), fileName));

        public Task<bool> DeleteIfExistsAsync(string type, string fileName)
            => Task.Run(() => DeleteIfExists(_storageLocation.GetPath(type), fileName));

        public Task<byte[]> DownloadAsync(string type, string fileName)
        {
            var path = Path.Combine(_storageLocation.GetPath(type), fileName);

            return File.Exists(path) ? File.ReadAllBytesAsync(path) : throw new FileNotFoundException();
        }

        private static void Delete(string directory, string fileName) => File.Delete(Path.Combine(directory, fileName));

        public static bool DeleteIfExists(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);

            if (!File.Exists(path)) return false;

            File.Delete(path);

            return true;
        }

        private static async Task<(string saveName, string displayName)> SaveAsync(IFormFile formFile, string directory)
        {
            var untrustedFileName = Path.GetFileName(formFile.Name);
            var ext               = $".{formFile.ContentType.Split("/").Last().Split('+').First()}";

            if (string.IsNullOrWhiteSpace(directory) ||
                string.IsNullOrWhiteSpace(untrustedFileName) ||
                formFile.Length == 0)
                throw new ArgumentException();

            // Create the directory if not exist 
            Directory.CreateDirectory(directory);

            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(formFile.FileName) + ext;

            var saveName = Guid.NewGuid().ToString("N") + ext;

            var path = Path.Combine(directory, saveName);

            await using var fileStream = File.Create(path);
            await formFile.CopyToAsync(fileStream);

            return (saveName, trustedFileNameForDisplay);
        }
    }
}
