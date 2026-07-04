using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace VMS
{
    [Table("visitors_emails")]
    public class Visitor : BaseModel
    {
        [PrimaryKey("id", false)]
        public int VisitorId { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("created_at")]
        public DateTime VisitDate { get; set; }

        [Column("site")]
        public int SiteId { get; set; }

        // The UI Checkbox trigger (ignored by Supabase because it lacks a [Column] attribute)
        public bool IsSelected { get; set; }
    }
}