using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTeamToVic_ConsoleApp.Logger;

namespace WinTeamToVic_ConsoleApp.Model
{
    public static class MoveFileModel
    {
        public static void MoveLatestBakFile(string sourceDir, string targetDir)
        {
            Utils.LogToFile(3, "[INFO]", "Inside MoveLatestBakFile");
            Console.WriteLine("Inside MoveLatestBakFile");
            try
            {
                var bakFiles = System.IO.Directory.GetFiles(sourceDir, "*.xlsx");
                if (bakFiles != null)
                {
                    if (bakFiles.Length > 0)
                    {
                        var latestBakFile = bakFiles.OrderByDescending(f => System.IO.File.GetLastWriteTime(f)).First();
                        var fileName = Path.GetFileName(latestBakFile);
                        var destinationFile = Path.Combine(targetDir, fileName);

                        Utils.LogToFile(3, "[INFO]", $"Latest downloaded backup file: {fileName}");
                        Console.WriteLine($"Latest downloaded backup file: {fileName}");

                        if (!System.IO.File.Exists(destinationFile))
                        {
                            // Move the latest .bak file to the target directory
                            System.IO.File.Move(latestBakFile, destinationFile);

                            Utils.LogToFile(3, "[INFO]", $"Source folder: '{sourceDir}'");
                            Console.WriteLine($"Source folder: '{sourceDir}'");

                            Utils.LogToFile(3, "[INFO]", $"Target folder: '{targetDir}'");
                            Console.WriteLine($"Target folder: '{targetDir}'");
                        }
                        else
                        {
                            Utils.LogToFile(3, "[INFO]", $"Already have this file in folder. Target folder: '{targetDir}'");
                            Console.WriteLine($"Already have this file in folder. Target folder: '{targetDir}'");

                        }

                    }
                    else
                    {
                        Utils.LogToFile(3, "[INFO]", $"File is empty");
                        Console.WriteLine($"File is empty");
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.LogToFile(1, "[EXCEPTION]", $"Error moving .bak file: {ex.ToString()}");
                Console.WriteLine($"Error moving .bak file: {ex.ToString()}");

                StringBuilder errorMsg = new StringBuilder();

                Utils.LogToFile(1, "[EXCEPTION]", $"Something went wrong when move file from {sourceDir} to {targetDir}. {ex.Message}");
                errorMsg.AppendLine($"Something went wrong when move file from {sourceDir} to {targetDir}. {ex.Message}");

                if (ex.InnerException != null && !String.IsNullOrEmpty(ex.InnerException.Message))
                {
                    errorMsg.AppendLine(ex.InnerException.Message);
                }

                //EmailService.SendEmailForFileRelatedError(errorMsg.ToString());
            }
        }
    }
}
