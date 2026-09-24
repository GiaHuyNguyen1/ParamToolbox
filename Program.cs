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
                Console.WriteLine("=== PARAM TOOLBOX ===");
                Console.WriteLine("1. Khởi tạo file JSON mặc định từ thẻ <Parameter> của XML (Feature 1)");
                Console.WriteLine("2. Tổng hợp XML và JSON xuất ra CSV/Excel (Feature 2)");
                Console.WriteLine("3. Nhân bản Merchant Template XML ra file ZIP (Feature 3)");
                Console.WriteLine("4. Generate C# Binding Code từ danh sách PID (Feature 4)");
                Console.WriteLine("5. Xuất Excel đầy đủ thuộc tính từ template XML (Feature 5)");
                Console.WriteLine("0. Thoát");
                Console.Write("\nChọn tính năng (0-5): ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                switch (choice)
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
                    case "0":
                        Console.WriteLine("Đang thoát chương trình...");
                        return;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ. Vui lòng thử lại.");
                        break;
                }

                Console.WriteLine("\nNhấn Enter để quay lại Menu...");
                Console.ReadLine();
            }
        }
    }
}
