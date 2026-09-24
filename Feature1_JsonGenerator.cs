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
            Console.WriteLine("=== FEATURE 1: XML to JSON Generator ===");
            Console.WriteLine("Tool chuyển XML Cashier sang JSON chứa DefaultValue và Type.");
            Console.Write("Nhập đường dẫn folder chứa các file XML: ");
            string folderPath = PathHelper.CleanPath(Console.ReadLine());

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Lỗi: Folder không tồn tại!");
                return;
            }

            var allFiles = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);

            var xmlFiles = allFiles
                    .Where(file => !Path.GetFileName(file).StartsWith(".") &&
                                   !Path.GetFileName(file).StartsWith("_"))
                    .ToArray();

            if (xmlFiles.Length == 0)
            {
                Console.WriteLine("Không tìm thấy file XML nào hợp lệ trong folder.");
                return;
            }

            Console.WriteLine($"Tìm thấy {xmlFiles.Length} file XML. Đang xử lý...\n");

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    ProcessSingleFile(xmlFile);
                    Console.WriteLine($"✓ Đã xử lý thành công: {Path.GetFileName(xmlFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Lỗi khi xử lý {Path.GetFileName(xmlFile)}: {ex.Message}");
                }
            }
            Console.WriteLine("Hoàn tất tiến trình Feature 1!");
        }

        private static void ProcessSingleFile(string xmlFilePath)
        {
            XDocument doc = XDocument.Load(xmlFilePath);

            // Lấy tất cả các Parameter có PID
            var parameters = doc.Descendants("Parameter")
                .Where(p => p.Element("PID") != null)
                .Select(p => new
                {
                    Pid = p.Element("PID")!.Value.Trim(),
                    DefaultValue = p.Element("Defaultvalue")?.Value.Trim() ?? "",
                    InputType = p.Element("InputType")?.Value.Trim() ?? "text" // mặc định text nếu không có
                })
                .Where(x => !string.IsNullOrEmpty(x.Pid))
                .ToList();

            // 1. JSON cho Default Value
            var defaultDict = parameters.ToDictionary(
                x => x.Pid,
                x => (object)x.DefaultValue
            );

            // 2. JSON cho Type (T hoặc P)
            var typeDict = parameters.ToDictionary(
                x => x.Pid,
                x => x.InputType.Equals("password", StringComparison.OrdinalIgnoreCase) ? "P" : "T"
            );

            // Đường dẫn output
            string baseName = Path.GetFileNameWithoutExtension(xmlFilePath);
            string directory = Path.GetDirectoryName(xmlFilePath)!;

            string defaultJsonPath = Path.Combine(directory, $"{baseName}.json");
            string typeJsonPath = Path.Combine(directory, $"{baseName}.type.json");
            string pidsTxtPath = Path.Combine(directory, $"{baseName}.pids.txt");

            // Options để JSON đẹp
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Ghi 3 file kết quả
            File.WriteAllText(defaultJsonPath, JsonSerializer.Serialize(defaultDict, options));
            File.WriteAllText(typeJsonPath, JsonSerializer.Serialize(typeDict, options));
            File.WriteAllLines(pidsTxtPath, parameters.Select(x => x.Pid));
        }
    }
}
