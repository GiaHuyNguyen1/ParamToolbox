# 📘 ĐẶC TẢ KỸ THUẬT (TECHNICAL SPECIFICATIONS) - PARAMTOOLBOX

## 1. Giới thiệu tổng quan (Overview)
`ParamToolbox` là công cụ dòng lệnh (CLI - Console Application) được phát triển bằng **C# / .NET**, phục vụ việc quản lý, chuẩn hóa, chuyển đổi, sinh mã và xuất báo cáo cho hệ thống tham số cấu hình XML/JSON (POS / Cashier / Merchant / Surcharge Templates).

### Công nghệ sử dụng
- **Ngôn ngữ:** C# 12 / .NET 10.0 (hoàn toàn tương thích ngược với .NET 8.0 LTS).
- **Thư viện bên thứ ba:**
  - `ClosedXML (v0.104.2)`: Xử lý định dạng bảng tính Excel `.xlsx` chuyên nghiệp (styling, column autofit, headers).
  - `Newtonsoft.Json (v13.0.4)` & `System.Text.Json`: Xử lý phân tích và sinh chuỗi JSON.
  - `System.Xml.Linq` (`XDocument`, `XElement`): Phân tích cấu trúc cây XML.
  - `System.IO.Compression`: Đóng gói file nén `.zip`.

---

## 2. Cấu trúc thư mục dự án (Project Structure)

```text
ParamToolbox/
├── Program.cs                                  # Entry point & Menu điều hướng chính (0-5)
├── PathHelper.cs                               # Tiện ích làm sạch đường dẫn kéo thả (strip quotes)
├── Feature1_JsonGenerator.cs                   # [Feature 1] XML -> JSON (DefaultValue & Type)
├── Feature2_CsvConverter.cs                    # [Feature 2] Tổng hợp XML + JSON -> CSV
├── Feature3_MerchantCloner.cs                  # [Feature 3] Nhân bản Merchant Template XML -> ZIP
├── Feature4_CSharpBindingGenerator.cs          # [Feature 4] Sinh mã C# Model Binding từ PID
├── Feature5_TemplateMetadataCsvExporter.cs     # [Feature 5] Xuất toàn bộ Metadata XML -> Excel (.xlsx)
├── ParamToolbox.csproj                         # Project file & dependencies
├── README.md                                   # Hướng dẫn cài đặt & sử dụng
├── SPECS.md                                    # Tài liệu đặc tả kỹ thuật chi tiết
├── build-all.sh                                # Script đóng gói tự động cho macOS / Linux
├── build-all.bat                               # Script đóng gói tự động cho Windows
└── Samples/                                    # Thư mục dữ liệu mẫu để test ngay
    ├── XMLs/                                   # Template XML mẫu (Merchant, System, Cashier...)
    └── JSONs/                                  # Cấu hình JSON mẫu tương ứng
```

---

## 3. Đặc tả chi tiết 5 tính năng (Feature Specifications)

### 3.1. Feature 1: Khởi tạo file JSON mặc định từ thẻ `<Parameter>` của XML
- **Class:** `Feature1_JsonGenerator`
- **Mục tiêu:** Quét toàn bộ các template XML để tự động sinh ra 2 tập tin JSON: file chứa giá trị mặc định (`DefaultValue`) và file chứa kiểu dữ liệu (`Type`).
- **Đầu vào (Input):**
  - Đường dẫn thư mục chứa các file XML.
  - Bộ lọc: Bỏ qua các file ẩn/tạm (bắt đầu bằng `.` hoặc `_`).
- **Quy tắc phân tích (Processing Logic):**
  1. Quét tất cả thẻ `<Parameter>` có thẻ con `<PID>`.
  2. Bỏ qua thẻ nếu `PID` rỗng.
  3. Lấy giá trị `<Defaultvalue>`.
  4. Lấy giá trị `<InputType>`. Nếu `InputType == "password"` (không phân biệt hoa thường) thì gán `Type = "P"`, ngược lại gán `Type = "T"` (Text).
- **Đầu ra (Output):**
  - Tạo 2 file trong cùng thư mục với file XML gốc:
    - `{xmlFileNameWithoutExt}.json`: Lưu Dictionary `<string, object>` (Key: PID, Value: DefaultValue).
    - `{xmlFileNameWithoutExt}.type.json`: Lưu Dictionary `<string, string>` (Key: PID, Value: "T" hoặc "P").

---

### 3.2. Feature 2: Tổng hợp XML và JSON xuất ra CSV
- **Class:** `Feature2_CsvConverter`
- **Mục tiêu:** Kết hợp dữ liệu cấu trúc tham số (từ XML) và giá trị cấu hình thực tế (từ JSON) để xuất ra file CSV tổng hợp ma trận tham số (dùng để gửi QC hoặc khách hàng rà soát).
- **Đầu vào (Input):**
  1. Đường dẫn thư mục chứa file XML.
  2. Đường dẫn thư mục chứa file JSON.
  3. Đường dẫn file xuất `.csv` (mặc định lưu ra Desktop nếu để trống).
  4. Số lượng Merchant cần tổng hợp (mặc định: 6).
- **Phân loại Template (Auto Categorization):**
  - Nhận diện các template: `System`, `Cashier`, `Surcharge`, và `Merchant` (tự động phân giải các merchant từ `merchant_1` đến `merchant_N`).
- **Cấu trúc cột trong CSV đầu ra (Output Schema):**
  | Cột | Ý nghĩa | Nguồn dữ liệu |
  | :--- | :--- | :--- |
  | `PID` | Mã định danh tham số | `<PID>` trong XML |
  | `Title` | Tên hiển thị tham số | `<Title>` trong XML |
  | `Type` | Kiểu hiển thị (T: Text, P: Password) | File `{name}.type.json` |
  | `Group` | Nhóm tham số | `<GroupTitle>` / `<GroupID>` |
  | `DefaultValue` | Giá trị mặc định | File JSON hoặc XML gốc |
  | `Merchant 1` ... `Merchant N` | Giá trị tương ứng từng Merchant | File JSON của từng Merchant |
- **Quy tắc định dạng CSV:** Sử dụng chuẩn mã hóa UTF-8 có BOM (`Encoding.UTF8`), escape dấu phẩy và nháy kép đúng chuẩn RFC 4180.

---

### 3.3. Feature 3: Nhân bản Merchant Template XML ra file ZIP
- **Class:** `Feature3_MerchantCloner`
- **Mục tiêu:** Sinh hàng loạt file XML Merchant từ một template gốc (ví dụ từ `SingleApp_merchant_1.xml` tạo ra `SingleApp_merchant_2.xml` đến `SingleApp_merchant_30.xml`), tự động tịnh tiến các ID để tránh trùng lặp.
- **Đầu vào (Input):**
  1. Đường dẫn file template XML nguồn (chứa `sys_G1`, `sys_F1`...).
  2. Thư mục xuất kết quả (tùy chọn, mặc định tạo thư mục con `Output_Cloned/`).
  3. Tổng số lượng file cần clone (mặc định: 30).
  4. Số lượng file tối đa trong mỗi file `.zip` (mặc định: 10 files/zip).
- **Quy tắc tịnh tiến & Thay thế (Transformation Rules):**
  - **Group ID:** `sys_G1` -> `sys_G{i}`
  - **File ID:** `sys_F1` -> `sys_F{i}`
  - **PID Suffix/Prefix:** Các PID có chứa chỉ số `1` (như `merchantProcessor.mid1`, `tid1`...) được cập nhật thành chỉ số tương ứng `{i}`.
  - **DefaultValue:** Thay thế chuỗi tham chiếu tương ứng nếu có.
- **Đầu ra (Output):**
  - Các file XML nhân bản được nén tự động thành từng gói:
    - `Merchant_Batch_1_to_10.zip`
    - `Merchant_Batch_11_to_20.zip`
    - `Merchant_Batch_21_to_30.zip`

---

### 3.4. Feature 4: Generate C# Binding Code từ danh sách PID
- **Class:** `Feature4_CSharpBindingGenerator`
- **Mục tiêu:** Hỗ trợ lập trình viên backend sinh nhanh mã nguồn C# Property và cú pháp Binding từ Model XML sang C# Class DTO/Entity.
- **Đầu vào (Input):**
  1. Đường dẫn file XML.
  2. Danh sách PID cần sinh code (nhập từng dòng trên Console, gõ dòng trống để kết thúc).
- **Logic sinh mã (Code Generation):**
  - Tìm kiếm thông tin thẻ `<Parameter>` tương ứng với mỗi PID trong XML:
    - Đọc `<Title>`, `<DataType>`, `<Defaultvalue>`.
    - Map `DataType` XML sang C# Type (`string`, `int`, `bool`, `decimal`...).
    - Tự động chuyển PID dạng camelCase/dot.notation sang tên Property PascalCase.
- **Đầu ra (Output):**
  - In trực tiếp đoạn code C# ra màn hình Console.
  - Tự động lưu hoặc ghi đè ra file `GeneratedCode.txt` tại thư mục chạy tool để dev copy nhanh.

---

### 3.5. Feature 5: Xuất Excel đầy đủ thuộc tính từ Template XML (ClosedXML)
- **Class:** `Feature5_TemplateMetadataCsvExporter`
- **Mục tiêu:** Phân tích sâu toàn bộ siêu dữ liệu (Metadata) của các template XML, bao gồm cấu trúc phân cấp (Group -> File -> Header -> Parameter) và xuất ra file Excel chuẩn hóa `.xlsx`.
- **Đầu vào (Input):**
  1. Thư mục chứa các file XML.
  2. Đường dẫn và tên file `.xlsx` kết quả.
- **Bộ cột được trích xuất (Columns Schema):**
  - **Context:** `Template File`
  - **Group:** `GroupID`, `GroupTitle`, `GroupOrder`, `GroupDescription`
  - **File:** `FileID`, `FileName`, `FileDescription`
  - **Header:** `HeaderTitle`, `HeaderDisplayStyle`, `HeaderDefaultStyle`, `HeaderDisplay`
  - **Parameter:** `PID`, `Title`, `Defaultvalue`, `DataType`, `Readonly`, `Required`, `Display`, `InputType`, `Select`, `Type`, `Description`, `ValueType`...
  - **Dynamic Attributes:** Tự động phát hiện và bổ sung thêm các thẻ con tùy biến nếu template có thuộc tính mới.
- **Định dạng bảng tính Excel:**
  - Header được tô màu nền (`Color.FromArgb(41, 128, 185)`), chữ trắng in đậm.
  - Kích hoạt AutoFilter trên toàn bộ bảng dữ liệu.
  - Tự động căn chỉnh độ rộng cột (`AdjustToContents()`).
  - Freeze pane dòng tiêu đề để thuận tiện khi cuộn dữ liệu lớn.

---

## 4. Xử lý lỗi & Ngoại lệ (Fault Tolerance)
1. **Làm sạch đường dẫn (Sanitization):** Mọi đầu vào nhập từ `Console.ReadLine()` đều được xử lý qua `PathHelper.CleanPath()` nhằm loại bỏ dấu nháy kép `"` hoặc nháy đơn `'` do thao tác kéo thả file/thư mục từ giao diện hệ điều hành vào terminal.
2. **Kiểm tra tồn tại:** Tất cả các hàm đều kiểm tra `File.Exists()` và `Directory.Exists()` trước khi thực hiện I/O.
3. **Bọc Try-Catch cục bộ theo từng file:** Trong Feature 1, 2, 5, nếu một file XML cụ thể bị lỗi cú pháp cấu trúc, tiến trình sẽ thông báo lỗi riêng cho file đó và tiếp tục xử lý các file còn lại thay vì dừng đột ngột.

---

## 5. Hướng dẫn mở rộng thêm tính năng mới (Extensibility Guide)
Khi cần bổ sung tính năng mới (ví dụ Feature 6):
1. Tạo file class mới: `Feature6_{TênTínhNăng}.cs`.
2. Khai báo namespace `ParamToolbox`.
3. Định nghĩa phương thức tĩnh: `public static void Execute()`.
4. Mở file `Program.cs`:
   - Thêm dòng giới thiệu tính năng 6 vào Menu in ra màn hình.
   - Thêm nhánh `case "6": Feature6_{TênTínhNăng}.Execute(); break;` vào vòng lặp `switch-case`.
