using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Act2_Mizal
{
    class DbConnections
    {
        private static SqlConnection connection = new SqlConnection();
        private static SqlCommand command = new SqlCommand();
        private static SqlDataAdapter adapter = new SqlDataAdapter();
        public SqlTransaction? DbTran;

        private static string strConnString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DB_Ordering_Mizal;Integrated Security=True;Trust Server Certificate=True";
         
        public void createConn()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {   
                    connection.ConnectionString = strConnString;
                    connection.Open();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void closeConn()
        {
            if (connection.State == ConnectionState.Open)
                connection.Close();
        }

        public int executeQuery(SqlCommand cmd)
        {
            try
            {
                createConn();
                cmd.Connection = connection;
                int result = cmd.ExecuteNonQuery();
                closeConn();
                return result;
            }
            catch (Exception)
            {
                closeConn();
                throw;
            }
        }

        public void readDatathroughAdapter(string query, DataTable tblName)
        {
            try
            {
                createConn();
                adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(tblName);
                closeConn();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
