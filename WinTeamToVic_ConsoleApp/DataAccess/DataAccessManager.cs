using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;

namespace WinTeamToVic_ConsoleApp.DataAccess
{
    public class DataAccessManager
    {
        private DataTable _dt;
        private string _connectionString;
        private string emailBody;

        public DataTable ExecuteDataTable(string _spName, SqlParameter[] _spParameters = null)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in ExecuteDataTable");

            _dt = new DataTable();
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings["sqlConnStr"].ConnectionString;

                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = _spName;
                        if (_spParameters != null)
                        {
                            cmd.Parameters.AddRange(_spParameters);
                        }

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = Convert.ToInt16(ConfigurationManager.AppSettings["sqlCommandTimeout"]);

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            try
                            {
                                da.Fill(_dt);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception Message: {ex.Message}");
                                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return _dt;
        }

        public DataTable ExecuteDataTableForTotalService(string _spName, SqlParameter[] _spParameters = null)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in ExecuteDataTableForTotalService");

            _dt = new DataTable();
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings["totalServiceConnStr"].ConnectionString;

                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = _spName;
                        if (_spParameters != null)
                        {
                            cmd.Parameters.AddRange(_spParameters);
                        }

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = Convert.ToInt16(ConfigurationManager.AppSettings["sqlCommandTimeout"]);

                        conn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            try
                            {
                                da.Fill(_dt);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Exception Message: {ex.Message}");
                                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return _dt;
        }

        public DataTable ExecuteQuery(string query)
        {
            Utils.LogToFile(3, "[INFO]", "ExecuteQuery");

            DataTable resultTable = new DataTable();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["totalServiceConnStr"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sqlCommandTimeout"]);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(resultTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL query: {ex.Message}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return resultTable;
        }

    }
}
