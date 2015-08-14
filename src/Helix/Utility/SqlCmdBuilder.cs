using System.Data;
using System.Data.SqlClient;

namespace Helix.Utility
{
    public sealed class SqlCmdBuilder
    {
        public SqlCommand CompletedSqlCommand { get; set; }

        public SqlCmdBuilder(string sql)
        {
            CompletedSqlCommand = new SqlCommand
            {
                CommandText = sql,
            };
        }

        public SqlCmdBuilder WithCommandTimeOut(int timeOut)
        {
            CompletedSqlCommand.CommandTimeout = timeOut;
            return this;
        }

        public SqlCmdBuilder WithCommandType(CommandType type)
        {
            CompletedSqlCommand.CommandType = type;
            return this;
        }

        public SqlCmdBuilder WithParams(SqlParameter[] parameters)
        {
            foreach (var sqlParameter in parameters)
                CompletedSqlCommand.Parameters.AddWithValue(sqlParameter.ParameterName, sqlParameter.Value);

            return this;
        }

        public SqlCmdBuilder WithParam(SqlParameter param)
        {
            CompletedSqlCommand.Parameters.AddWithValue(param.ParameterName, param.Value);
            return this;
        }

        public SqlCmdBuilder GetCommand()
        {
            return this;
        }

        public string Sql
        {
            get { return CompletedSqlCommand.CommandText; }
        }

    }
}
