using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using WinTeamToVic_ConsoleApp.DataAccess;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class TotalService
    {
        private DataAccessManager _dataAccess;
        private DataTable _dt;
        private string _spName;
        private SqlParameter[] _spParameters;
        public TotalService()
        {
            this._dataAccess = new DataAccessManager();
            this._dt = null;
        }

        // get total service jobs
        public List<TotalServiceJobModel> GetTotalServiceJobs()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetTotalServiceJobs");

            List<TotalServiceJobModel> jobList = new List<TotalServiceJobModel>();

            try
            {
                _spName = "SPGetActiveJobs";
                _dt = _dataAccess.ExecuteDataTableForTotalService(_spName);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

                //--------------------get employee master data---------------
                try
                {
                    jobList = (from DataRow row in _dt.Rows
                               select new TotalServiceJobModel
                               {
                                   ID = row.Field<int>("ID"),
                                   Status = row.Field<short>("Status"),
                                   JobNumber = row.Field<string>("JobNumber") ?? string.Empty, // If JobNumber is int
                                   JobDescription = row.Field<string>("JobDescription") ?? string.Empty
                               }).ToList();

                    Console.WriteLine($"Total active joblist for total service: {jobList.Count}");
                    Utils.LogToFile(3, "[INFO]", $"Total service active joblist: {jobList.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }
                return jobList;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return jobList;
        }

        // get closed job
        public ClosedJobModel GetClosedJob(int jobNumber)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetClosedJob");

            ClosedJobModel job = new ClosedJobModel();

            try
            {
                _spName = "SPGetClosedJob";
                _spParameters = new SqlParameter[]
                {
                    new SqlParameter("@jobNumber", jobNumber)
                };
                _dt = _dataAccess.ExecuteDataTableForTotalService(_spName, _spParameters);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

                //--------------------get closed job---------------
                try
                {
                    job = (from DataRow row in _dt.Rows
                               select new ClosedJobModel
                               {
                                   Status = row.Field<short>("Status"),
                                   JobNumber = row.Field<int>("JobNumber"), // If JobNumber is int
                                   JobDescription = row.Field<string>("JobDescription") ?? string.Empty
                               }).FirstOrDefault();

                   
                    Utils.LogToFile(3, "[INFO]", $"Closed job number: {job?.JobNumber}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return job;
        }

        // get total service vendors
        public List<TotalServiceVendorModel> GetTotalServiceVendors()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetTotalServiceVendors");

            List<TotalServiceVendorModel> vendorList = new List<TotalServiceVendorModel>();

            try
            {
                _spName = "SPGetActiveVendors";
                _dt = _dataAccess.ExecuteDataTableForTotalService(_spName);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

                vendorList = (from DataRow row in _dt.Rows
                        select new TotalServiceVendorModel
                        {
                            VendorNumber = row.Field<string>("VendorNumber") ?? string.Empty,
                            VendorName = row.Field<string>("VendorName") ?? string.Empty,
                            Address = row.Field<string>("Address")?.Replace("\r\n", "  ")
                                                                        .Replace("\n", "  ")
                                                                        .Replace("\r", "  ")
                                                                        ?? string.Empty,

                            City = row.Field<string>("City") ?? string.Empty,
                            State = row.Field<string>("State") ?? string.Empty,
                            Zip = row.Field<string>("Zip") ?? string.Empty,
                            Phone = row.Field<string>("Phone") ?? string.Empty,
                            Status = row.Field<short>("Status"),
                            Country = row.Field<string>("Country") ?? string.Empty,
                            CountryCode = row.Field<string>("CountryCode") ?? string.Empty

                        }).ToList();

                var activeVendors = vendorList;
                Console.WriteLine($"Total active vendors for total service: {activeVendors.Count}");

                vendorList = vendorList.ToList();
                Utils.LogToFile(3, "[INFO]", $"Total service active vendors: {vendorList.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return vendorList;

        }

        // get total service gl accounts
        public List<TotalServiceGLModel> GetTotalServiceGLAccounts()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetTotalServiceGLAccounts");

            List<TotalServiceGLModel> glAccountList = new List<TotalServiceGLModel>();

            //--------------------get employee master data---------------
            try
            {
                _spName = "GetActiveGLAccounts";
                _dt = _dataAccess.ExecuteDataTableForTotalService(_spName);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

                glAccountList = (from DataRow row in _dt.Rows
                            select new TotalServiceGLModel
                            {
                                ID = row.Field<int>("ID"),
                                GLAccountNumber = row.Field<string>("Acct") ?? string.Empty,
                                GLAccountDescription = row.Field<string>("fDesc") ?? string.Empty,
                                Status = row.Field<string>("Status") ?? string.Empty,
                                   
                            }).ToList();

                Console.WriteLine($"Total active gl accounts from total service: {glAccountList.Count}");
                Utils.LogToFile(3,"[INFO]", $"Total active gl accounts: {glAccountList.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            return glAccountList;
        }

        // get total service purchase orders

        public List<PurchaseOrderDetailsModel> GetTotalServiceOpenPurchaseOrders()
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetTotalServiceOpenPurchaseOrders");

            List<PurchaseOrderDetailsModel> purchaseOrderList = new List<PurchaseOrderDetailsModel>();

            try
            {
                _spName = "SPGetOpenPurchaseOrders";
                //_spName = "SPGetOpenPurchaseOrders_TEST_03072025";

                _dt = _dataAccess.ExecuteDataTableForTotalService(_spName);

                Utils.LogToFile(3, "[INFO]", $"Sp name: {_spName}");

                purchaseOrderList = (from DataRow row in _dt.Rows
                                select new PurchaseOrderDetailsModel
                                {
                                    PO = row.Field<int?>("PO"),
                                    IssueDate = row.Field<DateTime?>("fDate"),
                                    DueDate = row.Field<DateTime?>("Due"),
                                    TotalAmount = row.Field<decimal>("TotalAmount"),
                                    VendorNumber = row.Field<string>("VendorNumber"),
                                    VendorName = row.Field<string>("VendorName") ?? string.Empty,
                                    ProductNumber = row.Field<string>("ProductNumber") ?? string.Empty,
                                    LineNumber = row.Field<short?>("Line"),
                                    Quan = row.Field<decimal?>("Quan"),
                                    Price = row.Field<decimal?>("Price"),
                                    Amount = row.Field<decimal?>("Amount"),
                                    GL = row.Field<int?>("GL"),
                                    Job = row.Field<int?>("Job"),
                                    Inv = row.Field<int?>("Inv"),
                                    fdesc = row.Field<string>("fdesc"),
                                }).ToList();

                Utils.LogToFile(3, "[INFO]", $"Total service open PO: {purchaseOrderList.GroupBy(x=>x.PO).ToList().Count}");
                Console.WriteLine($"Total service open PO: {purchaseOrderList.GroupBy(x => x.PO).ToList().Count}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return purchaseOrderList;
        }
    }
}
