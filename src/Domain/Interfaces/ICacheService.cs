using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICacheService
    {
        public enum Entity
        {
            Users
        }

        public enum Type
        {
            FindOne,
            FindAll,
            Create,
            Update
        }

        Task<string> GetCacheValueAsync(Entity entity, Type type);
        Task SetCacheValueAsync(Entity entity, Type type, object value);

        /// <summary>
        ///     Creates a key based on the input parameters to ensure consistency
        /// </summary>
        /// <param name="entity">Type of object being looked up</param>
        /// <param name="type">Type of look up</param>
        /// <param name="id">The id (can be left null for a <c>findAll</c> type)</param>
        /// <returns></returns>
        string CreateKey(Entity entity, Type type, int? id = null);
    }
}