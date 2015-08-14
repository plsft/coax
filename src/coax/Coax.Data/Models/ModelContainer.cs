using System;
using Helix.Infra.Peta;

namespace Coax.Data.Models.ModelContainer
{

    [TableName("Marketers")]
    public sealed class Marketer
    {
        public int ID { get; set; }
        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string WorkPhone { get; set; }
        public string CellPhone { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }
        public DateTime LastPwdChange { get; set; }
        public int? Status { get; set; }
        public int? AccountTypeId { get; set; }
        public int? CategoryId { get; set; }
        public Guid? UUID { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }
        public bool? Verified { get; set; }
    }



    [TableName("AccountTypes")]
    public sealed class AccountType
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



    [TableName("Categories")]
    public sealed class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



    [TableName("Domains")]
    public sealed class Domain
    {
        public int ID { get; set; }

        [Column("Domain")]
        public string DomainName { get; set; }

        public int? MarketerId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime LastSeenOn { get; set; }
        public DateTime Created { get; set; }
    }



    [TableName("Devices")]
    public sealed class Device
    {
        public int ID { get; set; }
        public string DeviceFingerPrint { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastSeenOn { get; set; }
    }



    [TableName("Users")]
    public sealed class User
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Zip { get; set; }
        public string Email { get; set; }
        public int? Age { get; set; }
        public string CellPhone { get; set; }
        public string Username { get; set; }
        public string Pwd { get; set; }
        public DateTime LastPwdChange { get; set; }
        public string FbId { get; set; }
        public string TwitterId { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime Created { get; set; }
        public int? Status { get; set; }
        public Guid? UUID { get; set; }
        public int? DeviceId { get; set; }
        public bool? Enabled { get; set; }
        public string Token { get; set; }
        public string Auth { get; set; }
        public bool? LoggedIn { get; set; }
    }



    [TableName("OfferTypes")]
    public sealed class OfferType
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



    [TableName("Offers")]
    public sealed class Offer
    {
        public int ID { get; set; }
        public int? OfferTypeId { get; set; }
        public int? MarketerId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
    }



    [TableName("OfferAttributes")]
    public sealed class OfferAttribute
    {
        public int ID { get; set; }
        public int? OfferId { get; set; }
        public float? Longitude { get; set; }
        public float? Latitude { get; set; }
        public string LocalPhoneNumber { get; set; }
        public string LocalAddress { get; set; }
        public string LocalCity { get; set; }
        public string LocalState { get; set; }
        public string LocalZip { get; set; }
        public string MetaData { get; set; }
        public string CallFwdNumber { get; set; }
        public string LandingUrl { get; set; }
        public bool? AllowForward { get; set; }
        public bool? AllowCall { get; set; }
        public bool? AllowPurchase { get; set; }
    }



    [TableName("Impressions")]
    public sealed class Impression
    {
        public int ID { get; set; }
        public int? OfferId { get; set; }
        public int? UserId { get; set; }
        public int? MarketerId { get; set; }
        public DateTime Created { get; set; }
    }



    [TableName("Forwards")]
    public sealed class Forward
    {
        public int ID { get; set; }
        public int? OfferId { get; set; }
        public int? MarketerId { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string ToEmail { get; set; }
    }



    [TableName("Mutes")]
    public sealed class Mute
    {
        public int ID { get; set; }
        public int? UserId { get; set; }
        public int? MarketerId { get; set; }
    }



    [TableName("Actions")]
    public sealed class Action
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }



    [TableName("Admins")]
    public sealed class Admin
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Pwd { get; set; }
        public DateTime Created { get; set; }
        public bool? Enabled { get; set; }
        public string AllowedIP { get; set; }
    }



    [TableName("Audits")]
    public sealed class Audit
    {
        public int ID { get; set; }
        public string ObjectType { get; set; }
        public int? ObjectId { get; set; }
        public string Src { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string SrcIP { get; set; }
        public DateTime Created { get; set; }
    }



    [TableName("Responses")]
    public sealed class Response
    {
        public int ID { get; set; }
        public int? Timestamp { get; set; }
        public Guid? RequestId { get; set; }
        public string Server { get; set; }
        public int? Code { get; set; }
        public int? Records { get; set; }
        public bool? Status { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public decimal? TransactionTime { get; set; }
    }



    [TableName("ProviderSettings")]
    public sealed class ProviderSetting
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public string ServerType { get; set; }
        public int? Port { get; set; }
        public int? Timeout { get; set; }
    }



    [TableName("Credentials")]
    public sealed class Credential
    {
        public int ID { get; set; }
        public int? UserId { get; set; }
        public int? ProviderSettingId { get; set; }
        public string Login { get; set; }
        public string Pwd { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastLogin { get; set; }
        public int? Status { get; set; }
        public bool? Enabled { get; set; }
    }



    [TableName("Emails")]
    public sealed class Email
    {
        public int ID { get; set; }
        public int? UserId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime SentOn { get; set; }
        public string MessageID { get; set; }
        public int? Status { get; set; }
        public DateTime Created { get; set; }
    }



    [TableName("Headers")]
    public sealed class Header
    {
        public int ID { get; set; }
        public int? EmailId { get; set; }
        public string Keyword { get; set; }
        public string Value { get; set; }
    }
}
