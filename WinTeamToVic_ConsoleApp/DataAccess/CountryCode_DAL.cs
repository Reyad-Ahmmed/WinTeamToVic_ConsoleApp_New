using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;

namespace WinTeamToVic_ConsoleApp.DataAccess
{
    public class CountryCode_DAL
    {
        private DataTable _dt;
        private string _spName;
        private SqlParameter[] _spParameters;

        // read only
        private readonly DataAccessManager _baseDAL;

        public CountryCode_DAL()
        {
            _baseDAL = new DataAccessManager();
        }

        public DataTable GetCountryCodeById(int id)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetCountryCodeById");
            
            _dt = new DataTable();
            _spName = "SPGetCountryCode";

            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@id", id)
                };

                _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message.ToString());
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return _dt;
        }
    }
}
