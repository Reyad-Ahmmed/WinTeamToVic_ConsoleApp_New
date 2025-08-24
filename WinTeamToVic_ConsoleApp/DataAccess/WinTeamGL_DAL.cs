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
    public class WinTeamGL_DAL
    {
        private DataTable _dt;
        private string _spName;
        private SqlParameter[] _spParameters;

        // read only
        private readonly DataAccessManager _baseDAL;

        public WinTeamGL_DAL()
        {
            _baseDAL = new DataAccessManager();
        }

        public DataTable SaveWinTeamGL(WinTeamGL gl)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in SaveWinTeamGL");

            _dt = new DataTable();
            _spName = "SPInsertWinTeamGLData";

            Console.WriteLine($"Inside SaveWinTeamGL");
            Console.WriteLine($"Procedure Execute: {_spName}");
            Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");

            try
            {
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@WinTeamGLNumber", gl.WinTeamGLNumber),
                    new SqlParameter("@WinTeamGLDescription", gl.WinTeamGLDescription),
                    new SqlParameter("@Status", gl.Status),
                    new SqlParameter("@CreatedDate", gl.CreatedDate),
                    new SqlParameter("@ModifiedDate", gl.ModifiedDate)
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

        //public DataTable GetGLForDelete()
        //{
        //    Utils.LogToFile(3, "[INFO]", "Inside in SaveWinTeamGL");

        //    _dt = new DataTable();
        //    _spName = "SPInsertWinTeamGLData";

        //    Console.WriteLine($"Inside SaveWinTeamGL");
        //    Console.WriteLine($"Procedure Execute: {_spName}");
        //    Utils.LogToFile(3, "[INFO]", $"Procedure execute: {_spName}");

        //    try
        //    {
        //        _spParameters = new SqlParameter[]
        //        {
        //            new SqlParameter("@WinTeamGLNumber", gl.WinTeamGLNumber),
        //            new SqlParameter("@WinTeamGLDescription", gl.WinTeamGLDescription),
        //            new SqlParameter("@Status", gl.Status),
        //            new SqlParameter("@CreatedDate", gl.CreatedDate),
        //            new SqlParameter("@ModifiedDate", gl.ModifiedDate)
        //        };

        //        _dt = _baseDAL.ExecuteDataTable(_spName, _spParameters);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message.ToString());
        //        Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
        //    }
        //    return _dt;
        //}
    }
}
