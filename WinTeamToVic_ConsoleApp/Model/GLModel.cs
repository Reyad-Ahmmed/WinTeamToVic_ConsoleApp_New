using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class GLModel
    {
        public string GlNumber { get; set; }
        public string AccountDescription {  get; set; }
        public string Category { get; set; }
        //public string Active { get; set; }
        public string Type { get; set; }
        //public string Companies { get; set; }
        //public string Notes { get; set; }

        public bool Status { get; set; }
    }

    public class TotalServiceGLModel
    {
        public string Status { get; set; }         // currently we get only active but value will be 'Active', 'Inactive', or 'Hold'
        public int ID { get; set; }                // Chart.ID
        public string GLAccountNumber { get; set; }           // Chart.Acct
        public string GLAccountDescription { get; set; }          // Chart.fDesc
    }

    public class GLResponseModel 
    { 
        public string ExternalData { get; set; }
        public string ExternalId { get; set; }
        public string ExternalUpdatedAt { get; set; }
        public string InternalId { get; set; }
        public string InternalUpdatedAt { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
    }

    public class GlAccountFromWinTeam
    {
        public string GlAccountNumber { get; set; }
        public string GLAccountDescription { get; set; }
        public int CategoryID { get; set; }
        public bool Status { get; set; }
        public int AccountTypeID { get; set; }
        public string AccountTypeDescription { get; set; }
        public string Notes { get; set; }
    }


    public class GLRequestBodyModel
    {
        public string Number { get; set; }
        public string Name { get; set; }
        
    }

    public class WinTeamGL
    {
        public int WinTeamGLId { get; set; }

        public int WinTeamGLNumber { get; set; }

        public string WinTeamGLDescription { get; set; }

        public bool? Status { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }
    }

}
