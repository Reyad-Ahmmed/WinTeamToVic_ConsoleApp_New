using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class JobModel
    {
        public string JobNumber { get; set; }
        public Guid JobID { get; set; }
        public string JobDescription { get; set; }
        public int? LocationId { get; set; }
        public int? CompanyNumber { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? DateToStart { get; set; }
        public AddressModel Address { get; set; }
        public int? HoursRuleID { get; set; }
    }

    public class AddressModel
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string LocationCode { get; set; }
    }



    public class TotalServiceJobModel 
    {
        public int ID { get; set; }
        public short Status { get; set; }
        public string JobNumber { get; set; }
        public string JobDescription { get; set; }
    }

    public class JobResponseModel
    {
        public string DisplayName { get; set; }
        public string ExternalData { get; set; }
        public string ExternalId { get; set; }
        public string ExternalUpdatedAt { get; set; }
        public string InternalId { get; set; }
        public string InternalUpdatedAt { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Type { get; set; }
        public string TypeExternalId { get; set; }
        public string TypeName { get; set; }

    }

    public class JobRequestBodyModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string TypeExternalId { get; set; }
    }

    public class ClosedJobModel
    {
        public int JobNumber { get; set; }
        public string JobDescription { get; set; }
        public short Status { get; set; }

    }
}
