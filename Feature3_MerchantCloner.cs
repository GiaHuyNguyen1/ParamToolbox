using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace ParamToolbox
{
    public static class Feature3_MerchantCloner
    {
        public static void Execute()
        {
            Console.WriteLine("=== FEATURE 3: Merchant XML Template Cloner & Zipper ===");

            Console.Write("Nhập đường dẫn file template cần clone (vd: SingleApp_merchant_1.xml): ");
            string inputPath = PathHelper.CleanPath(Console.ReadLine());

            Console.Write("Nhập đường dẫn thư mục xuất kết quả (vd: C:\\Outputs) (Để trống tự tạo trong folder chứa template): ");
            string outputDir = PathHelper.CleanPath(Console.ReadLine());

            Console.Write("Nhập TỔNG SỐ LƯỢNG file cần clone [Mặc định: 30]: ");
            string totalStr = Console.ReadLine()?.Trim() ?? "";
            int totalFiles = string.IsNullOrEmpty(totalStr) ? 30 : (int.TryParse(totalStr, out int t) ? t : 0);
            if (totalFiles <= 0)
            {
                Console.WriteLine("Số lượng không hợp lệ!");
                return;
            }

            if (!File.Exists(inputPath))
            {
                Console.WriteLine("Lỗi: Không tìm thấy file template gốc.");
                return;
            }

            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Path.Combine(Path.GetDirectoryName(inputPath) ?? Environment.CurrentDirectory, "Merchant_Zips");
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Dọn dẹp folder output trước khi sinh file
            var tmpDir = Path.Combine(outputDir, "tmp_xmls");
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            Directory.CreateDirectory(tmpDir);

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(inputPath);

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                // Sinh ra tất cả file XML vào thư mục tmp
                List<string> generatedFiles = new List<string>();

                Console.WriteLine("Đang tiến hành nhân bản...");
                for (int i = 1; i <= totalFiles; i++)
                {
                    XmlDocument cloneDoc = (XmlDocument)doc.Clone();

                    // Sửa các Group Title / ID
                    var groups = cloneDoc.SelectNodes("//Group");
                    if (groups != null)
                    {
                        foreach (XmlNode group in groups)
                        {
                            var idNode = group.SelectSingleNode("ID");
                            if (idNode != null) idNode.InnerText = $"sys_G{i}";

                            var titleNode = group.SelectSingleNode("Title");
                            if (titleNode != null && titleNode.InnerText.Contains("Merchant", StringComparison.OrdinalIgnoreCase))
                            {
                                titleNode.InnerText = $"Merchant {i}";
                            }
                        }
                    }

                    // Sửa Files FileName / ID
                    var files = cloneDoc.SelectNodes("//File");
                    if (files != null)
                    {
                        foreach (XmlNode file in files)
                        {
                            var idNode = file.SelectSingleNode("ID");
                            if (idNode != null) idNode.InnerText = $"sys_F{i}";

                            var nameNode = file.SelectSingleNode("FileName");
                            if (nameNode != null) nameNode.InnerText = $"merchant_{i}.p";
                        }
                    }

                    // Sửa từng Parameter PID, GroupID, FileID
                    var parameters = cloneDoc.SelectNodes("//Parameter");
                    if (parameters != null)
                    {
                        foreach (XmlNode param in parameters)
                        {
                            var groupNode = param.SelectSingleNode("GroupID");
                            if (groupNode != null) groupNode.InnerText = $"sys_G{i}";

                            var fileNode = param.SelectSingleNode("FileID");
                            if (fileNode != null) fileNode.InnerText = $"sys_F{i}";

                            var pidNode = param.SelectSingleNode("PID");
                            if (pidNode != null)
                            {
                                string originalPid = pidNode.InnerText;
                                // Strip any trailing digits so re-cloning doesn't stack suffixes
                                // e.g. "merchantProcessor.mid1" -> "merchantProcessor.mid", then append i
                                string basePid = System.Text.RegularExpressions.Regex.Replace(originalPid, @"\d+$", "");
                                pidNode.InnerText = $"{basePid}{i}";

                                // Also update Defaultvalue if it references this PID via #{...} syntax
                                // Match #{basePidN} or #{basePid} (with or without trailing digits)
                                var defaultNode = param.SelectSingleNode("Defaultvalue");
                                if (defaultNode != null && !string.IsNullOrEmpty(defaultNode.InnerText))
                                {
                                    string newDefault = System.Text.RegularExpressions.Regex.Replace(
                                        defaultNode.InnerText,
                                        @"#\{" + System.Text.RegularExpressions.Regex.Escape(basePid) + @"\d*\}",
                                        $"#{{{basePid}{i}}}");
                                    defaultNode.InnerText = newDefault;
                                }
                            }
                        }
                    }

                    string outFilePath = Path.Combine(tmpDir, $"SingleApp_merchant_{i}.xml");
                    
                    // Ép thẻ rỗng viết đầy đủ thay vì self-closing
                    var allNodes = cloneDoc.SelectNodes("//*");
                    if (allNodes != null)
                    {
                        foreach (XmlNode node in allNodes)
                        {
                            if (node is XmlElement el && string.IsNullOrEmpty(el.InnerText))
                            {
                                el.IsEmpty = false;
                            }
                        }
                    }

                    using (XmlWriter writer = XmlWriter.Create(outFilePath, settings))
                    {
                        cloneDoc.Save(writer);
                    }
                    generatedFiles.Add(outFilePath);
                }

                Console.WriteLine("Đang xử lý đóng gói và copy file theo luồng cấu hình mới...");

                var zipGroups = new List<(int Start, int End)>
                {
                    (1, 1),
                    (2, 8),
                    (9, 16),
                    (17, 30)
                };

                // Hỗ trợ trường hợp nếu file clone > 30 thì phần dư sẽ tự zip chung
                if (totalFiles > 30)
                {
                    zipGroups.Add((31, totalFiles));
                }

                foreach (var group in zipGroups)
                {
                    int startIdx = group.Start;
                    int endIdx = group.End;

                    if (startIdx > totalFiles) break;

                    int actualEndIdx = Math.Min(endIdx, totalFiles);

                    if (startIdx == 1 && endIdx == 1)
                    {
                        // Merchant 1 không zip
                        for (int i = startIdx; i <= actualEndIdx; i++)
                        {
                            string f = generatedFiles[i - 1]; // 0-indexed
                            string destPath = Path.Combine(outputDir, Path.GetFileName(f));
                            if (File.Exists(destPath)) File.Delete(destPath);
                            File.Copy(f, destPath);
                            Console.WriteLine($"✓ Đã xuất file không nén: {Path.GetFileName(f)}");
                        }
                    }
                    else
                    {
                        string zipFilePath = Path.Combine(outputDir, $"Merchant_{startIdx}_{actualEndIdx}.zip");
                        if (File.Exists(zipFilePath)) File.Delete(zipFilePath);

                        bool hasFiles = false;
                        using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                        {
                            for (int i = startIdx; i <= actualEndIdx; i++)
                            {
                                int idx = i - 1;
                                if (idx < generatedFiles.Count)
                                {
                                    string f = generatedFiles[idx];
                                    archive.CreateEntryFromFile(f, Path.GetFileName(f));
                                    hasFiles = true;
                                }
                            }
                        }

                        if (hasFiles)
                        {
                            Console.WriteLine($"✓ Cụm {startIdx}-{actualEndIdx}: Đã đóng gói vào {Path.GetFileName(zipFilePath)}");
                        }
                        else
                        {
                            if (File.Exists(zipFilePath)) File.Delete(zipFilePath);
                        }
                    }
                }

                // Dọn dẹp folder tmp
                Directory.Delete(tmpDir, true);

                Console.WriteLine($"\nHoàn tất! Kết quả đã lưu tại: {outputDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Đã xảy ra lỗi: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.StackTrace}");
            }
        }
    }
}
