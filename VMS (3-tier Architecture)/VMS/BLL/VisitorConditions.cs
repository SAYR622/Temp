using System;
using System.Collections.Generic;
using System.Linq;
using VMS.DAL;

namespace VMS.BLL
{
    public class VisitorConditions
    {
        private VisitorDA _visitorDA;

        public VisitorConditions()
        {
            _visitorDA = new VisitorDA();
        }

        public List<Visitor> GetVisitorList(int siteId = 0)
        {
            return _visitorDA.GetVisitorsBySite(siteId);
        }

        public Dictionary<string, int> GetVisitorStatsByDate(int siteId = 0)
        {
            var visitors = _visitorDA.GetVisitorsBySite(siteId);

            var stats = visitors
                .GroupBy(v => v.VisitDate.ToString("MMM dd"))
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }
    }
}