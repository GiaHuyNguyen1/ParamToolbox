using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ParamToolbox
{
    public static class Feature2_CsvConverter
    {
        private const string SYSTEM_TEMPLATE = "System";
        private const string MERCHANT_TEMPLATE = "Merchant";
        private const string CASHIER_TEMPLATE = "Cashier";
        private const string SURCHARGE_TEMPLATE = "Surcharge";

        public static void Execute()
        {
            Console.WriteLine("=== FEATURE 2: XML & JSON to CSV Converter (Advanced) ===");

            Console.Write("Nhập đường dẫn thư mục chứa file XML (hoặc kéo thả folder vào): ");
            string xmlFolderPath = PathHelper.CleanPath(Console.ReadLine());

            Console.Write("Nhập đường dẫn thư mục chứa file JSON (hoặc kéo thả folder vào): ");
            string jsonFolderPath = PathHelper.CleanPath(Console.ReadLine());

            Console.Write("Nhập đường dẫn và tên file xuất CSV (vd: .../output.csv): ");
            string csvFilePath = PathHelper.CleanPath(Console.ReadLine());

            Console.Write("Nhập tổng số lượng Merchant cần tổng hợp [Mặc định: 6]: ");
            string merchantStr = Console.ReadLine()?.Trim() ?? "";
            int merchantQuantity = string.IsNullOrEmpty(merchantStr) ? 6 : (int.TryParse(merchantStr, out int m) ? m : 6);
            if (merchantQuantity <= 0) merchantQuantity = 6;

            if (!Directory.Exists(xmlFolderPath) || !Directory.Exists(jsonFolderPath))
            {
                Console.WriteLine("Lỗi: Thư mục XML hoặc JSON không tồn tại!");
                return;
            }

            if (Directory.Exists(csvFilePath) || string.IsNullOrEmpty(csvFilePath) || !csvFilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                csvFilePath = Path.Combine(Directory.Exists(csvFilePath) ? csvFilePath : Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.csv");
                Console.WriteLine($"Sẽ lưu file CSV tại: {csvFilePath}");
            }

            try
            {
                var xmlFiles = GetValidXmlFiles(xmlFolderPath);
                var categorizedFiles = CategorizeFiles(xmlFiles);
                List<CsvRow> allRows = new List<CsvRow>();

                if (categorizedFiles.ContainsKey(SYSTEM_TEMPLATE))
                {
                    Console.WriteLine($"\n=== Xử lý System Template ===");
                    ProcessSystemTemplate(categorizedFiles[SYSTEM_TEMPLATE], jsonFolderPath, allRows);
                }

                for (int merchantIndex = 1; merchantIndex <= merchantQuantity; merchantIndex++)
                {
                    Console.WriteLine($"\n=== Xử lý Merchant {merchantIndex}/{merchantQuantity} ===");
                    if (categorizedFiles.ContainsKey(MERCHANT_TEMPLATE))
                    {
                        ProcessMerchantTemplate(categorizedFiles[MERCHANT_TEMPLATE], jsonFolderPath, allRows, merchantIndex);
                    }
                    if (categorizedFiles.ContainsKey(CASHIER_TEMPLATE))
                    {
                        ProcessCashierTemplate(categorizedFiles[CASHIER_TEMPLATE], jsonFolderPath, allRows, merchantIndex);
                    }
                }

                if (categorizedFiles.ContainsKey(SURCHARGE_TEMPLATE))
                {
                    Console.WriteLine($"\n=== Xử lý Surcharge Template ===");
                    ProcessSurchargeTemplate(categorizedFiles[SURCHARGE_TEMPLATE], jsonFolderPath, allRows);
                }

                Console.WriteLine($"\nĐang tạo file CSV: {csvFilePath}");
                CreateCsvFile(csvFilePath, allRows);

                Console.WriteLine($"\nHoàn thành! Đã tạo {allRows.Count} dòng dữ liệu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nLỗi: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
            }
        }

        private static List<string> GetValidXmlFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.xml")
                .Where(file => !Path.GetFileName(file).StartsWith(".") && !Path.GetFileName(file).StartsWith("_"))
                .ToList();
        }

        private static Dictionary<string, List<string>> CategorizeFiles(List<string> xmlFiles)
        {
            var categorized = new Dictionary<string, List<string>>();
            foreach (var file in xmlFiles)
            {
                string fileName = Path.GetFileName(file).ToLower();
                if (fileName.Contains("system")) AddToCategory(categorized, SYSTEM_TEMPLATE, file);
                else if (fileName.Contains("singleapp_merchant")) AddToCategory(categorized, MERCHANT_TEMPLATE, file);
                else if (fileName.Contains("cashier")) AddToCategory(categorized, CASHIER_TEMPLATE, file);
                else if (fileName.Contains("surcharge")) AddToCategory(categorized, SURCHARGE_TEMPLATE, file);
                else Console.WriteLine($"Không xác định được template cho file: {fileName}");
            }
            return categorized;
        }

        private static void AddToCategory(Dictionary<string, List<string>> categorized, string category, string file)
        {
            if (!categorized.ContainsKey(category)) categorized[category] = new List<string>();
            categorized[category].Add(file);
        }

        private static Dictionary<string, string> LoadJsonFile(string jsonFolderPath, string xmlFilePath, string suffix = "")
        {
            string jsonFilePath = Path.Combine(jsonFolderPath, $"{Path.GetFileNameWithoutExtension(xmlFilePath)}.json");
            if (!File.Exists(jsonFilePath)) return new Dictionary<string, string>();

            var jsonValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(jsonFilePath)) ?? new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(suffix))
            {
                var result = new Dictionary<string, string>();
                string suffixPattern = $"_{suffix}";
                foreach (var kvp in jsonValues)
                {
                    if (kvp.Key.EndsWith(suffixPattern)) result[kvp.Key.Substring(0, kvp.Key.Length - suffixPattern.Length)] = kvp.Value;
                }
                foreach (var kvp in jsonValues)
                {
                    if (!kvp.Key.Contains("_") && !result.ContainsKey(kvp.Key)) result[kvp.Key] = kvp.Value;
                }
                return result;
            }
            return jsonValues;
        }

        private static Dictionary<string, string> LoadAuxJsonFile(string jsonFolderPath, string xmlFilePath, string ext)
        {
            string jsonFilePath = Path.Combine(jsonFolderPath, $"{Path.GetFileNameWithoutExtension(xmlFilePath)}.{ext}.json");
            if (!File.Exists(jsonFilePath)) return new Dictionary<string, string>();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(jsonFilePath)) ?? new Dictionary<string, string>();
        }

        private static void ProcessSystemTemplate(List<string> xmlFiles, string jsonFolderPath, List<CsvRow> allRows)
        {
            foreach (var file in xmlFiles) ProcessXmlFile(file, LoadJsonFile(jsonFolderPath, file), LoadAuxJsonFile(jsonFolderPath, file, "type"), LoadAuxJsonFile(jsonFolderPath, file, "remark"), allRows, 0);
        }

        private static void ProcessMerchantTemplate(List<string> xmlFiles, string jsonFolderPath, List<CsvRow> allRows, int index)
        {
            foreach (var file in xmlFiles) ProcessXmlFileWithSuffix(file, LoadJsonFile(jsonFolderPath, file, index.ToString()), LoadAuxJsonFile(jsonFolderPath, file, "type"), LoadAuxJsonFile(jsonFolderPath, file, "remark"), allRows, index);
        }

        private static void ProcessCashierTemplate(List<string> xmlFiles, string jsonFolderPath, List<CsvRow> allRows, int index)
        {
            foreach (var file in xmlFiles) ProcessXmlFileWithSuffix(file, LoadJsonFile(jsonFolderPath, file, index.ToString()), LoadAuxJsonFile(jsonFolderPath, file, "type"), LoadAuxJsonFile(jsonFolderPath, file, "remark"), allRows, index, true);
        }

        private static void ProcessSurchargeTemplate(List<string> xmlFiles, string jsonFolderPath, List<CsvRow> allRows)
        {
            foreach (var file in xmlFiles) ProcessXmlFileWithSuffix(file, LoadJsonFile(jsonFolderPath, file), LoadAuxJsonFile(jsonFolderPath, file, "type"), LoadAuxJsonFile(jsonFolderPath, file, "remark"), allRows, 0);
        }

        private static void ProcessXmlFile(string xmlPath, Dictionary<string, string> jValues, Dictionary<string, string> tValues, Dictionary<string, string> rValues, List<CsvRow> rows, int index = 0)
        {
            var prms = XDocument.Load(xmlPath).Descendants("Parameter");
            foreach (var p in prms) { var r = ExtractRow(p, jValues, tValues, rValues, index); if (r != null) rows.Add(r); }
        }

        private static void ProcessXmlFileWithSuffix(string xmlPath, Dictionary<string, string> jValues, Dictionary<string, string> tValues, Dictionary<string, string> rValues, List<CsvRow> rows, int index, bool isCashier = false)
        {
            var prms = XDocument.Load(xmlPath).Descendants("Parameter");
            foreach (var p in prms) { var r = ExtractRow(p, jValues, tValues, rValues, index, true, isCashier); if (r != null) rows.Add(r); }
        }

        private static CsvRow ExtractRow(XElement param, Dictionary<string, string> jValues, Dictionary<string, string> tValues, Dictionary<string, string> rValues, int index = 0, bool addSuffix = false, bool isCashier = false)
        {
            var pidElem = param.Element("PID");
            if (pidElem == null) return null;

            string pid = pidElem.Value.Trim();
            string rowPid = pid;

            if (addSuffix && index > 0)
            {
                if (isCashier)
                {
                    string propName = pid;
                    string[] parts = pid.Split('.');
                    if (parts.Length >= 2) propName = parts[1];
                    rowPid = $"cashier.{propName}.merchant_{index}";
                }
                else
                {
                    var match = System.Text.RegularExpressions.Regex.Match(pid, @"\d+$");
                    rowPid = match.Success ? pid.Substring(0, match.Index) + index.ToString() : $"{pid}{index}";
                }
            }

            var row = new CsvRow();
            row.VariableKey = rowPid;
            row.VariableValue = jValues.ContainsKey(pid) ? jValues[pid] : "NOT_FOUND_IN_JSON";
            row.Type = tValues.ContainsKey(pid) ? tValues[pid] : "T";
            
            string req = param.Element("Required")?.Value?.Trim().ToLower() == "true" ? "Mandatory" : "Optional";
            string remark = $"{req}.";
            var sel = param.Element("Select");
            if (sel != null && !string.IsNullOrWhiteSpace(sel.Value))
            {
                try 
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(sel.Value);
                    if (dict != null && dict.Count > 0)
                    {
                        var allowed = string.Join(", ", dict.Select(kv => $"{kv.Key} = {kv.Value}"));
                        remark += $" Allowed values: {allowed}.";
                    }
                }
                catch { }
            }
            row.Remark = remark;
            return row;
        }

        private static void CreateCsvFile(string csvPath, List<CsvRow> rows)
        {
            string dir = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (StreamWriter sw = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("PID,value,T/P,Remark");
                foreach (var r in rows) sw.WriteLine($"{EscapeCsv(r.VariableKey ?? "")},{EscapeCsv(r.VariableValue ?? "")},{r.Type},{EscapeCsv(r.Remark ?? "")}");
            }
        }

        private static string EscapeCsv(string f) => (f.Contains(",") || f.Contains("\"") || f.Contains("\n") || f.Contains("\r")) ? $"\"{f.Replace("\"", "\"\"")}\"" : f;

        private class CsvRow
        {
            public string VariableKey { get; set; }
            public string VariableValue { get; set; }
            public string Type { get; set; }
            public string Remark { get; set; }
        }
    }
}
