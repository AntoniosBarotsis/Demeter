using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public interface IDatabaseSeeder
    {
        public void Initialize();
        public void Seed();
    }
}