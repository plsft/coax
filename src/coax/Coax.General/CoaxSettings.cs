using System;
using Helix.Utility;

namespace Coax.General
{
    public sealed class CoaxSettings
    {
        public static string ApplicationName
        {
            get { return ""; }
        }

        public static string Culture
        {
            get { return Settings.Get("culture_lang").DefaultValue("en-US"); }
        }

        public static string MailServer
        {
            get
            {
                return Settings.Get("mail_server").DefaultValue("localhost");
            }
        }

        public static bool MailUseAuthFlag
        {
            get
            {
                return Settings.Get("mail_use_auth_flag").DefaultValue("true") == "true";
            }
        }

        public static string MailServerUsername
        {
            get { return Settings.Get("mail_username").DefaultValue("mailer"); }
        }

        public static string MailServerPwd
        {
            get { return Settings.Get("mail_username").DefaultValue("mailer"); }
        }

        public static int MailServerPort
        {
            get { return Convert.ToInt16(Settings.Get("mail_port")); }
        }

        public static string DefaultFromEmail
        {
            get { return Settings.Get("default_email");  }
        }
    }
}
