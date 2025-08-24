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
    public class VicServiceLog_DAL
    {
        private DataTable _dt;
        private string _spName;
        private SqlParameter[] _spParameters;

        // read only
        private readonly DataAccessManager _baseDAL;

        public VicServiceLog_DAL()
        {
            _baseDAL = new DataAccessManager();
        }

        public DataTable SaveVicServiceLog(VicServiceLog vicServiceLog)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in SaveVicServiceLog");

            _dt = new DataTable();
            _spName = "SPInsertVicServiceLog";

            Console.WriteLine($"Inside SaveVicServiceLog");
            Console.WriteLine($"Procedure Execute: {_spName}");
            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@InternalId", vicServiceLog.InternalId),
                    new SqlParameter("@DestinationENV", vicServiceLog.DestinationENV),
                    new SqlParameter("@ObjectType", vicServiceLog.ObjectType),
                    new SqlParameter("@SourceType", vicServiceLog.SourceType),
                    new SqlParameter("@VicRequest", vicServiceLog.VicRequest),
                    new SqlParameter("@SentDate", vicServiceLog.SentDate),
                    new SqlParameter("@VicResponse", vicServiceLog.VicResponse),
                    new SqlParameter("@ExternalId", vicServiceLog.ExternalId),
                    new SqlParameter("@Status", vicServiceLog.Status),
                    new SqlParameter("@Note", vicServiceLog.Note),
                }; 

                _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return _dt;
        }

        public DataTable UpdateVicServiceLog(VicServiceLog vicServiceLog)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in UpdateVicServiceLog");

            _dt = new DataTable();
            _spName = "SPUpdateVicServiceLog";

            Console.WriteLine($"Procedure Execute: {_spName}");
            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", vicServiceLog.Id),
                    new SqlParameter("@InternalId", vicServiceLog.InternalId),
                    new SqlParameter("@DestinationENV", vicServiceLog.DestinationENV),
                    new SqlParameter("@ObjectType", vicServiceLog.ObjectType),
                    new SqlParameter("@SourceType", vicServiceLog.SourceType),
                    new SqlParameter("@VicRequest", vicServiceLog.VicRequest),
                    new SqlParameter("@SentDate", vicServiceLog.SentDate),
                    new SqlParameter("@VicResponse", vicServiceLog.VicResponse),
                    new SqlParameter("@ExternalId", vicServiceLog.ExternalId),
                    new SqlParameter("@Status", vicServiceLog.Status),
                    new SqlParameter("@Note", vicServiceLog.Note),
                };

                _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return _dt;
        }

        public DataTable UpdateVicServiceLogForIncludeOrExclude(VicServiceLog vicServiceLog)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in AddVicServiceLogForIncludeOrExclude");

            _dt = new DataTable();
            _spName = "SPUpdateVicServiceLogForIncludeOrExclude";

            Console.WriteLine($"Procedure Execute: {_spName}");
            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@Id", vicServiceLog.Id),
                    new SqlParameter("@InternalId", vicServiceLog.InternalId),
                    new SqlParameter("@DestinationENV", vicServiceLog.DestinationENV),
                    new SqlParameter("@ObjectType", vicServiceLog.ObjectType),
                    new SqlParameter("@SourceType", vicServiceLog.SourceType),
                    new SqlParameter("@VicRequest", vicServiceLog.VicRequest),
                    new SqlParameter("@SentDate", vicServiceLog.SentDate),
                    new SqlParameter("@VicResponse", vicServiceLog.VicResponse),
                    new SqlParameter("@ExternalId", vicServiceLog.ExternalId),
                    new SqlParameter("@Status", vicServiceLog.Status),
                    new SqlParameter("@Note", vicServiceLog.Note),
                    new SqlParameter("@DoNotSend", vicServiceLog.DoNotSend)
                };

                _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return _dt;
        }


        public DataTable GetVicServiceLogs(string destinationEnv,string source, string objectType)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetVicServiceLogs");
            _dt = new DataTable();
            _spName = "SPGetExistingVicServiceLog";

            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@destinationEnv", destinationEnv),
                    new SqlParameter("@sourceType", source),
                    new SqlParameter("@objectType", objectType)
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


        public DataTable GetVendorTagByVendorNumber(string destinationEnv, string source, string objectType, string vendorNumber)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetVendorTagByVendorNumber");
            _dt = new DataTable();
            _spName = "SPGetExistingVendorTagByVendorNumber";

            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");
            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@destinationEnv", destinationEnv),
                    new SqlParameter("@sourceType", source),
                    new SqlParameter("@objectType", objectType),
                    new SqlParameter("@externalId", vendorNumber),
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
