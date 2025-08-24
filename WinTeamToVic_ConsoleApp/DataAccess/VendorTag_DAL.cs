using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.DataAccess
{
    public class VendorTag_DAL
    {
        private DataTable _dt;
        private string _spName;
        private SqlParameter[] _spParameters;

        // read only
        private readonly DataAccessManager _baseDAL;

        public VendorTag_DAL()
        {
            _baseDAL = new DataAccessManager();
        }

        public DataTable GetVendorTag(int id)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVendorTag");

            //--------------------get employee master data---------------
            try
            {
                _spName = "SPGetVendorTag";
                
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@id", id)
                };
                _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return _dt;
        }
    }
}
