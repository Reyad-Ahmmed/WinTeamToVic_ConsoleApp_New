using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class VicServiceLog
    {
        public int Id { get; set; } // identity column
        public string InternalId { get; set; } // vic system generated id
        public string ObjectType { get; set; } // Vendor, Dimention, GL Chart of Account
        public string SourceType { get; set; } // Winteam or Total Service
        public string VicRequest { get; set; } // request to post json data
        public string SentDate { get; set; } // when send data
        public string VicResponse { get; set; } // json data from vic after save data
        public string ExternalId { get; set; } // winteam or total service id/number
        public bool Status { get; set; } // 0 for error and 1 for success
        public string Note { get; set; } // success or error log
        public string DestinationENV { get; set; } // Test or UAT
        public int DoNotSend { get; set; }
    }
}
