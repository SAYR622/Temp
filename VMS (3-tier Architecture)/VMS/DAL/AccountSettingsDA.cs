using System;
using Npgsql;

namespace VMS.DAL
{
    public class AccountSettingsDA
    {
        // 1. Fetch current admin details based on the username they logged in with
        public AdminAccountDTO GetAdminAccount(string username)
        {
            AdminAccountDTO admin = new AdminAccountDTO();
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = "SELECT name, gender, contact, password FROM admins WHERE username = @user";
            using NpgsqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@user", username);

            conn.Open();
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                admin.Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : "";
                admin.Gender = reader["gender"] != DBNull.Value && Convert.ToBoolean(reader["gender"]);
                admin.Contact = reader["contact"] != DBNull.Value ? Convert.ToInt64(reader["contact"]) : 0;
                admin.Password = reader["password"] != DBNull.Value ? reader["password"].ToString() : "";
            }
            return admin;
        }

        // 2. Update the admin's details in the database
        public void UpdateAdminAccount(string username, AdminAccountDTO admin)
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = @"UPDATE admins 
                             SET name = @name, gender = @gender, contact = @contact, password = @password 
                             WHERE username = @user";

            using NpgsqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@name", admin.Name);
            cmd.Parameters.AddWithValue("@gender", admin.Gender);
            cmd.Parameters.AddWithValue("@contact", admin.Contact);
            cmd.Parameters.AddWithValue("@password", admin.Password);
            cmd.Parameters.AddWithValue("@user", username);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public class AdminAccountDTO
    {
        public string Name { get; set; }
        public bool Gender { get; set; } // Assuming True = Male, False = Female
        public long Contact { get; set; }
        public string Password { get; set; }
    }
}
