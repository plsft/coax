/* PetaPoco v4.0.3 - A Tiny ORMish thing for your POCO's.
 * Copyright © 2011 Topten Software.  All Rights Reserved.
 * 
 * Apache License 2.0 - http://www.toptensoftware.com/petapoco/license
 * 
 * Special thanks to Rob Conery (@robconery) for original inspiration (ie:Massive) and for 
 * use of Subsonic's T4 templates, Rob Sullivan (@DataChomp) for hard core DBA advice 
 * and Adam Schroder (@schotime) for lots of suggestions, improvements and Oracle support
 * heavily edited by George Rios 2013-2015
 */

// Define PETAPOCO_NO_DYNAMIC in your project settings on .NET 3.5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;


namespace Helix.Infra.Peta
{
    // Poco's marked [Explicit] require all column properties to be marked
    [AttributeUsage(AttributeTargets.Class)]
    public class ExplicitColumnsAttribute : Attribute
    {
    }

    // For non-explicit pocos, causes a property to be ignored
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }


    // For explicit pocos, marks property as a column and optionally supplies column name
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute()
        {
        }

        public ColumnAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    // For explicit pocos, marks property as a result (e.g. virtual--no binding from db) column and optionally supplies column name
    [AttributeUsage(AttributeTargets.Property)]
    public class VirtualColumnAttribute : ColumnAttribute
    {
        public VirtualColumnAttribute()
        {
        }

        public VirtualColumnAttribute(string name) : base(name)
        {
        }
    }

    //UpdateIfNull Attribute
    [AttributeUsage(AttributeTargets.Property)]
    public class UpdateIfNullAttribute : ColumnAttribute
    {
        public UpdateIfNullAttribute()
        {
        }

        public UpdateIfNullAttribute(string name) : base(name)
        {
        }
    }


    // Specify the table name of a poco
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
        public TableNameAttribute(string tableName)
        {
            Value = tableName;
        }

        public string Value { get; private set; }
    }

    // Specific the primary key of a poco class 
    [AttributeUsage(AttributeTargets.Class)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute(string primaryKey)
        {
            Value = primaryKey;
            autoIncrement = true;
        }

        public string Value { get; private set; }
        public string sequenceName { get; set; }
        public bool autoIncrement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoJoinAttribute : Attribute
    {
        public AutoJoinAttribute()
        {
        }
    }

    public class Page<T>
    {
        public long CurrentPage { get; set; }
        public long TotalPages { get; set; }
        public long TotalItems { get; set; }
        public long ItemsPerPage { get; set; }
        public List<T> Items { get; set; }
        public object Context { get; set; }
    }

    public class AnsiString
    {
        public AnsiString(string str)
        {
            Value = str;
        }

        public string Value { get; private set; }
    }

    public class TableInfo
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public bool AutoIncrement { get; set; }
        public string SequenceName { get; set; }
    }

    public interface IMapper
    {
        void GetTableInfo(Type t, TableInfo ti);
        bool MapPropertyToColumn(PropertyInfo pi, ref string columnName, ref bool resultColumn);
        Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType);
        Func<object, object> GetToDbConverter(Type SourceType);
    }

    public interface IMapper2 : IMapper
    {
        Func<object, object> GetFromDbConverter(Type DestType, Type SourceType);
    }

    public class Database : IDisposable
    {
        /// <summary>
        /// exposes current DbProvider
        /// </summary>
        public DBType DatabaseProviderType => this._dbType;

        public Transaction DatabaseTransaction => this.GetTransaction();

        public Sql DatabaseSql => new Sql();

        public Database(IDbConnection connection)
        {
            _sharedConnection = connection;
            _connectionString = connection.ConnectionString;
            _sharedConnectionDepth = 2; // Prevent closing external connection
            CommonConstruct();
        }

        public Database(string connectionString, string providerName, bool keepConnectionAlive = false)
        {
            _connectionString = connectionString;
            _providerName = providerName;
            KeepConnectionAlive = keepConnectionAlive;
            CommonConstruct();
        }

        public Database(string connectionString, DbProviderFactory provider, bool keepConnectionAlive = false)
        {
            _connectionString = connectionString;
            _factory = provider;
            KeepConnectionAlive = keepConnectionAlive;
            CommonConstruct();
        }

        public Database(string connectionStringName, bool keepConnectionAlive = false)
        {
            KeepConnectionAlive = keepConnectionAlive;

            if (connectionStringName == "")
                connectionStringName = ConfigurationManager.ConnectionStrings[0].Name;

            if (ConfigurationManager.ConnectionStrings == null)
            {
                throw new InvalidOperationException("Can't find any config file for connection string - StringName['" + connectionStringName + "']");
            }

            var providerName = "System.Data.SqlClient";
            if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName))
                    providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
            }
            else
            {
                throw new InvalidOperationException("Can't find a connection string with the name '" + connectionStringName + "'");
            }

            _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            _providerName = providerName;
            CommonConstruct();
        }

        public enum DBType
        {
            SqlServer,
            SqlServerCE,
            MySql,
            PostgreSQL,
            Oracle,
            SQLite
        }

        private DBType _dbType = DBType.SqlServer;

        private void CommonConstruct()
        {
            _transactionDepth = 0;
            EnableAutoSelect = true;
            EnableNamedParams = true;
            ForceDateTimesToUtc = true;

            if (_providerName != null)
                _factory = DbProviderFactories.GetFactory(_providerName);

            string dbtype = (_factory?.GetType() ?? _sharedConnection.GetType()).Name;

            // Try using type name first (more reliable)
            if (dbtype.StartsWith("MySql")) _dbType = DBType.MySql;
            else if (dbtype.StartsWith("SqlCe")) _dbType = DBType.SqlServerCE;
            else if (dbtype.StartsWith("Npgsql")) _dbType = DBType.PostgreSQL;
            else if (dbtype.StartsWith("Oracle")) _dbType = DBType.Oracle;
            else if (dbtype.StartsWith("SQLite")) _dbType = DBType.SQLite;
            else if (dbtype.StartsWith("System.Data.SqlClient.")) _dbType = DBType.SqlServer;
            // else try with provider name
            else if (_providerName.IndexOf("MySql", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.MySql;
            else if (_providerName.IndexOf("SqlServerCe", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.SqlServerCE;
            else if (_providerName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.PostgreSQL;
            else if (_providerName.IndexOf("Oracle", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.Oracle;
            else if (_providerName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0) _dbType = DBType.SQLite;

            if (_dbType == DBType.MySql && _connectionString != null && _connectionString.IndexOf("Allow User Variables=true") >= 0)
                _paramPrefix = "?";
            if (_dbType == DBType.Oracle)
                _paramPrefix = ":";
        }

        public void Dispose()
        {
            CloseSharedConnection();
        }

        public bool KeepConnectionAlive { get; set; }

        public void OpenSharedConnection()
        {
            if (_sharedConnectionDepth == 0)
            {
                _sharedConnection = _factory.CreateConnection();
                _sharedConnection.ConnectionString = _connectionString;
                _sharedConnection.Open();

                _sharedConnection = OnConnectionOpened(_sharedConnection);

                if (KeepConnectionAlive)
                    _sharedConnectionDepth++; // Make sure you call Dispose
            }
            _sharedConnectionDepth++;
        }

        public void CloseSharedConnection()
        {
            if (_sharedConnectionDepth > 0)
            {
                _sharedConnectionDepth--;
                if (_sharedConnectionDepth == 0)
                {
                    OnConnectionClosing(_sharedConnection);
                    _sharedConnection.Close();
                    _sharedConnection.Dispose();

                    _sharedConnection = null;
                }
            }
        }

        // Access to our shared connection
        public IDbConnection Connection => _sharedConnection;

        // Helper to create a transaction scope
        public Transaction GetTransaction()
        {
            return new Transaction(this);
        }

        public virtual void OnBeginTransaction()
        {
        }

        public virtual void OnEndTransaction()
        {
        }

        // Start a new transaction, can be nested, every call must be matched by a call to AbortTransaction or CompleteTransaction
        // Use `using (var scope=db.Transaction) { scope.Complete(); }` to ensure correct semantics
        public void BeginTransaction()
        {
            _transactionDepth++;

            if (_transactionDepth != 1)
                return;

            OpenSharedConnection();
            _transaction = _sharedConnection.BeginTransaction();
            _transactionCancelled = false;
            OnBeginTransaction();
        }

        private void CleanupTransaction()
        {
            OnEndTransaction();

            if (_transactionCancelled)
                _transaction.Rollback();
            else
                _transaction.Commit();

            _transaction.Dispose();
            _transaction = null;

            CloseSharedConnection();
        }

        public void CommitTransaction()
        {
            OnEndTransaction();
            _transaction.Commit();
        }

        public void RollbackTransaction()
        {
            OnEndTransaction();
            _transaction.Rollback();
        }

        public void AbortTransaction()
        {
            _transactionCancelled = true;
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        public void CompleteTransaction()
        {
            if ((--_transactionDepth) == 0)
                CleanupTransaction();
        }

        private static readonly Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        private static readonly Regex rxParamsPrefix = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        private readonly Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private readonly Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex rxOrderBy =
            new Regex(
                @"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        private static Dictionary<string, object> MultiPocoFactories = new Dictionary<string, object>();
        private static Dictionary<string, object> AutoMappers = new Dictionary<string, object>();
        private static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();



        public static string ProcessParams(string _sql, object[] args_src, List<object> args_dest)
        {
            return rxParams.Replace(_sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException($"Parameter `@{paramIndex}` specified but only {args_src.Length} parameters supplied (in `{_sql}`)");

                    arg_val = args_src[paramIndex];
                }
                else
                {
                    var found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException($"Parameter `@{param}` specified but none of the passed arguments have a property with this name (in `{_sql}`)");
                }

                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
                        args_dest.Add(i);
                    }
                    return sb.ToString();
                }
                else
                {
                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            });
        }

        private void AddParam(IDbCommand cmd, object item, string ParameterPrefix)
        {
            if (Mapper != null && item != null)
            {
                var fn = Mapper.GetToDbConverter(item.GetType());
                if (fn != null)
                    item = fn(item);
            }

            var idbParam = item as IDbDataParameter;
            if (idbParam != null)
            {
                idbParam.ParameterName = $"{ParameterPrefix}{cmd.Parameters.Count}";
                cmd.Parameters.Add(idbParam);
                return;
            }

            var p = cmd.CreateParameter();
            p.ParameterName = $"{ParameterPrefix}{cmd.Parameters.Count}";
            if (item == null)
            {
                p.Value = DBNull.Value;
            }
            else
            {
                var t = item.GetType();
                if (t.IsEnum) // PostgreSQL .NET driver wont cast enum to int
                {
                    p.Value = (int) item;
                }
                else if (t == typeof (Guid))
                {
                    p.Value = item.ToString();
                    p.DbType = DbType.String;
                    p.Size = 40;
                }
                else if (t == typeof (string))
                {
                    p.Size = Math.Max((item as string).Length + 1, 4000); // Help query plan caching by using common size
                    p.Value = item;
                }
                else if (t == typeof (AnsiString))
                {
                    // Thanks @DataChomp for pointing out the SQL Server indexing performance hit of using wrong string type on varchar
                    p.Size = Math.Max((item as AnsiString).Value.Length + 1, 4000);
                    p.Value = (item as AnsiString).Value;
                    p.DbType = DbType.AnsiString;
                }
                else if (t == typeof (bool) && _dbType != DBType.PostgreSQL)
                {
                    p.Value = ((bool) item) ? 1 : 0;
                }
                else if (item.GetType().Name == "SqlGeography") 
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); 
                    p.Value = item;
                }

                else if (item.GetType().Name == "SqlGeometry") 
                {
                    p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); 
                    p.Value = item;
                }
                else
                {
                    p.Value = item;
                }
            }

            cmd.Parameters.Add(p);
        }



        public IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
        {
            if (EnableNamedParams)
            {
                var new_args = new List<object>();
                sql = ProcessParams(sql, args, new_args);
                args = new_args.ToArray();
            }

            if (_paramPrefix != "@")
                sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@"); // <- double @@ escapes a single @

            var cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = _transaction;

            foreach (var item in args)
            {
                AddParam(cmd, item, _paramPrefix);
            }

            if (_dbType == DBType.Oracle)
            {
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            }

            if (!string.IsNullOrEmpty(sql))
                DoPreExecute(cmd);

            return cmd;
        }

        public virtual void OnException(Exception x)
        {
            var ex = $"Helix.Peta.OnException [{x}]: Command [{LastCommand}] SQL [{LastSQL}] Args [{LastArgs}] ";
            throw new Exception(ex);
        }

        public virtual IDbConnection OnConnectionOpened(IDbConnection conn)
        {
            return conn;
        }

        public virtual void OnConnectionClosing(IDbConnection conn)
        {
        }

        public virtual void OnExecutingCommand(IDbCommand cmd)
        {
        }

        public virtual void OnExecutedCommand(IDbCommand cmd)
        {
        }

        public int Execute(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public int Execute(Sql sql)
        {
            return Execute(sql.SQL, sql.Arguments);
        }

        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, sql, args))
                    {
                        var val = cmd.ExecuteScalar();
                        OnExecutedCommand(cmd);
                        return (T) Convert.ChangeType(val, typeof (T));
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public T ExecuteScalar<T>(Sql sql)
        {
            return ExecuteScalar<T>(sql.SQL, sql.Arguments);
        }


        private string AddSelectClause<T>(string sql)
        {
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (rxSelect.IsMatch(sql))
                return sql;

            var pd = PocoData.ForType(typeof (T));
            var tableName = EscapeTableName(pd.TableInfo.TableName);
            var cols = string.Join(", ", (from c in pd.QueryColumns select tableName + "." + EscapeSqlIdentifier(c)).ToArray());
            sql = !rxFrom.IsMatch(sql) ? $"SELECT {cols} FROM {tableName} {sql}" : $"SELECT {cols} {sql}";

            return sql;
        }

        public bool EnableAutoSelect { get; set; }
        public bool EnableNamedParams { get; set; }
        public bool ForceDateTimesToUtc { get; set; }

        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).ToList();
        }

        public List<T> Fetch<T>(Sql sql)
        {
            return Fetch<T>(sql.SQL, sql.Arguments);
        }


        public static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
        {
            sqlSelectRemoved = null;
            sqlCount = null;
            sqlOrderBy = null;

            // Extract the columns from "SELECT <whatever> FROM"
            var m = rxColumns.Match(sql);
            if (!m.Success)
                return false;

            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            sqlSelectRemoved = sql.Substring(g.Index);

            if (rxDistinct.IsMatch(sqlSelectRemoved))
                sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
            else
                sqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);


            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
            if (!m.Success)
            {
                sqlOrderBy = null;
            }
            else
            {
                g = m.Groups[0];
                sqlOrderBy = g.ToString();
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }

            return true;
        }

        public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
        {
            // Add auto select clause
            if (EnableAutoSelect)
                sql = AddSelectClause<T>(sql);

            // Split the SQL into the bits we need
            string sqlSelectRemoved, sqlOrderBy;
            if (!SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy))
                throw new Exception("Unable to parse SQL statement for paged query");
            if (_dbType == DBType.Oracle && sqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias `*` when performing a paged query.\neg. select t.* from table t order by t.id");

            // Build the SQL for the actual final result
            if (_dbType == DBType.SqlServer || _dbType == DBType.Oracle)
            {
                sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");
                if (rxDistinct.IsMatch(sqlSelectRemoved))
                {
                    sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
                }
                sqlPage =
                    $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({(sqlOrderBy ?? "ORDER BY (SELECT NULL)")}) peta_rn, {sqlSelectRemoved}) peta_paged WHERE peta_rn>@{args.Length} AND peta_rn<=@{args.Length + 1}";
                args = args.Concat(new object[] {skip, skip + take}).ToArray();
            }
            else if (_dbType == DBType.SqlServerCE)
            {
                sqlPage = $"{sql}\nOFFSET @{args.Length} ROWS FETCH NEXT @{args.Length + 1} ROWS ONLY";
                args = args.Concat(new object[] {skip, take}).ToArray();
            }
            else
            {
                sqlPage = $"{sql}\nLIMIT @{args.Length} OFFSET @{args.Length + 1}";
                args = args.Concat(new object[] {take, skip}).ToArray();
            }

        }

        public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>((page - 1)*itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);

            var saveTimeout = OneTimeCommandTimeout;

            var result = new Page<T>
            {
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = ExecuteScalar<long>(sqlCount, args)
            };

            result.TotalPages = result.TotalItems/itemsPerPage;

            if ((result.TotalItems%itemsPerPage) != 0)
                result.TotalPages++;

            OneTimeCommandTimeout = saveTimeout;
            result.Items = Fetch<T>(sqlPage, args);

            return result;
        }

        public Page<T> Page<T>(long page, long itemsPerPage, Sql sql)
        {
            return Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
        }


        public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
        {
            return SkipTake<T>((page - 1)*itemsPerPage, itemsPerPage, sql, args);
        }

        public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql)
        {
            return SkipTake<T>((page - 1)*itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
        }

        public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
        {
            string sqlCount, sqlPage;
            BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
            return Fetch<T>(sqlPage, args);
        }

        public List<T> SkipTake<T>(long skip, long take, Sql sql)
        {
            return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
        }

        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            if (EnableAutoSelect)
                sql = AddSelectClause<T>(sql);

            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    var pd = PocoData.ForType(typeof (T));
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = pd.GetFactory(cmd.CommandText, _sharedConnection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r) as Func<IDataReader, T>;
                    using (r)
                    {
                        while (true)
                        {
                            T poco;
                            try
                            {
                                if (!r.Read())
                                    yield break;
                                poco = factory != null ? factory(r) : default(T);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            yield return poco;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }

        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, TRet>(cb, sql, args).ToList();
        }

        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, T3, TRet>(cb, sql, args).ToList();
        }

        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList();
        }

        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2)}, cb, sql, args);
        }

        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2), typeof (T3)}, cb, sql, args);
        }

        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2), typeof (T3), typeof (T4)}, cb, sql, args);
        }

        public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
        {
            return Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
        {
            return Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
        {
            return Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList();
        }

        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2)}, cb, sql.SQL, sql.Arguments);
        }

        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2), typeof (T3)}, cb, sql.SQL, sql.Arguments);
        }

        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql)
        {
            return Query<TRet>(new Type[] {typeof (T1), typeof (T2), typeof (T3), typeof (T4)}, cb, sql.SQL, sql.Arguments);
        }

        public List<T1> Fetch<T1, T2>(string sql, params object[] args)
        {
            return Query<T1, T2>(sql, args).ToList();
        }

        public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args)
        {
            return Query<T1, T2, T3>(sql, args).ToList();
        }

        public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args)
        {
            return Query<T1, T2, T3, T4>(sql, args).ToList();
        }

        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2)}, null, sql, args);
        }

        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2), typeof (T3)}, null, sql, args);
        }

        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2), typeof (T3), typeof (T4)}, null, sql, args);
        }

        public List<T1> Fetch<T1, T2>(Sql sql)
        {
            return Query<T1, T2>(sql.SQL, sql.Arguments).ToList();
        }

        public List<T1> Fetch<T1, T2, T3>(Sql sql)
        {
            return Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList();
        }

        public List<T1> Fetch<T1, T2, T3, T4>(Sql sql)
        {
            return Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList();
        }

        public IEnumerable<T1> Query<T1, T2>(Sql sql)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2)}, null, sql.SQL, sql.Arguments);
        }

        public IEnumerable<T1> Query<T1, T2, T3>(Sql sql)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2), typeof (T3)}, null, sql.SQL, sql.Arguments);
        }

        public IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql)
        {
            return Query<T1>(new Type[] {typeof (T1), typeof (T2), typeof (T3), typeof (T4)}, null, sql.SQL, sql.Arguments);
        }

        private static object GetAutoMapper(Type[] types)
        {
            var kb = new StringBuilder();
            foreach (var t in types)
            {
                kb.Append(t);
                kb.Append(":");
            }
            var key = kb.ToString();
            RWLock.EnterReadLock();

            try
            {
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            RWLock.EnterWriteLock();
            try
            {
                object mapper;
                if (AutoMappers.TryGetValue(key, out mapper))
                    return mapper;

                var m = new DynamicMethod("helix_peta_automapper", types[0], types, true);
                var il = m.GetILGenerator();

                for (var i = 1; i < types.Length; i++)
                {
                    var handled = false;
                    for (var j = i - 1; j >= 0; j--)
                    {
                        var candidates = from p in types[j].GetProperties() where p.PropertyType == types[i] select p;

                        if (!candidates.Any())
                            continue;

                        if (candidates.Count() > 1)
                            throw new InvalidOperationException(string.Format("Can not auto join `{0}` as `{1}` has more than one property of type `{0}`", types[i], types[j]));

                        // Generate code
                        il.Emit(OpCodes.Ldarg_S, j);
                        il.Emit(OpCodes.Ldarg_S, i);
                        il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
                        handled = true;
                    }

                    if (!handled)
                        throw new InvalidOperationException($"Can't auto join `{types[i]}`");
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

                var del = m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
                AutoMappers.Add(key, del);
                return del;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }
        }

        private Delegate FindSplitPoint(Type typeThis, Type typeNext, string sql, IDataReader r, ref int pos)
        {
            if (typeNext == null)
                return PocoData.ForType(typeThis).GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, pos, r.FieldCount - pos, r);

            var pdThis = PocoData.ForType(typeThis);
            var pdNext = PocoData.ForType(typeNext);

            var firstColumn = pos;
            var usedColumns = new Dictionary<string, bool>();
            for (; pos < r.FieldCount; pos++)
            {
                var fieldName = r.GetName(pos);
                if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
                {
                    return pdThis.GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, firstColumn, pos - firstColumn, r);
                }
                usedColumns.Add(fieldName, true);
            }

            throw new InvalidOperationException($"Couldn't find split point between `{typeThis}` and `{typeNext}`");
        }

        private class MultiPocoFactory
        {
            public List<Delegate> m_Delegates;

            public Delegate GetItem(int index)
            {
                return m_Delegates[index];
            }
        }

        private Func<IDataReader, object, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            var m = new DynamicMethod("helix_peta_multipoco_factory", typeof (TRet), new[] {typeof (MultiPocoFactory), typeof (IDataReader), typeof (object)}, typeof (MultiPocoFactory));
            var il = m.GetILGenerator();

            il.Emit(OpCodes.Ldarg_2);

            var dels = new List<Delegate>();
            int pos = 0;
            for (int i = 0; i < types.Length; i++)
            {
                var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, sql, r, ref pos);
                dels.Add(del);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Callvirt, typeof (MultiPocoFactory).GetMethod("GetItem")); 
                il.Emit(OpCodes.Ldarg_1);

                var tDelInvoke = del.GetType().GetMethod("Invoke");
                il.Emit(OpCodes.Callvirt, tDelInvoke);
            }

            il.Emit(OpCodes.Callvirt, Expression.GetFuncType(types.Concat(new[] {typeof (TRet)}).ToArray()).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            return (Func<IDataReader, object, TRet>) m.CreateDelegate(typeof (Func<IDataReader, object, TRet>), new MultiPocoFactory() {m_Delegates = dels});
        }


        private Func<IDataReader, object, TRet> GetMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
        {
            var kb = new StringBuilder();
            kb.Append(typeof (TRet));
            kb.Append(":");

            foreach (var t in types)
            {
                kb.Append(":");
                kb.Append(t);
            }

            kb.Append(":");
            kb.Append(_sharedConnection.ConnectionString);
            kb.Append(":");
            kb.Append(ForceDateTimesToUtc);
            kb.Append(":");
            kb.Append(sql);
            var key = kb.ToString();

            RWLock.EnterReadLock();

            try
            {
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>) oFactory;
            }
            finally
            {
                RWLock.ExitReadLock();
            }

            RWLock.EnterWriteLock();
            try
            {
                object oFactory;
                if (MultiPocoFactories.TryGetValue(key, out oFactory))
                    return (Func<IDataReader, object, TRet>) oFactory;

                var Factory = CreateMultiPocoFactory<TRet>(types, sql, r);

                MultiPocoFactories.Add(key, Factory);
                return Factory;
            }
            finally
            {
                RWLock.ExitWriteLock();
            }

        }

        public IEnumerable<TRet> Query<TRet>(Type[] types, object cb, string sql, params object[] args)
        {
            OpenSharedConnection();
            try
            {
                using (var cmd = CreateCommand(_sharedConnection, sql, args))
                {
                    IDataReader r;
                    try
                    {
                        r = cmd.ExecuteReader();
                        OnExecutedCommand(cmd);
                    }
                    catch (Exception x)
                    {
                        OnException(x);
                        throw;
                    }
                    var factory = GetMultiPocoFactory<TRet>(types, sql, r);
                    if (cb == null)
                        cb = GetAutoMapper(types.ToArray());
                    var bNeedTerminator = false;
                    using (r)
                    {
                        while (true)
                        {
                            TRet poco;
                            try
                            {
                                if (!r.Read())
                                    break;
                                poco = factory(r, cb);
                            }
                            catch (Exception x)
                            {
                                OnException(x);
                                throw;
                            }

                            if (poco != null)
                                yield return poco;
                            else
                                bNeedTerminator = true;
                        }
                        if (bNeedTerminator)
                        {
                            var poco = (TRet) (cb as Delegate).DynamicInvoke(new object[types.Length]);
                            if (poco != null)
                                yield return poco;
                            else
                                yield break;
                        }
                    }
                }
            }
            finally
            {
                CloseSharedConnection();
            }
        }


        public IEnumerable<T> Query<T>(Sql sql)
        {
            return Query<T>(sql.SQL, sql.Arguments);
        }

        public bool Exists<T>(object primaryKey)
        {
            return FirstOrDefault<T>($"WHERE {EscapeSqlIdentifier(PocoData.ForType(typeof (T)).TableInfo.PrimaryKey)}=@0", primaryKey) != null;
        }

        public T Single<T>(object primaryKey)
        {
            return Single<T>($"WHERE {EscapeSqlIdentifier(PocoData.ForType(typeof (T)).TableInfo.PrimaryKey)}=@0", primaryKey);
        }

        public T SingleOrDefault<T>(object primaryKey)
        {
            return SingleOrDefault<T>($"WHERE {EscapeSqlIdentifier(PocoData.ForType(typeof (T)).TableInfo.PrimaryKey)}=@0", primaryKey);
        }

        public T Single<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).Single();
        }

        public T SingleOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).SingleOrDefault();
        }

        public T First<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).First();
        }

        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return Query<T>(sql, args).FirstOrDefault();
        }

        public T Single<T>(Sql sql)
        {
            return Query<T>(sql).Single();
        }

        public T SingleOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).SingleOrDefault();
        }

        public T First<T>(Sql sql)
        {
            return Query<T>(sql).First();
        }

        public T FirstOrDefault<T>(Sql sql)
        {
            return Query<T>(sql).FirstOrDefault();
        }

        public string EscapeTableName(string str)
        {
            return str.IndexOf('.') >= 0 ? str : EscapeSqlIdentifier(str);
        }

        public string EscapeSqlIdentifier(string str)
        {
            switch (_dbType)
            {
                case DBType.MySql:
                    return $"`{str}`";

                case DBType.PostgreSQL:
                    return $"\"{str}\"";

                case DBType.Oracle:
                    return $"\"{str.ToUpperInvariant()}\"";

                default:
                    return $"[{str}]";
            }
        }

        public object Insert(string tableName, string primaryKeyName, object poco)
        {
            return Insert(tableName, primaryKeyName, true, poco);
        }

        // Insert a poco into a table.  If the poco has a property with the same name as the primary key the id of the new record is assigned to it.  Either way, the new id is returned.
        public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var pd = PocoData.ForObject(poco, primaryKeyName);
                        var names = new List<string>();
                        var values = new List<string>();
                        var index = 0;
                        foreach (var i in pd.Columns)
                        {
                            // Don't insert result columns
                            if (i.Value.ResultColumn || i.Value.VirtualColumn)
                                continue;

                            if (i.Key.Equals("id", StringComparison.InvariantCultureIgnoreCase)) // we touch "ID" primary key
                                continue;

                            if (i.Key.Equals("uid", StringComparison.InvariantCultureIgnoreCase)) // we touch "ID" primary key
                                continue;

                            if (i.Key.Equals("created", StringComparison.InvariantCultureIgnoreCase)) // we don't touch date columns
                                continue;

                            if (i.Key.Equals("timestamp", StringComparison.InvariantCultureIgnoreCase)) // we don't touch timestamp columns
                                continue;

                            if (i.Key.Equals("rowver", StringComparison.InvariantCultureIgnoreCase)) // we don't touch rowver columns
                                continue;

                            if (i.Key.Equals("rowversion", StringComparison.InvariantCultureIgnoreCase)) // we don't touch rowver columns
                                continue;

                            // dont touch db unless the POCO contains a new (non-null) value.
                            if (i.Value.GetValue(poco) == null || !i.Value.GetValue(poco).HasValue())
                                continue;

                            // Don't insert the primary key (except under oracle where we need bring in the next sequence value)
                            if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true) == 0)
                            {
                                if (_dbType == DBType.Oracle && !string.IsNullOrEmpty(pd.TableInfo.SequenceName))
                                {
                                    names.Add(i.Key);
                                    values.Add($"{pd.TableInfo.SequenceName}.nextval");
                                }
                                continue;
                            }

                            names.Add(EscapeSqlIdentifier(i.Key));
                            values.Add($"{_paramPrefix}{index++}");
                            AddParam(cmd, i.Value.GetValue(poco), _paramPrefix);
                        }

                        cmd.CommandText = $"INSERT INTO {EscapeTableName(tableName)} ({string.Join(",", names.ToArray())}) VALUES ({string.Join(",", values.ToArray())})";

                        if (!autoIncrement)
                        {
                            DoPreExecute(cmd);
                            cmd.ExecuteNonQuery();
                            OnExecutedCommand(cmd);
                            return true;
                        }

                        object id;

                        switch (_dbType)
                        {
                            case DBType.SqlServerCE:
                                DoPreExecute(cmd);
                                cmd.ExecuteNonQuery();
                                OnExecutedCommand(cmd);
                                id = ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
                                break;
                            case DBType.SqlServer:
                                cmd.CommandText += ";\nSELECT SCOPE_IDENTITY() AS NewID;";
                                DoPreExecute(cmd);
                                id = cmd.ExecuteScalar();
                                OnExecutedCommand(cmd);
                                break;
                            case DBType.PostgreSQL:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += $"returning {EscapeSqlIdentifier(primaryKeyName)} as NewID";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;

                            case DBType.Oracle:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += $" returning {EscapeSqlIdentifier(primaryKeyName)} into :newid";
                                    var param = cmd.CreateParameter();
                                    param.ParameterName = ":newid";
                                    param.Value = DBNull.Value;
                                    param.Direction = ParameterDirection.ReturnValue;
                                    param.DbType = DbType.Int64;
                                    cmd.Parameters.Add(param);
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                    id = param.Value;
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;

                            case DBType.SQLite:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += ";\nSELECT last_insert_rowid();";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalar();
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQuery();
                                }
                                OnExecutedCommand(cmd);
                                break;

                            default:
                                cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
                                DoPreExecute(cmd);
                                id = cmd.ExecuteScalar();
                                OnExecutedCommand(cmd);
                                break;
                        }


                        if (primaryKeyName != null)
                        {
                            PocoColumn pc;
                            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                            {
                                pc.SetValue(poco, pc.ChangeType(id));
                            }
                        }

                        return id;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public object Insert(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, true, poco);
        }

        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            return Update(tableName, primaryKeyName, poco, primaryKeyValue, null);
        }


        public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            try
            {
                OpenSharedConnection();
                try
                {
                    using (var cmd = CreateCommand(_sharedConnection, ""))
                    {
                        var sb = new StringBuilder();
                        var index = 0;
                        var pd = PocoData.ForObject(poco, primaryKeyName);
                        if (columns == null)
                        {
                            foreach (var i in pd.Columns)
                            {
                                if (string.Compare(i.Key, primaryKeyName, true) == 0)
                                {
                                    if (primaryKeyValue == null)
                                        primaryKeyValue = i.Value.GetValue(poco);
                                    continue;
                                }

                                if ((i.Value.UpdateIfNullColumn && i.Value.GetValue(poco) == null))
                                {
                                    if (index > 0)
                                        sb.Append(", ");
                                    sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(i.Key), _paramPrefix, index++);

                                    AddParam(cmd, i.Value.GetValue(poco), _paramPrefix);
                                    continue;
                                }

                                if (i.Key.Equals("uid", StringComparison.InvariantCultureIgnoreCase)) // we dont touch "UID" rowid 
                                    continue;

                                if (i.Key.Equals("created", StringComparison.InvariantCultureIgnoreCase))
                                    continue;

                                if (i.Key.Equals("timestamp", StringComparison.InvariantCultureIgnoreCase)) // we don't touch timestamp columns
                                    continue;

                                if (i.Key.Equals("rowver", StringComparison.InvariantCultureIgnoreCase)) // we don't touch rowver columns
                                    continue;

                                if (i.Key.Equals("rowversion", StringComparison.InvariantCultureIgnoreCase)) // we don't touch rowver columns
                                    continue;

                                if (i.Value.ResultColumn || i.Value.VirtualColumn)
                                    continue;

                                if ((!i.Value.GetValue(poco).HasValue() && !i.Value.UpdateIfNullColumn))
                                    continue;

                                if (index > 0)
                                {
                                    sb.Append(", ");
                                }

                                sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(i.Key), _paramPrefix, index++);
                                AddParam(cmd, i.Value.GetValue(poco), _paramPrefix);
                            }
                        }
                        else
                        {
                            foreach (var colname in columns)
                            {
                                var pc = pd.Columns[colname];

                                if (index > 0)
                                {
                                    sb.Append(", ");
                                }

                                sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(colname), _paramPrefix, index++);
                                AddParam(cmd, pc.GetValue(poco), _paramPrefix);
                            }

                            if (primaryKeyValue == null)
                            {
                                var pc = pd.Columns[primaryKeyName];
                                primaryKeyValue = pc.GetValue(poco);
                            }

                        }

                        cmd.CommandText = $"UPDATE {EscapeTableName(tableName)} SET {sb} WHERE {EscapeSqlIdentifier(primaryKeyName)} = {_paramPrefix}{index++}";
                        AddParam(cmd, primaryKeyValue, _paramPrefix);

                        DoPreExecute(cmd);

                        var retv = cmd.ExecuteNonQuery();
                        OnExecutedCommand(cmd);
                        return retv;
                    }
                }
                finally
                {
                    CloseSharedConnection();
                }
            }
            catch (Exception x)
            {
                OnException(x);
                throw;
            }
        }

        public int Update(string tableName, string primaryKeyName, object poco)
        {
            return Update(tableName, primaryKeyName, poco, null);
        }

        public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns)
        {
            return Update(tableName, primaryKeyName, poco, null, columns);
        }

        public int Update(object poco, IEnumerable<string> columns)
        {
            return Update(poco, null, columns);
        }

        public int Update(object poco)
        {
            return Update(poco, null, null);
        }

        public int Update(object poco, object primaryKeyValue)
        {
            return Update(poco, primaryKeyValue, null);
        }

        public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Update(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
        }

        public int Update<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof (T));
            return Execute($"UPDATE {EscapeTableName(pd.TableInfo.TableName)} {sql}", args);
        }

        public int Update<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof (T));
            return Execute(new Sql($"UPDATE {EscapeTableName(pd.TableInfo.TableName)}").Append(sql));
        }

        public int Delete(string tableName, string primaryKeyName, object poco)
        {
            return Delete(tableName, primaryKeyName, poco, null);
        }

        public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            if (primaryKeyValue == null)
            {
                var pd = PocoData.ForObject(poco, primaryKeyName);
                PocoColumn pc;
                if (pd.Columns.TryGetValue(primaryKeyName, out pc))
                {
                    primaryKeyValue = pc.GetValue(poco);
                }
            }

            var sql = $"DELETE FROM {EscapeTableName(tableName)} WHERE {EscapeSqlIdentifier(primaryKeyName)}=@0";
            return Execute(sql, primaryKeyValue);
        }

        public int Delete(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int Delete<T>(object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof (T))
                return Delete(pocoOrPrimaryKey);

            var pd = PocoData.ForType(typeof (T));
            return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }

        public int Delete<T>(string tableName, string primaryKeyName, object pocoOrPrimaryKey)
        {
            if (pocoOrPrimaryKey.GetType() == typeof (T))
                return Delete(pocoOrPrimaryKey);

            var pd = PocoData.ForType(typeof (T));
            return Delete(tableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
        }


        public int Delete<T>(string sql, params object[] args)
        {
            var pd = PocoData.ForType(typeof (T));
            return Execute($"DELETE FROM {EscapeTableName(pd.TableInfo.TableName)} {sql}", args);
        }

        public int Delete<T>(Sql sql)
        {
            var pd = PocoData.ForType(typeof (T));
            return Execute(new Sql($"DELETE FROM {EscapeTableName(pd.TableInfo.TableName)}").Append(sql));
        }

        // Check if a poco represents a new record
        public bool IsNew(string primaryKeyName, object poco)
        {
            var pd = PocoData.ForObject(poco, primaryKeyName);
            object pk;
            PocoColumn pc;
            if (pd.Columns.TryGetValue(primaryKeyName, out pc))
            {
                pk = pc.GetValue(poco);
            }

#if !PETAPOCO_NO_DYNAMIC
            else if (poco is ExpandoObject)
            {
                return true;
            }
#endif
            else
            {
                var pi = poco.GetType().GetProperty(primaryKeyName);
                if (pi == null)
                    throw new ArgumentException($"The object doesn't have a property matching the primary key column name '{primaryKeyName}'");
                pk = pi.GetValue(poco, null);
            }

            if (pk == null)
                return true;

            var type = pk.GetType();

            if (type.IsValueType)
            {
                if (type == typeof (long))
                    return (long) pk == 0;

                if (type == typeof (ulong))
                    return (ulong) pk == 0;

                if (type == typeof (int))
                    return (int) pk == 0;

                if (type == typeof (uint))
                    return (uint) pk == 0;

                return pk == Activator.CreateInstance(pk.GetType());
            }

            return pk == null;
        }

        public bool IsNew(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());

            if (!pd.TableInfo.AutoIncrement)
                throw new InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");

            return IsNew(pd.TableInfo.PrimaryKey, poco);
        }

        public void Save(string tableName, string primaryKeyName, object poco)
        {
            if (IsNew(primaryKeyName, poco))
            {
                Insert(tableName, primaryKeyName, true, poco);
            }
            else
            {
                Update(tableName, primaryKeyName, poco);
            }
        }

        public void Save(object poco)
        {
            var pd = PocoData.ForType(poco.GetType());
            Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
        }

        public int CommandTimeout { get; set; } = 60;
        public int OneTimeCommandTimeout { get; set; } = 60;

        private void DoPreExecute(IDbCommand cmd)
        {
            if (OneTimeCommandTimeout != 0)
            {
                cmd.CommandTimeout = OneTimeCommandTimeout;
                OneTimeCommandTimeout = 0;
            }
            else if (CommandTimeout != 0)
            {
                cmd.CommandTimeout = CommandTimeout;
            }

            OnExecutingCommand(cmd);

            _lastSql = cmd.CommandText;
            _lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
        }

        public string LastSQL => _lastSql;
        public object[] LastArgs => _lastArgs;
        public string LastCommand => FormatCommand(_lastSql, _lastArgs);

        public string FormatCommand(IDbCommand cmd)
        {
            return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
        }

        public string FormatCommand(string sql, object[] args)
        {
            var sb = new StringBuilder();
            if (sql == null)
                return string.Empty;
            sb.Append(sql);

            if (args != null && args.Length > 0)
            {
                sb.Append("\n");
                for (var i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", _paramPrefix, i, args[i].GetType().Name, args[i]);
                }
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public static IMapper Mapper { get; set; }

        public class PocoColumn
        {
            public string ColumnName;
            public PropertyInfo PropertyInfo;
            public bool ResultColumn;
            public bool VirtualColumn;
            public bool UpdateIfNullColumn;

            public virtual void SetValue(object target, object val)
            {
                PropertyInfo.SetValue(target, val, null);
            }

            public virtual object GetValue(object target)
            {
                return PropertyInfo.GetValue(target, null);
            }

            public virtual object ChangeType(object val)
            {
                return Convert.ChangeType(val, PropertyInfo.PropertyType);
            }
        }

        public class ExpandoColumn : PocoColumn
        {
            public override void SetValue(object target, object val)
            {
                (target as IDictionary<string, object>)[ColumnName] = val;
            }

            public override object GetValue(object target)
            {
                object val = null;
                (target as IDictionary<string, object>).TryGetValue(ColumnName, out val);
                return val;
            }

            public override object ChangeType(object val)
            {
                return val;
            }
        }

        public class PocoData
        {
            public static PocoData ForObject(object o, string primaryKeyName)
            {
                var t = o.GetType();

#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof (ExpandoObject))
                {
                    var pd = new PocoData
                    {
                        TableInfo = new TableInfo(),
                        Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase) {{primaryKeyName, new ExpandoColumn() {ColumnName = primaryKeyName}}}
                    };

                    pd.TableInfo.PrimaryKey = primaryKeyName;
                    pd.TableInfo.AutoIncrement = true;

                    foreach (var col in (o as IDictionary<string, object>).Keys)
                    {
                        if (col != primaryKeyName)
                            pd.Columns.Add(col, new ExpandoColumn() {ColumnName = col});
                    }

                    return pd;
                }
                else
#endif
                    return ForType(t);
            }

            private static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

            public static PocoData ForType(Type t)
            {
#if !PETAPOCO_NO_DYNAMIC
                if (t == typeof (ExpandoObject))
                    throw new InvalidOperationException("Can't use dynamic types with this method");
#endif
                RWLock.EnterReadLock();
                PocoData pd;
                try
                {
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }


                RWLock.EnterWriteLock();
                try
                {
                    if (m_PocoDatas.TryGetValue(t, out pd))
                        return pd;

                    pd = new PocoData(t);
                    m_PocoDatas.Add(t, pd);
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }

                return pd;
            }

            public PocoData()
            {
            }

            public PocoData(Type t)
            {
                type = t;
                TableInfo = new TableInfo();

                var a = t.GetCustomAttributes(typeof (TableNameAttribute), true);
                TableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;
                a = t.GetCustomAttributes(typeof (PrimaryKeyAttribute), true);

                TableInfo.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
                TableInfo.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).sequenceName;
                TableInfo.AutoIncrement = a.Length != 0 && (a[0] as PrimaryKeyAttribute).autoIncrement;

                Mapper?.GetTableInfo(t, TableInfo);

                var ExplicitColumns = t.GetCustomAttributes(typeof (ExplicitColumnsAttribute), true).Length > 0;
                Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);

                foreach (var pi in t.GetProperties())
                {
                    var ColAttrs = pi.GetCustomAttributes(typeof (ColumnAttribute), true);
                    if (ExplicitColumns)
                    {
                        if (ColAttrs.Length == 0)
                            continue;
                    }
                    else
                    {
                        if (pi.GetCustomAttributes(typeof (IgnoreAttribute), true).Length != 0)
                            continue;
                    }

                    var pc = new PocoColumn
                    {
                        PropertyInfo = pi
                    };

                    if (ColAttrs.Length > 0)
                    {
                        var colattr = (ColumnAttribute) ColAttrs[0];
                        pc.ColumnName = colattr.Name;
                        pc.ResultColumn = ((colattr as VirtualColumnAttribute) != null);
                        pc.UpdateIfNullColumn = ((colattr as UpdateIfNullAttribute) != null);
                    }

                    if (pc.ColumnName == null)
                    {
                        pc.ColumnName = pi.Name;
                        if (Mapper != null && !Mapper.MapPropertyToColumn(pi, ref pc.ColumnName, ref pc.ResultColumn))
                            continue;
                    }

                    Columns.Add(pc.ColumnName, pc);
                }

                QueryColumns = (from c in Columns where (!c.Value.ResultColumn && !c.Value.VirtualColumn) select c.Key).ToArray();

            }

            private static bool IsIntegralType(Type t)
            {
                var tc = Type.GetTypeCode(t);
                return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
            }

            public Delegate GetFactory(string sql, string connString, bool ForceDateTimesToUtc, int firstColumn, int countColumns, IDataReader r)
            {
                var key = $"{sql}:{connString}:{ForceDateTimesToUtc}:{firstColumn}:{countColumns}";
                RWLock.EnterReadLock();
                try
                {
                    Delegate factory;
                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;
                }
                finally
                {
                    RWLock.ExitReadLock();
                }

                RWLock.EnterWriteLock();

                try
                {

                    Delegate factory;

                    if (PocoFactories.TryGetValue(key, out factory))
                        return factory;

                    var m = new DynamicMethod("helix_peta_factory_" + PocoFactories.Count, type, new[] {typeof (IDataReader)}, true);
                    var il = m.GetILGenerator();

#if !PETAPOCO_NO_DYNAMIC
                    if (type == typeof (object))
                    {
                        // var poco=new T()
                        il.Emit(OpCodes.Newobj, typeof (ExpandoObject).GetConstructor(Type.EmptyTypes));
                        var fnAdd = typeof (IDictionary<string, object>).GetMethod("Add");

                        for (var i = firstColumn; i < firstColumn + countColumns; i++)
                        {
                            var srcType = r.GetFieldType(i);

                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldstr, r.GetName(i));

                            Func<object, object> converter = null;

                            if (Mapper != null)
                                converter = Mapper.GetFromDbConverter(null, srcType);

                            if (ForceDateTimesToUtc && converter == null && srcType == typeof (DateTime))
                                converter = src => new DateTime(((DateTime) src).Ticks, DateTimeKind.Utc);

                            AddConverterToStack(il, converter);

                            il.Emit(OpCodes.Ldarg_0); // obj, obj, fieldname, converter?,    rdr
                            il.Emit(OpCodes.Ldc_I4, i); // obj, obj, fieldname, converter?,  rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue); // obj, obj, fieldname, converter?,  value

                            // Convert DBNull to null
                            il.Emit(OpCodes.Dup); // obj, obj, fieldname, converter?,  value, value
                            il.Emit(OpCodes.Isinst, typeof (DBNull)); // obj, obj, fieldname, converter?,  value, (value or null)
                            var lblNotNull = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblNotNull); // obj, obj, fieldname, converter?,  value
                            il.Emit(OpCodes.Pop); // obj, obj, fieldname, converter?

                            if (converter != null)
                                il.Emit(OpCodes.Pop); // obj, obj, fieldname, 
                            il.Emit(OpCodes.Ldnull); // obj, obj, fieldname, null

                            if (converter != null)
                            {
                                var lblReady = il.DefineLabel();
                                il.Emit(OpCodes.Br_S, lblReady);
                                il.MarkLabel(lblNotNull);
                                il.Emit(OpCodes.Callvirt, fnInvoke);
                                il.MarkLabel(lblReady);
                            }
                            else
                            {
                                il.MarkLabel(lblNotNull);
                            }

                            il.Emit(OpCodes.Callvirt, fnAdd);
                        }
                    }
                    else
#endif
                        if (type.IsValueType || type == typeof (string) || type == typeof (byte[]))
                        {
                            var srcType = r.GetFieldType(0);
                            var converter = GetConverter(ForceDateTimesToUtc, null, srcType, type);

                            il.Emit(OpCodes.Ldarg_0); // rdr
                            il.Emit(OpCodes.Ldc_I4_0); // rdr,0
                            il.Emit(OpCodes.Callvirt, fnIsDBNull); // bool
                            var lblCont = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblCont);
                            il.Emit(OpCodes.Ldnull); // null
                            var lblFin = il.DefineLabel();
                            il.Emit(OpCodes.Br_S, lblFin);

                            il.MarkLabel(lblCont);

                            AddConverterToStack(il, converter);

                            il.Emit(OpCodes.Ldarg_0); // rdr
                            il.Emit(OpCodes.Ldc_I4_0); // rdr,0
                            il.Emit(OpCodes.Callvirt, fnGetValue); // value

                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            il.MarkLabel(lblFin);
                            il.Emit(OpCodes.Unbox_Any, type); // value converted
                        }
                        else
                        {
                            // var poco=new T()
                            il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));

                            // Enumerate all fields generating a set assignment for the column
                            for (int i = firstColumn; i < firstColumn + countColumns; i++)
                            {
                                // Get the PocoColumn for this db column, ignore if not known
                                PocoColumn pc;
                                if (!Columns.TryGetValue(r.GetName(i), out pc))
                                    continue;

                                // Get the source type for this column
                                var srcType = r.GetFieldType(i);
                                var dstType = pc.PropertyInfo.PropertyType;

                                // "if (!rdr.IsDBNull(i))"
                                il.Emit(OpCodes.Ldarg_0); // poco,rdr
                                il.Emit(OpCodes.Ldc_I4, i); // poco,rdr,i
                                il.Emit(OpCodes.Callvirt, fnIsDBNull); // poco,bool
                                var lblNext = il.DefineLabel();
                                il.Emit(OpCodes.Brtrue_S, lblNext); // poco

                                il.Emit(OpCodes.Dup); // poco,poco

                                var converter = GetConverter(ForceDateTimesToUtc, pc, srcType, dstType);

                                // Fast
                                var Handled = false;
                                if (converter == null)
                                {
                                    var valuegetter = typeof (IDataRecord).GetMethod("Get" + srcType.Name, new Type[] {typeof (int)});
                                    if (valuegetter != null
                                        && valuegetter.ReturnType == srcType
                                        && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                                    {
                                        il.Emit(OpCodes.Ldarg_0); // *,rdr
                                        il.Emit(OpCodes.Ldc_I4, i); // *,rdr,i
                                        il.Emit(OpCodes.Callvirt, valuegetter); // *,value

                                        if (Nullable.GetUnderlyingType(dstType) != null)
                                        {
                                            il.Emit(OpCodes.Newobj, dstType.GetConstructor(new[] {Nullable.GetUnderlyingType(dstType)}));
                                        }

                                        il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true)); // poco
                                        Handled = true;
                                    }
                                }

                                if (!Handled)
                                {
                                    AddConverterToStack(il, converter);

                                    // "value = rdr.GetValue(i)"
                                    il.Emit(OpCodes.Ldarg_0); // *,rdr
                                    il.Emit(OpCodes.Ldc_I4, i); // *,rdr,i
                                    il.Emit(OpCodes.Callvirt, fnGetValue); // *,value

                                    // Call the converter
                                    if (converter != null)
                                        il.Emit(OpCodes.Callvirt, fnInvoke);

                                    // Assign it
                                    il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType); // poco,poco,value
                                    il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true)); // poco
                                }

                                il.MarkLabel(lblNext);
                            }

                            var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(type,
                                (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                            if (fnOnLoaded != null)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Callvirt, fnOnLoaded);
                            }
                        }

                    il.Emit(OpCodes.Ret);

                    var del = m.CreateDelegate(Expression.GetFuncType(typeof (IDataReader), type));
                    PocoFactories.Add(key, del);
                    return del;
                }
                finally
                {
                    RWLock.ExitWriteLock();
                }
            }

            private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
            {
                if (converter == null)
                    return;

                var converterIndex = m_Converters.Count;
                m_Converters.Add(converter);

                il.Emit(OpCodes.Ldsfld, fldConverters);
                il.Emit(OpCodes.Ldc_I4, converterIndex);
                il.Emit(OpCodes.Callvirt, fnListGetItem);
            }

            private static Func<object, object> GetConverter(bool forceDateTimesToUtc, PocoColumn pc, Type srcType, Type dstType)
            {
                Func<object, object> converter = null;

                if (Mapper != null)
                {
                    if (pc != null)
                    {
                        converter = Mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
                    }
                    else
                    {
                        var m2 = Mapper as IMapper2;

                        if (m2 != null)
                        {
                            converter = m2.GetFromDbConverter(dstType, srcType);
                        }
                    }
                }

                if (forceDateTimesToUtc && converter == null && srcType == typeof (DateTime) && (dstType == typeof (DateTime) || dstType == typeof (DateTime?)))
                {
                    converter = src => new DateTime(((DateTime) src).Ticks, DateTimeKind.Utc);
                }

                if (converter == null)
                {
                    try
                    {
                        if (dstType.IsEnum && IsIntegralType(srcType))
                        {
                            if (srcType != typeof (int))
                            {
                                converter = src => Convert.ChangeType(src, typeof (int), null);
                            }
                        }
                        else if (Nullable.GetUnderlyingType(dstType) != null)
                        {
                            var ty = Nullable.GetUnderlyingType(dstType);
                            converter = src => Convert.ChangeType(src, ty, null);
                        }
                        else if (!dstType.IsAssignableFrom(srcType))
                        {
                            converter = src => Convert.ChangeType(src, dstType, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException(ex.Message + "; Unable to cast " + dstType.Name + " to " + srcType, -1);
                    }
                }
                return converter;
            }


            private static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
            {
                while (t != null)
                {
                    T info = cb(t);
                    if (info != null)
                        return info;
                    t = t.BaseType;
                }
                return default(T);
            }


            private static readonly Dictionary<Type, PocoData> m_PocoDatas = new Dictionary<Type, PocoData>();
            private static readonly List<Func<object, object>> m_Converters = new List<Func<object, object>>();
            private static readonly MethodInfo fnGetValue = typeof (IDataRecord).GetMethod("GetValue", new Type[] {typeof (int)});
            private static readonly MethodInfo fnIsDBNull = typeof (IDataRecord).GetMethod("IsDBNull");
            private static readonly FieldInfo fldConverters = typeof (PocoData).GetField("m_Converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
            private static readonly MethodInfo fnListGetItem = typeof (List<Func<object, object>>).GetProperty("Item").GetGetMethod();
            private static readonly MethodInfo fnInvoke = typeof (Func<object, object>).GetMethod("Invoke");
            public Type type;
            public string[] QueryColumns { get; private set; }
            public TableInfo TableInfo { get; private set; }
            public Dictionary<string, PocoColumn> Columns { get; private set; }
            private Dictionary<string, Delegate> PocoFactories = new Dictionary<string, Delegate>();
        }


        // Member variables
        private readonly string _connectionString;
        private readonly string _providerName;
        private DbProviderFactory _factory;
        private IDbConnection _sharedConnection;
        private IDbTransaction _transaction;
        private int _sharedConnectionDepth;
        private int _transactionDepth;
        private bool _transactionCancelled;
        private string _lastSql;
        private object[] _lastArgs;
        private string _paramPrefix = "@";
    }

    public class Transaction : IDisposable
    {
        public Database Db
        {
            get { return _db; }
            set { _db = value; }
        }

        public Transaction(Database db)
        {
            _db = db;
            _db.BeginTransaction();
            Db = _db;
        }

        public virtual void Complete()
        {
            _db.CompleteTransaction();
            _db = null;
        }

        public virtual void Rollback()
        {
            _db.RollbackTransaction();
            _db = null;
        }

        public virtual void Commit()
        {
            _db.CommitTransaction();
            _db = null;
        }

        public virtual void Abort()
        {
            _db.AbortTransaction();
            _db = null;
        }

        public void Dispose()
        {
            _db?.AbortTransaction();
        }

        private Database _db;
    }

    public class Sql
    {
        public Sql()
        {
        }

        public Sql(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        public static Sql Builder => new Sql();

        public string FromTable { get; set; }

        private readonly string _sql;
        private readonly object[] _args;
        private Sql _rhs;
        private string _sqlFinal;
        private object[] _argsFinal;

        private void Build()
        {
            if (_sqlFinal != null)
                return;

            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        public Sql Append(Sql sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            return this;
        }

        public Sql Append(string sql, params object[] args)
        {
            return Append(new Sql(sql, args));
        }

        private static bool Is(Sql sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, Sql lhs)
        {
            if (!string.IsNullOrEmpty(_sql))
            {
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = Database.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }

            _rhs?.Build(sb, args, this);
        }

        public Sql Where(string sql, params object[] args)
        {
            return Append(new Sql("WHERE (" + sql + ")", args));
        }

        public Sql OrderBy(params object[] columns)
        {
            return Append(new Sql("ORDER BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql Select(params object[] columns)
        {
            return Append(new Sql("SELECT " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql From(string tableName, bool withNoLock = true)
        {
            return Append(new Sql("FROM " + tableName + (withNoLock ? " WITH(NOLOCK) " : "")));
        }

        public Sql From(params object[] tables)
        {
            return Append(new Sql("FROM " + string.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        public Sql GroupBy(params object[] columns)
        {
            return Append(new Sql("GROUP BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string JoinType, string table)
        {
            return new SqlJoinClause(Append(new Sql(JoinType + table)));
        }

        public SqlJoinClause InnerJoin(string table)
        {
            return Join("INNER JOIN ", table);
        }

        public SqlJoinClause LeftJoin(string table)
        {
            return Join("LEFT JOIN ", table);
        }

        public class SqlJoinClause
        {
            private readonly Sql _sql;

            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            public Sql On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }
    }
}
