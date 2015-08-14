using System.Collections.Generic;
using Helix.Infra.Peta;

namespace Helix.Data
{
    public interface IHelixPetaRepository
    {
        T Single<T>(object primaryKey);
        T First<T>(object primaryKey);
        T Last<T>(object primaryKey);
        T FirstBy<T>(object primaryKey, string sortKey);
        T LastBy<T>(object primaryKey, string sortKey);
        IEnumerable<T> Query<T>();
        IEnumerable<T> Query<T>(string domainSql, params object[] args);
        Page<T> PagedQuery<T>(long pageNumber, long itemsPerPage, string sql, params object[] args);
        object Insert(object itemToAdd);
        object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco);
        object Insert(string tableName, string primaryKeyName, object poco);
        object Update(object itemToAdd);
        object Update(object itemToUpdate, object primaryKeyValue);
        object Update(string tableName, string primaryKeyName, object itemToUpdate, object primaryKeyValue);
        
        bool Delete<T>(object primaryKeyValue);
        bool Delete<T>(string domainSql, params object[] args);
        bool Delete<TPassType>(string tableName, object pocoOrPrimaryKey);
        bool Delete<TPassType>(string tableName, string primaryKeyIdName, object pocoOrPrimaryKey);

    }
}
