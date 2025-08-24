using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinTeamToVic_ConsoleApp.DataAccess;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class VicDataManagerService
    {
        public VicAiAPIService _vicAiAPIService;
        public WinTeamAPIService _winteamAPIService;
        public GLService _glService;
        public TotalService _totalService;
        private readonly VendorTagService _vendorTagService;

        public VicDataManagerService()
        {
            _vicAiAPIService = new VicAiAPIService();
            _winteamAPIService = new WinTeamAPIService();
            _glService = new GLService();
            _totalService = new TotalService();
            _vendorTagService = new VendorTagService();
        }

        //public async Task<int> WinTeamDataManagerForTest(string environment, string source, string token)
        //{
        //    Utils.LogToFile(3, "[INFO]", $"Inside in WinTeamDataManagerForTest");

        //    var glList = _glService.GetGLData();
        //    var glActiveAccount = await _glService.GetActiveGLAccountFromWinTeam(glList);

        //    var vendorList = await _winteamAPIService.GetActiveVendors();
        //    var jobList = await _winteamAPIService.GetActiveJobs();


        //    if (vendorList.Count > 0)
        //    {
        //        //await _vicAiAPIService.PostTestVendorForWinTeam(vendorList, environment, source, token); // post vendor from winteam to vic.ai
        //    }
        //    else
        //    {
        //        Console.WriteLine("Vendor was null. Please check.");
        //    }

        //    if (jobList.Count > 0)
        //    {
        //        //await _vicAiAPIService.PostTestJobsForWinTeam(jobList, environment, source, token); // post job from winteam to vic.ai
        //    }
        //    else
        //    {
        //        Console.WriteLine("Job was null. Please check.");
        //    }

        //    if (glActiveAccount.Count > 0)
        //    {
        //        await _vicAiAPIService.PostTestGLAccountsForWinTeam(glActiveAccount, environment, source, token); // post gl account from winteam to vic.ai
        //    }
        //    else
        //    {
        //        Console.WriteLine("GL chart of account was null here. Please check.");
        //    }

        //    return 1;
        //}

        public async Task<int> TotalServiceDataManagerForTest(string environment, string source, string token)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in TotalServiceDataManagerForTest");

            var jobs = _totalService.GetTotalServiceJobs();
            var vendors = _totalService.GetTotalServiceVendors();
            var glAccounts = _totalService.GetTotalServiceGLAccounts();

            var purchaseOrders = _totalService.GetTotalServiceOpenPurchaseOrders();

            var vicAiService = new VicAiAPIService();

            // post function write for total service to vic data upload
            //if (vendors.Count > 0)
            //{
            //    await vicAiService.PostTestVendorForTotalService(vendors, environment, source, token); // post vendor from winteam to vic.ai
            //}
            //else
            //{
            //    Console.WriteLine("Vendor was null. Please check.");
            //}

            //if (jobs.Count > 0)
            //{
            //    await vicAiService.PostTestJobsForTotalService(jobs, environment, source, token); // post job from winteam to vic.ai
            //}
            //else
            //{
            //    Console.WriteLine("Job was null. Please check.");
            //}

            //if (glAccounts.Count > 0)
            //{
            //    await vicAiService.PostTestGLAccountsForTotalService(glAccounts, environment, source, token); // post gl account from winteam to vic.ai
            //}
            //else
            //{
            //    Console.WriteLine("GL chart of account was null here. Please check.");
            //}

            if (purchaseOrders.Count > 0)
            {
                // account check 
                var existingPOs = _vicAiAPIService.GetVicServiceLogList(environment, source, "Purchase Order");
                Utils.LogToFile(3, "[INFO]", $"Total service existing purchase orders: {existingPOs.Count}");

                var unmatchedPO = purchaseOrders
                        .Where(v => !existingPOs.Any(e => e.ExternalId == v.PO.ToString()))
                        .ToList();

                Utils.LogToFile(3, "[INFO]", $"Total service unmatched/new purchase orders: {unmatchedPO.Count}");

                await vicAiService.PostTESTPurchaseOrderForTotalService(unmatchedPO, environment, source, token); // post gl account from winteam to vic.ai
            }
            else
            {
                Console.WriteLine("Purchase order was null here. Please check.");
                Utils.LogToFile(3, "[INFO]", "Purchase order was null here.");

            }

            return 1;
        }

        public async Task<int> WinTeamDataManagerForUAT(string environment, string source, string token)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in WinTeamDataManagerForUAT");

            var glList = _glService.GetGLData();

            await _glService.GetGLListFromFile(glList); // for gl data active inactive functionality

            var vendorList = await _winteamAPIService.GetActiveVendors();
            var jobList = await _winteamAPIService.GetActiveJobs();

            //post function write for total service to vic data upload
            if (vendorList.Count > 0)
            {
                // vendor check 
                var existingVendors = _vicAiAPIService.GetVicServiceLogList(environment, source, "Vendor");


                // Compare
                var changeVendors = new List<VendorModel>();
                string countryCodeValue;

                foreach (var vendor in existingVendors)
                {
                    // Parse the JSON string into a JObject
                    JObject jObject = JObject.Parse(vendor.VicRequest);

                    // Remove a field
                    jObject.Remove("externalUpdatedAt");

                    // Convert back to string if needed
                    string updatedJson = jObject.ToString();

                    var existingVendor = JsonConvert.DeserializeObject<VendorRequestBodyModel>(updatedJson);

                    var changeVendor = vendorList.Where(x => x.VendorNumber == int.Parse(existingVendor.ExternalId)).FirstOrDefault();

                    // get single vendor
                    var singleVendor = await _vicAiAPIService.GetSingleVendor(int.Parse(vendor.ExternalId));


                    if (singleVendor != null)
                    {
                        var countryCodeObject = singleVendor.CustomFields.Where(x => x.FieldNumber == 2).FirstOrDefault();
                        int countryCodeId = 0;
                        countryCodeValue = "";

                        if (changeVendor != null)
                        {
                            if (countryCodeObject != null)
                            {
                                countryCodeId = int.Parse(countryCodeObject.Value);
                                if (countryCodeId > 0)
                                {
                                    var vendorCountryCode = _winteamAPIService.GetCountryCodes(countryCodeId);
                                    countryCodeValue = vendorCountryCode.ISOCode;
                                }
                            }

                            if (
                                changeVendor.VendorNumber.ToString() != existingVendor.ExternalId ||
                                !string.Equals(changeVendor.VendorName, existingVendor.Name, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(changeVendor.Phone, existingVendor.Phone, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(changeVendor.Address?.Address1, existingVendor.AddressStreet, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(changeVendor.Address?.City, existingVendor.AddressCity, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(changeVendor.Address?.State, existingVendor.AddressState, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(changeVendor.Address?.Zip, existingVendor.AddressPostalCode, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(countryCodeValue.Trim(), existingVendor.CountryCode, StringComparison.OrdinalIgnoreCase) ||

                                changeVendor.VendorStatus != existingVendor.State
                            )
                            {
                                changeVendors.Add(changeVendor);
                            }

                        }


                        // vendor tag update process
                        var existingVendorTag = _vicAiAPIService.GetVendorTagByVendorNumber(environment, source, "Vendor Tag Assign", vendor.ExternalId);

                        // get tag name by tag id
                        var vendorTags = await _vicAiAPIService.GetVendorTags(token);


                        if (existingVendorTag != null && !string.IsNullOrEmpty(existingVendorTag.ExternalId))
                        {
                            // parse request body data to object
                            var vendorTagRequestBody = existingVendorTag.VicResponse;
                            var vendorTagData = JsonConvert.DeserializeObject<VendorTagAssign>(vendorTagRequestBody);

                            // get single tag
                            var vendorTag = vendorTags.Where(x => x.Id.Trim() == vendorTagData?.TagId?.Trim()).FirstOrDefault();

                            // get tag name
                            var existingTagName = vendorTag?.Value.ToUpper();

                            // vendor tag is updated or not
                            if (changeVendor != null)
                            {
                                int? vendorNumber = changeVendor.VendorNumber;

                                if (singleVendor.CustomFields.Count > 0)
                                {
                                    // get vendor tag
                                    var vendorCustomFieldForTag = singleVendor.CustomFields.Where(x => x.FieldNumber == 1).FirstOrDefault();

                                    if (vendorCustomFieldForTag != null)
                                    {
                                        var newTagId = vendorCustomFieldForTag.Value.Trim();


                                        string newTagName = "";

                                        if (int.TryParse(newTagId, out var id))
                                        {
                                            var singleVendorTag = _vendorTagService.GetVendorTag(id);
                                            newTagName = singleVendorTag?.Description?.ToUpper();
                                        }
                                        else
                                        {
                                            Utils.LogToFile(3, "[INFO]", $"Invalid tag id. Return value: {newTagId}");
                                            Console.WriteLine($"Invalid tag ID format: {newTagId}");

                                            newTagName = vendorCustomFieldForTag.Value.Trim();
                                        }


                                        //var singleVendorTag = _vendorTagService.GetVendorTag(int.Parse(newTagId));
                                        //var newTagName = singleVendorTag?.Description?.ToUpper();

                                        // get single tag
                                        var newVendorTag = vendorTags.Where(x => x.Value.Trim() == newTagName).FirstOrDefault();


                                        // if updated then delete tag
                                        if (existingTagName != newTagName)
                                        {
                                            Utils.LogToFile(3, "[INFO]", $"Vendor tag wasn't match.");

                                            var tagId = newVendorTag.Id.Trim();

                                            // delete previous tag
                                            var isDeleted = await _vicAiAPIService.DeleteVendorTag(token, existingVendorTag.InternalId, vendorNumber);

                                            if (isDeleted == 1)
                                            {
                                                // assign vendor tag
                                                await _vicAiAPIService.VendorTagAssign(token, source, environment, vendorNumber, vendor.InternalId, tagId, 1);
                                            }

                                        }
                                        else
                                        {
                                            Utils.LogToFile(3, "[INFO]", $"Vendor tag was not change. Vendor number: {vendorNumber}");
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            Utils.LogToFile(3, "[INFO]", $"Vendor tag not found. Vendor number: {vendor.ExternalId}");

                            // create new tag and assign to vendor
                            int? vendorNumber = int.Parse(vendor.ExternalId);

                            if (singleVendor != null && singleVendor.CustomFields.Count > 0)
                            {
                                // get vendor tag
                                var vendorCustomFieldForTag = singleVendor.CustomFields.Where(x => x.FieldNumber == 1).FirstOrDefault();

                                if (vendorCustomFieldForTag != null)
                                {
                                    var tagIdFromWinTeam = vendorCustomFieldForTag.Value.Trim();

                                    string tag = "";

                                    if (int.TryParse(tagIdFromWinTeam, out var id))
                                    {
                                        var singleVendorTag = _vendorTagService.GetVendorTag(id);
                                        tag = singleVendorTag?.Description?.ToUpper();
                                    }
                                    else
                                    {
                                        Utils.LogToFile(3, "[INFO]", $"Invalid tag id. Return value: {tagIdFromWinTeam}");
                                        Console.WriteLine($"Invalid tag ID format: {tagIdFromWinTeam}");

                                        tag = vendorCustomFieldForTag.Value.Trim();
                                    }

                                    // already have this tag
                                    var existingTag = vendorTags.Where(x => x.Value.ToUpper().Trim() == tag.Trim()).FirstOrDefault();

                                    if (existingTag == null)
                                    {
                                        // new tag create.
                                        var vendorTagAfterSave = await _vicAiAPIService.AddVendorForWinTeamToVicUAT(token, tag, source, environment, vendorNumber);

                                        // assign vendor tag
                                        await _vicAiAPIService.VendorTagAssign(token, source, environment, vendorNumber, vendor.InternalId, vendorTagAfterSave.Id, 1);
                                    }
                                    else
                                    {
                                        // assign vendor tag
                                        await _vicAiAPIService.VendorTagAssign(token, source, environment, vendorNumber, vendor.InternalId, existingTag.Id, 1);
                                    }
                                }

                            }

                        }
                    }
                    else
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Get single vendor function was null here. Vendor number: {vendor.ExternalId}");
                    }
                }


                var unmatchedVendors = vendorList
                        .Where(v => !existingVendors.Any(e => e.ExternalId.Trim() == v.VendorNumber.ToString().Trim()))
                        .ToList();


                await _vicAiAPIService.PostUATVendorForWinTeam(changeVendors, environment, source, token, 1, existingVendors); // update vendor from winteam to vic.ai
                await _vicAiAPIService.PostUATVendorForWinTeam(unmatchedVendors, environment, source, token, 0, existingVendors); // post vendor from winteam to vic.ai
            }
            else
            {
                Console.WriteLine("Vendor was null. Please check.");
            }

            if (jobList.Count > 0)
            {
                // job check 
                var existingJobs = _vicAiAPIService.GetVicServiceLogList(environment, source, "Job");

                // Compare
                var changeJobs = new List<JobModel>();

                foreach (var job in existingJobs)
                {
                    // Parse the JSON string into a JObject
                    JObject jObject = JObject.Parse(job.VicRequest);

                    // Remove a field
                    jObject.Remove("externalUpdatedAt");

                    // Convert back to string if needed
                    string updatedJson = jObject.ToString();

                    var existingJob = JsonConvert.DeserializeObject<JobRequestBodyModel>(updatedJson);

                    var changeJob = jobList.Where(x => x.JobNumber == existingJob.Name).FirstOrDefault();

                    if (changeJob != null)
                    {

                        if (
                            changeJob.JobNumber.ToString() != existingJob.Name ||
                            !string.Equals(changeJob.JobDescription, existingJob.ShortName, StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            changeJobs.Add(changeJob);
                        }

                    }

                }


                var unmatchedJobs = jobList
                        .Where(v => !existingJobs.Any(e => e.ExternalId == v.JobNumber.ToString()))
                        .ToList();



                await _vicAiAPIService.PostUATJobsForWinTeam(unmatchedJobs, environment, source, token, 0, existingJobs); // post job from winteam to vic.ai
                await _vicAiAPIService.PostUATJobsForWinTeam(changeJobs, environment, source, token, 1, existingJobs); // update job from winteam to vic.ai
            }
            else
            {
                Console.WriteLine("Job was null. Please check.");
            }

            if (glList.Count > 0)
            {

                // account check 
                var existingGls = _vicAiAPIService.GetVicServiceLogList(environment, source, "GL Account");

                var existingDoNotSendGls = existingGls
                    .Where(x => x.DoNotSend == 0)
                    .ToList();

                var deleteGLList = existingGls
                    .Where(x => x.DoNotSend == 3)
                    .ToList();

                // Compare
                var changeGLs = new List<GLModel>();

                foreach (var gl in existingDoNotSendGls)
                {
                    try
                    {
                        // Parse the JSON string into a JObject
                        JObject jObject = JObject.Parse(gl.VicRequest);

                        // Remove a field
                        jObject.Remove("externalUpdatedAt");

                        // Convert back to string if needed
                        string updatedJson = jObject.ToString();

                        var existingGL = JsonConvert.DeserializeObject<GLRequestBodyModel>(updatedJson);

                        var changegl = glList.Where(x => x.GlNumber == existingGL.Number).FirstOrDefault();

                        if (changegl != null)
                        {

                            if (
                                changegl.GlNumber.ToString() != existingGL.Number ||
                                !string.Equals(changegl.AccountDescription, existingGL.Name, StringComparison.OrdinalIgnoreCase)
                            )
                            {
                                changeGLs.Add(changegl);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogToFile(1, "[INFO]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}, GL number: {gl.ExternalId}");
                    }
                }

                var unmatchedAccounts = glList
                        .Where(v => !existingGls.Any(e => e.ExternalId.Trim() == v.GlNumber.ToString().Trim()))
                        .ToList();

                var glActiveAccounts = await _glService.GetActiveGLAccountFromWinTeam(unmatchedAccounts);

                await _vicAiAPIService.PostUATGLAccountsForWinTeam(glActiveAccounts, environment, source, token, 0, existingGls); // post gl account from winteam to vic.ai
                await _vicAiAPIService.PostUATGLAccountsForWinTeam(changeGLs, environment, source, token, 1, existingGls); // update gl account from winteam to vic.ai

                // update winteam gl description
                //await _glService.GetGLListFromFile(changeGLs);

                // delete gl from vic when log DoNotSend is 3. And after delete DoNotSend will be 1
                //await _vicAiAPIService.RemoveWinTeamGL(token, environment, source, deleteGLList);
            }
            else
            {
                Console.WriteLine("GL chart of account was null here. Please check.");
            }

            return 1;
        }

        public async Task<int> TotalServiceDataManagerForUAT(string environment, string source, string token)
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in TotalServiceDataManagerForUAT");

            var jobs = _totalService.GetTotalServiceJobs();
            var vendors = _totalService.GetTotalServiceVendors();
            var glAccounts = _totalService.GetTotalServiceGLAccounts();
            var purchaseOrders = _totalService.GetTotalServiceOpenPurchaseOrders();

            var vicAiService = new VicAiAPIService();

            // for vendors
            if (vendors.Count > 0)
            {
                // vendor check 
                var existingVendors = _vicAiAPIService.GetVicServiceLogList(environment, source, "Vendor");
                Utils.LogToFile(3, "[INFO]", $"Total service existing vendors: {existingVendors.Count}");

                // Compare
                var changeVendors = new List<TotalServiceVendorModel>();

                foreach (var vendor in existingVendors)
                {
                    // Parse the JSON string into a JObject
                    JObject jObject = JObject.Parse(vendor.VicRequest);

                    // Remove a field
                    jObject.Remove("externalUpdatedAt");

                    // Convert back to string if needed
                    string updatedJson = jObject.ToString();

                    var existingVendor = JsonConvert.DeserializeObject<VendorRequestBodyModel>(updatedJson);

                    var changeVendor = vendors.Where(x => x.VendorNumber == existingVendor.ExternalId).FirstOrDefault();

                    if (changeVendor != null)
                    {

                        if (
                            changeVendor.VendorNumber.ToString() != existingVendor.ExternalId ||
                            !string.Equals(changeVendor.VendorName, existingVendor.Name, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(changeVendor.Phone, existingVendor.Phone) ||
                            !string.Equals(changeVendor.Address, existingVendor.AddressStreet, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(changeVendor.City, existingVendor.AddressCity, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(changeVendor.State, existingVendor.AddressState, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(changeVendor.Zip, existingVendor.AddressPostalCode, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(changeVendor.CountryCode, existingVendor.CountryCode, StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            changeVendors.Add(changeVendor);
                        }

                    }

                }

                var unmatchedVendors = vendors
                        .Where(v => !existingVendors.Any(e => e.ExternalId.Trim() == v.VendorNumber.Trim()))
                        .ToList();


                Utils.LogToFile(3, "[INFO]", $"Total service unmatched/new vendors: {unmatchedVendors.Count}");

                await vicAiService.PostUATVendorForTotalService(unmatchedVendors, environment, source, token, 0, existingVendors); // post vendor from total service to vic.ai
                await vicAiService.PostUATVendorForTotalService(changeVendors, environment, source, token, 1, existingVendors); // update vendor from total service to vic.ai
            }
            else
            {
                Console.WriteLine("Vendor was null. Please check.");
                Utils.LogToFile(3, "[INFO]", "Vendor was null.");
            }

            // for jobs
            if (jobs.Count > 0)
            {
                // job check
                var existingJobs = _vicAiAPIService.GetVicServiceLogList(environment, source, "Job");
                Utils.LogToFile(3, "[INFO]", $"Total service existing jobs: {existingJobs.Count}");

                // Compare
                var changeJobs = new List<TotalServiceJobModel>();

                foreach (var job in existingJobs)
                {
                    // Parse the JSON string into a JObject
                    JObject jObject = JObject.Parse(job.VicRequest);

                    // Remove a field
                    jObject.Remove("externalUpdatedAt");

                    // Convert back to string if needed
                    string updatedJson = jObject.ToString();

                    var existingJob = JsonConvert.DeserializeObject<JobRequestBodyModel>(updatedJson);


                    var changeJob = jobs.Where(x => x.ID == int.Parse(existingJob.Name)).FirstOrDefault();

                    if (changeJob != null)
                    {

                        if (
                            changeJob.ID.ToString().Trim() != existingJob.Name.Trim() ||
                            !string.Equals(changeJob.JobDescription.Trim(), existingJob.ShortName.Trim(), StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            changeJobs.Add(changeJob);
                        }

                    }

                }

                var unmatchedJobs = jobs
                        .Where(v => !existingJobs.Any(e => e.ExternalId.Trim() == v.ID.ToString().Trim()))
                        .ToList();

                Utils.LogToFile(3, "[INFO]", $"Total service unmatched/new jobs: {unmatchedJobs.Count}");

                await vicAiService.PostUATJobsForTotalService(unmatchedJobs, environment, source, token, 0, existingJobs); // post job from total service to vic.ai
                await vicAiService.PostUATJobsForTotalService(changeJobs, environment, source, token, 1, existingJobs); // update job from total service to vic.ai
            }
            else
            {
                Console.WriteLine("Job was null. Please check.");
                Utils.LogToFile(3, "[INFO]", "Job was null");
            }

            // for gl accounts
            if (glAccounts.Count > 0)
            {
                // account check 
                var existingGls = _vicAiAPIService.GetVicServiceLogList(environment, source, "GL Account");
                Utils.LogToFile(3, "[INFO]", $"Total service existing gl accounts: {existingGls.Count}"); ;

                // Compare
                var changeGLs = new List<TotalServiceGLModel>();

                foreach (var gl in existingGls)
                {
                    // Parse the JSON string into a JObject
                    JObject jObject = JObject.Parse(gl.VicRequest);

                    // Remove a field
                    jObject.Remove("externalUpdatedAt");

                    // Convert back to string if needed
                    string updatedJson = jObject.ToString();

                    var existingGL = JsonConvert.DeserializeObject<GLRequestBodyModel>(updatedJson);

                    var changegl = glAccounts.Where(x => x.GLAccountNumber == existingGL.Number).FirstOrDefault();

                    if (changegl != null)
                    {

                        if (
                            changegl.GLAccountNumber.ToString() != existingGL.Number ||
                            !string.Equals(changegl.GLAccountDescription, existingGL.Name, StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            changeGLs.Add(changegl);
                        }

                    }

                }


                var unmatchedAccounts = glAccounts
                        .Where(v => !existingGls.Any(e => e.ExternalId.Trim() == v.GLAccountNumber.Trim()))
                        .ToList();

                Utils.LogToFile(3, "[INFO]", $"Total service unmatched/new gl accounts: {unmatchedAccounts.Count}");

                await vicAiService.PostUATGLAccountsForTotalService(unmatchedAccounts, environment, source, token, 0, existingGls); // post gl account from total service to vic.ai
                await vicAiService.PostUATGLAccountsForTotalService(changeGLs, environment, source, token, 1, existingGls); // update gl account from total service to vic.ai
            }
            else
            {
                Console.WriteLine("GL chart of account was null here. Please check.");
                Utils.LogToFile(3, "[INFO]", "GL chart of account was null here.");
            }

            if (purchaseOrders.Count > 0)
            {
                // account check 
                var existingPOs = _vicAiAPIService.GetVicServiceLogList(environment, source, "Purchase Order");
                Utils.LogToFile(3, "[INFO]", $"Total service existing purchase orders: {existingPOs.Count}");


                // Compare
                var changePOs = new List<PurchaseOrderDetailsModel>();
                List<PurchaseOrderDetailsModel> openJobPO = new List<PurchaseOrderDetailsModel>();

                foreach (var po in existingPOs)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(po.VicRequest))
                        {
                            // Parse the JSON string into a JObject
                            JObject jObject = JObject.Parse(po.VicRequest);

                            // Remove a field
                            jObject.Remove("externalUpdatedAt");

                            // Convert back to string if needed
                            string updatedJson = jObject.ToString();

                            var existingPO = JsonConvert.DeserializeObject<PurchaseOrderRequestBodyModel>(updatedJson);

                            var changePO = purchaseOrders.Where(x => x.PO == int.Parse(existingPO.PoNumber)).ToList();

                            if (changePO.Count > 0)
                            {
                                var groupByPO = changePO.GroupBy(x => x.PO).ToList();
                                var vendor = new VendorResponseModel();

                                foreach (var p in groupByPO)
                                {
                                    vendor = await _vicAiAPIService.GetVendorByVendorNumber(p.First().VendorNumber, token);
                                    if (vendor != null && vendor.InternalId != null) break;
                                }

                                var mismatchedFields = new List<string>();
                                var firstPO = changePO.FirstOrDefault();

                                if (vendor?.InternalId != null)
                                {
                                    if (!string.Equals(vendor?.InternalId?.Trim(), existingPO.Vendor?.InternalId?.Trim(), StringComparison.OrdinalIgnoreCase))
                                        mismatchedFields.Add("Vendor InternalId");

                                    if (!string.Equals(firstPO.TotalAmount.ToString(), existingPO.Amount, StringComparison.OrdinalIgnoreCase))
                                        mismatchedFields.Add("Total Amount");

                                    if (changePO.Count != existingPO.LineItems.Count)
                                        mismatchedFields.Add("PO Line Count");

                                    if (mismatchedFields.Any())
                                    {
                                        changePOs.AddRange(changePO);

                                        // Optional: log or return mismatched fields
                                        var fieldsMismatch = string.Join(", ", mismatchedFields);
                                        Console.WriteLine("Mismatched Fields: " + fieldsMismatch);
                                        Utils.LogToFile(3, "[INFO]", $"PO: {changePO.FirstOrDefault().PO}, Mismatched Fields: {fieldsMismatch}");
                                    }
                                }
                                else
                                {
                                    Utils.LogToFile(1, "[EXCEPTION]", $"Vendor was null for this PO: {po.ExternalId}");
                                }

                            }
                        }
                        else
                        {

                            var allItems = purchaseOrders.Where(x => x.PO == int.Parse(po.ExternalId)).ToList();

                            var groupPO = allItems.GroupBy(x => x.PO).FirstOrDefault();

                            // check job is closed or not
                            var result = _vicAiAPIService.IsClosedJob(groupPO);

                            if (!result.isClosed)
                            {
                                openJobPO.AddRange(allItems);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, Stack trace: {ex.StackTrace}");
                    }

                }

                var unmatchedPO = purchaseOrders
                    .Where(v => !existingPOs.Any(e => e.ExternalId == v.PO.ToString()))
                    .ToList();

                Utils.LogToFile(3, "[INFO]", $"Total service unmatched/new purchase orders: {unmatchedPO.Count}");

                await vicAiService.PostUATPurchaseOrderForTotalService(unmatchedPO, environment, source, token); // post purchase order from Total service to vic.ai

                await vicAiService.PostUATPurchaseOrderForTotalService(openJobPO, environment, source, token); // post purchase order from Total Service to vic.ai when all jobs are open


                //await vicAiService.UpdateUATPurchaseOrderForTotalService(changePOs, environment, source, token); // update purchase order from Total Service to vic.ai
            }
            else
            {
                Console.WriteLine("Purchase order was null here. Please check.");
                Utils.LogToFile(3, "[INFO]", "Purchase order was null here.");

            }

            return 1;
        }

    }
}
