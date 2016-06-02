using DapperDemo.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;

namespace DapperDemo
{
    class Program
    {
        private static string sqlConnectionString =ConfigurationManager.ConnectionStrings["DapperTest"].ConnectionString;// "Data Source=10.14.86.186,1433;Initial Catalog=master;User ID=TCBaseTrain;Password=TCBaseTrain;MultipleActiveResultSets=True";

        static void Main(string[] args)
        {
        }

        /// <summary>
        /// 一对一映射
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        private static void OneToOne()
        {
            List<Customer> userList = new List<Customer>();
            using (IDbConnection conn = GetSqlConnection())
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

        /// <summary>
        /// 一对多映射
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        private static void OneToMany()
        {
            Console.WriteLine("One To Many");
            List<User> userList = new List<User>();

            using (IDbConnection connection = GetSqlConnection())
            {

                string sqlCommandText3 = @"SELECT c.UserId,
       c.Username      AS UserName,
       c.PasswordHash  AS [Password],
       c.Email,
       c.PhoneNumber,
       c.IsFirstTimeLogin,
       c.AccessFailedCount,
       c.CreationDate,
       c.IsActive,
       r.RoleId,
       r.RoleName
FROM   dbo.CICUser c WITH(NOLOCK)
       LEFT JOIN CICUserRole cr
            ON  cr.UserId = c.UserId
       LEFT JOIN CICRole r
            ON  r.RoleId = cr.RoleId";

                var lookUp = new Dictionary<int, User>();
                userList = connection.Query<User, Role, User>(sqlCommandText3,
                    (user, role) =>
                    {
                        User u;
                        if (!lookUp.TryGetValue(user.UserId, out u))
                        {
                            lookUp.Add(user.UserId, u = user);
                        }
                        u.Role.Add(role);
                        return user;
                    }, null, null, true, "RoleId", null, null).ToList();
                var result = lookUp.Values;
            }

            if (userList.Count > 0)
            {
                userList.ForEach((item) => Console.WriteLine("UserName:" + item.UserName +
                                             "----Password:" + item.Password +
                                             "-----Role:" + item.Role.First().RoleName +
                                             "\n"));

                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("No Data In UserList!");
            }
        }

        /// <summary>
        /// 插入实体
        /// </summary>
        private static void InsertObject()
        {
            string sqlCommandText = @"INSERT INTO CICUser(Username,PasswordHash,Email,PhoneNumber)VALUES(
    @UserName,
    @Password,
    @Email,
    @PhoneNumber
)";
            using (IDbConnection conn = GetSqlConnection())
            {
                User user = new User();
                user.UserName = "Dapper";
                user.Password = "654321";
                user.Email = "Dapper@infosys.com";
                user.PhoneNumber = "13795666243";
                int result = conn.Execute(sqlCommandText, user);
                if (result > 0)
                {
                    Console.WriteLine("Data have already inserted into DB!");
                }
                else
                {
                    Console.WriteLine("Insert Failed!");
                }

                Console.ReadLine();
            }
        }

        /// <summary>
        /// Execute StoredProcedure and map result to POCO
        /// </summary>
        /// <param name="sqlConnnectionString"></param>
        private static void ExecuteStoredProcedure()
        {
            List<User> users = new List<User>();
            using (IDbConnection cnn = GetSqlConnection())
            {
                users = cnn.Query<User>("dbo.p_getUsers",
                                        new { UserId = 2 },
                                        null,
                                        true,
                                        null,
                                        CommandType.StoredProcedure).ToList();
            }
            if (users.Count > 0)
            {
                users.ForEach((user) => Console.WriteLine(user.UserName + "\n"));
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Execute StroedProcedure and get result from return value
        /// </summary>
        /// <param name="sqlConnnectionString"></param>
        private static void ExecuteStoredProcedureWithParms(string sqlConnnectionString)
        {
            DynamicParameters p = new DynamicParameters();
            p.Add("@UserName", "cooper");
            p.Add("@Password", "123456");
            p.Add("@LoginActionType", null, DbType.Int32, ParameterDirection.ReturnValue);
            using (IDbConnection cnn = GetSqlConnection())
            {
                cnn.Execute("dbo.p_validateUser", p, null, null, CommandType.StoredProcedure);
                int result = p.Get<int>("@LoginActionType");
                Console.WriteLine(result);
            }

            Console.ReadLine();
        }

        private static SqlConnection GetSqlConnection()
        {
            SqlConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            return conn;
        }
    }
}
