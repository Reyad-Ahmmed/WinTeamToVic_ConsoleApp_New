using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public class VendorModel
    {
        public int? VendorNumber { get; set; }
        public int? VendorTypeId { get; set; }
        public string VendorName { get; set; }
        public bool? VendorStatus { get; set; }
        public AddressModelForVendor Address { get; set; }
        public string Phone { get; set; }
        public int? ParentVendorNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool? CreditCardVendor { get; set; }

        // for country code
        public string CountryCode { get; set; }
    }

    public class AddressModelForVendor
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }


    public class TotalServiceVendorModel
    {
        public string VendorNumber { get; set; }
        public string VendorName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public short Status { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
    }

    public class VendorResponseModel
    {
        public string AddressCity { get; set; }
        public string AddressPostalCode { get; set; }
        public string AddressState { get; set; }
        public string AddressStreet { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string CountryCode { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string ExternalData { get; set; }
        public string ExternalId { get; set; }
        public DateTime? ExternalUpdatedAt { get; set; }
        public string InternalId { get; set; }
        public DateTime? InternalUpdatedAt { get; set; }
        public string Name { get; set; }
        public string PaymentTermId { get; set; }
        public string Phone { get; set; }
        public bool? PoMatchingDocumentLevel { get; set; }
        public string State { get; set; }
        public string VendorGroupId { get; set; }
    }

    public class VendorRequestBodyModel 
    {
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string AddressStreet { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressPostalCode { get; set; }
        public bool? State { get; set; }
        public string CountryCode { get; set; }
    }


    public class CustomField
    {
        public int FieldNumber { get; set; }
        public string Value { get; set; }
    }

    public class VendorForTag
    {
        public int? VendorNumber { get; set; }
        public int? VendorTypeId { get; set; }
        public string VendorName { get; set; }
        public bool? VendorStatus { get; set; }
        public AddressModelForVendor Address { get; set; }
        public string Phone { get; set; }
        public int? ParentVendorNumber { get; set; }
        public string AccountNumber { get; set; }
        public string TaxID { get; set; }
        public List<CustomField> CustomFields { get; set; }
        public bool? CreditCardVendor { get; set; }
    }
    public class VendorTag
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

    public class VendorTagAssign
    {
        public string Id { get; set; }
        public string VendorId { get; set; }
        public string TagId { get; set; }
    }

    public class VendorCountryCodes 
    { 
        public string Id { get;set; }
        public string Description { get; set; }
        public string ISOCode { get; set; }
    }

}
