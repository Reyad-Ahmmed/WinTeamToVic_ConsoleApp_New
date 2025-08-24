using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace WinTeamToVic_ConsoleApp.Logger
{
    public static class Utils
    {
        private static int LogDetailLevel = 6;
        private static ILog _log;
        static Utils()
        {

            string sbaseLevel = ConfigurationManager.AppSettings["logDetailLevel"];
            int baseLevel = (sbaseLevel == null) ? 3 : Convert.ToInt32(sbaseLevel);
            LogDetailLevel = baseLevel;
            log4net.Config.XmlConfigurator.Configure();
            _log = log4net.LogManager.GetLogger("log4netFileLogger");

        }

        public static void LogToFile(int detailLevel, string messageType, string Message)
        {

            try
            {
                if (detailLevel > LogDetailLevel) return;

                _log.Info($"Time: {DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss:fff tt")} | {messageType} | {Message}");

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", ex.Message);
            }
        }

        public static void BlankLineToLogFile(int detailLevel)
        {

            try
            {
                if (detailLevel > LogDetailLevel) return;

                _log.Info("\n");

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", ex.Message);
            }
        }
    }
}
