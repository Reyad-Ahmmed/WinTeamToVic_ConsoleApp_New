using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTeamToVic_ConsoleApp.Model
{
    public static class EncoderModel
    {
        
        public static string EncodeVendorNumber(string vendorNumber)
        {
            return vendorNumber.Contains("/") ? Uri.EscapeDataString(vendorNumber) : vendorNumber;
        }

        public static string TruncatePOItemDescription(string itemDescription)
        {
            int maxLength = int.Parse(ConfigurationManager.AppSettings["poItemDescriptionLength"]);

            if (string.IsNullOrEmpty(itemDescription)) return itemDescription;

            return itemDescription.Length > maxLength
                ? itemDescription.Substring(0, maxLength)
                : itemDescription;
        }
    }
}
