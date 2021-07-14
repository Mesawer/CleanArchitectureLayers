using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface IStorageService
    {
        Task<string> SaveAsync(string type, byte[] file, string fileName = null);

        Task<(string saveName, string displayName)> SaveAsync(string type, IFormFile file);

        Task DeleteAsync(string type, string fileName);

        Task<bool> DeleteIfExistsAsync(string type, string fileName);

        Task<byte[]> DownloadAsync(string type, string fileName);
    }
}
