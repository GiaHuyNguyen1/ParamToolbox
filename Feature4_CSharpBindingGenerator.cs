using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ParamToolbox
{
    public static class Feature4_CSharpBindingGenerator
    {
        public static void Execute()
        {
            Console.WriteLine("=== FEATURE 4: Generate C# Binding Code ===");

            Console.Write("Nhập đường dẫn file XML (vd: Samples/XMLs/SingleApp_merchant_1.xml): ");
            string xmlPath = PathHelper.CleanPath(Console.ReadLine());

            if (!File.Exists(xmlPath))
            {
                Console.WriteLine("Lỗi: Không tìm thấy file XML.");
                return;
            }

            Console.WriteLine("Nhập danh sách PID (cách nhau bởi khoảng trắng hoặc xuống dòng).");
            Console.WriteLine("Nhập 'DONE' hoặc để trống 2 lần liên tiếp để kết thúc:");
            List<string> pids = new List<string>();
            int emptyCount = 0;
            while (true)
            {
                string line = Console.ReadLine()?.Trim();
                if (line == "DONE") break;
                if (string.IsNullOrEmpty(line))
                {
                    emptyCount++;
                    if (emptyCount >= 2) break;
                    continue;
                }
                emptyCount = 0;
                
                var parts = line.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                pids.AddRange(parts);
            }

            if (pids.Count == 0)
            {
                Console.WriteLine("Không có PID nào được nhập.");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(xmlPath);
                var parameters = doc.Descendants("Parameter").ToList();

                StringBuilder sb = new StringBuilder();

                foreach (var inputPid in pids.Distinct())
                {
                    var paramElement = parameters.FirstOrDefault(p => p.Element("PID")?.Value == inputPid);
                    if (paramElement == null)
                    {
                        sb.AppendLine($"// Lỗi: Không tìm thấy PID '{inputPid}' trong XML\n");
                        continue;
                    }

                    string cleanPid = inputPid.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
                    
                    string variableName = cleanPid;
                    if (cleanPid.Contains("."))
                    {
                        variableName = cleanPid.Substring(cleanPid.LastIndexOf(".") + 1);
                    }

                    string propertyName = char.ToUpper(variableName[0]) + variableName.Substring(1);
                    if (variableName.EndsWith("Enabled", StringComparison.OrdinalIgnoreCase) && !propertyName.StartsWith("Is"))
                    {
                        propertyName = "Is" + propertyName;
                    }

                    string inputType = paramElement.Element("InputType")?.Value ?? "";
                    string dataType = paramElement.Element("DataType")?.Value ?? "";
                    string selectOptions = paramElement.Element("Select")?.Value ?? "";

                    sb.AppendLine($"            var {variableName} = parameterElement.Element(\"{cleanPid}\");");
                    sb.AppendLine($"            if ({variableName} != null)");
                    sb.AppendLine("            {");

                    if (inputType == "select" && selectOptions.Contains("\"Y\""))
                    {
                        sb.AppendLine($"                merchantInfo.{propertyName} = {variableName}.Value?.Equals(\"Y\") ?? false;");
                    }
                    else if (dataType == "Time_hhmm")
                    {
                        sb.AppendLine($"                if (DateTime.TryParseExact({variableName}?.Value, GlobalConstants.FORMAT_TIME_HH_MM, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime {variableName}Value))");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    merchantInfo.{propertyName} = {variableName}Value;");
                        sb.AppendLine("                }");
                    }
                    else if (dataType == "Number")
                    {
                        sb.AppendLine($"                if (uint.TryParse({variableName}?.Value, out uint {variableName}Value))");
                        sb.AppendLine("                {");
                        sb.AppendLine($"                    merchantInfo.{propertyName} = {variableName}Value;");
                        sb.AppendLine("                }");
                    }
                    else
                    {
                        sb.AppendLine($"                merchantInfo.{propertyName} = {variableName}?.Value;");
                    }
                    
                    sb.AppendLine("            }");
                    sb.AppendLine();
                }

                Console.WriteLine("\n--- KẾT QUẢ CODE GENERATED ---\n");
                string generatedCode = sb.ToString();
                Console.WriteLine(generatedCode);

                string outPath = "GeneratedCode.txt";
                File.WriteAllText(outPath, generatedCode);
                Console.WriteLine($"[Đã lưu kết quả vào file: {Path.GetFullPath(outPath)}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi xử lý: {ex.Message}");
            }
        }
    }
}
