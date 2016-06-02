using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Entity;

namespace Dapper.Dao
{
    public class BookDB
    {
        public static IDbConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DapperTest"].ConnectionString);

        /// <summary>
        /// insert
        /// </summary>
        /// <param name="BookName"></param>
        public static void AddBook(string BookName)
        {
            Book book = new Book();
            book.Name = "C#本质论";
            string query = "INSERT INTO Book(Name)VALUES(@name)";
            //对对象进行操作
            conn.Execute(query, book);
            //方法2直接赋值操作
            //conn.Execute(query, new { name = "C#本质论" });
        }

        /// <summary>
        /// update
        /// </summary>
        /// <param name="BookName"></param>
        /// <param name="id"></param>
        public static void UpdateBookById(string BookName, int Id)
        {
            Book book = new Book();
            book.Name = BookName;
            book.Id = Id;

            string query = "UPDATE Book set Name=@Name where id=@Id";
            conn.Execute(query, book);
        }

        /// <summary>
        /// delete
        /// </summary>
        /// <param name="BookName"></param>
        /// <param name="Id"></param>
        public static void DeleteBookById(string BookName, int Id)
        {
            Book book = new Book();
            book.Id = Id;

            string query = "DELETE FROM Book WHERE id = @Id";
            conn.Execute(query, book);
            //conn.Execute(query, new { id = Id });  //方法2
        }

        /// <summary>
        /// query
        /// </summary>
        /// <returns></returns>
        public static List<Book> SelectBookList()
        {
            string query = "SELECT * FROM Book";
            //无参数查询，返回列表，带参数查询和之前的参数赋值法相同。
            return conn.Query<Book>(query).ToList();
        }

        /// <summary>
        /// 返回单条信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Book SelectBookById(int id)
        {
            //返回单条信息
            string query = "SELECT * FROM Book WHERE id = @id";
            return conn.Query<Book>(query, new { id = id }).SingleOrDefault();
        }

        /// <summary>
        /// 事务操作
        /// </summary>
        /// <param name="id"></param>
        public static void TranDeleteBook(int id)
        {
            using (conn)
            {
                //开始事务
                IDbTransaction transaction = conn.BeginTransaction();
                try
                {
                    string query = "DELETE FROM Book WHERE id = @id";
                    string query2 = "DELETE FORM BookReview WHERE BookId = @BookId";
                    conn.Execute(query2, new { BookId = id }, transaction, null, null);
                    conn.Execute(query, new { id = id }, transaction, null, null);
                    //提交事务
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    //出现异常，事务Rollback
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
