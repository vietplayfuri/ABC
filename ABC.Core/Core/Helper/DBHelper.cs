using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Core
{
    public static class DBHelper
    {
        #region Doing


        /// <summary>
        /// Create entity - used for tables have primary key
        /// </summary>
        public static int Add<T>(T entity, IDbConnection db)
        {
            List<string> rootFields = new List<string>();

            PropertyInfo[] pinfos = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in pinfos)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                bool isExternal = attrs.Any(a => (a as External) != null);
                if (isExternal) continue;
                rootFields.Add(prop.Name);
            }

            List<string> dapperFields = new List<string>();
            rootFields.ForEach(f => { dapperFields.Add("@" + f); });
            string tableName = QueryHelper.GetTableName<T>();
            StringBuilder query = new StringBuilder();

            query.AppendLine("INSERT INTO " + tableName + " (");
            query.AppendLine(string.Join(",", rootFields) + ")");
            query.AppendLine("Output Inserted." + GetPrimaryIdColumn(typeof(T)));
            query.AppendLine("VALUES(" + string.Join(",", dapperFields) + ")");

            string sqlQuery = query.ToString();

            return db.Query<int>(sqlQuery, entity).FirstOrDefault();
        }


        /// <summary>
        /// Create entity - used for tables don't have primary key
        /// </summary>
        public static bool Add<T>(IDbConnection db, T entity)
        {
            List<string> rootFields = new List<string>();

            PropertyInfo[] pinfos = entity.GetType().GetProperties();
            foreach (PropertyInfo prop in pinfos)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                bool isExternal = attrs.Any(a => (a as External) != null);
                if (isExternal) continue;
                rootFields.Add(prop.Name);
            }

            List<string> dapperFields = new List<string>();
            rootFields.ForEach(f => { dapperFields.Add("@" + f); });
            string tableName = QueryHelper.GetTableName<T>();
            StringBuilder query = new StringBuilder();

            query.AppendLine("INSERT INTO " + tableName + " (");
            query.AppendLine(string.Join(",", rootFields) + ")");
            //query.AppendLine("Output Inserted." + GetPrimaryIdColumn(tableName));
            query.AppendLine("VALUES(" + string.Join(",", dapperFields) + ")");

            string sqlQuery = query.ToString();

            return 1 == db.Execute(sqlQuery, entity);
        }


        /// <summary>
        /// Update entity directly
        /// </summary>
        public static bool Update<T>(T entity)
        {
            return true;
        }

        /// <summary>
        /// Update entity with transaction
        /// </summary>
        public static bool Update<T>(T entity, IDbConnection db)
        {
            return true;
        }


        /// <summary>
        /// Delete entity with transaction
        /// </summary>
        public static bool Delete<T>(int Id, IDbConnection db)
        {
            return true;
        }


        /// <summary>
        /// Delete entity directly
        /// </summary>
        public static bool Delete<T>(int Id)
        {
            return true;
        }


        //public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
        //public static int Count<TSource>(this IEnumerable<TSource> source);

        //public static decimal? Min(this IEnumerable<decimal?> source);
        //public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector);
        //public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector);
        #endregion

        #region Count


        /// <summary>
        /// result[0] = result[1]
        /// </summary>
        private static string[] GetForeignKeyColumn(Type table1, Type table2)
        {
            string[] result = new string[2];
            PropertyInfo[] table1PInfos = table1.GetProperties();
            string table1NameInDb = QueryHelper.GetTableName(table1);
            string table2NameInDb = QueryHelper.GetTableName(table2);

            PropertyInfo pForeignKeyInfosTable1 = table1PInfos.FirstOrDefault(p => p.GetCustomAttributes(true)
                .Any(a => (a as ForeignKeyAttribute) != null && (a as ForeignKeyAttribute).Name == table2NameInDb));
            if (pForeignKeyInfosTable1 != null)
            {
                result[0] = table1NameInDb + '.' + pForeignKeyInfosTable1.Name;
                result[1] = table2NameInDb + '.' + pForeignKeyInfosTable1.Name;
                return result;
            }
            else
            {
                PropertyInfo[] table2PInfos = table2.GetProperties();
                PropertyInfo pForeignKeyInfosTable2 = table2PInfos.FirstOrDefault(p => p.GetCustomAttributes(true)
                    .Any(a => (a as ForeignKeyAttribute) != null && (a as ForeignKeyAttribute).Name == table1NameInDb));
                if (pForeignKeyInfosTable2 != null)
                {
                    result[0] = table1NameInDb + '.' + pForeignKeyInfosTable1.Name;
                    result[1] = table2NameInDb + '.' + pForeignKeyInfosTable1.Name;
                    return result;
                }

                //External fields of table 1
                //search id of table 2 in table 1
                PropertyInfo primaryKeyTable2 = table2PInfos.FirstOrDefault(p => string.Compare(p.Name, "id", true) == 0);
                if (primaryKeyTable2 == null)
                    primaryKeyTable2 = table2PInfos.FirstOrDefault(p => p.GetCustomAttributes(true).Any(a => (a as PrimaryKey) != null));
                if (primaryKeyTable2 == null)
                    throw new Exception("Could not found the primary key in entity: " + table1.FullName);

                PropertyInfo allPExternalTable2InTable1 = table1PInfos.FirstOrDefault(p => string.Compare(p.Name, primaryKeyTable2.Name, true) == 0);

                if (allPExternalTable2InTable1 != null) // primary key of table 2 is existed in table 1
                {
                    result[0] = table1NameInDb + '.' + primaryKeyTable2.Name;
                    result[1] = table2NameInDb + '.' + primaryKeyTable2.Name;
                    return result;
                }
                else
                {
                    //search id of table 1 in table 2
                    PropertyInfo primaryKeyTable1 = table1PInfos.FirstOrDefault(p => string.Compare(p.Name, "id", true) == 0);
                    if (primaryKeyTable1 == null)
                        primaryKeyTable1 = table1PInfos.FirstOrDefault(p => p.GetCustomAttributes(true).Any(a => (a as PrimaryKey) != null));
                    if (primaryKeyTable1 == null)
                        throw new Exception("Could not found the primary key in entity: " + table1.FullName);

                    PropertyInfo pExternalTable1InTable2 = table2PInfos.FirstOrDefault(p => string.Compare(p.Name, primaryKeyTable1.Name, true) == 0);
                    if (pExternalTable1InTable2 != null)
                    {
                        result[0] = table1NameInDb + '.' + primaryKeyTable1.Name;
                        result[1] = table2NameInDb + '.' + primaryKeyTable1.Name;
                        return result;
                    }
                    else
                        throw new Exception("Could not found the foreign key between two entities: " + table1.FullName + " and " + table2.FullName);
                }
            }
        }

        private static string GetPrimaryIdColumn(Type table)
        {
            PropertyInfo[] table1PInfos = table.GetProperties();
            PropertyInfo primaryKeyTable1 =
                table1PInfos.FirstOrDefault(p => string.Compare(p.Name, "id", true) == 0);
            if (primaryKeyTable1 == null)
                primaryKeyTable1 = table1PInfos.FirstOrDefault(p => p.GetCustomAttributes(true).Any(a => (a as PrimaryKey) != null));
            if (primaryKeyTable1 == null)
                throw new Exception("Count not find the primaryKey of table: " + table.FullName);

            return primaryKeyTable1.Name;
        }


        /// <summary>
        /// "Count" keyword is used without expression -- 
        /// Way to use: [Table].Count()
        /// </summary>
        public static int Count<T>(IDbConnection db)
        {
            SQlQuery query = new SQlQuery(QueryHelper.GetTableName<T>());
            string newSql = @"SELECT COUNT(1) FROM ( " + query.Query + " ) as Extend";
            var result = db.Query<int>(newSql).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// "Count" keyword is used without expression -- 
        /// Way to use: [Table].Count(expression)
        /// </summary>
        public static int Count<T>(Expression<Func<T, bool>> expression, IDbConnection db)
        {
            var body = expression.Body as BinaryExpression;
            SQlQuery query = new SQlQuery(QueryHelper.GetTableName<T>());
            WalkTree<T>(expression, ref query);

            string newSql = @"SELECT COUNT(1) FROM ( " + query.Query + " ) as Extend";
            var result = db.Query<int>(newSql, (object)query.Param).FirstOrDefault();
            return result;
        }
        #endregion

        #region Order
        private static QueryResult<TSource> CommonOrder<TSource, TKey>(QueryResult<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isASC)
        {
            string orderBy = string.Empty;
            string tableName = QueryHelper.GetTableName<TSource>();
            //item.Member.Name -- name of properties
            string[] expression = keySelector.Body.ToString().Split('.');
            int count = expression.Count();
            if (count == 2)
                orderBy = tableName + "." + expression[count - 1];
            else
            {
                string[] conditions = new string[expression.Count()];
                GetExactlyNameParam(typeof(TSource), expression, ref conditions);
                orderBy = string.Join(".", conditions.Where(c => !string.IsNullOrEmpty(c)));
            }
            List<string> joinOperator = new List<string>();
            if (string.Compare(tableName, orderBy.Split('.')[0]) != 0)
            {
                GetJoinOperation(typeof(TSource), orderBy, ref joinOperator);
            }


            source.Sql.AddJoinOperator(joinOperator);
            var orders = orderBy.Split('.');
            if (orders.Count() > 2)
                orderBy = orders[orders.Length - 2] + '.' + orders[orders.Length - 1];
            source.Sql.OrderItems.Add(orderBy + (isASC ? " ASC " : " DESC "));
            return source;
        }

        public static QueryResult<TSource> OrderBy<TSource, TKey>(this QueryResult<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return CommonOrder(source, keySelector, true);
        }

        public static QueryResult<TSource> OrderByDescending<TSource, TKey>(this QueryResult<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return CommonOrder(source, keySelector, false);
        }

        public static QueryResult<TSource> ThenBy<TSource, TKey>(this QueryResult<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return CommonOrder(source, keySelector, true);
        }

        public static QueryResult<TSource> ThenByDescending<TSource, TKey>(this QueryResult<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            return CommonOrder(source, keySelector, false);
        }
        #endregion

        #region Skip - Take - work with sql server 2012 >>
        /// <summary>
        /// It is work with sql server 2012 >>
        /// </summary>
        public static QueryResult<TSource> Skip<TSource>(this QueryResult<TSource> source, int count)
        {
            if (string.IsNullOrEmpty(source.Sql.OrderBy))
                throw new Exception("The method 'OrderBy' must be called before the method 'Skip'.");

            source.Sql.Skip = count;
            return source;
        }

        /// <summary>
        /// It is work with sql server 2012 >>
        /// </summary>
        public static QueryResult<TSource> Take<TSource>(this QueryResult<TSource> source, int count)
        {
            if (string.IsNullOrEmpty(source.Sql.OrderBy))
                throw new Exception("The method 'OrderBy' must be called before the method 'Take'.");

            if (!source.Sql.Skip.HasValue)
                throw new Exception("The method 'Skip' must be called before the method 'Take'.");

            source.Sql.Take = count;
            return source;
        }
        #endregion

        #region Any
        /// <summary>
        /// "Any" keyword is used with expression
        /// Way to use:
        /// Any[Table](x=> x.id = 10);
        /// Find[Table](x=> x.id = 10).Any(x=>x.id = 10);
        /// </summary>
        public static bool Any<T>(IDbConnection db, Expression<Func<T, bool>> expression)
        {
            var body = expression.Body as BinaryExpression;
            SQlQuery query = new SQlQuery(QueryHelper.GetTableName<T>());
            WalkTree<T>(expression, ref query);

            string newSql = @"SELECT CASE WHEN ( EXISTS ( " + query.Query + " )) THEN cast(1 as bit) ELSE cast(0 as bit) END";
            var result = db.Query<bool>(newSql, (object)query.Param).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// "Any" keyword is used without expression
        /// Way to use:
        /// Condition[Table].Any()
        /// </summary>
        public static bool Any<T>(IDbConnection db)
        {
            SQlQuery sql = new SQlQuery(QueryHelper.GetTableName<T>());
            string newSql = @"SELECT CASE WHEN ( EXISTS ( " + sql.Query + " )) THEN cast(1 as bit) ELSE cast(0 as bit) END";
            var result = db.Query<bool>(newSql).FirstOrDefault();
            return result;
        }
        #endregion

        #region Get
        public static QueryResult<T> Get<T>()
        {
            SQlQuery query = new SQlQuery(QueryHelper.GetTableName<T>());
            return new QueryResult<T>(query);
        }

        public static QueryResult<T> Get<T>(Expression<Func<T, bool>> expression)
        {
            var body = expression.Body as BinaryExpression;
            SQlQuery query = new SQlQuery(QueryHelper.GetTableName<T>());
            WalkTree<T>(expression, ref query);

            return new QueryResult<T>(query);
        }
        #endregion

        #region Select
        public static QueryResult<TResult> Select<TSource, TResult>(this QueryResult<TSource> source, Expression<Func<TSource, TResult>> keySelector)
        {
            var body = keySelector.Body as MemberInitExpression;
            if (body == null)
                throw new Exception("This function is not supported anonymous type.");

            string tableName = QueryHelper.GetTableName<TSource>();
            List<string> selected = new List<string>();
            foreach (var item in body.Bindings)
            {
                //item.Member.Name -- name of properties
                string[] expression = item.ToString().Split('.');
                bool isRemoved = false;
                for (int i = 0; i < expression.Count(); i++)
                {
                    if (isRemoved)
                        expression[i] = string.Empty;

                    if (expression[i].Contains("()"))
                    {
                        expression[i] = string.Empty;
                        isRemoved = true;
                    }
                }

                expression = expression.Where(e => !string.IsNullOrEmpty(e)).ToArray();
                int count = expression.Count();
                if (count == 2)
                    selected.Add(tableName + "." + expression[count - 1]);
                else
                {
                    var slides = item.ToString().Split(new char[] { '.' });
                    string[] conditions = new string[slides.Count()];
                    GetExactlyNameParam(typeof(TSource), slides, ref conditions);
                    selected.Add(string.Join(".", conditions.Where(c => !string.IsNullOrEmpty(c))));
                }
            }

            List<string> joinOperator = new List<string>();

            foreach (var item in selected)
                if (string.Compare(tableName, item.Split('.')[0]) != 0)
                    GetJoinOperation(typeof(TSource), item, ref joinOperator);

            source.Sql.AddJoinOperator(joinOperator);
            for (int i = 0; i < selected.Count(); i++)
            {
                string[] splited = selected[i].Split('.');
                if (splited.Count() > 2)
                    selected[i] = splited[splited.Count() - 2] + "." + splited.LastOrDefault();
            }

            string select = string.Join(", ", selected);
            source.Sql.Select = select;
            QueryResult<TResult> result = new QueryResult<TResult>(source.Sql);
            return result;
        }

        //TODO: can do it later
        //public static QueryResult<T> Select<T>(Expression<Func<T, bool>> keySelector)
        //{
        //    SQlQuery source = new SQlQuery(HelperExtension.GetTableName<TSource>());

        //    string orderBy = keySelector.Body.ToString().Split('.')[1];
        //    var body = (MemberInitExpression)keySelector.Body;
        //    string outputObject = typeof(TResult).Name;
        //    string sourceObject = typeof(TSource).Name;

        //    List<string> selected = new List<string>();
        //    var rootInstance = Activator.CreateInstance(Type.GetType(typeof(TSource).FullName));
        //    foreach (var item in body.Bindings)
        //    {
        //        //item.Member.Name -- name of properties
        //        string[] expression = item.ToString().Split('.');
        //        int count = expression.Count();
        //        if (count == 2)
        //            selected.Add(sourceObject + "." + expression[count - 1]);
        //        else
        //        {
        //            var slides = item.ToString().Split(new char[] { '.' });
        //            string[] conditions = new string[slides.Count()];
        //            GetExactlyNameParam(typeof(TSource), slides, ref conditions);
        //            selected.Add(string.Join(".", conditions.Where(c => !string.IsNullOrEmpty(c))));
        //        }
        //    }

        //    List<string> joinOperator = new List<string>();
        //    string tableName = HelperExtension.GetTableName<TSource>();
        //    foreach (var item in selected)
        //    {
        //        if (string.Compare(tableName, item.Split('.')[0]) != 0)
        //        {
        //            GetJoinOperation(tableName, item, ref joinOperator);
        //        }
        //    }

        //    source.AddJoinOperator(joinOperator);


        //    for (int i = 0; i < selected.Count(); i++)
        //    {
        //        string[] splited = selected[i].Split('.');
        //        if (splited.Count() > 2)
        //        {
        //            selected[i] = splited[splited.Count() - 2] + "." + splited.LastOrDefault();
        //        }
        //    }

        //    string select = string.Join(", ", selected);
        //    source.Select = select;
        //    QueryResult<TResult> result = new QueryResult<TResult>(source);
        //    return result;
        //}
        #endregion


        #region Return results
        public static TSource FirstOrDefault<TSource>(this QueryResult<TSource> source, IDbConnection db)
        {
            source.Sql.Select = "TOP 1 " + source.Sql.Select;
            var result = db.Query<TSource>(source.Sql.Query, (object)source.Sql.Param).FirstOrDefault();
            return result;
        }
        public static List<TSource> ToList<TSource>(this QueryResult<TSource> source, IDbConnection db)
        {
            var result = db.Query<TSource>(source.Sql.Query, (object)source.Sql.Param).ToList();
            return result;
        }
        #endregion

        #region Support query
        private static void WalkTree<T>(Expression<Func<T, bool>> exp, ref SQlQuery query)
        {
            var expression = exp.Body;
            var body = expression as BinaryExpression;
            BinaryExpression binary = expression as BinaryExpression;
            UnaryExpression unary = expression as UnaryExpression;
            MethodCallExpression methodCall;
            if (binary == null)
                if (unary != null)
                {
                    methodCall = unary.Operand as MethodCallExpression;
                }
                else
                {
                    methodCall = expression as MethodCallExpression;
                    string strUnary = unary.ToString();
                    string[] arrUnary = unary.ToString().Split('(');
                    string condition = string.Empty;
                    GetCondition<T>(string.Empty, arrUnary, ref condition, ref query);
                }

            WalkTree<T>(body, ExpressionType.Default, ref query);
        }

        private static void GetCondition<T>(string linkOperator, string[] arrUnary, ref string condition, ref SQlQuery query)
        {

            switch (arrUnary.FirstOrDefault())
            {
                case "Not":
                    condition += " NOT {0}";
                    break;
                case "IsNullOrEmpty":
                    string value = arrUnary[1].Replace(")", string.Empty);

                    List<string> joinOperator = new List<string>();
                    string whereCondition = string.Empty;
                    string[] arrValue = value.Split('.');
                    if (arrValue.Count() > 1)
                    {
                        var replacedTable = value[0];
                        string newProperties = string.Empty;
                        for (int i = 0; i < arrValue.Count(); i++)
                        {
                            if (i == 0)
                            {
                                continue;
                            }
                            else if (i == arrValue.Count() - 1)
                                newProperties += arrValue[i];
                            else
                            {
                                newProperties += arrValue[i];
                                newProperties += '.';
                            }
                        }
                        string tableName = QueryHelper.GetTableName<T>();
                        GetJoinOperation(typeof(T), newProperties, ref joinOperator);
                        GetWhereOperation(linkOperator, newProperties, ref whereCondition);
                    }

                    if (!string.IsNullOrEmpty(condition))
                    {
                        whereCondition = string.Format(condition, whereCondition);
                    }

                    query.AddJoinOperator(joinOperator);
                    query.AddWhereCondition(whereCondition);
                    condition = string.Empty;
                    break;
            }
            arrUnary = arrUnary.Where((value, index) => index != 0).ToArray();
            if (arrUnary != null && arrUnary.Count() > 1)
                GetCondition<T>(linkOperator, arrUnary, ref condition, ref query);
        }


        private static void WalkTree<T>(BinaryExpression body, ExpressionType linkingType, ref SQlQuery query)
        {
            List<QueryParameter> queryProperties = new List<QueryParameter>();
            if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
            {
                string tableName = QueryHelper.GetTableName<T>();
                var slides = body.Left.ToString().Split(new char[] { '.' });
                string[] conditions = new string[slides.Count()];
                GetExactlyNameParam(typeof(T), slides, ref conditions);

                string propertyName = string.Join(".", conditions.Where(c => !string.IsNullOrEmpty(c)));
                if (body.Left.NodeType == ExpressionType.Convert)
                {
                    //Remove the trailing ) when convering.
                    propertyName = propertyName.Replace(")", string.Empty);
                }

                List<string> joinOperator = new List<string>();
                if (propertyName.Contains('.')) //apply join
                    GetJoinOperation(typeof(T), propertyName, ref joinOperator);
                else
                    propertyName = tableName + '.' + propertyName;

                dynamic propertyValue = body.Right;
                string opr = GetOperator(body.NodeType);
                string link = GetOperator(linkingType);

                string[] elements = propertyName.Split(new char[] { '.' });
                if (elements.Count() > 2)
                {
                    string removed = elements[0];
                    propertyName = propertyName.Replace(removed + '.', string.Empty);
                }

                query.AddQueryParameter(new QueryParameter(link, propertyName, propertyValue.Value, opr));
                if (joinOperator != null && joinOperator.Any())
                    query.AddJoinOperator(joinOperator);
            }
            else
            {
                query = BuildCondition<T>(body.Left, body, query);
                query = BuildCondition<T>(body.Right, body, query);
            }
        }

        private static void GetExactlyNameParam(Type type, string[] body, ref string[] condition)
        {
            string[] elements = body;
            elements = elements.Where((value, index) => index != 0).ToArray();

            if (elements.Count() > 1)
            {
                var instance = Activator.CreateInstance(Type.GetType(type.FullName));
                var destinationProperty = instance.GetType().GetProperty(elements[0]);
                var destinationInstance = Activator.CreateInstance(Type.GetType(destinationProperty.PropertyType.FullName));
                var attribute = destinationInstance.GetType().GetCustomAttributes(typeof(TableAttribute), true);

                string tableName = string.Empty;
                if (attribute.Any())
                    tableName = (attribute[0] as TableAttribute).Name;
                else
                    tableName = destinationProperty.PropertyType.Name;

                for (int i = 0; i < condition.Length; i++)
                {
                    if (string.IsNullOrEmpty(condition[i]))
                    {
                        condition[i] = tableName;
                        break;
                    }
                }

                GetExactlyNameParam(destinationInstance.GetType(), elements, ref condition);
            }
            else
            {
                for (int i = 0; i < condition.Length; i++)
                {
                    if (string.IsNullOrEmpty(condition[i]))
                    {
                        condition[i] = elements[0];
                        break;
                    }
                }
            }
        }

        private static SQlQuery BuildCondition<T>(Expression expression, BinaryExpression body, SQlQuery query)
        {
            BinaryExpression binary = expression as BinaryExpression;
            UnaryExpression unary = expression as UnaryExpression;
            MethodCallExpression methodCall;
            if (binary == null)
                if (unary != null)
                {
                    methodCall = unary.Operand as MethodCallExpression;
                    string strUnary = methodCall.ToString();
                    string[] arrUnary = methodCall.ToString().Split('(');
                    string condition = string.Empty;
                    GetCondition<T>(string.Empty, arrUnary, ref condition, ref query);
                }
                else
                {
                    methodCall = expression as MethodCallExpression;
                    string strUnary = methodCall.ToString();
                    string[] arrUnary = methodCall.ToString().Split('(');
                    string condition = string.Empty;
                    GetCondition<T>(string.Empty, arrUnary, ref condition, ref query);
                }
            else
                WalkTree<T>((BinaryExpression)expression, body.NodeType, ref query);
            return query;
        }


        private static void GetJoinOperation(Type rootTableName, string propertyName, ref List<string> joins)
        {
            string[] elements = propertyName.Split(new char[] { '.' });
            string tableName = elements[0];
            PropertyInfo[] table2PInfos = rootTableName.GetProperties();
            PropertyInfo primaryKeyTable2 = table2PInfos.FirstOrDefault(p => string.Compare(QueryHelper.GetTableName(p.PropertyType), tableName, true) == 0);
            string[] keyJoin = GetForeignKeyColumn(rootTableName, primaryKeyTable2.PropertyType);
            StringBuilder joinComment = new StringBuilder();
            joinComment.Append("JOIN ");
            joinComment.Append(tableName);
            joinComment.Append(" ON " + keyJoin[0] + " = " + keyJoin[1]);
            joins.Add(joinComment.ToString());

            if (elements.Count() > 2)
            {
                string newPropertyName = elements[1] + "." + elements[2];
                Type destinationType = rootTableName.GetProperty(tableName).PropertyType;

                GetJoinOperation(destinationType, newPropertyName, ref joins);
            }


        }

        private static void GetWhereOperation(string linkOperator, string propertyName, ref string where)
        {
            string[] elements = propertyName.Split(new char[] { '.' });

            if (elements.Count() > 2)
                throw new Exception("Wrong condition");

            string tableName = elements[0];
            string primaryKey = (tableName.LastOrDefault() == 's' ? tableName.Substring(0, tableName.Length - 1) : tableName) + "Id";
            StringBuilder partialComment = new StringBuilder();
            partialComment.Append(linkOperator);
            partialComment.Append("((" + tableName + '.' + elements.LastOrDefault() + " IS NULL)");
            partialComment.Append(" OR ");
            partialComment.Append("(LEN(" + tableName + '.' + elements.LastOrDefault() + ") = 0))");

            where = partialComment.ToString();
        }

        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Default:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion
    }
}
