using System.Net;
using System.Net.Mail;
using System.Text;

namespace Helix.Utility
{
    public sealed class Mailer
    {
        public sealed class MailerServerSettings
        {
            public string Server { get; set; }
            public int Port { get; set; } = 587;
            public string Username { get; set; }
            public string Pwd { get; set; }

            public MailerServerSettings()
            {
            }

            public MailerServerSettings(string server, int port, string username, string pwd)
            {
                Server = server;
                Port = port;
                Username = username;
                Pwd = pwd;
            }

        }

        public sealed class MailerMessage
        {
            public string To { get; set; }
            public string From { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string[] Attachments { get; set; }

            public string[] Cc { get; set; }
            public string[] Bcc { get; set; }


            public MailerMessage()
            {
            }

            public MailerMessage(string to, string from, string subject, string body, string[] attachements, string[] cc, string[] bcc)
            {
                To = to;
                From = from;
                Subject = subject;
                Body = body;
                Attachments = attachements;
                Cc = cc;
                Bcc = bcc;
            }
        }

        private static readonly object o;
        static Mailer()
        {
            o = new object();
        }

        public static bool SendEmail(MailerServerSettings settings, MailerMessage message)
        {

            lock (o)
            {
                var msg = new MailMessage(message.From, message.To)
                {
                    Body = message.Body,
                    Subject = message.Subject,
                    Priority = MailPriority.Normal,
                    IsBodyHtml = message.Body.Contains("<") || message.Body.Contains(">"),
                    BodyEncoding = Encoding.UTF8
                };

                if ((message.Cc != null) && message.Cc.Length != 0)
                {
                    foreach (var c in message.Cc)
                        msg.CC.Add(c);
                }

                if ((message.Bcc != null) && message.Bcc.Length != 0)
                {
                    foreach (var bc in message.Bcc)
                        msg.Bcc.Add(bc);
                }

                if (message.Attachments != null)
                    foreach (var a in message.Attachments)
                        msg.Attachments.Add(new Attachment(a));


                using (var smtp = new SmtpClient())
                {
                    smtp.Host = settings.Server;
                    smtp.UseDefaultCredentials = false;
                    smtp.Timeout = 30000;
                    smtp.EnableSsl = false;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(settings.Username, settings.Pwd);
                    smtp.Port = settings.Port;
                    smtp.Send(msg);
                }

                return true;
            }
        }
    }
}
