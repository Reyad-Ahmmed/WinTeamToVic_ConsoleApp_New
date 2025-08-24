using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class InvoiceReadyForPostModel
    {
        public string MarkedAs { get; set; }
        public string PostingError { get; set; }
        public DateTime? ServicePeriodEnd { get; set; }
        public List<LineItem> LineItems { get; set; }
        public decimal? TotalVatAmount { get; set; }
        public string ExternalPaymentNumber { get; set; }
        public decimal? AmountWithoutTax { get; set; }
        public string PaymentTermId { get; set; }
        public DateTime? ExternalPaymentDate { get; set; }
        public string PoNumber { get; set; }
        public DateTime? InternalUpdatedAt { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
        public string ExternalId { get; set; }
        public string Source { get; set; }
        public List<object> BolNumbers { get; set; }
        public string InternalId { get; set; }
        public DateTime? ExternalUpdatedAt { get; set; }
        public decimal? AmountTax { get; set; }
        public string Status { get; set; }
        public decimal? SelfAssessedUseTaxAmount { get; set; }
        public string Description { get; set; }
        public DateTime? GlDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string PaymentRef { get; set; }
        public string Currency { get; set; }
        public string BillStatus { get; set; }
        public string VendorInternalId { get; set; }
        public string RefNumber { get; set; }
        public string AccountNumber { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public string ExternalData { get; set; }
        public string Language { get; set; }
        public Vendor Vendor { get; set; }
        public DateTime? ServicePeriodStart { get; set; }
        public string InvoiceInfo { get; set; }
        public string SelfAssessedUseTaxAccount { get; set; }
        public string TransactionType { get; set; }
        public string VendorExternalId { get; set; }
        public string DocumentUrl { get; set; }
        public string ExternalPaymentStatus { get; set; }
        public List<object> CustomFields { get; set; }
        public List<object> Fields { get; set; }
    }

    public class LineItem
    {
        public decimal? Amount { get; set; }
        public string AmountTax { get; set; }
        public bool? Billable { get; set; }
        public CostAccount CostAccount { get; set; }
        public string CostAccountExternalId { get; set; }
        public string CostAccountInternalId { get; set; }
        public string Description { get; set; }
        public List<Dimension> Dimensions { get; set; }
        public List<string> DimensionsExternalIds { get; set; }
        public List<string> DimensionsInternalIds { get; set; }
        public int? Index { get; set; }
        public InvoiceLineItemInfo InvoiceLineItemInfo { get; set; }
        public decimal? LineItemTotal { get; set; }
        public string LineType { get; set; }
        public string Number { get; set; }
        public List<object> PoItemsMatched { get; set; }
        public string PoLineNumber { get; set; }
        public string PoNumber { get; set; }
        public decimal? QuantityInvoiced { get; set; }
        public string TaxCode { get; set; }
        public decimal? UnitPrice { get; set; }
        public Vat Vat { get; set; }
    }

    public class CostAccount
    {
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string Number { get; set; }
    }

    public class Dimension
    {
        public string DisplayName { get; set; }
        public string ExternalData { get; set; }
        public string ExternalId { get; set; }
        public DateTime? ExternalUpdatedAt { get; set; }
        public string InternalId { get; set; }
        public DateTime? InternalUpdatedAt { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Type { get; set; }
        public string TypeExternalId { get; set; }
        public string TypeName { get; set; }
    }

    public class InvoiceLineItemInfo
    {
        public decimal? VatAmount { get; set; }
    }

    public class Vat
    {
        public decimal? Amount { get; set; }
        public string Code { get; set; }
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string Rate { get; set; }
    }

    public class PaymentInfo
    {
        public PaymentTerm PaymentTerm { get; set; }
    }

    public class PaymentTerm
    {
        public int? Count { get; set; }
        public string Unit { get; set; }
    }

    public class Vendor
    {
        public string CountryCode { get; set; }
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string Name { get; set; }
        public string OrgNumber { get; set; }
    }

}
