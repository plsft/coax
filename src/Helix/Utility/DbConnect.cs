using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml;
using Helix.Json;
using Helix.Security;
using Helix.Xml;

namespace Helix.Utility
{
    public sealed class DbConnect
    {
        private const string DefaultConnectionStringName = "DbConnection";

        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }


        public DbConnect(string connectionStringName)
            : this()
        {
            ConnectionStringName = connectionStringName;
        }

        public DbConnect()
        {
            if (string.IsNullOrEmpty(ConnectionStringName))
                ConnectionStringName = DefaultConnectionStringName;

            ValidationConnectionString();
        }

        public SqlConnection GetSqlConnection(string connectionStringName = "")
        {
            if (!string.IsNullOrEmpty(connectionStringName))
                ConnectionStringName = connectionStringName;

            try
            {
                return new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString);
            }
            catch
            {
                return new SqlConnection(ConnectionString);
            }
        }

        public string GetSqlConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;
        }

        internal void ValidationConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings[ConnectionStringName] == null)
            {
                var appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

                if (appConfig.AppSettings.Settings["ConnectionStringName"] == null)
                    throw new ArgumentException("Unable to load app.config from " + Assembly.GetExecutingAssembly().Location + "; looked for "
                        + ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location).FilePath);

                ConnectionStringName = appConfig.AppSettings.Settings["ConnectionStringName"].Value;
                ConnectionString = appConfig.ConnectionStrings.ConnectionStrings[ConnectionStringName].ConnectionString;

                if (string.IsNullOrEmpty(ConnectionString))
                    throw new ArgumentNullException("DbConnect cannot file connection string named ['" + ConnectionStringName + "'] in configuration file(s).");
            }
        }

        public static SqlConnection GetLegacyConnection(bool useConnectionStringProperty = false, string connectionStringName = "")
        {
            var str1 = "";
            var str2 = ConfigurationManager.AppSettings["HelixConn"] ?? ConfigurationManager.AppSettings["PluralConn"];
            var val = ConfigurationManager.AppSettings["ConnCredentials"] ?? "";

            if (useConnectionStringProperty)
            {
                if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
                    return new SqlConnection(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);

                throw new Exception("Unable to find connection string: " + connectionStringName);
            }

            if ((string.IsNullOrEmpty(val) && !string.IsNullOrEmpty(str2)))
                throw new Exception("Error! Missing 'ConnectionString' entry in app or web.config.");

            if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(val))
                str1 = Crypto.Decrypt(val);

            return !string.IsNullOrEmpty(str1) ? new SqlConnection(str2 + str1) : new SqlConnection(GetConnectionStringFromFile());
        }


        public static string GetConnectionStringFromFile(bool useConnectionStringProperty = false, string connectionStringName = "")
        {
            var directoryRunning = AppDomain.CurrentDomain.BaseDirectory;
            const string defaultConfigPath = "C:\\";
            var file = string.Empty;
            if (useConnectionStringProperty)
            {
                if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
                    return (ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);

                throw new Exception("Unable to find connection string: " + connectionStringName);
            }

            if (File.Exists(Path.Combine(directoryRunning, "Helix.config")))
                file = FileOps.Loadfile(Path.Combine(directoryRunning, "Helix.config"));
            else if (File.Exists(Path.Combine(defaultConfigPath, "Helix.config")))
                file = FileOps.Loadfile(Path.Combine(defaultConfigPath, "Helix.config"));
            else
                throw new Exception("Error: unable to find 'Helix.config' in working directory: " + directoryRunning + " or default path " + defaultConfigPath);

            var xml = new XmlDocument();
            xml.LoadXml(file);

            dynamic config = DynamicJson.Parse(XmlToJson.Convert(xml.InnerXml));

            if (config == null)
                throw new Exception("Error: unable to parse 'Helix.config' in working directory: " + directoryRunning);

            return string.Format("server={0};database={1};uid={2};password={3};Max Pool Size={5};Connection Timeout={4};",
                config.Helix.connection.dbserver, config.Helix.connection.dbname, config.Helix.connection.dbuser,
                config.Helix.connection.dbpwd, config.Helix.connection.dbtimeout, config.Helix.connection.dbmaxpool);
        }
    }
}
