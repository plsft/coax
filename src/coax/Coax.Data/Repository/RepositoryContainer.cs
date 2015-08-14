using Helix.Infra.Peta;
using Coax.Data.Infra;

namespace Coax.Data.Repository.RepositoryContainer
{
    public sealed class RepositoryContainer
    {
        [TableName("Marketer")]
        public sealed class MarketerRepository : CoaxRepository, IRepository
        {
        }

        [TableName("AccountType")]
        public sealed class AccountTypeRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Categorie")]
        public sealed class CategorieRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Domain")]
        public sealed class DomainRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Device")]
        public sealed class DeviceRepository : CoaxRepository, IRepository
        {
        }

        [TableName("User")]
        public sealed class UserRepository : CoaxRepository, IRepository
        {
        }

        [TableName("OfferType")]
        public sealed class OfferTypeRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Offer")]
        public sealed class OfferRepository : CoaxRepository, IRepository
        {
        }

        [TableName("OfferAttribute")]
        public sealed class OfferAttributeRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Impression")]
        public sealed class ImpressionRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Forward")]
        public sealed class ForwardRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Mute")]
        public sealed class MuteRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Action")]
        public sealed class ActionRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Users_Offer")]
        public sealed class Users_OfferRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Actions_Users_Offer")]
        public sealed class Actions_Users_OfferRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Admin")]
        public sealed class AdminRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Audit")]
        public sealed class AuditRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Response")]
        public sealed class ResponseRepository : CoaxRepository, IRepository
        {
        }

        [TableName("ProviderSetting")]
        public sealed class ProviderSettingRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Credential")]
        public sealed class CredentialRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Email")]
        public sealed class EmailRepository : CoaxRepository, IRepository
        {
        }

        [TableName("Header")]
        public sealed class HeaderRepository : CoaxRepository, IRepository
        {
        }
    }
}
