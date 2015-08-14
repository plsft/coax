using System;
using System.Data;

namespace Helix.Utility
{
	public sealed class Settings
	{
		private static readonly object o = new object();

		static Settings()
		{
			Init();
		}

		public static string Get(string settingName)
		{
			lock (o)
			{
				const string selectSettingSql = "select SettingValue from Settings with(nolock) where SettingName = '{0}'";
				var cmd = new SqlCmdBuilder(string.Format(selectSettingSql, settingName)).WithCommandTimeOut(10).WithCommandType(CommandType.Text).CompletedSqlCommand;
				using (var connection = new DbConnect().GetSqlConnection())
				{
					return Convert.ToString(DatabaseOps.ExecuteScalar(connection, cmd, false));
				}
			}
		}

		private static void Init()
		{
			lock (o)
			{
				const string createSettingSql = @"if object_id('Settings') is null
									begin 
									create table dbo.Settings (
										ID int not null identity(1,1) primary key,
										SettingName varchar(32) not null,
										SettingValue varchar(128) not null
									) on [primary] 
									end";

				var cmd = new SqlCmdBuilder(createSettingSql).WithCommandTimeOut(10).WithCommandType(CommandType.Text).CompletedSqlCommand;

				using (var connection = new DbConnect().GetSqlConnection())
				{
					DatabaseOps.ExecuteSqlNonQuery(connection, cmd, false);
				}
			}
		}
	}
}
