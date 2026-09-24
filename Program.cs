using System;

namespace ParamToolbox
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==========================================================");
                Console.WriteLine("                   === PARAM TOOLBOX ===                  ");
                Console.WriteLine("==========================================================");
                Console.WriteLine(I18n.T("1. Generate default JSON from XML <Parameter> tags (Feature 1)",
                                         "1. Khởi tạo file JSON mặc định từ thẻ <Parameter> của XML (Feature 1)"));
                Console.WriteLine(I18n.T("2. Consolidate XML & JSON to CSV/Excel (Feature 2)",
                                         "2. Tổng hợp XML và JSON xuất ra CSV/Excel (Feature 2)"));
                Console.WriteLine(I18n.T("3. Clone Merchant XML Templates to ZIP (Feature 3)",
                                         "3. Nhân bản Merchant Template XML ra file ZIP (Feature 3)"));
                Console.WriteLine(I18n.T("4. Generate C# Binding Code from PIDs (Feature 4)",
                                         "4. Generate C# Binding Code từ danh sách PID (Feature 4)"));
                Console.WriteLine(I18n.T("5. Export XML Template Metadata to Excel (Feature 5)",
                                         "5. Xuất Excel đầy đủ thuộc tính từ template XML (Feature 5)"));
                Console.WriteLine(I18n.T("9. Switch language / Đổi ngôn ngữ (Current: English)",
                                         "9. Switch language / Đổi ngôn ngữ (Hiện tại: Tiếng Việt)"));
                Console.WriteLine(I18n.T("0. Exit",
                                         "0. Thoát"));
                Console.WriteLine("----------------------------------------------------------");
                Console.Write(I18n.T("Select an option (0-5, 9): ",
                                     "Chọn tính năng (0-5, 9): "));

                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice.ToUpperInvariant())
                {
                    case "1":
                        Console.Clear();
                        Feature1_JsonGenerator.Execute();
                        break;
                    case "2":
                        Console.Clear();
                        Feature2_CsvConverter.Execute();
                        break;
                    case "3":
                        Console.Clear();
                        Feature3_MerchantCloner.Execute();
                        break;
                    case "4":
                        Console.Clear();
                        Feature4_CSharpBindingGenerator.Execute();
                        break;
                    case "5":
                        Console.Clear();
                        Feature5_TemplateMetadataCsvExporter.Execute();
                        break;
                    case "9":
                    case "L":
                        I18n.ToggleLanguage();
                        continue;
                    case "0":
                        Console.WriteLine(I18n.T("Exiting application...", "Đang thoát chương trình..."));
                        return;
                    default:
                        Console.WriteLine(I18n.T("Invalid choice. Please try again.", "Lựa chọn không hợp lệ. Vui lòng thử lại."));
                        break;
                }

                Console.WriteLine(I18n.T("\nPress Enter to return to Menu...", "\nNhấn Enter để quay lại Menu..."));
                Console.ReadLine();
            }
        }
    }
}
