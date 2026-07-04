using System;
using System.Collections.Generic;
using Npgsql;

namespace VMS.DAL
{
    public class HeritageSiteDA
    {
        // --- READ ---
        public List<HeritageSite> GetAllSites()
        {
            List<HeritageSite> sites = new List<HeritageSite>();

            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);
            string query = "SELECT * FROM sites";

            using NpgsqlCommand cmd = new(query, conn);

            conn.Open();
            using NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                HeritageSite site = new HeritageSite
                {
                    Id = Convert.ToInt32(reader["id"]),
                    siteName = reader["sitename"].ToString(),
                    province = reader["province"].ToString(),
                    visitors = Convert.ToInt32(reader["visitors"]),
                    latitude = Convert.ToDouble(reader["latitude"]),
                    longitude = Convert.ToDouble(reader["longitude"])
                };

                sites.Add(site);
            }

            return sites;
        }

        // --- CREATE ---
        public void AddSite(HeritageSite site)
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = @"
                INSERT INTO sites (sitename, province, visitors, latitude, longitude)
                VALUES (@siteName, @province, @visitors, @latitude, @longitude)";

            using NpgsqlCommand cmd = new(query, conn);

            cmd.Parameters.AddWithValue("@siteName", site.siteName);
            cmd.Parameters.AddWithValue("@province", site.province);
            cmd.Parameters.AddWithValue("@visitors", site.visitors);
            cmd.Parameters.AddWithValue("@latitude", site.latitude);
            cmd.Parameters.AddWithValue("@longitude", site.longitude);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // --- UPDATE ---
        public void UpdateSite(HeritageSite site)
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = @"
                UPDATE sites SET
                sitename=@siteName, province=@province,
                visitors=@visitors, latitude=@latitude, longitude=@longitude
                WHERE id=@Id";

            using NpgsqlCommand cmd = new(query, conn);

            cmd.Parameters.AddWithValue("@Id", site.Id);
            cmd.Parameters.AddWithValue("@siteName", site.siteName);
            cmd.Parameters.AddWithValue("@province", site.province);
            cmd.Parameters.AddWithValue("@visitors", site.visitors);
            cmd.Parameters.AddWithValue("@latitude", site.latitude);
            cmd.Parameters.AddWithValue("@longitude", site.longitude);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // --- DELETE ---
        public void DeleteSite(int id)
        {
            using NpgsqlConnection conn = new(DatabaseConfig.ConnectionString);

            string query = "DELETE FROM sites WHERE id=@Id";

            using NpgsqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}