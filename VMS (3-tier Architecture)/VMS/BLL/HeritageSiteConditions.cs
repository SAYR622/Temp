using VMS.DAL;
using System.Collections.Generic;

namespace VMS.BLL
{
    public class HeritageSiteConditions
    {
        private HeritageSiteDA siteDA = new HeritageSiteDA();

        public List<HeritageSite> GetAllSites()
        {
            return siteDA.GetAllSites();
        }

        public void AddSite(HeritageSite site)
        {
            siteDA.AddSite(site);
        }

        public void DeleteSite(int id)
        {
            siteDA.DeleteSite(id);
        }

        public void UpdateSite(HeritageSite site)
        {
            siteDA.UpdateSite(site);
        }
    }
}