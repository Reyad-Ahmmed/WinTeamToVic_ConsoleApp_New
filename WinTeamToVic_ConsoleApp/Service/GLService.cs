using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Model;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.DataAccess;

namespace WinTeamToVic_ConsoleApp.Service
{
    public class GLService
    {
        public string _baseDirectory;
        public string _glFilePath;
        public string _glDataFileFullPath;

        public HttpClient _httpClient;

        public string _baseAddress;
        public string _glSubUrl;
        public string _tenantId;
        public string _subscriptionKeyForGL;

        private readonly WinTeamGL_DAL _winteamGLDal;

        public GLService()
        {
            _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _glFilePath = ConfigurationManager.AppSettings["glFilePath"];
            _glDataFileFullPath = _baseDirectory + _glFilePath;

            _httpClient = new HttpClient();

            _baseAddress = ConfigurationManager.AppSettings["winTeamBaseAddress"];
            _glSubUrl = ConfigurationManager.AppSettings["glUrl"];
            _tenantId = ConfigurationManager.AppSettings["tenantId"];
            _subscriptionKeyForGL = ConfigurationManager.AppSettings["subscriptionkeyForVendor"];

            _winteamGLDal = new WinTeamGL_DAL();
        }

        // get gl data from winteam
        public List<GLModel> GetGLData()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in WinTeam GetGLData");

            var filePath = _glDataFileFullPath;
            string[] files = Directory.GetFiles(filePath);

            List<GLModel> glAccounts = new List<GLModel>();

            if (files.Count() > 0)
            {
                var glDataFilePath = files[0];
                
                try
                {
                    
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(glDataFilePath, false))
                    {
                        WorkbookPart workbookPart = doc.WorkbookPart;
                        Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                        //Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().LastOrDefault(); // 2nd sheet
                        WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
                        SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                        IEnumerable<Row> rows = sheetData.Descendants<Row>();

                        // Create a SharedStringTable to handle shared string references
                        SharedStringTablePart stringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        SharedStringTable sharedStringTable = stringTablePart.SharedStringTable;

                        foreach (Row row in rows)
                        {
                            // Assuming the first row contains column names
                            if (row.RowIndex.Value < 5) continue;

                            // Check if the row is empty
                            if (IsRowEmpty(row))
                                continue; // Skip processing empty rows
                            GLModel model = new GLModel();

                            // Get the cell values for each property
                            model.GlNumber = GetCellValue(workbookPart, row.Descendants<Cell>().ElementAt(1), sharedStringTable);
                            model.AccountDescription = GetCellValue(workbookPart, row.Descendants<Cell>().ElementAt(2), sharedStringTable);
                            //model.Category = GetCellValue(workbookPart, row.Descendants<Cell>().ElementAt(3), sharedStringTable);
                            //model.Type = GetCellValue(workbookPart, row.Descendants<Cell>().ElementAt(4), sharedStringTable);

                            if (model.GlNumber != null)
                            {
                                glAccounts.Add(model);
                            }
                        }

                        var data = glAccounts.ToList();

                        Console.WriteLine($"Total active gl accounts: {glAccounts.Count}");

                        Utils.LogToFile(3, "[INFO]", $"Total gl account in excel file: {data.Count}");

                        var targetFolder = ConfigurationManager.AppSettings["archiveFolderPath"];

                        MoveFileModel.MoveLatestBakFile(_glDataFileFullPath, targetFolder);

                        return data;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception Message: {ex.Message}, Exception StackTrace: {ex.StackTrace}");
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                }

                
            }
            else
            {
                Console.WriteLine($"File path was null. File path: {filePath}");
                Utils.LogToFile(3, "INFO", $"File path was null. File path: {filePath}");
            }
            return glAccounts;
        }

        public static string GetCellValue(WorkbookPart workbookPart, Cell cell, SharedStringTable sharedStringTable)
        {
            if (cell == null)
                return string.Empty;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                int index;
                if (int.TryParse(cell.InnerText, out index) && sharedStringTable != null && index < sharedStringTable.Count())
                {
                    return sharedStringTable.ElementAt(index).InnerText;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return cell.InnerText;
            }
        }



        public static bool IsRowEmpty(Row row)
        {
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (!string.IsNullOrEmpty(cell.InnerText))
                {
                    // If any cell in the row is not empty, the row is not considered empty
                    return false;
                }
            }
            return true;
        }


        // get gl account by gl number
        public async Task<List<GLModel>> GetActiveGLAccountFromWinTeam(List<GLModel> glAccounts)
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetActiveGLAccountFromWinTeam");

            var glActiveAccounts = new List<GLModel>();
            var glInActiveAccounts = new List<GLModel>();

            try
            {
                foreach (var account in glAccounts)
                {
                    var uri = _baseAddress + _glSubUrl + $"{account?.GlNumber}";
                    Utils.LogToFile(3, "[INFO]", $"Request uri: {uri}");

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("TenantId", _tenantId);
                    _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _subscriptionKeyForGL);

                    var response = await _httpClient.GetAsync(uri);

                    if (response.IsSuccessStatusCode)
                    {
                        var reponseData = await response.Content.ReadAsStringAsync();
                        Utils.LogToFile(3, "[INFO]", $"Response body: {reponseData}");

                        try
                        {
                            var data = JsonConvert.DeserializeObject<GlAccountFromWinTeam>(reponseData);

                            if (data?.Status == true)
                            {
                                glActiveAccounts.Add(new GLModel
                                {
                                    GlNumber = data.GlAccountNumber,
                                    AccountDescription = data.GLAccountDescription,
                                    Status = data.Status
                                });
                            }
                            else
                            {
                                glInActiveAccounts.Add(new GLModel
                                {
                                    GlNumber = data.GlAccountNumber,
                                    AccountDescription = data.GLAccountDescription,
                                    Status = data.Status
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message, ex.StackTrace);
                            Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
                        }
                    }
                    else
                    {
                        Console.WriteLine(response.ReasonPhrase);

                        var errorsResponse = await response.Content.ReadAsStringAsync();
                        var errors = JArray.Parse(errorsResponse);

                        Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {errors}");

                        Console.WriteLine(errors);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return glActiveAccounts.ToList();
        }


        // GL data entry in vic from winteam
        public async Task GetGLListFromFile(List<GLModel> winteamGlList)
        {
            try
            {
                var activeGLList = await GetActiveGLAccountFromWinTeam(winteamGlList);

                // Mapping GLModel to WinTeamGL
                List<WinTeamGL> winTeamGLList = activeGLList.Select((gl, index) => new WinTeamGL
                {
                    WinTeamGLNumber = int.TryParse(gl.GlNumber, out int glNum) ? glNum : 0,
                    WinTeamGLDescription = gl.AccountDescription,
                    Status = false,
                    CreatedDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt"),
                    ModifiedDate = USATimeModel.GetUSATime().ToString("MM-dd-yyyy hh:mm:ss tt")
                }).ToList();

                foreach (var gl in winTeamGLList)
                {
                    _winteamGLDal.SaveWinTeamGL(gl);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
        }
    }
}
