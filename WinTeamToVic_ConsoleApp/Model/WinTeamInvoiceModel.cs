using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class WinTeamInvoiceModel
    {
        public int? VendorNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public int CompanyNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string PostingDate { get; set; }
        public string DueDate { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public List<GLDistribution> GLDistributions { get; set; }
        public string VicInternalId { get; set; }
    }

    public class GLDistribution
    {
        public string JobNumber { get; set; }
        public int? GLAccountNumber { get; set; }
        public decimal? Amount { get; set; }
        public string Notes { get; set; }
    }
}
