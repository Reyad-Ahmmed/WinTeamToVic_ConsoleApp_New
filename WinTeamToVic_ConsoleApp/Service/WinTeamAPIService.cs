using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.DataAccess;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class WinTeamAPIService
    {
        public string _baseAddress = ConfigurationManager.AppSettings["winTeamBaseAddress"];
        public string _vendorUrl = ConfigurationManager.AppSettings["vendorSubUrl"];
        public string _jobSubUrl = ConfigurationManager.AppSettings["jobSubUrl"];

        public string _invoiceUrl = ConfigurationManager.AppSettings["invoiceUrl"];

        public string tenantId = ConfigurationManager.AppSettings["tenantId"];
        public string testTenantId = ConfigurationManager.AppSettings["testTenantId"];

        public string vendorSubscriptionKey = ConfigurationManager.AppSettings["subscriptionkeyForVendor"];
        public string jobSubcriptionKey = ConfigurationManager.AppSettings["subscriptionkeyForJob"];

        public HttpClient _httpClient;

        public VicServiceLog_DAL _dal;
        public VicAiAPIService _vicAiAPIService;
        public CountryCode_DAL _countryCodeDAL;
        private DataTable _dt;

        public WinTeamAPIService()
        {
            _httpClient = new HttpClient();
            _dal = new VicServiceLog_DAL();
            _vicAiAPIService = new VicAiAPIService();
            _countryCodeDAL = new CountryCode_DAL();
            this._dt = null;
        }

        // Get All Active Vendor
        public async Task<List<VendorModel>> GetActiveVendors()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetActiveVendors");

            List<VendorModel> vendorList = new List<VendorModel>();

            try
            {
                var uri = _baseAddress + _vendorUrl;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("TenantId", tenantId);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", vendorSubscriptionKey);

                var response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");
                    try
                    {
                        vendorList = JsonConvert.DeserializeObject<List<VendorModel>>(responseData);
                        
                        var activeList = vendorList.Where(x => x.VendorStatus == true).ToList();

                        vendorList = activeList.ToList();
                        //Utils.LogToFile(3, "[INFO]", $"WinTeam active vendors: {vendorList.Count}");
                        //Console.WriteLine($"WinTeam active vendors: {vendorList.Count}");
                    }
                    catch (Exception ex) 
                    { 
                        Console.WriteLine(ex.Message );
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);

                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {errors}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return vendorList;
        }

        public VendorCountryCodes GetCountryCodes(int id)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetCountryCodes");

            _dt = _countryCodeDAL.GetCountryCodeById(id);

            VendorCountryCodes vendorCountryCode = new VendorCountryCodes();

            vendorCountryCode = (from DataRow row in _dt.Rows
                         select new VendorCountryCodes
                         {
                             Id = row["Id"] != DBNull.Value ? row["Id"].ToString() : string.Empty,
                             Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : string.Empty,
                             ISOCode = row["ISOCode"] != DBNull.Value ? row["ISOCode"].ToString() : string.Empty,
                             
                         }).FirstOrDefault();

            return vendorCountryCode;
        }

        
        // Get All Active Jobs
        public async Task<List<JobModel>> GetActiveJobs()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetActiveJobs");

            List<JobModel> jobList = new List<JobModel>();

            try
            {
                var uri = _baseAddress + _jobSubUrl;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("TenantId", tenantId);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", jobSubcriptionKey);

                var response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var reponseData = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {reponseData}");

                    try
                    {
                        var activejobs = JsonConvert.DeserializeObject<List<JobModel>>(reponseData).ToList();
                        Console.WriteLine($"Total active joblist for winteam: {activejobs.Count}");

                        jobList = activejobs.ToList();
                        Utils.LogToFile(3, "[INFO]", $"WinTeam active jobs: {jobList}");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);

                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {errors}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return jobList;
        }

        // Get All Active Jobs
        public async Task<JobModel> GetSingleJobByNumber(string jobNumber)
        {
            JobModel job = new JobModel();

            try
            {
                var uri = _baseAddress + _jobSubUrl + $"/{jobNumber}";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("TenantId", tenantId);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", jobSubcriptionKey);

                var response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var reponseData = await response.Content.ReadAsStringAsync();
                    
                    try
                    {
                        var data = JsonConvert.DeserializeObject<JobModel>(reponseData);
                        job = data;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return job;
        }

        // POST Invoice from Vic.Ai
        public async Task<int> SaveInvoice(string env, string sourceType, WinTeamInvoiceModel winTeamInvoice, string token)
        {
            int result = 0;

            if (winTeamInvoice != null) 
            {
                var uri = _baseAddress + _invoiceUrl + $"/{winTeamInvoice.VendorNumber}/APInvoices";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("TenantId", testTenantId);
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", vendorSubscriptionKey);

                var dataToSend = new
                {
                    VendorNumber = winTeamInvoice.VendorNumber,
                    InvoiceNumber = winTeamInvoice.InvoiceNumber,
                    CompanyNumber = winTeamInvoice.CompanyNumber,
                    InvoiceDate = winTeamInvoice.InvoiceDate,
                    PostingDate = winTeamInvoice.PostingDate,
                    DueDate = winTeamInvoice.DueDate,
                    InvoiceAmount = winTeamInvoice.InvoiceAmount,
                    GLDistributions = winTeamInvoice.GLDistributions
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(dataToSend, Formatting.Indented);

                // Serialize the request body to JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

                // create log msg object
                StringBuilder msgForLog = new StringBuilder();

                // create vic service log object
                var vicServiceLog = new VicServiceLog();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var data = JsonConvert.DeserializeObject<WinTeamInvoiceModel>(responseData);
                        result = 1;

                        vicServiceLog.Status = true;
                        vicServiceLog.VicResponse = responseData;

                        // update invoice when successfully post invoice data to winteam.
                        await _vicAiAPIService.UpdateInvoiceForConfirmation(winTeamInvoice, token);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        result = 0;

                        msgForLog.AppendLine("Invoice data parse error.");
                        msgForLog.AppendLine(ex.Message);
                        vicServiceLog.Status = false;
                        vicServiceLog.VicResponse = msgForLog.ToString();

                    }
                }
                else
                {
                    Console.WriteLine(response.ReasonPhrase);
                    result = 0;

                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    
                    var errors = JArray.Parse(responseBody);


                    msgForLog.AppendLine($"Failed to save the invoice data from Vic.Ai to WinTeam. Error msg: {errors}");
                    vicServiceLog.Status = false;
                    vicServiceLog.VicResponse = msgForLog.ToString();

                    // update invoice when successfully post invoice data to winteam.
                    await _vicAiAPIService.UpdateInvoiceForConfirmation(winTeamInvoice, token);
                }

                vicServiceLog.InternalId = winTeamInvoice.VicInternalId;
                vicServiceLog.ExternalId = winTeamInvoice.InvoiceNumber;
                vicServiceLog.ObjectType = "Invoice";
                vicServiceLog.SourceType = sourceType;
                vicServiceLog.VicRequest = json;
                vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                vicServiceLog.Note = msgForLog.ToString();
                vicServiceLog.DestinationENV = env;

                // save vic service log
                _dal.SaveVicServiceLog(vicServiceLog);
            }

            return result;
        }

    }
}
