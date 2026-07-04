using System;
using System.Collections.Generic;
using System.Text;

namespace VMS
{
    public class AdminUser
    {
        public long AdminID { get; set; } // int8 in Supabase
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Contact { get; set; }  // int4 in Supabase
        public string Username { get; set; }
        public string Password { get; set; }
        public string AType { get; set; }
    }
}
