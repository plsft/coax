using Coax.Data.Models.ModelContainer;

using Coax.Data.Repository.RepositoryContainer;

namespace Coax.Data.Controllers
{
    public class ControllerContainer
    {
        public sealed partial class MarketerController : GenericControllerBase<Marketer>, IController<Marketer>
        {
            public MarketerController() : base(new RepositoryContainer.MarketerRepository(), "Marketers")
            {
            }
        }

        public sealed partial class AccountTypeController : GenericControllerBase<AccountType>, IController<AccountType>
        {
            public AccountTypeController() : base(new RepositoryContainer.AccountTypeRepository(), "AccountTypes")
            {
            }
        }

        public sealed partial class CategorieController : GenericControllerBase<Category>, IController<Category>
        {
            public CategorieController() : base(new RepositoryContainer.CategorieRepository(), "Categories")
            {
            }
        }

        public sealed partial class DomainController : GenericControllerBase<Domain>, IController<Domain>
        {
            public DomainController() : base(new RepositoryContainer.DomainRepository(), "Domains")
            {
            }
        }

        public sealed partial class DeviceController : GenericControllerBase<Device>, IController<Device>
        {
            public DeviceController() : base(new RepositoryContainer.DeviceRepository(), "Devices")
            {
            }
        }

        public sealed partial class UserController : GenericControllerBase<User>, IController<User>
        {
            public UserController() : base(new RepositoryContainer.UserRepository(), "Users")
            {
            }
        }

        public sealed partial class OfferTypeController : GenericControllerBase<OfferType>, IController<OfferType>
        {
            public OfferTypeController() : base(new RepositoryContainer.OfferTypeRepository(), "OfferTypes")
            {
            }
        }

        public sealed partial class OfferController : GenericControllerBase<Offer>, IController<Offer>
        {
            public OfferController() : base(new RepositoryContainer.OfferRepository(), "Offers")
            {
            }
        }

        public sealed partial class OfferAttributeController : GenericControllerBase<OfferAttribute>, IController<OfferAttribute>
        {
            public OfferAttributeController() : base(new RepositoryContainer.OfferAttributeRepository(), "OfferAttributes")
            {
            }
        }

        public sealed partial class ImpressionController : GenericControllerBase<Impression>, IController<Impression>
        {
            public ImpressionController() : base(new RepositoryContainer.ImpressionRepository(), "Impressions")
            {
            }
        }

        public sealed partial class ForwardController : GenericControllerBase<Forward>, IController<Forward>
        {
            public ForwardController() : base(new RepositoryContainer.ForwardRepository(), "Forwards")
            {
            }
        }

        public sealed partial class MuteController : GenericControllerBase<Mute>, IController<Mute>
        {
            public MuteController() : base(new RepositoryContainer.MuteRepository(), "Mutes")
            {
            }
        }

        public sealed partial class ActionController : GenericControllerBase<Models.ModelContainer.Action>, IController<Models.ModelContainer.Action>
        {
            public ActionController() : base(new RepositoryContainer.ActionRepository(), "Actions")
            {
            }
        }


        public sealed partial class AdminController : GenericControllerBase<Admin>, IController<Admin>
        {
            public AdminController() : base(new RepositoryContainer.AdminRepository(), "Admins")
            {
            }
        }

        public sealed partial class AuditController : GenericControllerBase<Audit>, IController<Audit>
        {
            public AuditController() : base(new RepositoryContainer.AuditRepository(), "Audits")
            {
            }
        }

        public sealed partial class ResponseController : GenericControllerBase<Response>, IController<Response>
        {
            public ResponseController() : base(new RepositoryContainer.ResponseRepository(), "Responses")
            {
            }
        }

        public sealed partial class ProviderSettingController : GenericControllerBase<ProviderSetting>, IController<ProviderSetting>
        {
            public ProviderSettingController() : base(new RepositoryContainer.ProviderSettingRepository(), "ProviderSettings")
            {
            }
        }

        public sealed partial class CredentialController : GenericControllerBase<Credential>, IController<Credential>
        {
            public CredentialController() : base(new RepositoryContainer.CredentialRepository(), "Credentials")
            {
            }
        }

        public sealed partial class EmailController : GenericControllerBase<Email>, IController<Email>
        {
            public EmailController() : base(new RepositoryContainer.EmailRepository(), "Emails")
            {
            }
        }

        public sealed partial class HeaderController : GenericControllerBase<Header>, IController<Header>
        {
            public HeaderController() : base(new RepositoryContainer.HeaderRepository(), "Headers")
            {
            }
        }

    }
}
