using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;

namespace WinTeamToVic_ConsoleApp.Service
{
    public static class TokenService
    {
        public static string client_id_winteam = ConfigurationManager.AppSettings["client_id_winteam"];
        public static string client_secret_winteam = ConfigurationManager.AppSettings["client_secret_winteam"];

        public static string client_id_total_service = ConfigurationManager.AppSettings["client_id_total_service"];
        public static string client_secret_total_service = ConfigurationManager.AppSettings["client_secret_total_service"];


        public static string client_id_winteam_UAT = ConfigurationManager.AppSettings["client_id_test_winteam_UAT"];
        public static string client_secret_winteam_UAT = ConfigurationManager.AppSettings["client_secret_test_winteam_UAT"];

        public static string client_id_winteam_UAT_Production = ConfigurationManager.AppSettings["client_id_test_winteam_UAT_Production"];
        public static string client_secret_winteam_UAT_Production = ConfigurationManager.AppSettings["client_secret_test_winteam_UAT_Production"];

        public static string client_id_total_service_UAT = ConfigurationManager.AppSettings["client_id_total_service_UAT"];
        public static string client_secret_total_service_UAT = ConfigurationManager.AppSettings["client_secret_total_service_UAT"];

        public static string client_id_total_service_production = ConfigurationManager.AppSettings["client_id_total_service_production"];
        public static string client_secret_total_service_production = ConfigurationManager.AppSettings["client_secret_total_service_production"];


        public static string vicTestTokenUrl = ConfigurationManager.AppSettings["vicTestTokenUrl"];
        public static string vicTestUATTokenUrl = ConfigurationManager.AppSettings["vicTestUATTokenUrl"];

        public static HttpClient _httpClient;
        static TokenService()
        {
            _httpClient = new HttpClient();
        }

        #region For WinTeam
        public static async Task<string> GetTestVicAccessTokenForWinTeam()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetTestVicAccessTokenForWinTeam");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_winteam,
                    client_secret = client_secret_winteam
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            
            return token;
        }

        public static async Task<string> GetUATTestVicAccessTokenForWinTeam()
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetUATTestVicAccessTokenForWinTeam");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_winteam_UAT,
                    client_secret = client_secret_winteam_UAT
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestUATTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return token;
        }


        public static async Task<string> GetUATTestVicAccessTokenForWinTeamProduction()
        {
            Utils.LogToFile(3, "[INFO]", $"Inside in GetUATTestVicAccessTokenForWinTeamProduction");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_winteam_UAT_Production,
                    client_secret = client_secret_winteam_UAT_Production
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestUATTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return token;
        }

        #endregion

        #region For Total Service
        public static async Task<string> GetTestVicAccessTokenForTotalService()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetTestVicAccessTokenForTotalService");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_total_service,
                    client_secret = client_secret_total_service
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return token;
        }

        #endregion

        #region For Total Service UAT
        public static async Task<string> GetUATVicAccessTokenForTotalService()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetUATVicAccessTokenForTotalService");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_total_service_UAT,
                    client_secret = client_secret_total_service_UAT
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestUATTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return token;
        }

        // total service production token get
        public static async Task<string> GetProductionVicAccessTokenForTotalService()
        {
            Utils.LogToFile(3, "[INFO]", "Inside in GetProductionVicAccessTokenForTotalService");

            var token = "";
            try
            {
                var requestData = new
                {
                    client_id = client_id_total_service_production,
                    client_secret = client_secret_total_service_production
                };

                // Serialize the object to JSON
                var json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
                Utils.LogToFile(3, "[INFO]", $"Request body: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(vicTestUATTokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Utils.LogToFile(3, "[INFO]", $"Response body: {responseBody}");

                    var data = JsonConvert.DeserializeObject<TokenModel>(responseBody);
                    token = data.access_token;
                    Console.WriteLine("Response: " + responseBody);
                    Console.WriteLine("token: " + token);
                }
                else
                {
                    Console.WriteLine("Error: " + response.ReasonPhrase);
                    Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Utils.LogToFile(1, "[EXCEPTION]", $"Error Msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }

            return token;
        }

        #endregion
    }
}
