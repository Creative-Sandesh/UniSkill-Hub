using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace UniSkillHub
{
    /// <summary>
    /// Central place for all ADO.NET database access.
    /// Pages never open connections themselves; they pass a parameterized
    /// SQL string plus SqlParameter values to one of these methods.
    ///
    /// Example:
    ///   DataTable dt = DBHelper.GetDataTable(
    ///       "SELECT * FROM Users WHERE Email = @Email",
    ///       DBHelper.Param("@Email", email));
    /// </summary>
    public static class DBHelper
    {
        // The connection string is stored in Web.config, never in the pages.
        private static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["UniSkillHubDB"].ConnectionString;
            }
        }

        /// <summary>
        /// Creates a SqlParameter. A C# null becomes a database NULL.
        /// </summary>
        public static SqlParameter Param(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        /// <summary>
        /// Runs INSERT / UPDATE / DELETE. Returns the number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Runs a query that returns a single value (COUNT, SCOPE_IDENTITY, ...).
        /// Returns null if the query returns no rows or a database NULL.
        /// </summary>
        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddRange(parameters);
                con.Open();
                object result = cmd.ExecuteScalar();
                return (result == DBNull.Value) ? null : result;
            }
        }

        /// <summary>
        /// Runs a SELECT and returns an open SqlDataReader.
        /// The connection closes automatically when the reader is closed,
        /// so always wrap the call in a using block:
        ///   using (SqlDataReader dr = DBHelper.ExecuteReader(...)) { ... }
        /// </summary>
        public static SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            SqlConnection con = new SqlConnection(ConnectionString);
            try
            {
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                con.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Runs a SELECT and returns all rows in a DataTable
        /// (handy for GridView / Repeater data binding).
        /// </summary>
        public static DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddRange(parameters);
                DataTable table = new DataTable();
                adapter.Fill(table);   // opens and closes the connection itself
                return table;
            }
        }
    }
}
