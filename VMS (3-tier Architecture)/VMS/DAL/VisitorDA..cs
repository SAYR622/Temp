using System;
using System.Collections.Generic;
using Npgsql;

namespace VMS.DAL
{
    public class VisitorDA
    {
        public List<Visitor> GetVisitorsBySite(int siteId)
        {
            List<Visitor> visitors = new List<Visitor>();
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = "SELECT \"visitorID\", created_at, email, site FROM visitors_emails";

            if (siteId > 0)
            {
                query += " WHERE site = @siteId";
            }

            query += " ORDER BY created_at DESC";

            using NpgsqlCommand cmd = new(query, conn);

            if (siteId > 0)
            {
                cmd.Parameters.AddWithValue("@siteId", siteId);
            }

            conn.Open();
            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                visitors.Add(new Visitor
                {
                    VisitorId = Convert.ToInt32(reader["visitorID"]),
                    VisitDate = Convert.ToDateTime(reader["created_at"]),
                    Email = reader["email"].ToString(),
                    SiteId = reader["site"] != DBNull.Value ? Convert.ToInt32(reader["site"]) : 0
                });
            }

            return visitors;
        }
    }
}