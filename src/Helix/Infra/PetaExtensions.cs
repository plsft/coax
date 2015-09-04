using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Helix.Infra
{
    public static class PetaExtensions
    {
        /// <summary>
        /// Assumes default values are "" (string), null for all other types
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool HasValue(this object f)
        {
            if (f == null)
                return false;

            switch (f.GetType().Name.ToLower())
            {
                case "string":
                    return !string.IsNullOrEmpty(Convert.ToString(f));
                case "datetime":
                    return Convert.ToDateTime(f) > DateTime.MinValue;

                default:
                    return true;
            }
        }

        public static IEnumerable<dynamic> AsDynamicList(this DataTable dt)
        {
            if (dt == null)
                return null;

            var result = new List<dynamic>();
            foreach (DataRow r in dt.Rows)
            {
                dynamic d = new ExpandoObject();
                foreach (DataColumn c in dt.Columns)
                {
                    var dictionary = (IDictionary<string, object>) d;
                    dictionary[c.ColumnName] = r[c];
                    result.Add(dictionary);
                }
            }

            return result;
        }

        public static void SetColumnsOrder(this DataTable table, params string[] columnNames)
        {
            for (var columnIndex = 0; columnIndex < columnNames.Length && columnIndex < table.Columns.Count; columnIndex++)
            {
                var tableColumn = table.Columns[columnNames[columnIndex]];

                if (tableColumn != null)
                    tableColumn.SetOrdinal(columnIndex);
                else
                    table.Columns.Add(new DataColumn(columnNames[columnIndex]));
            }
        }

        public static string ToCsv(this DataTable dt)
        {
            var sb = new StringBuilder();

            var columnNames = dt.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            return sb.ToString();
        }

        public static string ToHtml(this DataTable dt)
        {
            var html = "<table>";

            html += "<tr>";
            for (var i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";

            for (var i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (var j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j] + "</td>";
                html += "</tr>";
            }

            html += "</table>";
            return html;
        }

        public static DataTable ToDataTable<T>(IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof (T));
            var table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                var row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, string tableName)
        {
            var dt = new DataTable(tableName);
            var t = typeof (T);
            var pia = t.GetProperties();

            foreach (var pi in pia)
            {
                dt.Columns.Add(pi.Name, pi.PropertyType);
            }

            foreach (T item in collection)
            {
                var dr = dt.NewRow();
                dr.BeginEdit();

                foreach (var pi in pia)
                {
                    dr[pi.Name] = pi.GetValue(item, null);
                }

                dr.EndEdit();
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static DataTable Pivot(this DataTable dt, string pivotColumn, string pivotValue, string[] includedColumns = null)
        {
            var pc = new DataColumn(pivotColumn);
            var pv = new DataColumn(pivotValue);

            var temp = dt.Copy();
            temp.Columns.Remove(pc.ColumnName);
            temp.Columns.Remove(pv.ColumnName);
            var pkColumnNames = temp.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            var result = temp.DefaultView.ToTable(true, pkColumnNames).Copy();
            result.PrimaryKey = result.Columns.Cast<DataColumn>().ToArray();
            dt.AsEnumerable().Select(r => r[pc.ColumnName].ToString()).Distinct().ToList().ForEach(c => result.Columns.Add(c, pc.DataType));

            foreach (DataRow row in dt.Rows)
            {
                DataRow aggRow = result.Rows.Find(pkColumnNames.Select(c => row[c]).ToArray());
                aggRow[row[pc.ColumnName].ToString()] = row[pv.ColumnName];
            }

            return result;
        }

        public static DataTable ToTransposedTable(this DataTable inputTable)
        {
            DataTable outputTable = new DataTable();

            outputTable.Columns.Add(inputTable.Columns[0].ColumnName.ToString());

            foreach (DataRow inRow in inputTable.Rows)
            {
                string newColName = inRow[0].ToString();
                outputTable.Columns.Add(newColName);
            }

            for (int rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
            {
                DataRow newRow = outputTable.NewRow();

                newRow[0] = inputTable.Columns[rCount].ColumnName.ToString();
                for (int cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
                {
                    string colValue = inputTable.Rows[cCount][rCount].ToString();
                    newRow[cCount + 1] = colValue;
                }
                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }

        public static DataTable Pivot(this DataTable dt, DataColumn pivotColumn, DataColumn pivotValue)
        {
            var temp = dt.Copy();
            temp.Columns.Remove(pivotColumn.ColumnName);
            temp.Columns.Remove(pivotValue.ColumnName);
            var pkColumnNames = temp.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            var result = temp.DefaultView.ToTable(true, pkColumnNames).Copy();
            result.PrimaryKey = result.Columns.Cast<DataColumn>().ToArray();
            dt.AsEnumerable().Select(r => r[pivotColumn.ColumnName].ToString()).Distinct().ToList().ForEach(c => result.Columns.Add(c, pivotColumn.DataType));

            foreach (DataRow row in dt.Rows)
            {
                DataRow aggRow = result.Rows.Find(pkColumnNames.Select(c => row[c]).ToArray());
                aggRow[row[pivotColumn.ColumnName].ToString()] = row[pivotValue.ColumnName];
            }

            return result;
        }
    }
}
