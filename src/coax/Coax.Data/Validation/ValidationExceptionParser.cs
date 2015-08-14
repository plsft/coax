using System.Collections;
using System.Data.SqlClient;
using Helix.Utility;

namespace Coax.Data.Validation
{
    public sealed class ValidationExceptionParser
    {
        public string ValidationErrorMessage { get; set; }

        public ValidationExceptionParser(string tableName, SqlException ex)
        {
            foreach (SqlError e in ex.Errors)
            {
                switch (e.Number)
                {
                    case 2627: ValidationErrorMessage += string.Format(" {0}: data must be unique in table {1}", e.Number, tableName); break;
                    case 3621:
                        continue;
                }
               
            }
        }


    }
}
