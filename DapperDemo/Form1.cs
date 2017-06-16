using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DapperDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        IDbConnection connection = new SqlConnection("Data Source=192.168.1.100;Initial Catalog=Test;user=sa;password=123456");
        private void button1_Click(object sender, EventArgs e)
        {
            connection.Open();
            var result = connection.Execute("Insert into Users values (@UserName, @Email, @Address)",
                                 new { UserName = "jack", Email = "380234234@qq.com", Address = "上海" });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var list = Enumerable.Range(0, 10).Select(i => new Users()
            {
                //UserId = i,
                UserName = i + "张三",
                Email = i + "@qq.com",
                Address = i + "深圳"
            });
            connection.Execute("Insert into Users values(@UserName, @Email, @Address)", list);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var users = connection.Query<Users>("select * from Users where UserName=@UserName", new { UserName = "jack" });
            MessageBox.Show(users.ToList()[0].UserName);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            connection.Open();
            IDbTransaction trans = connection.BeginTransaction();
            try
            { 
            var result = connection.Execute("Insert into Users values (@UserName, @Email, @Address)",
                                new { UserName = "Tom", Email = "380234234@qq.com", Address = "深圳" },trans);

            trans.Commit();
            }
            catch
            {
                trans.Rollback();
            }
            finally
            {
                connection.Close();
            }
        }
    }
    public class Users
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
