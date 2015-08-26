using ABC.Core;
using ABC.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC
{
    class Program
    {
        static void Main(string[] args)
        {
            DapperConnection();
        }



        #region Connect Dapper
        public static void DapperConnection()
        {
            string strConnection = @"Server=DELL-PC\SQLEXPRESS;Initial Catalog=NORTHWIND;Persist Security Info=True;User ID=username;Password=password;";

            SqlConnection connection = new SqlConnection(strConnection);
            connection.Open();

            var db = ((IDbConnection)connection);

            #region  Doing
            //var ListOrderWithConditionSelect2 = DBHelper.Select<OrderDetails>(o => new NewTestSelect
            //{
            //    a = o.OrderID,
            //    b = o.Orders.CustomerID,
            //    c = o.Products.CategoryID
            //}).ToList(db);

            //Create entity
            //OrderDetails created = new OrderDetails
            //{
            //    OrderID = 10249,
            //    Discount = 1,
            //    ProductID = 10,
            //    UnitPrice = 164,
            //    Quantity = 16
            //};
            //var createdOrderId = DBHelper.Add<OrderDetails>(created, db);

            //  var data = (db.Query(
            //    from p in new SQLinq<OrderDetails>()
            //    where p.UnitPrice == 10
            //    orderby p.OrderID
            //    select p
            //)).ToList();
            #endregion

            //Worked
            var IsNullOrEmpty1 = DBHelper.Get<OrderDetails>(
                c => !string.IsNullOrEmpty(c.Orders.CustomerID)
                  && c.ProductID > 10
            ).OrderBy(o => o.OrderID).Skip(10).Take(10).ToList(db);

            //Worked
            var IsNullOrEmpty2 = DBHelper.Get<OrderDetails>(
                c => c.ProductID > 10 && string.IsNullOrEmpty(c.Orders.CustomerID)
            ).OrderBy(o => o.OrderID).Skip(10).Take(10).ToList(db);

            //Worked
            var ListOrderWithJoinCondition = DBHelper.Get<OrderDetails>(x => x.Orders.Customer.CustomerID != "100").ToList(db);

            //Worked
            var ListOrderWithJoinCondition2 = DBHelper.Get<OrderDetails>(x =>
                    x.Orders.ShipName == "abc"
                && x.Orders.Customer.CustomerID != "100"
                && x.UnitPrice > 10).ToList(db);

            //Worked
            var ListOrderWithCondition = DBHelper.Get<OrderDetails>(c => c.Quantity > 10).OrderBy(o => o.OrderID).Skip(10).Take(10).ToList(db);

            //Worked
            var ListOrderWithConditionSelect1 = DBHelper.Get<OrderDetails>(c => c.Orders.CustomerID != "")
                .Select(o => new NewTestSelect
                {
                    a = o.OrderID,
                    b = o.Orders.CustomerID,
                    c = o.Products.CategoryID
                }).ToList(db);
            
            //Worked
            var userLogins = DBHelper.Get<OrderDetails>().ToList(db);

            //Worked
            var orderDetail = DBHelper.Get<OrderDetails>(x =>
                    x.Orders.ShipName == "abc"
                && x.Orders.Customer.CustomerID != "100"
                && x.UnitPrice > 10)
            .Select(x => new customer_login_password { username = x.Orders.Customer.ContactName, customer_account_id = x.OrderID })
            .FirstOrDefault(db);

            //Worked
            var TestCount1 = DBHelper.Count<Category>(x => x.CategoryID == 40, db);
            //Worked
            var TestCount2 = DBHelper.Count<Category>(db);

            //Worked
            var TestAny1 = DBHelper.Any<Category>(db, x => x.CategoryID == 40);
            //Worked
            var TestAny2 = DBHelper.Any<Category>(db);

            //Worked
            var exists = DBHelper.Get<OrderDetails>(o => o.OrderID == 10248)
                    .OrderBy(o => o.ProductID)
                    .ThenBy(o => o.Quantity)
                    .OrderByDescending(o => o.Orders.OrderID)
                    .ThenByDescending(o => o.Orders.Customer.CustomerID)
                    .ToList(db);


            connection.Close();
        }

        #endregion
    }
}
