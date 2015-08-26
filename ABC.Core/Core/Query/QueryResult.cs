using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core
{
    public class QueryResult<T>
    {
        /// <summary>
        /// The _result
        /// </summary>
        private readonly QuerySource<SQlQuery> _result;

        /// <summary>
        /// Gets the SQL.
        /// </summary>
        /// <value>
        /// The SQL.
        /// </value>
        public SQlQuery Sql
        {
            get
            {
                return _result.Sql;
            }
            set { _result.Sql = value; }
        }

        ///// <summary>
        ///// Gets the param.
        ///// </summary>
        ///// <value>
        ///// The param.
        ///// </value>
        //public dynamic Param
        //{
        //    get
        //    {
        //        return _result.Param;
        //    }
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult" /> class.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="param">The param.</param>
        public QueryResult(SQlQuery sql)
        {
            _result = new QuerySource<SQlQuery>(sql);
        }
    }

    public class QuerySource<T>
    {
        public QuerySource(SQlQuery sql)
        {
            Sql = sql;
        }

        public SQlQuery Sql { get; set; }
    }


    public class SQlQuery
    {
        public List<string> JoinContidion { get; set; }
        public List<string> WhereCondition { get; set; }
        public List<QueryParameter> QueryParameters { get; set; }

        public string Select { get; set; }
        public string From { get; set; }
        public string Join
        {
            get
            {
                if (JoinContidion == null && !JoinContidion.Any())
                    return string.Empty;
                return string.Join(Environment.NewLine, JoinContidion.Distinct());
            }
        }
        public string SpecialWhere
        {
            get
            {
                if (WhereCondition == null && !WhereCondition.Any())
                    return string.Empty;
                return string.Join(" AND ", WhereCondition.Distinct());
            }
        }

        public List<string> OrderItems { get; set; }
        public string OrderBy
        {
            get
            {
                if (OrderItems == null || !OrderItems.Any())
                    return string.Empty;
                return "ORDER BY " + string.Join(",", OrderItems);
            }
        }


        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string Limit
        {
            get
            {
                if (!Skip.HasValue)
                    return string.Empty;
                string sql = "OFFSET " + Skip + " ROWS";
                if (Take.HasValue)
                    sql += " FETCH NEXT " + Take + " ROWS ONLY ";

                return sql;
            }
        }

        public IDictionary<string, object> Param
        {
            get
            {
                IDictionary<string, Object> expando = new ExpandoObject();
                for (int i = 0; i < this.QueryParameters.Count(); i++)
                {
                    QueryParameter item = this.QueryParameters[i];
                    expando[item.PropertyName.Replace(".", string.Empty)] = item.PropertyValue;
                }
                return expando;
            }
        }
        public string Query
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("SELECT " + this.Select + " FROM ");
                builder.AppendLine(this.From);
                builder.AppendLine(this.Join);

                List<QueryParameter> normalWhere = this.QueryParameters;
                if (!string.IsNullOrEmpty(SpecialWhere) || (normalWhere != null && normalWhere.Any()))
                    builder.AppendLine("WHERE");
                builder.AppendLine(SpecialWhere);


                for (int i = 0; i < normalWhere.Count(); i++)
                {
                    QueryParameter item = normalWhere[i];
                    if (!string.IsNullOrEmpty(SpecialWhere) || (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0))
                    {
                        builder.AppendLine(string.Format("{0} {1} {2} @{3} ", item.LinkingOperator, item.PropertyName,
                                                     item.QueryOperator, item.PropertyName.Replace(".", string.Empty)));
                    }
                    else
                    {
                        builder.AppendLine(string.Format("{0} {1} @{2} ", item.PropertyName, item.QueryOperator, item.PropertyName.Replace(".", string.Empty)));
                    }
                }

                builder.AppendLine(this.OrderBy);
                builder.AppendLine(this.Limit);

                return builder.ToString();
            }
        }


        internal static void BuildQuery(IDictionary<string, object> expando, StringBuilder builder)
        {

        }

        internal SQlQuery(string tableName)
        {
            Select = tableName + ".*";
            From = tableName;
            QueryParameters = new List<QueryParameter>();
            JoinContidion = new List<string>();
            WhereCondition = new List<string>();
            OrderItems = new List<string>();
        }
    }

    public class QueryParameter
    {
        public string LinkingOperator { get; set; }
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
        public string QueryOperator { get; set; }

        internal QueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator)
        {
            this.LinkingOperator = linkingOperator;
            this.PropertyName = propertyName;
            this.PropertyValue = propertyValue;
            this.QueryOperator = queryOperator;
        }
    }

    public static class ExtensionHelper
    {
        public static void AddQueryParameter(this SQlQuery query, QueryParameter parameters)
        {
            query.QueryParameters.Add(parameters);
        }

        public static void AddJoinOperator(this SQlQuery query, List<string> joinOperator)
        {
            if (joinOperator != null && joinOperator.Any())
                query.JoinContidion.AddRange(joinOperator);

            query.JoinContidion = query.JoinContidion.Distinct().ToList();
        }

        public static void AddWhereCondition(this SQlQuery query, string whereCondition)
        {

            if (!string.IsNullOrEmpty(whereCondition))
                query.WhereCondition.Add(whereCondition);
        }
    }
}
