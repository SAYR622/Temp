using System;
using System.Collections.Generic;
using Npgsql;

namespace VMS.DAL
{
    public class DashboardDA
    {
        public int GetTotalSites()
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);
            string query = "SELECT COUNT(*) FROM sites";
            using NpgsqlCommand cmd = new(query, conn);
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int GetTotalVisitors()
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);
            // Uses COALESCE to ensure it returns 0 instead of null if the table is empty
            string query = "SELECT COALESCE(SUM(visitors), 0) FROM sites";
            using NpgsqlCommand cmd = new(query, conn);
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<HeritageSite> GetAllSites()
        {
            List<HeritageSite> sites = new List<HeritageSite>();
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);
            string query = "SELECT id, sitename, province, visitors, latitude, longitude FROM sites";
            using NpgsqlCommand cmd = new(query, conn);

            conn.Open();
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sites.Add(new HeritageSite
                {
                    Id = Convert.ToInt32(reader["id"]),
                    siteName = reader["sitename"].ToString(),
                    province = reader["province"].ToString(),
                    visitors = Convert.ToInt32(reader["visitors"]),
                    latitude = Convert.ToDouble(reader["latitude"]),
                    longitude = Convert.ToDouble(reader["longitude"])
                });
            }
            return sites;
        }

        public List<ActiveAdminDTO> GetActiveAdmins()
        {
            List<ActiveAdminDTO> admins = new List<ActiveAdminDTO>();
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            // Grabs admins, sorting online users to the top
            string query = "SELECT name, a_type, status FROM admins ORDER BY status DESC LIMIT 4";
            using NpgsqlCommand cmd = new(query, conn);

            conn.Open();
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bool isOnline = reader["status"] != DBNull.Value && Convert.ToBoolean(reader["status"]);
                string role = reader["a_type"].ToString().ToLower() == "super" ? "Super Administrator" : "Standard Admin";

                admins.Add(new ActiveAdminDTO
                {
                    FullName = reader["name"].ToString(),
                    Role = role,
                    StatusText = isOnline ? "● Online" : "● Offline",
                    StatusColor = isOnline ? "#059669" : "#64748B",
                    StatusBg = isOnline ? "#ECFDF5" : "#F1F5F9"
                });
            }
            return admins;
        }

        // Mocking Recent Visitors since the table schema isn't fully established yet. 
        // You can update this to an SQL query later when your Visitors table is finalized!
        public List<RecentVisitorDTO> GetRecentVisitors()
        {
            List<RecentVisitorDTO> visitors = new List<RecentVisitorDTO>();
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            // Uses a LEFT JOIN to match the 'site' int8 ID to the actual 'sitename' in the sites table!
            string query = @"
            SELECT v.created_at, v.email, s.sitename 
            FROM visitors_emails v
            LEFT JOIN sites s ON v.site = s.id
            ORDER BY v.created_at DESC 
            LIMIT 6";

            using NpgsqlCommand cmd = new(query, conn);

            try
            {
                conn.Open();
                using NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // Safely pulling data
                    string email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "No Email";
                    string siteName = reader["sitename"] != DBNull.Value ? reader["sitename"].ToString() : "Unknown Site";

                    // TRICK: Since there is no 'name' column, extract the part before the '@' in the email to act as a username!
                    // E.g., "johndoe@gmail.com" becomes "johndoe"
                    string visitorName = email != "No Email" && email.Contains("@") ? email.Split('@')[0] : "Guest";

                    // Format the timestamp nicely
                    string dateStr = "-";
                    if (reader["created_at"] != DBNull.Value)
                    {
                        dateStr = Convert.ToDateTime(reader["created_at"]).ToString("MMM dd, yyyy");
                    }

                    visitors.Add(new RecentVisitorDTO
                    {
                        VisitorName = visitorName, // Using the extracted email prefix
                        Site = siteName,           // Using the joined sitename
                        Date = dateStr,
                        Email = email
                    });
                }
            }
            catch (Exception)
            {
                // If the table doesn't exist yet or columns are wrong, it safely returns an empty list
                // instead of crashing the whole dashboard.
            }

            return visitors;
        }
    }

    // Data Transfer Objects (DTOs) specifically formatted for Dashboard UI bindings
    public class ActiveAdminDTO
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string StatusBg { get; set; }
    }

    public class RecentVisitorDTO
    {
        public string VisitorName { get; set; }
        public string Site { get; set; }
        public string Date { get; set; }
        public string Email { get; set; } // Changed from Country
    }
}