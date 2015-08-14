 
using System.Configuration;
 
namespace Coax.General
{
    public sealed class Config
    {
        public static string DbConnectionStringName
        {
            get
            {
                return ConfigurationManager.AppSettings["ConnectionStringName"] ?? "DbConnection";
            }
        }

        public static string DbConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[DbConnectionStringName].ConnectionString;
            }
        }
    }
}
