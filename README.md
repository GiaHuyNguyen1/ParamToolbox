# 🧰 ParamToolbox - Parameter & Template Management Suite

Bộ công cụ dòng lệnh (CLI) hỗ trợ quản lý cấu hình tham số, chuyển đổi dữ liệu XML/JSON, nhân bản template Merchant, sinh mã C# Model Binding và xuất báo cáo Excel chuyên nghiệp.

> 📌 **Điều hướng nhanh theo vai trò:**
> - 🧪 **[Cẩm nang dành riêng cho QA / QC Tester](GUIDE_FOR_QA.md)**: Cách tải file, chạy không cần cài .NET, mẹo kéo thả thư mục và xuất báo cáo.
> - 🛠️ **[Tài liệu dành riêng cho Developer](GUIDE_FOR_DEV.md)**: Thiết lập IDE, cấu trúc code, cách debug và hướng dẫn thêm Feature mới.
> - 📘 **[Đặc tả Kỹ thuật chi tiết (SPECS)](SPECS.md)**: Chi tiết thuật toán bóc tách dữ liệu, mapping PID và schema XML/JSON.

---

## ⚡ Hướng dẫn sử dụng nhanh (Dành cho Người Dùng / QC / BA)

Không cần cài đặt .NET SDK hay bất kỳ môi trường lập trình nào.

### 1. Tải về gói thực thi (Portable):
- **Windows (64-bit):** Giải nén `ParamToolbox_Win64.zip`
- **macOS Apple Silicon (M1/M2/M3/M4):** Giải nén `ParamToolbox_MacArm64.zip`
- **macOS Intel:** Giải nén `ParamToolbox_MacX64.zip`

### 2. Cách chạy:
- **Trên Windows:** Click đúp vào file `Run_Tool.bat` (hoặc chạy trực tiếp `ParamToolbox.exe`).
- **Trên macOS:**
  1. Click đúp vào file `Run_Tool.command`.
  2. *Lưu ý (nếu macOS cảnh báo bảo mật lần đầu mở):*
     Mở Terminal tại thư mục đã giải nén và chạy lệnh:
     ```bash
     chmod +x ParamToolbox Run_Tool.command
     xattr -d com.apple.quarantine ParamToolbox 2>/dev/null || true
     ```

> [!TIP]
> Khi tool yêu cầu nhập đường dẫn file hoặc thư mục, bạn chỉ cần **kéo và thả (drag & drop)** file/folder trực tiếp từ Finder hoặc File Explorer vào cửa sổ dòng lệnh và nhấn **Enter**.

---

## 💻 Hướng dẫn cho Lập trình viên (Developer Guide)

### 1. Yêu cầu môi trường
- Cài đặt **.NET SDK 10.0** (hoặc **.NET SDK 8.0 LTS**).
- IDE khuyên dùng: Visual Studio 2022 / 2025, JetBrains Rider, hoặc VS Code (kèm C# Dev Kit).

### 2. Chạy ứng dụng từ mã nguồn
```bash
# Di chuyển vào thư mục ParamToolbox
cd ParamToolbox

# Khôi phục dependencies và chạy
dotnet run
```

*(Nếu máy bạn chỉ cài .NET 8, bạn có thể sửa thẻ `<TargetFramework>net10.0</TargetFramework>` trong file `ParamToolbox.csproj` thành `net8.0` là có thể chạy bình thường).*

### 3. Dữ liệu thử nghiệm (Samples)
Dự án có sẵn thư mục `Samples/` để test ngay các tính năng:
- `Samples/XMLs/`: Chứa template XML mẫu (`SingleApp_merchant_1.xml`, `SingleApp_system.xml`, `SingleApp_cashiers.xml`...).
- `Samples/JSONs/`: Chứa các file JSON cấu hình mẫu tương ứng.

---

## 📋 Danh mục tính năng (Feature Menu)

Khi khởi động, màn hình menu sẽ hiển thị 5 tính năng:

```text
=== PARAM TOOLBOX ===
1. Khởi tạo file JSON mặc định từ thẻ <Parameter> của XML (Feature 1)
2. Tổng hợp XML và JSON xuất ra CSV/Excel (Feature 2)
3. Nhân bản Merchant Template XML ra file ZIP (Feature 3)
4. Generate C# Binding Code từ danh sách PID (Feature 4)
5. Xuất Excel đầy đủ thuộc tính từ template XML (Feature 5)
0. Thoát
```

| Phím | Tính năng | Mục đích sử dụng |
| :---: | :--- | :--- |
| **`1`** | **XML to JSON Generator** | Đọc các thẻ `<Parameter>` từ XML, sinh file JSON chứa `DefaultValue` và `Type` ("T" hoặc "P"). |
| **`2`** | **XML & JSON to CSV Consolidation** | Kết hợp cấu trúc từ XML và giá trị cấu hình từ JSON để xuất ma trận tham số ra CSV gửi cho QC/Khách hàng kiểm tra. |
| **`3`** | **Merchant Template Cloner** | Tự động nhân bản hàng loạt template Merchant (`merchant_1` -> `merchant_N`), tự động tịnh tiến ID nhóm (`sys_G`), ID trường (`sys_F`), PID và đóng gói thành các file `.zip`. |
| **`4`** | **C# Model Binding Generator** | Nhập danh sách PID cần map, tool tự động sinh mã nguồn C# Properties & Mapping gán giá trị, lưu ra file `GeneratedCode.txt`. |
| **`5`** | **Template XML Metadata to Excel** | Trích xuất toàn bộ thuộc tính, mô tả, validation, styling từ các file XML và xuất ra file Excel `.xlsx` chuẩn hóa (ClosedXML) có màu sắc và filter. |

---

## 📦 Hướng dẫn Tự động Build & Đóng gói (Multi-Platform Publish)

Để tạo ra các bộ cài đặt độc lập (Self-Contained Single-File) cho tất cả các nền tảng:

### Cách 1: Sử dụng Script tự động (Khuyến nghị)
- **Trên macOS / Linux:**
  ```bash
  chmod +x build-all.sh
  ./build-all.sh
  ```
- **Trên Windows:**
  Chạy file `build-all.bat`

Kết quả các file `.zip` hoàn chỉnh sẽ nằm trong thư mục `dist/`.

### Cách 2: Chạy lệnh `dotnet publish` thủ công
- **Windows (x64):**
  ```bash
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish/win-x64
  ```
- **macOS Apple Silicon (M-series):**
  ```bash
  dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish/osx-arm64
  ```
- **macOS Intel (x64):**
  ```bash
  dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o publish/osx-x64
  ```

---

## 📖 Đặc tả kỹ thuật chi tiết (Technical Specs)
Chi tiết về cấu trúc thuật toán, xử lý mapping, schema XML/JSON, và hướng dẫn mở rộng code, xem thêm tại: [SPECS.md](SPECS.md).
