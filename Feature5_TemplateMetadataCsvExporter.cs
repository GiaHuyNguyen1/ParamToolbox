using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ClosedXML.Excel;

namespace ParamToolbox
{
    public static class Feature5_TemplateMetadataCsvExporter
    {
        private static readonly string[] ContextColumns =
        {
            "Template File"
        };

        private static readonly string[] GroupColumns =
        {
            "GroupID",
            "GroupTitle",
            "GroupOrder",
            "GroupDescription"
        };

        private static readonly string[] FileColumns =
        {
            "FileID",
            "FileName",
            "FileDescription"
        };

        private static readonly string[] HeaderColumns =
        {
            "HeaderTitle",
            "HeaderDisplayStyle",
            "HeaderDefaultStyle",
            "HeaderDisplay"
        };

        private static readonly string[] PreferredParameterColumns =
        {
            "PID",
            "Title",
            "Defaultvalue",
            "DataType",
            "Readonly",
            "Required",
            "Display",
            "InputType",
            "Select",
            "Type"
        };

        private static readonly string[] PostParameterColumns =
        {
            "Description",
            "ValueType"
        };

        private static readonly HashSet<string> ReservedColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "Template File",
            "GroupID",
            "GroupTitle",
            "GroupOrder",
            "GroupDescription",
            "FileID",
            "FileName",
            "FileDescription",
            "HeaderTitle",
            "HeaderDisplayStyle",
            "HeaderDefaultStyle",
            "HeaderDisplay",
            "ValueType"
        };

        public static void Execute()
        {
            Console.WriteLine(I18n.T("=== FEATURE 5: Template XML Metadata to Excel ===",
                                     "=== FEATURE 5: Template XML Metadata to Excel ==="));

            Console.Write(I18n.T("Enter XML template directory path: ",
                                 "Nhập đường dẫn thư mục chứa file template XML: "));
            string xmlFolderPath = PathHelper.CleanPath(Console.ReadLine());

            Console.Write(I18n.T("Enter output Excel file path (e.g. .../template-metadata.xlsx): ",
                                 "Nhập đường dẫn và tên file Excel xuất ra (vd: .../template-metadata.xlsx): "));
            string excelPath = PathHelper.CleanPath(Console.ReadLine());

            if (!Directory.Exists(xmlFolderPath))
            {
                Console.WriteLine(I18n.T("Error: XML directory does not exist!",
                                         "Lỗi: Thư mục XML không tồn tại!"));
                return;
            }

            if (Directory.Exists(excelPath) || string.IsNullOrWhiteSpace(excelPath) ||
                !excelPath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                excelPath = Path.Combine(
                    Directory.Exists(excelPath) ? excelPath : Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "template-metadata.xlsx");
                Console.WriteLine(I18n.T($"Will save Excel file at: {excelPath}",
                                         $"Sẽ lưu file Excel tại: {excelPath}"));
            }

            var xmlFiles = Directory.GetFiles(xmlFolderPath, "*.xml", SearchOption.TopDirectoryOnly)
                .Where(file => !Path.GetFileName(file).StartsWith(".") &&
                               !Path.GetFileName(file).StartsWith("._"))
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (xmlFiles.Count == 0)
            {
                Console.WriteLine(I18n.T("No valid XML files found in the directory.",
                                         "Không tìm thấy file XML hợp lệ trong thư mục."));
                return;
            }

            try
            {
                var rows = new List<MetadataExcelRow>();
                var parameterColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var xmlFile in xmlFiles)
                {
                    rows.AddRange(ExtractRows(xmlFile, parameterColumns));
                }

                CreateExcelFile(excelPath, rows, parameterColumns);
                Console.WriteLine(I18n.T($"\nCompleted! Generated Excel file with {rows.Count} rows of data.",
                                         $"\nHoàn thành! Đã tạo file Excel với {rows.Count} dòng dữ liệu."));
            }
            catch (Exception ex)
            {
                Console.WriteLine(I18n.T($"Error exporting Excel metadata: {ex.Message}",
                                         $"Lỗi khi xuất Excel metadata: {ex.Message}"));
            }
        }

        private static List<MetadataExcelRow> ExtractRows(string xmlFilePath, ISet<string> parameterColumns)
        {
            var document = XDocument.Load(xmlFilePath);

            var groupLookup = document
                .Descendants("Groups")
                .Descendants("Group")
                .Select(group => new GroupInfo
                {
                    Id = GetElementValue(group, "ID"),
                    Title = GetElementValue(group, "Title"),
                    Order = GetElementValue(group, "Order"),
                    Description = GetElementValue(group, "Description")
                })
                .Where(group => !string.IsNullOrWhiteSpace(group.Id))
                .GroupBy(group => group.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var fileLookup = document
                .Descendants("Files")
                .Descendants("File")
                .Select(file => new FileInfo
                {
                    Id = GetElementValue(file, "ID"),
                    FileName = GetElementValue(file, "FileName"),
                    Description = GetElementValue(file, "Description")
                })
                .Where(file => !string.IsNullOrWhiteSpace(file.Id))
                .GroupBy(file => file.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(file => file.Key, file => file.First(), StringComparer.OrdinalIgnoreCase);

            string templateFileName = Path.GetFileName(xmlFilePath);

            return document.Descendants("Parameter")
                .Where(parameter => parameter.Element("PID") != null)
                .Select(parameter => CreateRow(parameter, templateFileName, groupLookup, fileLookup, parameterColumns))
                .ToList();
        }

        private static MetadataExcelRow CreateRow(
            XElement parameter,
            string templateFileName,
            IReadOnlyDictionary<string, GroupInfo> groupLookup,
            IReadOnlyDictionary<string, FileInfo> fileLookup,
            ISet<string> parameterColumns)
        {
            var parameterValues = parameter.Elements()
                .GroupBy(element => element.Name.LocalName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => string.Join(" | ", group.Select(element => element.Value.Trim())),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var columnName in parameterValues.Keys)
            {
                if (!ReservedColumns.Contains(columnName))
                {
                    parameterColumns.Add(columnName);
                }
            }

            string groupId = GetParameterValue(parameterValues, "GroupID");
            string fileId = GetParameterValue(parameterValues, "FileID");
            string inputType = GetParameterValue(parameterValues, "InputType");
            XElement? header = parameter.Parent;

            groupLookup.TryGetValue(groupId, out var groupInfo);
            fileLookup.TryGetValue(fileId, out var fileInfo);

            return new MetadataExcelRow
            {
                TemplateFile = templateFileName,
                Group = groupInfo ?? new GroupInfo { Id = groupId },
                File = fileInfo ?? new FileInfo { Id = fileId },
                Header = new HeaderInfo
                {
                    Title = GetElementValue(header, "Title"),
                    DisplayStyle = GetElementValue(header, "DisplayStyle"),
                    DefaultStyle = GetElementValue(header, "DefaultStyle"),
                    Display = GetElementValue(header, "Display")
                },
                ValueType = inputType.Equals("password", StringComparison.OrdinalIgnoreCase) ? "P" : "T",
                ParameterValues = parameterValues
            };
        }

        private static string GetParameterValue(IReadOnlyDictionary<string, string> values, string key)
        {
            return values.TryGetValue(key, out var value) ? value : string.Empty;
        }

        private static string GetElementValue(XElement? element, string name)
        {
            return element?.Element(name)?.Value.Trim() ?? string.Empty;
        }

        private static void CreateExcelFile(string excelPath, IReadOnlyList<MetadataExcelRow> rows, ISet<string> parameterColumns)
        {
            string? directory = Path.GetDirectoryName(excelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var orderedParameterColumns = BuildParameterColumns(parameterColumns);
            var allColumns = ContextColumns
                .Concat(GroupColumns)
                .Concat(FileColumns)
                .Concat(HeaderColumns)
                .Concat(orderedParameterColumns)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TemplateMetadata");

            ApplySectionHeader(worksheet, 1, 1, ContextColumns.Length, "Context", XLColor.FromHtml("#D9EAF7"));
            ApplySectionHeader(worksheet, 1, 1 + ContextColumns.Length, GroupColumns.Length, "Groups", XLColor.FromHtml("#E2F0D9"));
            ApplySectionHeader(worksheet, 1, 1 + ContextColumns.Length + GroupColumns.Length, FileColumns.Length, "Files", XLColor.FromHtml("#FCE4D6"));
            ApplySectionHeader(worksheet, 1, 1 + ContextColumns.Length + GroupColumns.Length + FileColumns.Length, HeaderColumns.Length, "Header", XLColor.FromHtml("#FFF2CC"));
            ApplySectionHeader(worksheet, 1, 1 + ContextColumns.Length + GroupColumns.Length + FileColumns.Length + HeaderColumns.Length, orderedParameterColumns.Count, "Parameters", XLColor.FromHtml("#EADCF8"));

            for (int columnIndex = 0; columnIndex < allColumns.Count; columnIndex++)
            {
                var cell = worksheet.Cell(2, columnIndex + 1);
                cell.Value = allColumns[columnIndex];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var values = new List<string>
                {
                    row.TemplateFile,
                    row.Group.Id,
                    row.Group.Title,
                    row.Group.Order,
                    row.Group.Description,
                    row.File.Id,
                    row.File.FileName,
                    row.File.Description,
                    row.Header.Title,
                    row.Header.DisplayStyle,
                    row.Header.DefaultStyle,
                    row.Header.Display
                };

                values.AddRange(orderedParameterColumns.Select(column =>
                    column.Equals("ValueType", StringComparison.OrdinalIgnoreCase)
                        ? row.ValueType
                        : row.ParameterValues.TryGetValue(column, out var value) ? value : string.Empty));

                for (int columnIndex = 0; columnIndex < values.Count; columnIndex++)
                {
                    worksheet.Cell(rowIndex + 3, columnIndex + 1).Value = values[columnIndex];
                }
            }

            var usedRange = worksheet.Range(2, 1, Math.Max(2, rows.Count + 2), allColumns.Count);
            usedRange.SetAutoFilter();
            usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            usedRange.Style.Alignment.WrapText = true;

            worksheet.SheetView.FreezeRows(2);
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(excelPath);
        }

        private static List<string> BuildParameterColumns(ISet<string> parameterColumns)
        {
            var remainingColumns = parameterColumns
                .Where(column => !PreferredParameterColumns.Contains(column, StringComparer.OrdinalIgnoreCase))
                .Where(column => !PostParameterColumns.Contains(column, StringComparer.OrdinalIgnoreCase))
                .OrderBy(column => column, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return PreferredParameterColumns
                .Where(column => parameterColumns.Contains(column))
                .Concat(remainingColumns)
                .Concat(PostParameterColumns.Where(column =>
                    column.Equals("ValueType", StringComparison.OrdinalIgnoreCase) || parameterColumns.Contains(column)))
                .ToList();
        }

        private static void ApplySectionHeader(IXLWorksheet worksheet, int row, int startColumn, int width, string title, XLColor color)
        {
            if (width <= 0)
            {
                return;
            }

            var range = worksheet.Range(row, startColumn, row, startColumn + width - 1);
            range.Merge();
            range.Value = title;
            range.Style.Font.Bold = true;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Fill.BackgroundColor = color;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        private sealed class MetadataExcelRow
        {
            public string TemplateFile { get; set; } = string.Empty;
            public GroupInfo Group { get; set; } = new GroupInfo();
            public FileInfo File { get; set; } = new FileInfo();
            public HeaderInfo Header { get; set; } = new HeaderInfo();
            public string ValueType { get; set; } = string.Empty;
            public IReadOnlyDictionary<string, string> ParameterValues { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class GroupInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string Order { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        private sealed class FileInfo
        {
            public string Id { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        private sealed class HeaderInfo
        {
            public string Title { get; set; } = string.Empty;
            public string DisplayStyle { get; set; } = string.Empty;
            public string DefaultStyle { get; set; } = string.Empty;
            public string Display { get; set; } = string.Empty;
        }
    }
}
