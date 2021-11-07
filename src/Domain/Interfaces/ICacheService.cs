using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetCacheValueAsync(string key);
        Task SetCacheValueAsync(string key, object value);
    }
}