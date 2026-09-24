# 🛠️ HƯỚNG DẪN DÀNH CHO LẬP TRÌNH VIÊN (DEVELOPER CONTRIBUTION GUIDE)

Tài liệu này cung cấp toàn bộ quy trình thiết lập môi trường, quy chuẩn mã nguồn, cách gỡ lỗi (debug), thêm tính năng mới và đóng gói dự án **ParamToolbox**.

---

## 💻 1. Môi trường phát triển (Prerequisites)

- **SDK:** Cài đặt **.NET 8.0 SDK (LTS)** hoặc **.NET 10.0 SDK**.
  - Kiểm tra bằng lệnh: `dotnet --list-sdks`
  - *Mặc định project để `<TargetFramework>net10.0</TargetFramework>`, nhưng hoàn toàn tương thích và có thể đổi sang `net8.0` nếu môi trường máy bạn chỉ có .NET 8.*
- **IDE đề xuất:**
  - **Visual Studio 2022 / 2025** (Windows / macOS)
  - **JetBrains Rider** (Khuyên dùng trên cả macOS & Windows)
  - **Visual Studio Code** (Cài extension `C# Dev Kit` và `.NET Install Tool`)

---

## 🚀 2. Khởi chạy và Debug mã nguồn

### Bước 1: Mở Solution
Mở trực tiếp file solution độc lập:
```bash
# Mở solution bằng VS Code
code ParamToolbox/ParamToolbox.sln

# Hoặc mở bằng Rider / Visual Studio
open ParamToolbox/ParamToolbox.sln
```

### Bước 2: Chạy trực tiếp từ dòng lệnh
```bash
cd ParamToolbox
dotnet restore
dotnet run
```

---

## 🏛️ 3. Kiến trúc dự án & Quy chuẩn lập trình (Architecture & Standards)

### Kiến trúc phân rã theo Tính năng (Feature-based Modular Design)
Mỗi tính năng trong menu CLI được đóng gói độc lập trong một file `static class`:
- `Program.cs`: Chỉ quản lý vòng lặp Menu chính và điều hướng người dùng.
- `PathHelper.cs`: Hàm tiện ích chuẩn hóa chuỗi đường dẫn (xóa bỏ dấu nháy kép `"`, nháy đơn `'` do thao tác kéo thả).
- `Feature1_JsonGenerator.cs` -> `Feature5_TemplateMetadataCsvExporter.cs`: Các module nghiệp vụ riêng biệt.

### Quy chuẩn khi viết code:
1. **Không dùng đường dẫn tuyệt đối (Absolute Path):** Luôn dùng `Path.Combine()` và đường dẫn tương đối.
2. **Luôn dùng `PathHelper.CleanPath()`:** Khi nhận chuỗi từ `Console.ReadLine()`, luôn bọc qua `PathHelper.CleanPath(...)` để tránh crash khi người dùng kéo thả thư mục vào Terminal.
3. **Bắt ngoại lệ cục bộ (Fault-tolerant):** Mỗi tác vụ duyệt file phải bọc `try...catch` theo từng file riêng lẻ. Một file hỏng không được làm sập cả tiến trình.
4. **Encoding chuẩn:** Ghi file CSV hoặc Text với `Encoding.UTF8` (có BOM) để Excel hiển thị đúng dấu tiếng Việt.

---

## ➕ 4. Hướng dẫn thêm một Tính năng mới (Ví dụ: Feature 6)

Để thêm một tính năng mới (ví dụ: `Feature6_ValidateXmlSchema`):

### Bước 1: Tạo file class mới `Feature6_ValidateXmlSchema.cs`
```csharp
using System;
using System.IO;

namespace ParamToolbox
{
    public static class Feature6_ValidateXmlSchema
    {
        public static void Execute()
        {
            Console.WriteLine("=== FEATURE 6: Validate XML Schema ===");
            Console.Write("Nhập đường dẫn file/folder cần kiểm tra: ");
            string targetPath = PathHelper.CleanPath(Console.ReadLine());

            if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
            {
                Console.WriteLine("Lỗi: Đường dẫn không tồn tại!");
                return;
            }

            // Triển khai logic nghiệp vụ tại đây...
            Console.WriteLine("Hoàn tất xử lý Feature 6!");
        }
    }
}
```

### Bước 2: Đăng ký vào Menu trong `Program.cs`
1. Thêm dòng in menu:
   ```csharp
   Console.WriteLine("6. Kiểm tra hợp lệ cấu trúc XML Schema (Feature 6)");
   ```
2. Thêm nhánh xử lý trong `switch (choice)`:
   ```csharp
   case "6":
       Console.Clear();
       Feature6_ValidateXmlSchema.Execute();
       break;
   ```

---

## 📦 5. Quy trình Đóng gói phát hành (Build & Publish)

Dự án hỗ trợ đóng gói độc lập không phụ thuộc môi trường (.NET Runtime) bằng 2 script tự động:

- **Trên macOS / Linux:**
  ```bash
  chmod +x build-all.sh
  ./build-all.sh
  ```
- **Trên Windows:**
  ```cmd
  build-all.bat
  ```

Toàn bộ gói nén cho 3 nền tảng (`ParamToolbox_Win64.zip`, `ParamToolbox_MacArm64.zip`, `ParamToolbox_MacX64.zip`) sẽ được tạo tự động trong thư mục `dist/`.

---

## 🛡️ 6. Checklist trước khi tạo Pull Request / Push lên Git

- [ ] Chạy `dot_clean .` (nếu dùng macOS) để xóa sạch các file rác `._*`.
- [ ] Không commit các thư mục `bin/`, `obj/`, `publish/`, `dist/` hoặc các file `.zip`.
- [ ] Kiểm tra các file trong `Samples/` không chứa dữ liệu bí mật (mật khẩu thật, token nội bộ công ty).
- [ ] Biên dịch thử nghiệm thành công với cả `dotnet build` và `./build-all.sh`.
