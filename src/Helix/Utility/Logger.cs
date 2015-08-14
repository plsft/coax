
namespace Helix.Utility
{
	using System.Data;
	using System.Data.SqlClient;
	using System.Reflection;
	
	public  sealed class Logger
	{
		public enum LogType
		{
			Info = 0,
			Warning = 1,
			Error = 2,
			Critical = 4
		}

		static Logger()
		{
			Init();
		}

		public static void Log(string title, string descr, LogType logType , params object[] args)
		{
			var fullDescr = "";
			var fullTitle = "";
			
			if (args != null)
			{
				fullDescr = string.Format(descr, args);
				fullTitle = string.Format(title, args);
			}

			fullDescr = fullDescr.LimitString(999);
			fullTitle = fullTitle.LimitString(50);

			var source = string.Format("{0}, .NET {1}", Assembly.GetCallingAssembly().FullName, Assembly.GetCallingAssembly().ImageRuntimeVersion);
			source = source.LimitString(246);

			using (var connection = new DbConnect().GetSqlConnection())
			{
				var cmd = new SqlCmdBuilder("insert into Logs( Title, Descr, Code, Src) values (@title,@descr,@code,@src)")
					.WithCommandTimeOut(60)
					.WithCommandType(CommandType.Text)
					.WithParams(new[]
						{
							new SqlParameter("@title", fullTitle),
							new SqlParameter("@descr", fullDescr),
							new SqlParameter("@code", (int) logType),
							new SqlParameter("@src", source)
						});

				try
				{
					DatabaseOps.ExecuteSqlNonQuery(connection, cmd.CompletedSqlCommand, false);
				}
				catch
				{
					// we won't throw exceptions for logging errors.
				}
			}

		}

		private static void Init()
		{
			const string createLogSql = @"if object_id('Logs') is null
									begin 
									create table dbo.Logs(
										Title varchar(64) null, 
										Descr varchar(1024) null, 
										Code int null, 
										[Src] varchar(256) null, 
										Created smalldatetime not null default(getdate())
									) on [primary] 
									end";

			var cmd = new SqlCmdBuilder(createLogSql).WithCommandTimeOut(10).WithCommandType(CommandType.Text).CompletedSqlCommand;
			
			using (var connection = new DbConnect().GetSqlConnection())
			{
				DatabaseOps.ExecuteSqlNonQuery(connection, cmd, false);
			}
		}
	}
}
