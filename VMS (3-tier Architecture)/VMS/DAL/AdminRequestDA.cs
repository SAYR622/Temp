using System;
using Npgsql;

namespace VMS.DAL
{
    public class AdminRequestDA
    {
        // ---> NEW METHOD: Checks if the username already exists <---
        public bool IsUsernameTaken(string username)
        {
            using (NpgsqlConnection conn = new(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                // Counts how many rows have this exact username
                string query = "SELECT COUNT(*) FROM admins WHERE username = @username";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    // ExecuteScalar returns the first column of the first row (the count)
                    long count = (long)cmd.ExecuteScalar();

                    // If count is greater than 0, the username is taken!
                    return count > 0;
                }
            }
        }

        // ---> EXISTING METHOD <---
        public bool InsertAdminRequest(AdminRequest req)
        {
            using (NpgsqlConnection conn = new(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                string query = @"INSERT INTO admin_req (fullname, username, contact, gender, password) 
                                 VALUES (@fullname, @username, @contact, @gender, @password)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@fullname", req.FullName);
                    cmd.Parameters.AddWithValue("@username", req.Username);
                    cmd.Parameters.AddWithValue("@contact", req.Contact);
                    cmd.Parameters.AddWithValue("@gender", req.Gender);
                    cmd.Parameters.AddWithValue("@password", req.Password);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}