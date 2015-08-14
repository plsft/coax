using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Helix.Infra.Peta;

namespace Helix.Data
{
    public abstract partial  class HelixPetaRepository : IHelixPetaRepository, IDisposable
    {
        private readonly Database db;

        protected HelixPetaRepository(string connectionStringName, bool keepConnectionAlive = false)
        {
            db = new Database(connectionStringName)
            {
                KeepConnectionAlive = keepConnectionAlive
            };
        }

        protected HelixPetaRepository(bool keepAliveConnection)
        {
            var collection = System.Web.Configuration.WebConfigurationManager.ConnectionStrings ?? System.Configuration.ConfigurationManager.ConnectionStrings;

            if (collection[1].Name == null)
                throw new Exception("Unable to detect connection string in app.config or web.config! Default connection string name is \'DbConnection.\'");

            db = new Database(collection[1].Name)
            {
                KeepConnectionAlive = keepAliveConnection
            };
        }

        protected HelixPetaRepository()
        {
            if (ConfigurationManager.ConnectionStrings[1].Name == null)
                throw new Exception("Unable to detect connection string in app.config or web.config! Default connection string name is \'DbConnection.\'");

            db = new Database(ConfigurationManager.ConnectionStrings[1].Name)
            {
                KeepConnectionAlive = false
            };
        }

        public TPassType Single<TPassType>(object primaryKey)
        {
            return db.Single<TPassType>(primaryKey);
        }

        public TPassType First<TPassType>(object primaryKey)
        {
            //to do - make Oracle, MySql and CE compat. 
            var pd = Database.PocoData.ForType(typeof (TPassType));
            var sql = "SELECT TOP 1 * from " + pd.TableInfo.TableName + " with(nolock) order by ID asc";
            return db.Query<TPassType>(new Sql(sql)).SingleOrDefault();
        }

        public TPassType Last<TPassType>(object primaryKey)
        {
            //to do - make Oracle, MySql and CE compat. 
            var pd = Database.PocoData.ForType(typeof(TPassType));
            var sql = "SELECT TOP 1 * from " + pd.TableInfo.TableName + " with(nolock) order by ID desc";
            return db.Query<TPassType>(new Sql(sql)).SingleOrDefault();
        }

        public TPassType FirstBy<TPassType>(object primaryKey, string sortKeyName)
        {
            //to do - make Oracle, MySql and CE compat. 
            var pd = Database.PocoData.ForType(typeof(TPassType));
            var sql = string.Format("SELECT TOP 1 * from {0} with(nolock) order by {1} asc",pd.TableInfo.TableName, sortKeyName);
            return db.Query<TPassType>(new Sql(sql)).SingleOrDefault();
        }

        public TPassType LastBy<TPassType>(object primaryKey, string sortKeyName)
        {
            //to do - make Oracle, MySql and CE compat. 
            var pd = Database.PocoData.ForType(typeof(TPassType));
            var sql = string.Format("SELECT TOP 1 * from {0} with(nolock) order by {1} desc", pd.TableInfo.TableName, sortKeyName);
            return db.Query<TPassType>(new Sql(sql)).SingleOrDefault();
        }

        public IEnumerable<TPassType> Query<TPassType>()
        {
            var pd = Database.PocoData.ForType(typeof (TPassType));

            var sql = "SELECT * FROM " + pd.TableInfo.TableName;

            return db.Query<TPassType>(sql);
        }

        public IEnumerable<TPassType> Query<TPassType>(string where, string orderBy = "", int limit = 0, string columns = "*", params object[] args)
        {
            var pd = Database.PocoData.ForType(typeof (TPassType));

            string sql = BuildSql(pd.TableInfo.TableName, where, orderBy, limit, columns);

            return Query<TPassType>(sql, args);
        }

        public IEnumerable<TPassType> Query<TPassType>(string sql, params object[] args)
        {
            return db.Query<TPassType>(sql, args);
        }

        public Page<TPassType> PagedQuery<TPassType>(long pageNumber, long itemsPerPage, string sql, params object[] args)
        {
            return db.Page<TPassType>(pageNumber, itemsPerPage, sql, args) as Page<TPassType>;
        }

        public Page<TPassType> PagedQuery<TPassType>(long pageNumber, long itemsPerPage, Sql sql)
        {
            return db.Page<TPassType>(pageNumber, itemsPerPage, sql) as Page<TPassType>;
        }

        public object Insert(object poco)
        {
            OnBeforeInsert(poco);
            var ret = db.Insert(poco);
            OnAfterInsert(poco);
            return ret;
        }

        public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
        {
            OnBeforeInsert(poco);
            var ret = db.Insert(tableName, primaryKeyName, autoIncrement, poco);
            OnAfterInsert(poco);
            return ret;
        }

        public object Insert(string tableName, string primaryKeyName, object poco)
        {
            OnBeforeInsert(poco);
            var ret = db.Insert(tableName, primaryKeyName, poco);
            OnAfterInsert(poco);
            return ret;
        }

        public object Update(object poco)
        {
            OnBeforeUpdate(poco);
            var ret= db.Update(poco);
            OnAfterUpdate(poco);
            return ret;
        }

        public object Update(object poco, object primaryKeyValue)
        {
            OnBeforeUpdate(poco);
            var ret= db.Update(poco, primaryKeyValue);
            OnAfterUpdate(poco);
            return ret; 
        }

        public object Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            OnBeforeUpdate(poco);
            var ret= db.Update(tableName, primaryKeyName, poco, primaryKeyValue);
            OnAfterUpdate(poco);
            return ret;
        }

        public bool Delete<TPassType>(object pocoOrPrimaryKey)
        {
            OnBeforeDelete(pocoOrPrimaryKey);
            var ret= db.Delete<TPassType>(pocoOrPrimaryKey) > 0;
            OnAfterDelete(pocoOrPrimaryKey);
            return ret;
        }

        public bool Delete<TPassType>(string domainSql, params object[] args)
        {
            OnBeforeDelete(null);
            var ret = db.Delete<TPassType>(domainSql, args);
            OnAfterDelete(null);
            return ret!=0;
        }

        public bool Delete<TPassType>(string tableName, object pocoOrPrimaryKey)
        {
            OnBeforeDelete(pocoOrPrimaryKey);
            var ret = db.Delete<TPassType>(tableName, pocoOrPrimaryKey) > 0;
            OnAfterDelete(pocoOrPrimaryKey);
            return ret;
        }

        public bool Delete<TPassType>(string tableName, string primaryKeyIdName, object pocoOrPrimaryKey)
        {
            OnBeforeDelete(pocoOrPrimaryKey);
            var ret= db.Delete<TPassType>(tableName, primaryKeyIdName, pocoOrPrimaryKey) > 0;
            OnAfterDelete(pocoOrPrimaryKey);
            return ret;
        }

        public static string BuildSql(string tableName, string where = "", string orderBy = "", int limit = 0, string columns = "*")
        {
            string sql = limit > 0 ? "SELECT TOP " + limit + " {0} FROM {1} " : "SELECT {0} FROM {1} ";
            if (!string.IsNullOrEmpty(where))
                sql += where.Trim().StartsWith("where", StringComparison.CurrentCultureIgnoreCase) ? where : "WHERE " + where;
            if (!String.IsNullOrEmpty(orderBy))
                sql += orderBy.Trim().StartsWith("order by", StringComparison.CurrentCultureIgnoreCase) ? orderBy : " ORDER BY " + orderBy;
            return string.Format(sql, columns, tableName);
        }

        public virtual void OnBeforeInsert(object poco)
        {
        }

        public virtual void OnAfterInsert(object poco)
        {
        }

        public virtual void OnBeforeUpdate(object poco)
        {
        }

        public virtual void OnAfterUpdate(object poco)
        {
        }

        public virtual void OnBeforeDelete(object poco)
        {
        }

        public virtual void OnAfterDelete(object poco)
        {
        }

        public virtual void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }
    }
}
