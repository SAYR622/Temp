using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Supabase;

namespace VMS.DAL
{
    public class EmailCenterDA
    {
        private Client _supabase;

        // Initializes the Supabase C# Client specifically for Storage & Edge Functions
        public async Task InitializeAsync()
        {
            if (_supabase != null) return;

            string url = "https://hwhqwjivazdozojhkeli.supabase.co";
            string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh3aHF3aml2YXpkb3pvamhrZWxpIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc4MTc1ODA2NywiZXhwIjoyMDk3MzM0MDY3fQ.B0rcFvJ8Kr9HROHl6kQA8l23zItj65soF8IfQFBUrXk";

            var options = new SupabaseOptions { AutoConnectRealtime = false };
            _supabase = new Client(url, key, options);
            await _supabase.InitializeAsync();
        }

        // --- READ OPERATIONS ---
        public async Task<List<SiteModel>> GetSitesAsync()
        {
            var response = await _supabase.From<SiteModel>().Get();
            return response.Models;
        }

        public async Task<List<VisitorModel>> GetVisitorsAsync()
        {
            var response = await _supabase.From<VisitorModel>().Get();
            return response.Models;
        }

        public async Task<List<EmailTemplate>> GetTemplatesAsync()
        {
            var response = await _supabase.From<EmailTemplate>().Get();
            return response.Models;
        }

        public async Task<IReadOnlyList<Supabase.Storage.FileObject>> GetLibraryImagesAsync()
        {
            return await _supabase.Storage.From("email-images").List();
        }

        // --- WRITE/ACTION OPERATIONS ---
        public async Task UpsertTemplateAsync(EmailTemplate template)
        {
            await _supabase.From<EmailTemplate>().Upsert(template);
        }

        public async Task<string> UploadImageAsync(byte[] fileBytes, string fileName)
        {
            await _supabase.Storage.From("email-images").Upload(fileBytes, fileName);
            return _supabase.Storage.From("email-images").GetPublicUrl(fileName);
        }

        public string GetImageUrl(string fileName)
        {
            return _supabase.Storage.From("email-images").GetPublicUrl(fileName);
        }

        public async Task<int> SendCustomEmailsAsync(List<string> emails, string subject, string htmlBody)
        {
            int successCount = 0;
            foreach (var email in emails)
            {
                var payload = new Dictionary<string, object>
                {
                    { "email", email },
                    { "subject", subject },
                    { "body", htmlBody }
                };

                var invokeOptions = new Supabase.Functions.Client.InvokeFunctionOptions { Body = payload };
                await _supabase.Functions.Invoke("send-email", null, invokeOptions);
                successCount++;
            }
            return successCount;
        }
    }

    // --- SUPABASE MODELS ---
    [Supabase.Postgrest.Attributes.Table("sites")]
    public class SiteModel : Supabase.Postgrest.Models.BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("id", false)]
        public int Id { get; set; }

        [Supabase.Postgrest.Attributes.Column("siteName")]
        public string siteName { get; set; }
    }

    [Supabase.Postgrest.Attributes.Table("email_templates")]
    public class EmailTemplate : Supabase.Postgrest.Models.BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("site_id", true)]
        public string SiteId { get; set; }

        [Supabase.Postgrest.Attributes.Column("subject")]
        public string Subject { get; set; }

        [Supabase.Postgrest.Attributes.Column("html_body")]
        public string HtmlBody { get; set; }
    }

    [Supabase.Postgrest.Attributes.Table("visitors_emails")]
    public class VisitorModel : Supabase.Postgrest.Models.BaseModel
    {
        [Supabase.Postgrest.Attributes.PrimaryKey("visitorID", false)]
        public int Id { get; set; }

        [Supabase.Postgrest.Attributes.Column("email")]
        public string Email { get; set; }

        [Supabase.Postgrest.Attributes.Column("site")]
        public int SiteId { get; set; }

        [Supabase.Postgrest.Attributes.Column("created_at")]
        public DateTime VisitDate { get; set; }

        // Local UI state (ignored by database)
        public bool IsSelected { get; set; }
    }
}
