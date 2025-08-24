using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;
using WinTeamToVic_ConsoleApp.Model;
using WinTeamToVic_ConsoleApp.Service;

namespace WinTeamToVic_ConsoleApp
{
    public class Program
    {
        public static string _testEnv = ConfigurationManager.AppSettings["testEnv"];
        public static string _uatEnv = ConfigurationManager.AppSettings["uatEnv"];
        public static string _prodEnv = ConfigurationManager.AppSettings["prodEnv"];

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine($"Application start time: {DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt")}");
                Utils.LogToFile(3, "[INFO]", $"Application start time: {DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt")}");

                //var tokenForWinTeam = await TokenService.GetTestVicAccessTokenForWinTeam();
                
                var tokenForWinTeamUAT = await TokenService.GetUATTestVicAccessTokenForWinTeam();
                var tokenForWinTeamUATProduction = await TokenService.GetUATTestVicAccessTokenForWinTeamProduction();

                var tokenForTotalService = await TokenService.GetTestVicAccessTokenForTotalService(); // for test
                var tokenForTotalServiceUAT = await TokenService.GetUATVicAccessTokenForTotalService(); // for uat

                var winteamSource = ConfigurationManager.AppSettings["winTeamSource"];
                var totalServiceSource = ConfigurationManager.AppSettings["totalServiceSource"];

                VicDataManagerService vicDataManagerService = new VicDataManagerService();

                // winteam test
                //await vicDataManagerService.WinTeamDataManagerForTest(_testEnv, winteamSource, tokenForWinTeam);

                // winteam uat
                //await vicDataManagerService.WinTeamDataManagerForUAT(_uatEnv, winteamSource, tokenForWinTeamUAT);

                await vicDataManagerService.WinTeamDataManagerForUAT(_prodEnv, winteamSource, tokenForWinTeamUATProduction);

                // total service test
                //await vicDataManagerService.TotalServiceDataManagerForTest(_testEnv, totalServiceSource, tokenForTotalService);

                // total service uat
                await vicDataManagerService.TotalServiceDataManagerForUAT(_uatEnv, totalServiceSource, tokenForTotalServiceUAT);

                Console.WriteLine($"Application end time: {DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt")}");
                Utils.LogToFile(3, "[INFO]", $"Application end time: {DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss tt")}");
            }
            catch(Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error msg: {ex.Message}, stack trace: {ex.StackTrace}");
            }
            //Console.ReadLine();
        }
    }
}
