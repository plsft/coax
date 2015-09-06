using System.Collections.Generic;
using System.Linq;
using System.Web;
using Helix.Infra.Peta;

namespace Helix.Utility
{
    public class Db
    {
        private readonly Database _db;
        private readonly object o;
        
        public Db(string connectionStringName)
        {
            _db = new Database(connectionStringName);
            o = new object();
        }

        public  IEnumerable<dynamic> Fetch(string query, params object[] args)
        {
            lock (o)
            {
                return _db.Fetch<dynamic>(query, args).ToList();
            }
        }

        public  IEnumerable<string> FetchString(string query, params object[] args)
        {
            lock (o)
            {
                return _db.Fetch<string>(query, args).ToList();
            }
        }

        public IEnumerable<int> FetchInt(string query, params object[] args)
        {
            lock (o)
            {
                return _db.Fetch<int>(query, args).ToList();
            }
        }


        public int ExecuteSql(string query, params object[] args)
        {
            lock (o)
            {
                return _db.Execute(query, args);
            }
        }

        public IEnumerable<T> Query<T>(string q, params object[] args)
        {
            lock (o)
            {
                try
                {
                    return _db.Query<T>(new Sql(q, args));
                }
                catch
                {
                    return new List<T>();
                }
            }
        }

        public T ExecuteScalar<T>(string q, params object[] args)
        {
            lock (o)
            {
                try
                {
                    return _db.ExecuteScalar<T>(q, args);
                }
                catch
                {
                    return default(T);
                }
            }
        }
    }
}
