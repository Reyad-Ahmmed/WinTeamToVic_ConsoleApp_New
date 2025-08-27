using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.DataAccess;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class VicAiAPIService
    {
        public string _baseAddressForUAT = ConfigurationManager.AppSettings["baseAddressForUAT"];
        public string _baseAddress = ConfigurationManager.AppSettings["vicBaseAddress"];
        public string _winTeamBaseAddress = ConfigurationManager.AppSettings["winTeamBaseAddress"];

        public string _vendorUrl = ConfigurationManager.AppSettings["vicVendorSubUrl"];
        public string _jobUrl = ConfigurationManager.AppSettings["vicJobSubUrl"];
        public string _glUrl = ConfigurationManager.AppSettings["vicGLSubUrl"];
        public string _poUrl = ConfigurationManager.AppSettings["vicPOSubUrl"];

        public string _vendorTagAssign = ConfigurationManager.AppSettings["vicVendorTags"];
        public string _vicVendorTagAdd = ConfigurationManager.AppSettings["vicVendorTagAdd"];

        public string _winteamVendorSubUrl = ConfigurationManager.AppSettings["vendorSubUrl"];

        public string _invoiceReadyForPostUrl = ConfigurationManager.AppSettings["vicInvoiceReadyToPostUrl"];
        public string _invoiceUrl = ConfigurationManager.AppSettings["vicInvoiceConfirmUrl"];
        

        public HttpClient _httpClient;

        public VicServiceLog_DAL _dal;
        private DataTable _dt;
        private readonly VendorTagService _vendorTagService;
        private readonly TotalService _totalService;

        public VicAiAPIService()
        {
            _httpClient = new HttpClient();
            _dal = new VicServiceLog_DAL();
            this._dt = null;
            _vendorTagService = new VendorTagService();
            _totalService = new TotalService();
        }

        #region WinTeam to Vic Test Environment Master Data Post/Update

        public async Task<int> PostTestVendorForWinTeam(List<VendorModel> vendors, string env, string sourceType, string token)
        {
            if(vendors.Count > 0)
            {
                foreach(VendorModel vendor in vendors)
                {
                    var uri = _baseAddress + _vendorUrl + vendor.VendorNumber;

                    var requestBody = new
                    {
                        externalId = $"{vendor.VendorNumber}",
                        externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                        name = vendor.VendorName,
                        phone = vendor.Phone,
                        addressStreet = vendor.Address?.Address1,
                        addressCity = vendor.Address?.City,
                        addressState = vendor.Address?.State,
                        addressPostalCode = vendor.Address?.Zip,
                        state = vendor.VendorStatus,
                    };

                    // Serialize the object to JSON
                    var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                    // Serialize the request body to JSON
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.PutAsync(uri, content);
                    
                    // create log msg object
                    StringBuilder msgForLog = new StringBuilder();

                    // create vendor response object
                    VendorResponseModel vendorResponse = new VendorResponseModel();

                    // create vic service log object
                    var vicServiceLog = new VicServiceLog();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        try
                        {
                            vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseData);
                            vicServiceLog.Status = true;
                            vicServiceLog.VicResponse = responseData;
                        }
                        catch (Exception ex) 
                        {
                            msgForLog.AppendLine("Vendor reponse parse error.");
                            msgForLog.AppendLine(ex.Message);
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }

                        msgForLog.AppendLine($"Successfully add/update the vendor. Vendor number: {vendor.VendorNumber}.");
                    }
                    else
                    {
                        Console.WriteLine($"Vendor number: {vendor.VendorNumber}");
                        Console.WriteLine(response.ReasonPhrase);

                        // error log
                        msgForLog.AppendLine($"Failed to add/update the vendor. Error msg: {response.ReasonPhrase}");
                        vicServiceLog.Status = false;
                        vicServiceLog.VicResponse = msgForLog.ToString();
                    }


                    vicServiceLog.InternalId = vendorResponse.InternalId;
                    vicServiceLog.ObjectType = "Vendor";
                    vicServiceLog.SourceType = sourceType;
                    vicServiceLog.VicRequest = json;
                    vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                    vicServiceLog.ExternalId = vendorResponse.ExternalId;
                    vicServiceLog.Note = msgForLog.ToString();
                    vicServiceLog.DestinationENV = env;

                    // save vic service log
                    _dal.SaveVicServiceLog(vicServiceLog);
                }
                
            }

            return 1;
        }

        public async Task<int> PostTestJobsForWinTeam(List<JobModel> jobs, string env, string sourceType, string token) 
        {
            if (jobs.Count > 0) 
            {
                try
                {
                    foreach (JobModel job in jobs)
                    {
                        var uri = _baseAddress + _jobUrl + job.JobNumber;

                        var requestBody = new
                        {
                            externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                            name = job.JobNumber,
                            shortName = job.JobDescription,
                            type = "jobnumber",
                            typeName = "Job Number",
                            typeExternalId = "jobnumber"
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        JobResponseModel jobResponse = new JobResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            try
                            {
                                jobResponse = JsonConvert.DeserializeObject<JobResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("Job reponse parse error.");
                                msgForLog.AppendLine(ex.Message);
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();
                            }
                            msgForLog.AppendLine($"Successfully add/update the Job. Job number: {job.JobNumber}.");
                        }
                        else
                        {
                            Console.WriteLine($"Job number: {job.JobNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            msgForLog.AppendLine($"Failed to add/update the Job. Error msg: {response.ReasonPhrase}");
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }

                        vicServiceLog.InternalId = jobResponse.InternalId;
                        vicServiceLog.ObjectType = "Job";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                        vicServiceLog.ExternalId = jobResponse.ExternalId;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        _dal.SaveVicServiceLog(vicServiceLog);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return 1;
        }

        public async Task<int> PostTestGLAccountsForWinTeam(List<GLModel> glAccounts, string env, string sourceType, string token)
        {
            if (glAccounts.Count > 0)
            {
                try
                {
                    foreach (GLModel glAccount in glAccounts)
                    {
                        var uri = _baseAddress + _glUrl + glAccount.GlNumber;

                        var requestBody = new
                        {
                            externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                            number = glAccount.GlNumber,
                            name = glAccount.AccountDescription
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        GLResponseModel glResponse = new GLResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();

                            try
                            {
                                glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("GL reponse parse error.");
                                msgForLog.AppendLine(ex.Message);
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();
                            }
                            msgForLog.AppendLine($"Successfully add/update the gl account. GL number: {glAccount.GlNumber}.");
                        }
                        else
                        {
                            Console.WriteLine($"Gl number: {glAccount.GlNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            msgForLog.AppendLine($"Failed to add/update the gl account. Error msg: {response.ReasonPhrase}");
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }

                        vicServiceLog.InternalId = glResponse.InternalId;
                        vicServiceLog.ObjectType = "GL Account";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = glResponse.ExternalId;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        _dal.SaveVicServiceLog(vicServiceLog);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return 1;
        }

        #endregion

        #region WinTeam to Vic UAT Environment Master Data Post/Update
        public async Task<int> PostUATVendorForWinTeam(List<VendorModel> vendors, string env, 
            string sourceType, string token, int isUpdate, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATVendorForWinTeam");

            WinTeamAPIService _winteamAPIService = new WinTeamAPIService();

            if (vendors.Count > 0)
            {
                foreach (VendorModel vendor in vendors)
                {
                    try
                    {
                        // get single vendor
                        var singleVendor = await GetSingleVendor(vendor.VendorNumber);
                        var countryCode = "";
                        object requestBody = null;

                        var uri = "";

                        if (singleVendor.CustomFields.Count > 0)
                        {
                            var vendorCountryCode = singleVendor.CustomFields.Where(x => x.FieldNumber == 2).FirstOrDefault();

                            if (vendorCountryCode != null)
                            {
                                int countryCodeId = int.Parse(vendorCountryCode.Value);
                                var VendorCountryCode = _winteamAPIService.GetCountryCodes(countryCodeId);

                                uri = _baseAddressForUAT + _vendorUrl + vendor.VendorNumber;
                                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                                countryCode = VendorCountryCode?.ISOCode;

                                requestBody = new
                                {
                                    externalId = $"{vendor.VendorNumber}",
                                    externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                                    name = vendor.VendorName,
                                    phone = vendor.Phone,
                                    addressStreet = vendor.Address?.Address1,
                                    addressCity = vendor.Address?.City,
                                    addressState = vendor.Address?.State,
                                    addressPostalCode = vendor.Address?.Zip,
                                    state = vendor.VendorStatus,
                                    countryCode = countryCode
                                };
                            }

                        }


                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                        Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        VendorResponseModel vendorResponse = new VendorResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                            try
                            {
                                vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("Vendor reponse parse error.");
                                msgForLog.AppendLine(ex.Message);
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();

                                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                            }

                            if (isUpdate > 0)
                            {
                                msgForLog.AppendLine($"Successfully updated the vendor. Vendor number: {vendor.VendorNumber}.");
                            }
                            else
                            {
                                msgForLog.AppendLine($"Successfully added the vendor. Vendor number: {vendor.VendorNumber}.");

                            }

                        }
                        else
                        {
                            Console.WriteLine($"Vendor number: {vendor.VendorNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            var responseBody = await response.Content.ReadAsStringAsync();
                            //var errors = JArray.Parse(responseBody);

                            // error log
                            if (isUpdate > 0)
                            {
                                msgForLog.AppendLine($"Failed to updated the vendor. Error msg: {responseBody}");
                            }
                            else
                            {
                                msgForLog.AppendLine($"Failed to added the vendor. Error msg: {responseBody}");
                            }

                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = responseBody;
                        }


                        vicServiceLog.InternalId = vendorResponse.InternalId;
                        vicServiceLog.ObjectType = "Vendor";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = vendorResponse.ExternalId;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        if (isUpdate > 0)
                        {
                            // vendor check 
                            var existingVendor = vicServiceLogs.Where(x => x.ExternalId == vendor.VendorNumber.ToString()).FirstOrDefault();
                            Utils.LogToFile(3, "[INFO]", $"Vendor number: {existingVendor?.ExternalId}, vic vendor id: {existingVendor.InternalId}");

                            vicServiceLog.Id = existingVendor.Id;
                            _dal.UpdateVicServiceLog(vicServiceLog);
                        }
                        else
                        {
                            _dal.SaveVicServiceLog(vicServiceLog);

                            // save vendor tag task

                            // vendor single api call for check if vendor have tag
                            var vendorData = await GetSingleVendor(vendor.VendorNumber);

                            if (vendorData != null)
                            {
                                var vendorTag = vendorData.CustomFields.Where(x => x.FieldNumber == 1).FirstOrDefault();

                                if (vendorTag != null)
                                {
                                    // check tag if already have vic.ai vendor tag
                                    var vicVendorTags = await GetVendorTags(token);

                                    var tagIdFromWinTeam = vendorTag.Value.Trim();

                                    string tagValue = "";

                                    if (int.TryParse(tagIdFromWinTeam, out var id))
                                    {
                                        var singleVendorTag = _vendorTagService.GetVendorTag(id);
                                        tagValue = singleVendorTag?.Description?.ToUpper();
                                    }
                                    else
                                    {
                                        Utils.LogToFile(3, "[INFO]", $"Invalid tag id. Return value: {tagIdFromWinTeam}");
                                        Console.WriteLine($"Invalid tag ID format: {tagIdFromWinTeam}");

                                        tagValue = vendorTag.Value.Trim();
                                    }

                                    //var singleVendorTag = _vendorTagService.GetVendorTag(int.Parse(vendorTag.Value));


                                    var isExistVendorTag = vicVendorTags.Where(x => x.Value.ToUpper().Trim() == tagValue.ToUpper().Trim()).FirstOrDefault();

                                    if (isExistVendorTag != null)
                                    {
                                        // vendor tag assign.
                                        await VendorTagAssign(token, sourceType, env, vendor.VendorNumber, vendorResponse.InternalId, isExistVendorTag.Id, 0);
                                    }
                                    else
                                    {
                                        // vendor tag add
                                        var tag = tagValue.ToUpper().Trim();

                                        var vendorTagAfterSave = await AddVendorForWinTeamToVicUAT(token, tag, sourceType, env, vendor.VendorNumber);

                                        // vendor tag assign

                                        await VendorTagAssign(token, sourceType, env, vendor.VendorNumber, vendorResponse.InternalId, vendorTagAfterSave.Id, 0);
                                    }
                                }
                            }


                        }
                    }
                    catch(Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from winteam vendor: {vendor.VendorNumber}, name: {vendor.VendorName}");
                    }
                    
                        
                }

            }

            return 1;
        }

        // get vendor information with custom field / tag
        public async Task<VendorForTag> GetSingleVendor(int? vendorNumber)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetSingleVendor");
            VendorForTag vendor = null;
            
            string vendorSubscriptionKey = ConfigurationManager.AppSettings["subscriptionkeyForVendor"];
            string tenantId = ConfigurationManager.AppSettings["tenantId"];

            try
            {
                var uri = _winTeamBaseAddress + _winteamVendorSubUrl + $"/{vendorNumber}" ;
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
                        vendor = new VendorForTag();
                        vendor = JsonConvert.DeserializeObject<VendorForTag>(responseData);

                        Utils.LogToFile(3, "[INFO]", $"WinTeam active vendors: {vendor.VendorNumber}");
                        Console.WriteLine($"WinTeam active vendors: {vendor.VendorNumber}");
                    }
                    catch (Exception ex)
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
                    //var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return vendor;
        }

        // get all vendor tags
        public async Task<List<VendorTag>> GetVendorTags(string token)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetVendorTags");
            List<VendorTag> vendorTags = new List<VendorTag>();

            try
            {
                var uri = _baseAddressForUAT + _vicVendorTagAdd;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                    try
                    {
                        var dataObjects = JsonConvert.DeserializeObject <List<VendorTag>>(responseData);
                        vendorTags = dataObjects;
                    }
                    catch(Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}");
            }

            return vendorTags;
        }

        // vendor tag add
        public async Task<VendorTag> AddVendorForWinTeamToVicUAT(string token, string tag, string sourceType, string env, int? vendorNumber)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in AddVendorForWinTeamToVicUAT");
            
            // create vendor tag response object
            VendorTag vendorTagResponse = new VendorTag();

            try
            {
                var uri = _baseAddressForUAT + _vicVendorTagAdd;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                var requestBody = new
                {
                    value = tag
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                // Serialize the request body to JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(uri, content);

                // create log msg object
                StringBuilder msgForLog = new StringBuilder();

                
                // create vic service log object
                var vicServiceLog = new VicServiceLog();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                    try
                    {
                        vendorTagResponse = JsonConvert.DeserializeObject<VendorTag>(responseData);
                        vicServiceLog.Status = true;
                        vicServiceLog.VicResponse = responseData;
                    }
                    catch (Exception ex)
                    {
                        msgForLog.AppendLine("Vendor tag reponse parse error.");
                        msgForLog.AppendLine(ex.Message);
                        vicServiceLog.Status = false;
                        vicServiceLog.VicResponse = msgForLog.ToString();

                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }

                    //if (isUpdate > 0)
                    //{
                    //    msgForLog.AppendLine($"Successfully updated the Job. Job number: {job.JobNumber}.");
                    //}
                    //else
                    //{
                    //    msgForLog.AppendLine($"Successfully added the Job. Job number: {job.JobNumber}.");
                    //}

                    msgForLog.AppendLine($"Successfully added the Vendor Tag. Vendor Tag: {tag}.");
                }
                else
                {
                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();

                    //var errors = JArray.Parse(responseBody);

                    // error log
                    //if (isUpdate > 0)
                    //{
                    //    msgForLog.AppendLine($"Failed to updated the Job. Error msg: {responseBody}");
                    //}
                    //else
                    //{
                    //    msgForLog.AppendLine($"Failed to added the Job. Error msg: {responseBody}");
                    //}

                    msgForLog.AppendLine($"Failed to added the Vendor Tag. Error msg: {responseBody}");

                    vicServiceLog.Status = false;
                    vicServiceLog.VicResponse = responseBody;
                }

                vicServiceLog.InternalId = vendorTagResponse.Id;
                vicServiceLog.ObjectType = "Vendor Tag";
                vicServiceLog.SourceType = sourceType;
                vicServiceLog.VicRequest = json;
                vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                vicServiceLog.ExternalId = vendorNumber.ToString();
                vicServiceLog.Note = msgForLog.ToString();
                vicServiceLog.DestinationENV = env;

                _dal.SaveVicServiceLog(vicServiceLog);
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}");
            }

            return vendorTagResponse;
        }

        // vendor tag assign to vendor
        public async Task<VendorTagAssign> VendorTagAssign(string token, string sourceType, string env, int? vendorNumber, string vendorId, string tagId, int isUpdated)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in VendorTagAssign");

            // create vendor tag assign response object
            VendorTagAssign vendorTagAssignResponse = new VendorTagAssign();

            try
            {
                var uri = _baseAddressForUAT + _vendorTagAssign;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                var requestBody = new
                {
                    vendorId = vendorId,
                    tagId = tagId
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                // Serialize the request body to JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync(uri, content);

                // create log msg object
                StringBuilder msgForLog = new StringBuilder();

                // create vic service log object
                var vicServiceLog = new VicServiceLog();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                    try
                    {
                        vendorTagAssignResponse = JsonConvert.DeserializeObject<VendorTagAssign>(responseData);
                        vicServiceLog.Status = true;
                        vicServiceLog.VicResponse = responseData;
                    }
                    catch (Exception ex)
                    {
                        msgForLog.AppendLine("Vendor tag assign reponse parse error.");
                        msgForLog.AppendLine(ex.Message);
                        vicServiceLog.Status = false;
                        vicServiceLog.VicResponse = msgForLog.ToString();

                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }

                    if (isUpdated > 0)
                    {
                        msgForLog.AppendLine($"Successfully updated the Vendor tag assign. tag id: {vendorTagAssignResponse.TagId}, vendor id: {vendorTagAssignResponse.VendorId}.");
                    }
                    else
                    {
                        msgForLog.AppendLine($"Successfully added the Vendor tag assign. tag id: {vendorTagAssignResponse.TagId}, vendor id: {vendorTagAssignResponse.VendorId}.");
                    }
                }
                else
                {
                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    //var errors = JArray.Parse(responseBody);

                    //error log
                    if (isUpdated > 0)
                    {
                        msgForLog.AppendLine($"Failed to updated the Vendor tag assign. Error msg: {responseBody}");
                    }
                    else
                    {
                        msgForLog.AppendLine($"Failed to added the Vendor tag assign. Error msg: {responseBody}");
                    }

                    vicServiceLog.Status = false;
                    vicServiceLog.VicResponse = responseBody;
                }

                vicServiceLog.InternalId = vendorTagAssignResponse.Id;
                vicServiceLog.ObjectType = "Vendor Tag Assign";
                vicServiceLog.SourceType = sourceType;
                vicServiceLog.VicRequest = json;
                vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                vicServiceLog.ExternalId = vendorNumber.ToString();
                vicServiceLog.Note = msgForLog.ToString();
                vicServiceLog.DestinationENV = env;

                if(isUpdated > 0)
                {
                    _dal.SaveVicServiceLog(vicServiceLog);
                }
                else
                {
                    _dal.SaveVicServiceLog(vicServiceLog);
                }
                    
            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}");
            }

            return vendorTagAssignResponse;
        }

        public async Task<int> DeleteVendorTag(string token, string vendorTagId, int? vendorNumber)
        {
            var uri = _baseAddressForUAT + _vendorTagAssign + $"/{vendorTagId}";
            
            int deleted = 0;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                //Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                try
                {
                    deleted = 1;
                    Utils.LogToFile(3, "[INFO]", $"Successfully delete vendor tag. Vendor number: {vendorNumber}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error msg: {ex.Message}, Id: {vendorTagId}");
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Vendor number: {vendorNumber}");
                    deleted = 0;
                }
            }
            else
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {deleted}");
            }

            return deleted;
        }

        public async Task<int> PostUATJobsForWinTeam(List<JobModel> jobs, string env, 
            string sourceType, string token, int isUpdate, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATJobsForWinTeam");

            if (jobs.Count > 0)
            {
                try
                {
                    foreach (JobModel job in jobs)
                    {
                        try
                        {
                            var uri = _baseAddressForUAT + _jobUrl + job.JobNumber;
                            Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                            var requestBody = new
                            {
                                externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                                name = job.JobNumber,
                                shortName = job.JobDescription,
                                type = "jobnumber",
                                typeName = "Job Number",
                                typeExternalId = "jobnumber"
                            };

                            // Serialize the object to JSON
                            var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                            Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                            // Serialize the request body to JSON
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                            var response = await _httpClient.PutAsync(uri, content);

                            // create log msg object
                            StringBuilder msgForLog = new StringBuilder();

                            // create vendor response object
                            JobResponseModel jobResponse = new JobResponseModel();

                            // create vic service log object
                            var vicServiceLog = new VicServiceLog();

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                                try
                                {
                                    jobResponse = JsonConvert.DeserializeObject<JobResponseModel>(responseData);
                                    vicServiceLog.Status = true;
                                    vicServiceLog.VicResponse = responseData;
                                }
                                catch (Exception ex)
                                {
                                    msgForLog.AppendLine("Job reponse parse error.");
                                    msgForLog.AppendLine(ex.Message);
                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = msgForLog.ToString();

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                                }

                                if (isUpdate > 0)
                                {
                                    msgForLog.AppendLine($"Successfully updated the Job. Job number: {job.JobNumber}.");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Successfully added the Job. Job number: {job.JobNumber}.");
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Job number: {job.JobNumber}");
                                Console.WriteLine(response.ReasonPhrase);

                                // error log
                                var responseBody = await response.Content.ReadAsStringAsync();
                                //var errors = JArray.Parse(responseBody);

                                // error log
                                if (isUpdate > 0)
                                {
                                    msgForLog.AppendLine($"Failed to updated the Job. Error msg: {responseBody}");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Failed to added the Job. Error msg: {responseBody}");
                                }

                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = responseBody;
                            }

                            vicServiceLog.InternalId = jobResponse.InternalId;
                            vicServiceLog.ObjectType = "Job";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = json;
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = jobResponse.ExternalId;
                            vicServiceLog.Note = msgForLog.ToString();
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            if (isUpdate > 0)
                            {
                                var existingJob = vicServiceLogs.Where(x => x.ExternalId == job.JobNumber).FirstOrDefault();
                                Utils.LogToFile(3, "[INFO]", $"Job numbe: {existingJob.ExternalId}, Vic job id: {existingJob.InternalId}");

                                vicServiceLog.Id = existingJob.Id;
                                _dal.UpdateVicServiceLog(vicServiceLog);
                            }
                            else
                            {
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                        }
                        catch (Exception ex) 
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from winteam job: {job.JobNumber}, description: {job.JobDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }

        public async Task<int> PostUATGLAccountsForWinTeam(List<GLModel> glAccounts, string env, 
            string sourceType, string token, int isUpdated, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATGLAccountsForWinTeam");

            if (glAccounts.Count > 0)
            {
                try
                {
                    foreach (GLModel glAccount in glAccounts)
                    {
                        try
                        {
                            var uri = _baseAddressForUAT + _glUrl + glAccount.GlNumber;
                            Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                            var requestBody = new
                            {
                                externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                                number = glAccount.GlNumber,
                                name = glAccount.AccountDescription
                            };

                            // Serialize the object to JSON
                            var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                            Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                            // Serialize the request body to JSON
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                            var response = await _httpClient.PutAsync(uri, content);

                            // create log msg object
                            StringBuilder msgForLog = new StringBuilder();

                            // create vendor response object
                            GLResponseModel glResponse = new GLResponseModel();

                            // create vic service log object
                            var vicServiceLog = new VicServiceLog();

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");
                                try
                                {
                                    glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                                    vicServiceLog.Status = true;
                                    vicServiceLog.VicResponse = responseData;
                                }
                                catch (Exception ex)
                                {
                                    msgForLog.AppendLine("GL reponse parse error.");
                                    msgForLog.AppendLine(ex.Message);
                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = msgForLog.ToString();

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                                }

                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Successfully updated the gl account. GL number: {glAccount.GlNumber}.");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Successfully added the gl account. GL number: {glAccount.GlNumber}.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Gl number: {glAccount.GlNumber}");
                                Console.WriteLine(response.ReasonPhrase);

                                // error log
                                var responseBody = await response.Content.ReadAsStringAsync();
                                //var errors = JArray.Parse(responseBody);

                                // error log
                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Failed to updated the gl account. Error msg: {responseBody}");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Failed to added the gl account. Error msg: {responseBody}");
                                }

                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = responseBody;
                            }

                            vicServiceLog.InternalId = glResponse.InternalId;
                            vicServiceLog.ObjectType = "GL Account";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = json;
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = glResponse.ExternalId;
                            vicServiceLog.Note = msgForLog.ToString();
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            if (isUpdated > 0)
                            {
                                var existingAccount = vicServiceLogs.Where(x => x.ExternalId == glAccount.GlNumber).FirstOrDefault();
                                Utils.LogToFile(3, "[INFO]", $"GL Account number: {existingAccount.ExternalId}, Vic gl account id: {existingAccount.InternalId}");

                                vicServiceLog.Id = existingAccount.Id;
                                _dal.UpdateVicServiceLog(vicServiceLog);
                            }
                            else
                            {
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                        }
                        catch(Exception ex)
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from gl: {glAccount.GlNumber}, description: {glAccount.AccountDescription}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }


        // Delete GL
        public async Task<int> RemoveWinTeamGL(string token, string env, string sourceType, List<VicServiceLog> glLogForDelete)
        {
            
            foreach(var item in glLogForDelete)
            {
                var uri = _baseAddressForUAT + _glUrl + int.Parse(item.ExternalId);

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = _httpClient.DeleteAsync(uri).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                        // save log and DoNotSend value 3 to 1;

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        vicServiceLog.Status = true;
                        vicServiceLog.VicResponse = responseData;

                        vicServiceLog.InternalId = glResponse.InternalId;
                        vicServiceLog.ObjectType = "GL Account";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = $"Request URI: {uri}";
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = glResponse.ExternalId;
                        vicServiceLog.Note = $"Successfully excluded/deleted GL account from VIC. GL Number: {glResponse.Number}"
;
                        vicServiceLog.DestinationENV = env;
                        vicServiceLog.DoNotSend = 1; // 1 means Do Not Send this status data

                        // insert gl log
                        _dal.SaveVicServiceLog(vicServiceLog);
                    }
                    catch (Exception ex)
                    {
                        var glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                        // save log and DoNotSend value 3 to 1;

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        vicServiceLog.Status = true;
                        vicServiceLog.VicResponse = responseData;

                        vicServiceLog.InternalId = glResponse.InternalId;
                        vicServiceLog.ObjectType = "GL Account";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = $"Request URI: {uri}";
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = glResponse.ExternalId;
                        vicServiceLog.Note = $"Failed to excluded/deleted GL account from VIC. {ex.Message}, GL Number: {glResponse.Number}"
;
                        vicServiceLog.DestinationENV = env;
                        vicServiceLog.DoNotSend = 3; // 1 means Do Not Send this status data and 3 means will be deleted this status next run

                        _dal.SaveVicServiceLog(vicServiceLog);

                        Console.WriteLine($"Error msg: {ex.Message}");

                        Utils.LogToFile(1, "[INFO]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");

                    }
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();

                    // create vic service log object
                    var vicServiceLog = new VicServiceLog();

                    vicServiceLog.Status = true;
                    vicServiceLog.VicResponse = errorMsg;

                    vicServiceLog.InternalId = item.InternalId;
                    vicServiceLog.ObjectType = "GL Account";
                    vicServiceLog.SourceType = sourceType;
                    vicServiceLog.VicRequest = $"Request URI: {uri}";
                    vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                    vicServiceLog.ExternalId = item.ExternalId;
                    vicServiceLog.Note = $"Failed to excluded/deleted GL account from VIC. {errorMsg}, GL Number: {item.ExternalId}"
;
                    vicServiceLog.DestinationENV = env;
                    vicServiceLog.DoNotSend = 1; // 1 means Do Not Send this status data and 3 means will be deleted this status next run

                    _dal.SaveVicServiceLog(vicServiceLog);

                    Console.WriteLine(errorMsg);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Failed to added the gl account. Error msg: {errorMsg}");
                }
            }

            return 1;
        }
        #endregion


        #region Total Service to Vic Master Data Post/Update

        public async Task<int> PostTestVendorForTotalService(List<TotalServiceVendorModel> vendors, string env, string sourceType, string token)
        {
            if (vendors.Count > 0)
            {
                foreach (TotalServiceVendorModel vendor in vendors)
                {
                    var uri = _baseAddress + _vendorUrl + vendor.VendorNumber;

                    var requestBody = new
                    {
                        externalId = $"{vendor.VendorNumber}",
                        externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                        name = vendor.VendorName,
                        phone = vendor.Phone,
                        addressStreet = vendor.Address,
                        addressCity = vendor.City,
                        addressState = vendor.State,
                        addressPostalCode = vendor.Zip,
                        state = vendor.Status,
                    };

                    // Serialize the object to JSON
                    var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                    // Serialize the request body to JSON
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await _httpClient.PutAsync(uri, content);
                    
                    // create log msg object
                    StringBuilder msgForLog = new StringBuilder();

                    // create vendor response object
                    VendorResponseModel vendorResponse = new VendorResponseModel();

                    // create vic service log object
                    var vicServiceLog = new VicServiceLog();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        try
                        {
                            vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseData);
                            vicServiceLog.Status = true;
                            vicServiceLog.VicResponse = responseData;
                        }
                        catch (Exception ex)
                        {
                            msgForLog.AppendLine("Vendor reponse parse error.");
                            msgForLog.AppendLine(ex.Message);
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }

                        msgForLog.AppendLine($"Successfully add/update the vendor. Vendor number: {vendor.VendorNumber}.");
                    }
                    else
                    {
                        Console.WriteLine($"Vendor number: {vendor.VendorNumber}");
                        Console.WriteLine(response.ReasonPhrase);

                        // error log
                        msgForLog.AppendLine($"Failed to add/update the vendor. Error msg: {response.ReasonPhrase}");
                        vicServiceLog.Status = false;
                        vicServiceLog.VicResponse = msgForLog.ToString();
                    }

                    vicServiceLog.InternalId = vendorResponse.InternalId;
                    vicServiceLog.ObjectType = "Vendor";
                    vicServiceLog.SourceType = sourceType;
                    vicServiceLog.VicRequest = json;
                    vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                    vicServiceLog.ExternalId = vendorResponse.ExternalId;
                    vicServiceLog.Note = msgForLog.ToString();
                    vicServiceLog.DestinationENV = env;

                    // save vic service log
                    _dal.SaveVicServiceLog(vicServiceLog);
                }

            }

            return 1;
        }

        public async Task<int> PostTestJobsForTotalService(List<TotalServiceJobModel> jobs, string env, string sourceType, string token)
        {
            if (jobs.Count > 0)
            {
                try
                {
                    foreach (TotalServiceJobModel job in jobs)
                    {
                        var uri = _baseAddress + _jobUrl + job.ID;

                        var requestBody = new
                        {
                            externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                            name = job.ID.ToString(),
                            shortName = job.JobDescription,
                            type = "jobnumber",
                            typeName = "Job",
                            typeExternalId = "jobnumber"
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        JobResponseModel jobResponse = new JobResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();

                            try
                            {
                                jobResponse = JsonConvert.DeserializeObject<JobResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("Job reponse parse error.");
                                msgForLog.AppendLine(ex.Message);
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();
                            }
                            msgForLog.AppendLine($"Successfully add/update the Job. Job number: {job.ID}.");
                        }
                        else
                        {
                            Console.WriteLine($"Job number: {job.ID}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            msgForLog.AppendLine($"Failed to add/update the Job. Error msg: {response.ReasonPhrase}");
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }

                        vicServiceLog.InternalId = jobResponse.InternalId;
                        vicServiceLog.ObjectType = "Job";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                        vicServiceLog.ExternalId = jobResponse.ExternalId;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        _dal.SaveVicServiceLog(vicServiceLog);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return 1;
        }

        public async Task<int> PostTestGLAccountsForTotalService(List<TotalServiceGLModel> glAccounts, string env, string sourceType, string token)
        {
            if (glAccounts.Count > 0)
            {
                try
                {
                    foreach (TotalServiceGLModel glAccount in glAccounts)
                    {
                        var uri = _baseAddress + _glUrl + glAccount.GLAccountNumber;

                        var requestBody = new
                        {
                            externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                            number = glAccount.GLAccountNumber,
                            name = glAccount.GLAccountDescription
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        GLResponseModel glResponse = new GLResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();

                            try
                            {
                                glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("GL reponse parse error.");
                                msgForLog.AppendLine(ex.Message);
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();
                            }
                            msgForLog.AppendLine($"Successfully add/update the gl account. GL number: {glAccount.GLAccountNumber}.");

                        }
                        else
                        {
                            Console.WriteLine($"Gl number: {glAccount.GLAccountNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            msgForLog.AppendLine($"Failed to add/update the gl account. Error msg: {response.ReasonPhrase}");
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = msgForLog.ToString();
                        }


                        vicServiceLog.InternalId = glResponse.InternalId;
                        vicServiceLog.ObjectType = "GL Account";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss");
                        vicServiceLog.ExternalId = glResponse.ExternalId;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        _dal.SaveVicServiceLog(vicServiceLog);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return 1;
        }


        public async Task<int> PostTESTPurchaseOrderForTotalService(List<PurchaseOrderDetailsModel> purchaseOrders, string env, string sourceType, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostTESTPurchaseOrderForTotalService");

            if (purchaseOrders.Count > 0)
            {
                try
                {
                    var groupByPO = purchaseOrders.GroupBy(x => x.PO).ToList();

                    foreach (var poGroup in groupByPO)
                    {
                        var uri = _baseAddress + _poUrl;
                        Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                        var po = poGroup.First(); // Use the first item for PO-level data
                        var vendorNumber = await GetVendorInternalIdByVendorNumberForTotalService(poGroup.First().VendorNumber, token);

                        var requestBody = new
                        {
                            issuedOn = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // ISO format with Zulu timezone
                            poNumber = po.PO.ToString(),
                            amount = po.TotalAmount.ToString(), // Ensure string with 2 decimals
                            currencyId = "USD",
                            vendor = new
                            {
                                internalId = vendorNumber // <-- You must set this in your model
                            },
                            lineItems = poGroup.Select(item => new
                            {
                                productNumber = item.ProductNumber,
                                quantityRequested = item.Quan.ToString(),
                                quantityReceived = string.Empty, // As per your JSON
                                unitAmount = item.Price.ToString(),
                                lineItemTotal = (item.Quan * item.Price).ToString(),
                                lineNumber = item.LineNumber.ToString(),
                                matchingType = "two_way"
                            }).ToList()
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                        Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PostAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        PurchaseOrderResponse poResponse = new PurchaseOrderResponse();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            Utils.LogToFile(3, "[INFO]", $"Reponse body: {responseData}");

                            try
                            {
                                poResponse = JsonConvert.DeserializeObject<PurchaseOrderResponse>(responseData);

                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("PO reponse parse error.");
                                msgForLog.AppendLine(ex.Message);

                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();

                                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}, stack trace: {ex.StackTrace}");

                            }
                            msgForLog.AppendLine($"Successfully add purchase order. PO number: {po.PO}.");

                        }
                        else
                        {
                            Console.WriteLine($"Gl number: {poResponse.PoNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            var responseBody = await response.Content.ReadAsStringAsync();

                            //var errors = JArray.Parse(responseBody);

                            // error log
                            msgForLog.AppendLine($"Failed to the purchase order. PO number: {po.PO}, Error msg: {responseBody}");
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = responseBody;

                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}");
                        }


                        vicServiceLog.InternalId = poResponse.InternalId;
                        vicServiceLog.ObjectType = "Purchase Order";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = po.PO.ToString();
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        _dal.SaveVicServiceLog(vicServiceLog);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }

        public async Task<string> GetVendorInternalIdByVendorNumberForTotalService(string vendorNumber, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVendorInternalIdByVendorNumber");

            string vendorInternalId = "";
            try
            {
                var url = _baseAddress + _vendorUrl + vendorNumber;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {url}");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseContent}");

                    // Optional: deserialize the response
                    try
                    {
                        var vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseContent);
                        if (vendorResponse != null)
                        {
                            vendorInternalId = vendorResponse.InternalId;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }

                    Console.WriteLine("Vendor data retrieved successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve vendor data. Status Code: {response.StatusCode}");

                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    //var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {responseBody}");
                }

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return vendorInternalId;
        }

        #endregion


        #region Total Service to Vic Master Data Post/Update For UAT
        public async Task<int> PostUATVendorForTotalService(List<TotalServiceVendorModel> vendors, string env, 
            string sourceType, string token, int isUpdated, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATVendorForTotalService");

            if (vendors?.Count > 0)
            {
                foreach (TotalServiceVendorModel vendor in vendors)
                {
                    try
                    {
                        string vendorNumber = EncoderModel.EncodeVendorNumber(vendor?.VendorNumber);
                        var uri = _baseAddressForUAT + _vendorUrl + vendorNumber;
                        Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                        var requestBody = new
                        {
                            externalId = $"{vendor?.VendorNumber}",
                            externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                            name = vendor?.VendorName,
                            phone = vendor?.Phone,
                            addressStreet = vendor?.Address,
                            addressCity = vendor?.City,
                            addressState = vendor?.State,
                            addressPostalCode = vendor?.Zip,
                            state = vendor?.Status,
                            countryCode = vendor.CountryCode
                        };

                        // Serialize the object to JSON
                        var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                        Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                        // Serialize the request body to JSON
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                        var response = await _httpClient.PutAsync(uri, content);

                        // create log msg object
                        StringBuilder msgForLog = new StringBuilder();

                        // create vendor response object
                        VendorResponseModel vendorResponse = new VendorResponseModel();

                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        if (response.IsSuccessStatusCode)
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                            try
                            {
                                vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseData);
                                vicServiceLog.Status = true;
                                vicServiceLog.VicResponse = responseData;
                            }
                            catch (Exception ex)
                            {
                                msgForLog.AppendLine("Vendor reponse parse error.");
                                msgForLog.AppendLine(ex.Message);

                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = msgForLog.ToString();

                                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                            }

                            if (isUpdated > 0)
                            {
                                msgForLog.AppendLine($"Successfully updated the vendor. Vendor number: {vendor.VendorNumber}.");
                            }
                            else
                            {
                                msgForLog.AppendLine($"Successfully added the vendor. Vendor number: {vendor.VendorNumber}.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Vendor number: {vendor.VendorNumber}");
                            Console.WriteLine(response.ReasonPhrase);

                            // error log
                            var responseBody = await response.Content.ReadAsStringAsync();
                            //var errors = JArray.Parse(responseBody);

                            // error log
                            if (isUpdated > 0)
                            {
                                msgForLog.AppendLine($"Failed to updated the vendor. Error msg: {responseBody}");
                            }
                            else
                            {
                                msgForLog.AppendLine($"Failed to added the vendor. Error msg: {responseBody}");
                            }
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = responseBody;
                        }

                        vicServiceLog.InternalId = vendorResponse.InternalId;
                        vicServiceLog.ObjectType = "Vendor";
                        vicServiceLog.SourceType = sourceType;
                        vicServiceLog.VicRequest = json;
                        vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                        vicServiceLog.ExternalId = vendor.VendorNumber;
                        vicServiceLog.Note = msgForLog.ToString();
                        vicServiceLog.DestinationENV = env;

                        // save vic service log
                        if (isUpdated > 0)
                        {
                            var existingVendor = vicServiceLogs?.Where(x => x.ExternalId == vendor?.VendorNumber).FirstOrDefault();
                            Utils.LogToFile(3, "[INFO]", $"Vendor number: {existingVendor?.ExternalId}, Vic vendor id: {existingVendor?.InternalId}");

                            vicServiceLog.Id = existingVendor.Id;
                            _dal.UpdateVicServiceLog(vicServiceLog);
                        }
                        else
                        {
                            _dal.SaveVicServiceLog(vicServiceLog);
                        }
                    }
                    catch (Exception ex) 
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from vendor: {vendor.VendorNumber}, name: {vendor.VendorName}");
                    
                    }
                    
                }

            }

            return 1;
        }

        public async Task<int> PostUATJobsForTotalService(List<TotalServiceJobModel> jobs, string env, 
            string sourceType, string token, int isUpdated, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATJobsForTotalService");

            if (jobs.Count > 0)
            {
                try
                {
                    foreach (TotalServiceJobModel job in jobs)
                    {
                        try
                        {
                            var uri = _baseAddressForUAT + _jobUrl + job.ID;
                            Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                            var requestBody = new
                            {
                                externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                                name = job.ID.ToString(),
                                shortName = job.JobDescription,
                                type = "jobnumber",
                                typeName = "Job",
                                typeExternalId = "jobnumber"
                            };

                            // Serialize the object to JSON
                            var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                            Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                            // Serialize the request body to JSON
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                            var response = await _httpClient.PutAsync(uri, content);

                            // create log msg object
                            StringBuilder msgForLog = new StringBuilder();

                            // create vendor response object
                            JobResponseModel jobResponse = new JobResponseModel();

                            // create vic service log object
                            var vicServiceLog = new VicServiceLog();

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                                try
                                {
                                    jobResponse = JsonConvert.DeserializeObject<JobResponseModel>(responseData);
                                    vicServiceLog.Status = true;
                                    vicServiceLog.VicResponse = responseData;
                                }
                                catch (Exception ex)
                                {
                                    msgForLog.AppendLine("Job reponse parse error.");
                                    msgForLog.AppendLine(ex.Message);
                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = msgForLog.ToString();

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                                }

                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Successfully updated the Job. Job number: {job.ID}.");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Successfully added the Job. Job number: {job.ID}.");
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Job number: {job.ID}");
                                Console.WriteLine(response.ReasonPhrase);

                                // error log
                                var responseBody = await response.Content.ReadAsStringAsync();
                                //var errors = JArray.Parse(responseBody);

                                // error log
                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Failed to updated the Job. Error msg: {responseBody}");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Failed to added the Job. Error msg: {responseBody}");
                                }
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = responseBody;
                            }

                            vicServiceLog.InternalId = jobResponse.InternalId;
                            vicServiceLog.ObjectType = "Job";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = json;
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = job.ID.ToString();
                            vicServiceLog.Note = msgForLog.ToString();
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            if (isUpdated > 0)
                            {
                                var existingJob = vicServiceLogs.Where(x => x.ExternalId == job.ID.ToString()).FirstOrDefault();
                                Utils.LogToFile(3, "[INFO]", $"Job number: {existingJob.ExternalId}, Vic job id: {existingJob.InternalId}");

                                vicServiceLog.Id = existingJob.Id;
                                _dal.UpdateVicServiceLog(vicServiceLog);
                            }
                            else
                            {
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                        }
                        catch (Exception ex) 
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from job: {job.ID}, description: {job.JobDescription}");
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }

        public async Task<int> PostUATGLAccountsForTotalService(List<TotalServiceGLModel> glAccounts, string env, 
            string sourceType, string token, int isUpdated, List<VicServiceLog> vicServiceLogs)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATGLAccountsForTotalService");

            if (glAccounts.Count > 0)
            {
                try
                {
                    foreach (TotalServiceGLModel glAccount in glAccounts)
                    {
                        try
                        {
                            var uri = _baseAddressForUAT + _glUrl + glAccount.GLAccountNumber;
                            Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                            var requestBody = new
                            {
                                externalUpdatedAt = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fff"),
                                number = glAccount.GLAccountNumber,
                                name = glAccount.GLAccountDescription
                            };

                            // Serialize the object to JSON
                            var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                            Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                            // Serialize the request body to JSON
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                            var response = await _httpClient.PutAsync(uri, content);

                            // create log msg object
                            StringBuilder msgForLog = new StringBuilder();

                            // create vendor response object
                            GLResponseModel glResponse = new GLResponseModel();

                            // create vic service log object
                            var vicServiceLog = new VicServiceLog();

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                Utils.LogToFile(3, "[INFO]", $"Response body: {responseData}");

                                try
                                {
                                    glResponse = JsonConvert.DeserializeObject<GLResponseModel>(responseData);
                                    vicServiceLog.Status = true;
                                    vicServiceLog.VicResponse = responseData;
                                }
                                catch (Exception ex)
                                {
                                    msgForLog.AppendLine("GL reponse parse error.");
                                    msgForLog.AppendLine(ex.Message);
                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = msgForLog.ToString();

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                                }

                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Successfully updated the gl account. GL number: {glAccount.GLAccountNumber}.");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Successfully added the gl account. GL number: {glAccount.GLAccountNumber}.");
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Gl number: {glAccount.GLAccountNumber}");
                                Console.WriteLine(response.ReasonPhrase);

                                // error log
                                var responseBody = await response.Content.ReadAsStringAsync();

                                if (isUpdated > 0)
                                {
                                    msgForLog.AppendLine($"Failed to updated the GL Account. Error msg: {responseBody}");
                                }
                                else
                                {
                                    msgForLog.AppendLine($"Failed to added the GL Account. Error msg: {responseBody}");
                                }
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = responseBody;

                                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {responseBody}");
                            }


                            vicServiceLog.InternalId = glResponse.InternalId;
                            vicServiceLog.ObjectType = "GL Account";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = json;
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = glAccount.GLAccountNumber;
                            vicServiceLog.Note = msgForLog.ToString();
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            if (isUpdated > 0)
                            {
                                var exitingAccount = vicServiceLogs.Where(x => x.ExternalId == glAccount.GLAccountNumber).FirstOrDefault();
                                Utils.LogToFile(3, "[INFO]", $"GL account number: {exitingAccount.ExternalId}, Vic gl account id: {exitingAccount.InternalId}");

                                vicServiceLog.Id = exitingAccount.Id;
                                _dal.UpdateVicServiceLog(vicServiceLog);
                            }
                            else
                            {
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                        }
                        catch (Exception ex) 
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from gl: {glAccount.GLAccountNumber}, description: {glAccount.GLAccountDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[INFO]", $"Error msg: {ex.Message}, stack trace: {ex.Message}");

                }

            }

            return 1;
        }


        public async Task<int> PostUATPurchaseOrderForTotalService(List<PurchaseOrderDetailsModel> purchaseOrders, string env, string sourceType, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATPurchaseOrderForTotalService");

            if (purchaseOrders.Count > 0)
            {
                try
                {
                    var groupByPO = purchaseOrders.GroupBy(x => x.PO).ToList();

                    foreach (var poGroup in groupByPO)
                    {
                        try
                        {
                            // create vic service log object
                            var vicServiceLog = new VicServiceLog();

                            // check job is closed or not
                            var result = IsClosedJob(poGroup);

                            if (result.isClosed)
                            {
                                int poNumber = poGroup.Key ?? 0; // Handle nullable PO
                                string closedJobs = string.Join(", ", result.closedJobNumbers);
                                string msg = $"Failed to save purchase order. PO Number: {poGroup.Key} has closed job(s): {closedJobs}";

                                Utils.LogToFile(3, "[INFO]", msg);

                                // save to log
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = "";
                                vicServiceLog.InternalId = "";
                                vicServiceLog.ObjectType = "Purchase Order";
                                vicServiceLog.SourceType = sourceType;
                                vicServiceLog.VicRequest = "";
                                vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                                vicServiceLog.ExternalId = poNumber.ToString();
                                vicServiceLog.Note = msg;
                                vicServiceLog.DestinationENV = env;

                                // save vic service log
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                            else
                            {
                                var uri = _baseAddressForUAT + _poUrl;
                                Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                                var po = poGroup.First(); // Use the first item for PO-level data
                                var vendorNumber = await GetVendorInternalIdByVendorNumber(poGroup.First().VendorNumber, token);

                                //object requestBody = null;

                                var requestBody = new
                                {
                                    issuedOn = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // ISO format
                                    poNumber = po.PO.ToString(),
                                    externalId = po.PO.ToString(),
                                    amount = po.TotalAmount.ToString(), // Ensure 2 decimal places
                                    currencyId = "USD",
                                    vendor = new
                                    {
                                        internalId = vendorNumber
                                    },
                                    status = "open",
                                    lineItems = poGroup.Select((item, index) => new
                                    {
                                        productNumber = item.Inv != null ? item.ProductNumber : EncoderModel.TruncatePOItemDescription(item.fdesc), // need trim if description char > 255
                                        quantityRequested = item.Quan.ToString(),
                                        quantityReceived = item.Inv != null ? string.Empty : "0",
                                        unitAmount = item.Price.ToString(),
                                        lineItemTotal = (item.Quan * item.Price).ToString(),
                                        lineNumber = (index + 1).ToString(),
                                        matchingType = "two_way",
                                        status = "open",
                                        dimensions = item.Job != null
                                            ? new[]
                                            {
                                        new
                                        {
                                            externalId = item.Job.ToString(),
                                            typeExternalId = "jobnumber"
                                        }
                                            }
                                            : new[]
                                            {
                                        new
                                        {
                                            externalId = string.Empty,
                                            typeExternalId = string.Empty
                                        }
                                            }.Take(0).ToArray() // no dimension for inventory-only items
                                    }).ToList()
                                };

                                // Serialize the object to JSON
                                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                                // Serialize the request body to JSON
                                var content = new StringContent(json, Encoding.UTF8, "application/json");

                                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                                var response = await _httpClient.PostAsync(uri, content);

                                // create log msg object
                                StringBuilder msgForLog = new StringBuilder();

                                // create vendor response object
                                PurchaseOrderResponse poResponse = new PurchaseOrderResponse();

                                if (response.IsSuccessStatusCode)
                                {
                                    var responseData = await response.Content.ReadAsStringAsync();
                                    Utils.LogToFile(3, "[INFO]", $"Reponse body: {responseData}");

                                    try
                                    {
                                        poResponse = JsonConvert.DeserializeObject<PurchaseOrderResponse>(responseData);

                                        vicServiceLog.Status = true;
                                        vicServiceLog.VicResponse = responseData;
                                    }
                                    catch (Exception ex)
                                    {
                                        msgForLog.AppendLine("PO reponse parse error.");
                                        msgForLog.AppendLine(ex.Message);

                                        vicServiceLog.Status = false;
                                        vicServiceLog.VicResponse = msgForLog.ToString();

                                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}, stack trace: {ex.StackTrace}");

                                    }
                                    msgForLog.AppendLine($"Successfully add purchase order. PO number: {po.PO}.");

                                }
                                else
                                {
                                    Console.WriteLine($"Gl number: {poResponse.PoNumber}");
                                    Console.WriteLine(response.ReasonPhrase);

                                    // error log
                                    var responseBody = await response.Content.ReadAsStringAsync();

                                    //var errors = JArray.Parse(responseBody);

                                    // error log
                                    msgForLog.AppendLine($"Failed to save purchase order. PO number: {po.PO}, Error msg: {responseBody}");
                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = responseBody;

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}");
                                }


                                vicServiceLog.InternalId = poResponse.InternalId;
                                vicServiceLog.ObjectType = "Purchase Order";
                                vicServiceLog.SourceType = sourceType;
                                vicServiceLog.VicRequest = json;
                                vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                                vicServiceLog.ExternalId = po.PO.ToString();
                                vicServiceLog.Note = msgForLog.ToString();
                                vicServiceLog.DestinationENV = env;

                                // save vic service log
                                _dal.SaveVicServiceLog(vicServiceLog);
                            }
                        }
                        catch (Exception ex) 
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg from po: { poGroup.Key}");
                        }

                        
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }

        public async Task<int> UpdateUATPurchaseOrderForTotalService(List<PurchaseOrderDetailsModel> purchaseOrders, string env, string sourceType, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in PostUATPurchaseOrderForTotalService");

            if (purchaseOrders.Count > 0)
            {
                try
                {
                    var groupByPO = purchaseOrders.GroupBy(x => x.PO).ToList();

                    foreach (var poGroup in groupByPO)
                    {
                        // create vic service log object
                        var vicServiceLog = new VicServiceLog();

                        // check job is closed or not
                        var result = IsClosedJob(poGroup);

                        if (result.isClosed)
                        {
                            int poNumber = poGroup.Key ?? 0; // Handle nullable PO
                            string closedJobs = string.Join(", ", result.closedJobNumbers);
                            string msg = $"Failed to save purchase order. PO Number: {poGroup.Key} has closed job(s): {closedJobs}";

                            Utils.LogToFile(3, "[INFO]", msg);

                            // save to log
                            vicServiceLog.Status = false;
                            vicServiceLog.VicResponse = "";
                            vicServiceLog.InternalId = "";
                            vicServiceLog.ObjectType = "Purchase Order";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = "";
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = poNumber.ToString();
                            vicServiceLog.Note = msg;
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            _dal.SaveVicServiceLog(vicServiceLog);
                        }
                        else
                        {
                            var uri = _baseAddressForUAT + _poUrl + $"/{poGroup.Key}?useSystem=external";
                            Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                            var po = poGroup.First(); // Use the first item for PO-level data
                            var vendorNumber = await GetVendorInternalIdByVendorNumber(poGroup.First().VendorNumber, token);

                            var requestBody = new
                            {
                                issuedOn = USATimeModel.GetUSATime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), // ISO format with Zulu timezone
                                poNumber = po.PO.ToString(),
                                externalId = po.PO.ToString(),
                                amount = po.TotalAmount.ToString(), // Ensure string with 2 decimals
                                currencyId = "USD",
                                vendor = new
                                {
                                    internalId = vendorNumber // <-- You must set this in your model
                                },
                                lineItems = poGroup.Select(item => new
                                {
                                    productNumber = item.ProductNumber,
                                    quantityRequested = item.Quan.ToString(),
                                    quantityReceived = string.Empty, // As per your JSON
                                    unitAmount = item.Price.ToString(),
                                    lineItemTotal = (item.Quan * item.Price).ToString(),
                                    lineNumber = item.LineNumber.ToString(),
                                    matchingType = "two_way"
                                }).ToList()
                            };

                            // Serialize the object to JSON
                            var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);
                            Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                            // Serialize the request body to JSON
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                            var response = await _httpClient.PutAsync(uri, content);

                            // create log msg object
                            StringBuilder msgForLog = new StringBuilder();

                            // create vendor response object
                            PurchaseOrderResponse poResponse = new PurchaseOrderResponse();

                            

                            if (response.IsSuccessStatusCode)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                Utils.LogToFile(3, "[INFO]", $"Reponse body: {responseData}");

                                try
                                {
                                    poResponse = JsonConvert.DeserializeObject<PurchaseOrderResponse>(responseData);

                                    vicServiceLog.Status = true;
                                    vicServiceLog.VicResponse = responseData;
                                }
                                catch (Exception ex)
                                {
                                    msgForLog.AppendLine("PO reponse parse error.");
                                    msgForLog.AppendLine(ex.Message);

                                    vicServiceLog.Status = false;
                                    vicServiceLog.VicResponse = msgForLog.ToString();

                                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}, stack trace: {ex.StackTrace}");

                                }
                                msgForLog.AppendLine($"Successfully updated purchase order. PO number: {po.PO}.");

                            }
                            else
                            {
                                Console.WriteLine($"Gl number: {poResponse.PoNumber}");
                                Console.WriteLine(response.ReasonPhrase);

                                // error log
                                var responseBody = await response.Content.ReadAsStringAsync();

                                //var errors = JArray.Parse(responseBody);

                                // error log
                                msgForLog.AppendLine($"Failed to update the purchase order. PO number: {po.PO}, Error msg: {responseBody}");
                                vicServiceLog.Status = false;
                                vicServiceLog.VicResponse = responseBody;

                                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {msgForLog.ToString()}");
                            }


                            vicServiceLog.InternalId = poResponse.InternalId;
                            vicServiceLog.ObjectType = "Purchase Order";
                            vicServiceLog.SourceType = sourceType;
                            vicServiceLog.VicRequest = json;
                            vicServiceLog.SentDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt");
                            vicServiceLog.ExternalId = po.PO.ToString();
                            vicServiceLog.Note = msgForLog.ToString();
                            vicServiceLog.DestinationENV = env;

                            // save vic service log
                            _dal.SaveVicServiceLog(vicServiceLog);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

            }

            return 1;
        }


        // check job is close or open
        //public (bool isClosed, List<int> closedJobNumbers) IsClosedJob(IGrouping<int?, PurchaseOrderDetailsModel> groupedItems)
        //{
        //    var closedJobs = new List<int>();

        //    foreach (var item in groupedItems)
        //    {
        //        if (item.Job.HasValue)
        //        {
        //            var isClosedJob = _totalService.GetClosedJob(item.Job.Value); // Your method should return job info if closed

        //            if (isClosedJob != null && isClosedJob.JobNumber > 0)
        //            {
        //                if (!closedJobs.Contains(isClosedJob.JobNumber))
        //                {
        //                    closedJobs.Add(isClosedJob.JobNumber);
        //                }
        //            }
        //        }
        //    }

        //    return (closedJobs.Any(), closedJobs);
        //}

        public (bool isClosed, List<int> closedJobNumbers) IsClosedJob(IGrouping<int?, PurchaseOrderDetailsModel> groupedItems)
        {
            var closedJobs = new List<int>();

            try
            {
                if (groupedItems != null)
                {

                    foreach (var item in groupedItems)
                    {
                        if (item != null)
                        {
                            if (item.Job.HasValue)
                            {
                                if (_totalService == null)
                                {
                                    //throw new NullReferenceException("_totalService is null");
                                    Utils.LogToFile(1, "[EXCEPTION]", $"_totalService is null");
                                }

                                var isClosedJob = _totalService.GetClosedJob(item.Job.Value);

                                if (isClosedJob != null && isClosedJob.JobNumber > 0)
                                {
                                    if (!closedJobs.Contains(isClosedJob.JobNumber))
                                    {
                                        closedJobs.Add(isClosedJob.JobNumber);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Utils.LogToFile(1, "[EXCEPTION]", $"An item in groupedItems is null");
                        }

                    }
                }
                else
                {
                    Utils.LogToFile(1, "[EXCEPTION]", $"groupedItems was null");
                }
            }
            catch(Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
            }
            

            return (closedJobs.Any(), closedJobs);
        }


        public async Task<string> GetVendorInternalIdByVendorNumber(string vendorNumber, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVendorInternalIdByVendorNumber");

            string vendorInternalId = "";
            try
            {
                var url = _baseAddressForUAT + _vendorUrl + vendorNumber;
                Utils.LogToFile(3, "[INFO]", $"Request uri: {url}");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseContent}");

                    // Optional: deserialize the response
                    try
                    {
                        var vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseContent);
                        if (vendorResponse != null) 
                        {
                            vendorInternalId = vendorResponse.InternalId;
                        }
                    }
                    catch(Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }

                    Console.WriteLine("Vendor data retrieved successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve vendor data. Status Code: {response.StatusCode}");
                    
                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    //var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {responseBody}");
                }

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return vendorInternalId;
        }

        public async Task<VendorResponseModel> GetVendorByVendorNumber(string vendorNumber, string token)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVendorByVendorNumber");

            var vendor = new VendorResponseModel();

            try
            {
                var url = _baseAddressForUAT + _vendorUrl + EncoderModel.EncodeVendorNumber(vendorNumber);
                Utils.LogToFile(3, "[INFO]", $"Request uri: {url}");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseContent}");

                    // Optional: deserialize the response
                    try
                    {
                        var vendorResponse = JsonConvert.DeserializeObject<VendorResponseModel>(responseContent);
                        if (vendorResponse != null)
                        {
                            vendor = vendorResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                    }

                    Console.WriteLine("Vendor data retrieved successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve vendor data. Status Code: {response.StatusCode}");

                    // error log
                    var responseBody = await response.Content.ReadAsStringAsync();
                    //var errors = JArray.Parse(responseBody);

                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {responseBody}");
                }

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return vendor;
        }

        #endregion



        #region Invoice post for WinTeam Test
        public async Task<List<InvoiceReadyForPostModel>> GetInvoiceForPostWinTeam(string token)
        {
            List<InvoiceReadyForPostModel> invoiceReadyForPostDataList = new List<InvoiceReadyForPostModel>();

            try
            {
                var uri = _baseAddress + _invoiceReadyForPostUrl;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var requestData = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var dataObject = JsonConvert.DeserializeObject<List<InvoiceReadyForPostModel>>(requestData).ToList();
                        if (dataObject.Count > 0)
                        {
                            invoiceReadyForPostDataList = dataObject;

                            Console.WriteLine($"Total Invoice Ready For Post WinTeam: {dataObject.Count}");
                        }

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

            return invoiceReadyForPostDataList;
        }

        // confirmation when post an invoice from vic to winteam/total service system.
        public async Task<int> UpdateInvoiceForConfirmation(WinTeamInvoiceModel winTeamInvoice, string token)
        {
            var uri = _baseAddress + _invoiceUrl + winTeamInvoice?.VicInternalId;
            int result = 0;

            if (!string.IsNullOrEmpty(winTeamInvoice?.VicInternalId))
            {
                var requestBody = new
                {
                    externalId = winTeamInvoice.InvoiceNumber,
                    externalUpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    postingError = ""
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestBody, Formatting.Indented);

                // Serialize the request body to JSON
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri)
                {
                    Content = content
                };

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                //var response = await _httpClient.PutAsync(uri, content); // need to change patch

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();

                    try
                    {
                        result = 1;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        result = 0;
                    }
                }
                else
                {
                    result = 0;
                    Console.WriteLine($"Error from Invoice Update API Post: {response.ReasonPhrase}");
                } 
            }

            return result;
        }
        #endregion


        #region Master Data Match IF Exist or Not
        public List<VicServiceLog> GetVicServiceLogList(string destinationEnv, string source,string objectType)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVicServiceLogList");

            _dt = _dal.GetVicServiceLogs(destinationEnv, source, objectType);

            List<VicServiceLog> vicServiceLogList = new List<VicServiceLog>();

            vicServiceLogList = (from DataRow row in _dt.Rows
                                 select new VicServiceLog
                                 {
                                     Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                                     InternalId = row["InternalId"] != DBNull.Value ? row["InternalId"].ToString() : string.Empty,
                                     ExternalId = row["ExternalId"] != DBNull.Value ? row["ExternalId"].ToString() : string.Empty,
                                     VicRequest = row["VicRequest"] != DBNull.Value ? row["VicRequest"].ToString() : string.Empty,
                                     VicResponse = row["VicResponse"] != DBNull.Value ? row["VicResponse"].ToString() : string.Empty,
                                     SentDate = row["SentDate"] != DBNull.Value ? row["SentDate"].ToString() : string.Empty,
                                     SourceType = row["SourceType"] != DBNull.Value ? row["SourceType"].ToString() : string.Empty,
                                     Status = row["Status"] != DBNull.Value ? Convert.ToBoolean(row["Status"]) : false,
                                     Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : string.Empty,
                                     ObjectType = row["ObjectType"] != DBNull.Value ? row["ObjectType"].ToString() : string.Empty,
                                     DestinationENV = row["DestinationENV"] != DBNull.Value ? row["DestinationENV"].ToString() : string.Empty,
                                     DoNotSend = row["DoNotSend"] != DBNull.Value ? Convert.ToInt32(row["DoNotSend"]) : 0
                                 }).ToList();

            Utils.LogToFile(3, "[INFO]", $"Destination: {destinationEnv}, Source: {source}, Object: {objectType}, Total data: {vicServiceLogList.Count}");

            return vicServiceLogList;
        }

        public VicServiceLog GetVendorTagByVendorNumber(string destinationEnv, string source, string objectType, string vendorNumber)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetVicServiceLogList");

            _dt = _dal.GetVendorTagByVendorNumber(destinationEnv, source, objectType, vendorNumber);

            VicServiceLog vendorTag = new VicServiceLog();

            vendorTag = (from DataRow row in _dt.Rows
                            select new VicServiceLog
                            {
                                Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                                InternalId = row["InternalId"] != DBNull.Value ? row["InternalId"].ToString() : string.Empty,
                                ExternalId = row["ExternalId"] != DBNull.Value ? row["ExternalId"].ToString() : string.Empty,
                                VicRequest = row["VicRequest"] != DBNull.Value ? row["VicRequest"].ToString() : string.Empty,
                                VicResponse = row["VicResponse"] != DBNull.Value ? row["VicResponse"].ToString() : string.Empty,
                                SentDate = row["SentDate"] != DBNull.Value ? row["SentDate"].ToString() : string.Empty,
                                SourceType = row["SourceType"] != DBNull.Value ? row["SourceType"].ToString() : string.Empty,
                                Status = row["Status"] != DBNull.Value ? Convert.ToBoolean(row["Status"]) : false,
                                Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : string.Empty,
                                ObjectType = row["ObjectType"] != DBNull.Value ? row["ObjectType"].ToString() : string.Empty,
                                DestinationENV = row["DestinationENV"] != DBNull.Value ? row["DestinationENV"].ToString() : string.Empty

                            }).FirstOrDefault();

            Utils.LogToFile(3, "[INFO]", $"Destination: {destinationEnv}, Source: {source}, Object: {objectType}, Vendor: {vendorTag?.ExternalId}");

            return vendorTag;
        }
        #endregion
    }
}
