using System;
using System.Collections.Generic;
using Npgsql;
using VMS; // Your root namespace

namespace VMS.DAL
{
    public class AdminManageDA
    {
        // 1. READ
        public List<AdminUser> GetStandardAdmins()
        {
            List<AdminUser> admins = new List<AdminUser>();
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                string query = "SELECT \"adminID\", name, gender, contact, username, password, a_type " +
                               "FROM admins WHERE COALESCE(a_type, '') != 'super' ORDER BY \"adminID\" ASC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // FIX: Translate database bool to string
                        bool isMale = reader["gender"] != DBNull.Value && Convert.ToBoolean(reader["gender"]);

                        admins.Add(new AdminUser
                        {
                            AdminID = Convert.ToInt64(reader["adminID"]),
                            Name = reader["name"].ToString(),
                            Gender = isMale ? "Male" : "Female", // Translated here
                            Contact = reader["contact"] != DBNull.Value ? Convert.ToInt32(reader["contact"]) : 0,
                            Username = reader["username"].ToString(),
                            Password = reader["password"].ToString(),
                            AType = reader["a_type"].ToString()
                        });
                    }
                }
            }
            return admins;
        }

        // 2. CREATE
        public bool InsertAdmin(AdminUser admin)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                string query = @"INSERT INTO admins (name, gender, contact, username, password, a_type) 
                                 VALUES (@name, @gender, @contact, @username, @password, 'standard')";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    // FIX: Translate string to database bool
                    bool isMale = (admin.Gender == "Male");

                    cmd.Parameters.AddWithValue("@name", admin.Name);
                    cmd.Parameters.AddWithValue("@gender", isMale); // Passes true/false instead of string
                    cmd.Parameters.AddWithValue("@contact", admin.Contact);
                    cmd.Parameters.AddWithValue("@username", admin.Username);
                    cmd.Parameters.AddWithValue("@password", admin.Password);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 3. UPDATE
        public bool UpdateAdmin(AdminUser admin)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                string query = @"UPDATE admins SET name = @name, gender = @gender, contact = @contact, 
                                 username = @username, password = @password 
                                 WHERE ""adminID"" = @id AND COALESCE(a_type, '') != 'super'";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    // FIX: Translate string to database bool
                    bool isMale = (admin.Gender == "Male");

                    cmd.Parameters.AddWithValue("@id", admin.AdminID);
                    cmd.Parameters.AddWithValue("@name", admin.Name);
                    cmd.Parameters.AddWithValue("@gender", isMale); // Passes true/false instead of string
                    cmd.Parameters.AddWithValue("@contact", admin.Contact);
                    cmd.Parameters.AddWithValue("@username", admin.Username);
                    cmd.Parameters.AddWithValue("@password", admin.Password);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 4. DELETE
        public bool DeleteAdmin(long adminId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();

                string query = "DELETE FROM admins WHERE \"adminID\" = @id AND COALESCE(a_type, '') != 'super'";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", adminId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 5. GET REQUESTS
        public List<PendingRequest> GetPendingRequests()
        {
            List<PendingRequest> requests = new List<PendingRequest>();
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                string query = "SELECT id, fullname, username, contact, gender, password, created_at FROM admin_req ORDER BY created_at ASC";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool isMale = reader["gender"] != DBNull.Value && Convert.ToBoolean(reader["gender"]);

                        requests.Add(new PendingRequest
                        {
                            Id = Convert.ToInt64(reader["id"]),
                            FullName = reader["fullname"].ToString(),
                            Username = reader["username"].ToString(),
                            Contact = reader["contact"] != DBNull.Value ? Convert.ToInt64(reader["contact"]) : 0,
                            Gender = isMale ? "Male" : "Female",
                            Password = reader["password"].ToString(),
                            TimeReceived = Convert.ToDateTime(reader["created_at"]).ToString("MMM dd, yyyy")
                        });
                    }
                }
            }
            return requests;
        }

        // 6. DELETE REQUEST
        public bool DeleteRequest(long reqId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
            {
                conn.Open();
                string query = "DELETE FROM admin_req WHERE id = @id";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", reqId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}