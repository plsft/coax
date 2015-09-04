using System;
using System.Data;
using System.Data.SqlClient;

namespace Helix.Utility
{
    public sealed class DatabaseOps
    {

        /// <summary>
        /// Executes Insert, Delete, or Update
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="closeConnection"> </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int ExecuteSqlNonQuery(SqlConnection conn, SqlCommand sql, bool closeConnection=true, params SqlParameter[] parameters)
        {
            try
            {
                if (conn == null)
                    throw new ArgumentNullException("conn", "Can't be null!");

                if (sql == null)
                    throw new ArgumentNullException("sql", "Can't be null!");

                if ( parameters != null )
                    foreach (var p in parameters)
                        sql.Parameters.Add(p);

                conn.Open();
                sql.Connection = conn;
                
                return sql.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null && closeConnection)
                {
                    conn.Close();
                    conn.Dispose();
                    
                }
            }
        } 
         
        /// <summary>
        /// Executes Selects
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="closeConnection"> </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataSet ExecuteSqlQuery(SqlConnection conn, SqlCommand sql, bool closeConnection=true, params SqlParameter[] parameters)
        {
            try
            {
                if (conn == null ) 
                    throw new ArgumentNullException("conn", "Can't be null!");

                if( sql == null )
                    throw new ArgumentNullException("sql", "Can't be null!");
                
                if (parameters != null ) 
                    foreach (var p in parameters) 
                        sql.Parameters.Add(p);

                conn.Open(); 
                sql.Connection = conn;
                
                using (var da = new SqlDataAdapter(sql))
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    sql.Parameters.Clear();
                    return ds;
                }
            }
            finally
            {
                if (conn != null && closeConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes query, returns top 1 row.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="closeConnection"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataRow ExecuteSqlQueryRow(SqlConnection conn, SqlCommand sql, bool closeConnection = true, params SqlParameter[] parameters)
        {
                var set = ExecuteSqlQuery(conn, sql, closeConnection, parameters);
                return set.Tables.Count == 0 ? null : set.Tables[0].Rows[0];
        }

        /// <summary>
        /// Executes Scalar Select
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="closeConnection"> </param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object ExecuteScalar(SqlConnection conn, SqlCommand sql, bool closeConnection=true, params SqlParameter[] parameters )
        {
            try
            {
                if (conn == null)
                    throw new ArgumentNullException("conn", "Can't be null!");

                if (sql == null)
                    throw new ArgumentNullException("sql", "Can't be null!");

                if ( parameters != null )
                    foreach (var p in parameters)
                        sql.Parameters.Add(p);


                conn.Open();
                sql.Connection = conn;

                return sql.ExecuteScalar();
            }
            finally
            {
                if (conn != null && closeConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}

