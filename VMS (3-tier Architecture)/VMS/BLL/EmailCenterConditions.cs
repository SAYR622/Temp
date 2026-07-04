using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMS.DAL;

namespace VMS.BLL
{
    public class EmailCenterConditions
    {
        private EmailCenterDA _EmailCenterDA = new EmailCenterDA();

        public async Task InitializeAsync() => await _EmailCenterDA.InitializeAsync();

        // Fetches sites and adds the "All Sites" default option for dropdowns
        public async Task<List<SiteModel>> GetFilterSitesAsync()
        {
            var sites = await _EmailCenterDA.GetSitesAsync();
            var filterList = new List<SiteModel> { new SiteModel { Id = 0, siteName = "All Sites" } };
            filterList.AddRange(sites);
            return filterList;
        }

        public async Task<List<SiteModel>> GetRawSitesAsync() => await _EmailCenterDA.GetSitesAsync();

        public async Task<List<VisitorModel>> GetFilteredVisitorsAsync(int siteId, string timeframe)
        {
            var visitors = await _EmailCenterDA.GetVisitorsAsync();
            var filtered = visitors.AsEnumerable();

            if (siteId != 0)
                filtered = filtered.Where(v => v.SiteId == siteId);

            if (timeframe == "Last 24 Hours")
            {
                DateTime yesterday = DateTime.Now.AddHours(-24);
                filtered = filtered.Where(v => v.VisitDate >= yesterday);
            }

            return filtered.ToList();
        }

        public async Task<EmailTemplate> GetTemplateForSiteAsync(string siteId, string siteName)
        {
            var templates = await _EmailCenterDA.GetTemplatesAsync();
            var template = templates.FirstOrDefault(t => t.SiteId == siteId);

            // Generate default HTML if it doesn't exist yet
            if (template == null)
            {
                template = new EmailTemplate
                {
                    SiteId = siteId,
                    Subject = $"Welcome to {siteName}!",
                    HtmlBody = $@"<div style='font-family: sans-serif; color: #333;'>
                                <h1 style='color: #3d2314;'>Welcome to {siteName}!</h1>
                                <p>Thank you for scanning the QR code and registering. We hope you enjoy the history.</p>
                            </div>"
                };

                await _EmailCenterDA.UpsertTemplateAsync(template);
            }
            return template;
        }

        public async Task SaveTemplateAsync(string siteId, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(subject)) throw new Exception("Subject cannot be empty.");

            var template = new EmailTemplate { SiteId = siteId, Subject = subject, HtmlBody = html };
            await _EmailCenterDA.UpsertTemplateAsync(template);
        }

        public async Task<int> SendCampaignAsync(List<VisitorModel> selectedVisitors, string subject, string html)
        {
            if (string.IsNullOrWhiteSpace(subject)) throw new Exception("Subject cannot be empty.");

            var distinctEmails = selectedVisitors.Where(v => v.IsSelected)
                                                 .Select(v => v.Email)
                                                 .Distinct()
                                                 .ToList();

            if (distinctEmails.Count == 0) throw new Exception("No visitors selected.");

            // NO WRAPPER IN C#! 
            // We just pass the raw inner HTML straight to Supabase.
            // The Edge Function handles the theme wrapper and image resizing.
            return await _EmailCenterDA.SendCustomEmailsAsync(distinctEmails, subject, html);
        }

        public async Task<string> UploadImageAsync(byte[] fileBytes, string fileName)
        {
            return await _EmailCenterDA.UploadImageAsync(fileBytes, fileName);
        }

        public async Task<IReadOnlyList<Supabase.Storage.FileObject>> GetImagesAsync()
        {
            return await _EmailCenterDA.GetLibraryImagesAsync();
        }

        public string GetImageUrl(string fileName) => _EmailCenterDA.GetImageUrl(fileName);
    }
}