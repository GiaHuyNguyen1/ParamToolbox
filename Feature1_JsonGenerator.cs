using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace ParamToolbox
{
    public static class Feature1_JsonGenerator
    {
        public static void Execute()
        {
            Console.WriteLine(I18n.T("=== FEATURE 1: XML to JSON Generator ===",
                                     "=== FEATURE 1: XML to JSON Generator ==="));
            Console.WriteLine(I18n.T("Converts XML Cashier parameters to JSON containing DefaultValue and Type.",
                                     "Tool chuyển XML Cashier sang JSON chứa DefaultValue và Type."));
            Console.Write(I18n.T("Enter folder path containing XML files: ",
                                 "Nhập đường dẫn folder chứa các file XML: "));
            string folderPath = PathHelper.CleanPath(Console.ReadLine());

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine(I18n.T("Error: Folder does not exist!",
                                         "Lỗi: Folder không tồn tại!"));
                return;
            }

            var allFiles = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);

            var xmlFiles = allFiles
                    .Where(file => !Path.GetFileName(file).StartsWith(".") &&
                                   !Path.GetFileName(file).StartsWith("_"))
                    .ToArray();

            if (xmlFiles.Length == 0)
            {
                Console.WriteLine(I18n.T("No valid XML files found in the folder.",
                                         "Không tìm thấy file XML nào hợp lệ trong folder."));
                return;
            }

            Console.WriteLine(I18n.T($"Found {xmlFiles.Length} XML file(s). Processing...\n",
                                     $"Tìm thấy {xmlFiles.Length} file XML. Đang xử lý...\n"));

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    ProcessSingleFile(xmlFile);
                    Console.WriteLine(I18n.T($"✓ Successfully processed: {Path.GetFileName(xmlFile)}",
                                             $"✓ Đã xử lý thành công: {Path.GetFileName(xmlFile)}"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(I18n.T($"✗ Error processing {Path.GetFileName(xmlFile)}: {ex.Message}",
                                             $"✗ Lỗi khi xử lý {Path.GetFileName(xmlFile)}: {ex.Message}"));
                }
            }
            Console.WriteLine(I18n.T("Feature 1 process completed!",
                                     "Hoàn tất tiến trình Feature 1!"));
        }

        private static void ProcessSingleFile(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            // Get all Parameters with PID
            var parameters = doc.Descendants("Parameter")
                .Where(p => p.Element("PID") != null)
                .Select(p => new
                {
                    Pid = p.Element("PID")!.Value.Trim(),
                    DefaultValue = p.Element("Defaultvalue")?.Value.Trim() ?? "",
                    InputType = p.Element("InputType")?.Value.Trim() ?? "text"
                })
                .Where(x => !string.IsNullOrEmpty(x.Pid))
                .ToList();

            // 1. JSON for Default Value
            var defaultDict = parameters.ToDictionary(
                x => x.Pid,
                x => (object)x.DefaultValue
            );

            // 2. JSON for Type (T or P)
            var typeDict = parameters.ToDictionary(
                x => x.Pid,
                x => x.InputType.Equals("password", StringComparison.OrdinalIgnoreCase) ? "P" : "T"
            );

            // Output paths
            string baseName = Path.GetFileNameWithoutExtension(xmlFilePath);
            string directory = Path.GetDirectoryName(xmlFilePath)!;

            string defaultJsonPath = Path.Combine(directory, $"{baseName}.json");
            string typeJsonPath = Path.Combine(directory, $"{baseName}.type.json");
            string pidsTxtPath = Path.Combine(directory, $"{baseName}.pids.txt");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            File.WriteAllText(defaultJsonPath, JsonSerializer.Serialize(defaultDict, options));
            File.WriteAllText(typeJsonPath, JsonSerializer.Serialize(typeDict, options));
            File.WriteAllLines(pidsTxtPath, parameters.Select(x => x.Pid));
        }
    }
}
