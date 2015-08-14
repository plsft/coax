 
using Coax.Data.Infra;

namespace Coax.Data.Repository
{
    public interface IGenericRepository<T> where T : class, ICoaxRepository
    {
    }
}
