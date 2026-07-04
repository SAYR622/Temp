using System;
using System.Collections.Generic;
using Npgsql;
using System.Text;

namespace VMS.DAL
{
    public class LoginDA
    {
        [Obsolete]
        public bool ValidateUserInDatabase(string username, string password)
        {
            bool isValid = false;

            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = "SELECT COUNT(1) FROM admins WHERE username = @username AND password = @password";

            using NpgsqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            try
            {
                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 1)
                {
                    isValid = true;

                    // ---> INSTANTLY SET USER ONLINE <---
                    string updateQuery = "UPDATE admins SET status = true WHERE username = @username";
                    using NpgsqlCommand updateCmd = new(updateQuery, conn);
                    updateCmd.Parameters.AddWithValue("@username", username);
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database error: " + ex.Message);
            }

            return isValid;
        }

        // ---> NEW METHOD FOR LOGOUT <---
        public void UpdateUserStatus(string username, bool isOnline)
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = "UPDATE admins SET status = @status WHERE username = @username";

            using NpgsqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@status", isOnline);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user status: " + ex.Message);
            }
        }
    }
}