using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.DataAccess;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class VendorTagService
    {
        private readonly VendorTag_DAL _dal;

        private DataTable _dt;


        // read only
        private readonly DataAccessManager _baseDAL;

        public VendorTagService()
        {
            this._dt = null;
            _dal = new VendorTag_DAL();
        }

        public VendorTagModel GetVendorTag(int id)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVendorTag");

            _dt = _dal.GetVendorTag(id);

            VendorTagModel vendorTag = new VendorTagModel();

            vendorTag = (from DataRow row in _dt.Rows
                         select new VendorTagModel
                         {
                             ID = row.Field<int>("ID"),
                             Description = row.Field<string>("Description") ?? string.Empty
                         }).FirstOrDefault();


            Utils.LogToFile(3, "[INFO]", $"Vendor tag id from winteam: {vendorTag.ID}, description: {vendorTag.Description}");

            return vendorTag;
        }
    }
}
