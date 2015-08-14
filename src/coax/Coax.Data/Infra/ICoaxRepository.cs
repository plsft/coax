
using Helix.Data;

namespace Coax.Data.Infra
{
    public interface ICoaxRepository : IHelixPetaRepository
    {
         void Dispose();
    }
}
