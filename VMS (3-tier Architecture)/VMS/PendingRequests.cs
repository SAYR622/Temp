using System;
using System.Collections.Generic;
using System.Text;

namespace VMS
{
    public class PendingRequest
    {
        public long Id { get; set; } // int8 in Supabase
        public string FullName { get; set; }
        public string Username { get; set; }
        public long Contact { get; set; } // int8 in Supabase
        public string Gender { get; set; }
        public string Password { get; set; }
        public string TimeReceived { get; set; }
    }
}
