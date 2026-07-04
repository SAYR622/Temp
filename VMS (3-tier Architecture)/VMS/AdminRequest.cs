using System;
using System.Collections.Generic;
using System.Text;

namespace VMS
{
    public class AdminRequest
    {
        // Notice we don't need 'id' or 'created_at' because Supabase handles those automatically!
        public string FullName { get; set; }
        public string Username { get; set; }
        public long Contact { get; set; } // int8 in Supabase = long in C#
        public bool Gender { get; set; }  // bool in Supabase (true = Male, false = Female)
        public string Password { get; set; }
    }
}
