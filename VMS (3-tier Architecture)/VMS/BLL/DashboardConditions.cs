using System.Collections.Generic;
using System.Linq;
using VMS.DAL;

namespace VMS.BLL
{
    public class DashboardConditions
    {
        private DashboardDA _dashboardDA = new DashboardDA();

        public int GetTotalSites() => _dashboardDA.GetTotalSites();
        public int GetTotalVisitors() => _dashboardDA.GetTotalVisitors();

        public List<HeritageSite> GetAllSites() => _dashboardDA.GetAllSites();

        // Gets the top 3 highest visited sites for the list next to the Map
        public List<HeritageSite> GetTopMapSites()
        {
            return _dashboardDA.GetAllSites().OrderByDescending(s => s.visitors).Take(3).ToList();
        }

        // Calculates the percentages and colors for the Bottom Chart
        public List<TopSiteChartDTO> GetTopVisitedSitesChart()
        {
            var sites = _dashboardDA.GetAllSites().OrderByDescending(s => s.visitors).Take(2).ToList();
            var result = new List<TopSiteChartDTO>();

            string[] colors = { "#10B981", "#3B82F6" }; // Green for #1, Blue for #2
            int maxVisitors = sites.Count > 0 && sites[0].visitors > 0 ? sites[0].visitors : 1;

            int index = 0;
            foreach (var s in sites)
            {
                result.Add(new TopSiteChartDTO
                {
                    SiteName = s.siteName,
                    VisitorsCount = s.visitors.ToString("N0"), // Formats number with commas (e.g. 4,500)
                    Percentage = ((double)s.visitors / maxVisitors) * 100, // Scales bar relative to top site
                    BarColor = colors[index % colors.Length]
                });
                index++;
            }

            return result;
        }

        public List<ActiveAdminDTO> GetActiveAdmins() => _dashboardDA.GetActiveAdmins();
        public List<RecentVisitorDTO> GetRecentVisitors() => _dashboardDA.GetRecentVisitors();
    }

    public class TopSiteChartDTO
    {
        public string SiteName { get; set; }
        public string VisitorsCount { get; set; }
        public double Percentage { get; set; }
        public string BarColor { get; set; }
    }
}