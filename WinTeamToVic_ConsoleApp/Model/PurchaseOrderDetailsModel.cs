using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class PurchaseOrderDetailsModel
    {
        public int? PO { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }

        public string VendorNumber { get; set; }
        public string VendorName { get; set; }

        public string ProductNumber { get; set; }
        public int? LineNumber { get; set; }

        public decimal? Quan { get; set; }
        public decimal? Price { get; set; }
        public decimal? Amount { get; set; }

        public int? GL { get; set; }
        public int? Job { get; set; }
        public int? Inv { get; set; }
        public string fdesc { get; set; }
    }

    public class PurchaseOrderRequestBodyModel
    {
        public string IssuedOn { get; set; }
        public string PoNumber { get; set; }
        public string Amount { get; set; }
        public string CurrencyId { get; set; }
        public VendorForTotalService Vendor { get; set; }
        public List<LineItemForTotalService> LineItems { get; set; }
    }

    public class VendorForTotalService
    {
        public string InternalId { get; set; }
    }

    public class LineItemForTotalService
    {
        public string ProductNumber { get; set; }
        public string QuantityRequested { get; set; }
        public string QuantityReceived { get; set; }
        public string UnitAmount { get; set; }
        public string LineItemTotal { get; set; }
        public string LineNumber { get; set; }
        public string MatchingType { get; set; }
    }



    public class PurchaseOrderResponse
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonPropertyName("currencyId")]
        public string CurrencyId { get; set; }

        [JsonPropertyName("deliverOn")]
        public DateTime? DeliverOn { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        [JsonPropertyName("internalUpdatedAt")]
        public DateTime InternalUpdatedAt { get; set; }

        [JsonPropertyName("issuedOn")]
        public DateTime IssuedOn { get; set; }

        [JsonPropertyName("lineItems")]
        public List<LineItemForVicReponse> LineItems { get; set; }

        [JsonPropertyName("matchingType")]
        public string MatchingType { get; set; }

        [JsonPropertyName("poNumber")]
        public string PoNumber { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("vendor")]
        public VendorForVicResponse Vendor { get; set; }
    }

    public class LineItemForVicReponse
    {
        [JsonPropertyName("dimensions")]
        public List<object> Dimensions { get; set; }

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        [JsonPropertyName("invoiceItemsMatched")]
        public List<InvoiceItemMatch> InvoiceItemsMatched { get; set; }

        [JsonPropertyName("lineItemTotal")]
        public string LineItemTotal { get; set; }

        [JsonPropertyName("lineNumber")]
        public int LineNumber { get; set; }

        [JsonPropertyName("matchingType")]
        public string MatchingType { get; set; }

        [JsonPropertyName("productDescription")]
        public string ProductDescription { get; set; }

        [JsonPropertyName("productNumber")]
        public string ProductNumber { get; set; }

        [JsonPropertyName("quantityAccepted")]
        public string QuantityAccepted { get; set; }

        [JsonPropertyName("quantityReceived")]
        public string QuantityReceived { get; set; }

        [JsonPropertyName("quantityRequested")]
        public string QuantityRequested { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("unitAmount")]
        public string UnitAmount { get; set; }

        [JsonPropertyName("unitOfMeasure")]
        public string UnitOfMeasure { get; set; }
    }

    public class InvoiceItemMatch
    {
        [JsonPropertyName("amountMatched")]
        public string AmountMatched { get; set; }

        [JsonPropertyName("invoiceItemId")]
        public string InvoiceItemId { get; set; }

        [JsonPropertyName("purchaseOrderItemId")]
        public string PurchaseOrderItemId { get; set; }

        [JsonPropertyName("quantityMatched")]
        public string QuantityMatched { get; set; }
    }

    public class VendorForVicResponse
    {
        [JsonPropertyName("addressCity")]
        public string AddressCity { get; set; }

        [JsonPropertyName("addressPostalCode")]
        public string AddressPostalCode { get; set; }

        [JsonPropertyName("addressState")]
        public string AddressState { get; set; }

        [JsonPropertyName("addressStreet")]
        public string AddressStreet { get; set; }

        [JsonPropertyName("confirmedAt")]
        public DateTime ConfirmedAt { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("errors")]
        public List<object> Errors { get; set; }

        [JsonPropertyName("externalData")]
        public string ExternalData { get; set; }

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; }

        [JsonPropertyName("externalUpdatedAt")]
        public DateTime ExternalUpdatedAt { get; set; }

        [JsonPropertyName("internalId")]
        public string InternalId { get; set; }

        [JsonPropertyName("internalUpdatedAt")]
        public DateTime InternalUpdatedAt { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("paymentTermId")]
        public string PaymentTermId { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("poMatchingDocumentLevel")]
        public bool PoMatchingDocumentLevel { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("tags")]
        public List<object> Tags { get; set; }

        [JsonPropertyName("vendorGroupId")]
        public string VendorGroupId { get; set; }
    }

}
