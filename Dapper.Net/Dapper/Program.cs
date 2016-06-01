using DapperDemo.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace DapperDemo
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        /// <summary>
        /// 一对一映射
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        private static void OneToOne(string sqlConnectionString)
        {
            List<Customer> userList = new List<Customer>();
            using (IDbConnection conn = GetSqlConnection(sqlConnectionString))
            {
                string sqlCommandText = @"SELECT c.UserId,c.Username AS UserName,
c.PasswordHash AS [Password],c.Email,c.PhoneNumber,c.IsFirstTimeLogin,c.AccessFailedCount,
c.CreationDate,c.IsActive,r.RoleId,r.RoleName 
    FROM dbo.CICUser c WITH(NOLOCK) 
INNER JOIN CICUserRole cr ON cr.UserId = c.UserId 
INNER JOIN CICRole r ON r.RoleId = cr.RoleId";
                userList = conn.Query<Customer, Role, Customer>(sqlCommandText,
                                                                (user, role) => { user.Role = role; return user; },
                                                                null,
                                                                null,
                                                                true,
                                                                "RoleId",
                                                                null,
                                                                null).ToList();
            }

            if (userList.Count > 0)
            {
                userList.ForEach((item) => Console.WriteLine("UserName:" + item.UserName +
                                                             "----Password:" + item.Password +
                                                             "-----Role:" + item.Role.RoleName +
                                                             "\n"));

                Console.ReadLine();
            }
        }

        private static SqlConnection GetSqlConnection(string sqlConnectionString)
        {
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            return conn;
        }
    }
}
