# 🛠️ DEVELOPER CONTRIBUTION GUIDE - PARAMTOOLBOX

🌐 Language: **English** | [Tiếng Việt](GUIDE_FOR_DEV.vi.md)

This document provides complete instructions for developers on environment setup, coding conventions, debugging, adding new features, and packaging **ParamToolbox**.

---

## 💻 1. Development Prerequisites

- **SDK:** Install **.NET 8.0 SDK (LTS)** or **.NET 10.0 SDK**.
  - Check your installed SDKs: `dotnet --list-sdks`
  - *By default the project targets `<TargetFramework>net10.0</TargetFramework>`, but it is fully backwards-compatible with `net8.0` if your environment uses .NET 8.*
- **Recommended IDEs:**
  - **JetBrains Rider** (Recommended across macOS and Windows)
  - **Visual Studio 2022 / 2025** (Windows / macOS)
  - **Visual Studio Code** (with `C# Dev Kit` and `.NET Install Tool` extensions)

---

## 🚀 2. Getting Started & Debugging

### Step 1: Open the Solution
Open the standalone solution file directly:
```bash
# Open in VS Code
code ParamToolbox/ParamToolbox.sln

# Or open in Rider / Visual Studio
open ParamToolbox/ParamToolbox.sln
```

### Step 2: Run from Command Line
```bash
cd ParamToolbox
dotnet restore
dotnet run
```

---

## 🏛️ 3. Architecture & Coding Standards

### Feature-Based Modular Architecture
Each CLI option is decoupled into an isolated `static class`:
- `Program.cs`: Entry point, main menu loop, and routing.
- `I18n.cs`: Bilingual internationalization helper (defaults to English, toggles to Vietnamese).
- `PathHelper.cs`: Input sanitizer stripping quotes (`"` and `'`) from drag-and-drop paths.
- `Feature1_JsonGenerator.cs` through `Feature5_TemplateMetadataCsvExporter.cs`: Business modules.

### Standards to Follow:
1. **No Hardcoded Absolute Paths:** Always use relative paths or `Path.Combine()`.
2. **Always Use `PathHelper.CleanPath()`:** Wrap every `Console.ReadLine()` path input with `PathHelper.CleanPath(...)` to prevent crashes when users drag & drop files.
3. **Use `I18n.T()` for Console Strings:** Provide both English and Vietnamese text:
   ```csharp
   Console.WriteLine(I18n.T("English message", "Thông báo tiếng Việt"));
   ```
4. **Per-File Exception Handling:** Wrap iterations in `try...catch` per file so a single corrupt template does not terminate batch processes.
5. **Standard Encoding:** Emit text/CSV with `Encoding.UTF8` (BOM enabled) to ensure Excel displays international and accented characters properly.

---

## ➕ 4. How to Add a New Feature (Example: Feature 6)

### Step 1: Create `Feature6_ValidateXmlSchema.cs`
```csharp
using System;
using System.IO;

namespace ParamToolbox
{
    public static class Feature6_ValidateXmlSchema
    {
        public static void Execute()
        {
            Console.WriteLine(I18n.T("=== FEATURE 6: Validate XML Schema ===",
                                     "=== FEATURE 6: Kiểm tra XML Schema ==="));

            Console.Write(I18n.T("Enter file/folder path to validate: ",
                                 "Nhập đường dẫn file/folder cần kiểm tra: "));
            string targetPath = PathHelper.CleanPath(Console.ReadLine());

            if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
            {
                Console.WriteLine(I18n.T("Error: Path does not exist!",
                                         "Lỗi: Đường dẫn không tồn tại!"));
                return;
            }

            // Implement your validation logic here...
            Console.WriteLine(I18n.T("Feature 6 completed!",
                                     "Hoàn tất xử lý Feature 6!"));
        }
    }
}
```

### Step 2: Register in `Program.cs`
1. Add a menu line:
   ```csharp
   Console.WriteLine(I18n.T("6. Validate XML Schema (Feature 6)",
                            "6. Kiểm tra hợp lệ XML Schema (Feature 6)"));
   ```
2. Add a `switch-case` branch:
   ```csharp
   case "6":
       Console.Clear();
       Feature6_ValidateXmlSchema.Execute();
       break;
   ```

---

## 📦 5. Automated Multi-Platform Packaging

The repository provides one-click build scripts that compile **Self-Contained Single-File** executables:

- **On macOS / Linux:**
  ```bash
  chmod +x build-all.sh
  ./build-all.sh
  ```
- **On Windows:**
  ```cmd
  build-all.bat
  ```

Output ZIP archives will be created in `dist/` containing ready-to-use binaries and launchers (`Run_Tool.bat` / `Run_Tool.command`).

---

## 🛡️ 6. Pre-Commit Checklist

- [ ] Run `dot_clean .` on macOS to purge AppleDouble metadata (`._*`).
- [ ] Ensure `bin/`, `obj/`, `publish/`, `dist/`, or `.zip` files are not staged for commit.
- [ ] Verify sample files in `Samples/` do not contain private tokens, internal credentials, or sensitive data.
- [ ] Verify both `dotnet build` and `./build-all.sh` succeed without errors.
