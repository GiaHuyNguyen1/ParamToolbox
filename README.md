# 🧰 ParamToolbox - Parameter & Template Management Suite

🌐 Language: **English** | [Tiếng Việt](README.vi.md)

A cross-platform Command-Line Interface (CLI) suite designed to manage parameter configurations, convert XML/JSON schemas, clone Merchant templates, generate C# model bindings, and export structured Excel metadata reports.

> 📌 **Quick Navigation by Role:**
> - 🧪 **[QA / QC Tester Guide](GUIDE_FOR_QA.md)**: How to download, run without installing .NET, drag & drop shortcuts, and audit configurations.
> - 🛠️ **[Developer Guide](GUIDE_FOR_DEV.md)**: IDE setup, solution structure, debugging, coding standards, and adding new features.
> - 📘 **[Technical Specifications (SPECS)](SPECS.md)**: Deep dive into parsing algorithms, PID mappings, and XML/JSON data contracts.

---

## ⚡ Quick Start for End-Users / QA / BA

No .NET SDK or programming setup is required.

### 1. Download the Portable Package:
From the [GitHub Releases](https://github.com/GiaHuyNguyen1/ParamToolbox/releases) page, download the zip matching your operating system:
- **Windows (64-bit):** `ParamToolbox_Win64.zip`
- **macOS Apple Silicon (M1/M2/M3/M4):** `ParamToolbox_MacArm64.zip`
- **macOS Intel:** `ParamToolbox_MacX64.zip`

### 2. Run the Tool:
- **On Windows:** Extract the zip and double-click **`Run_Tool.bat`** (or execute `ParamToolbox.exe`).
- **On macOS:**
  1. Extract the zip and double-click **`Run_Tool.command`**.
  2. *Note (if macOS displays a security prompt on first launch):*
     Open Terminal in the extracted folder and run:
     ```bash
     chmod +x ParamToolbox Run_Tool.command
     xattr -d com.apple.quarantine ParamToolbox 2>/dev/null || true
     ```

> [!TIP]
> **Drag & Drop Paths:** When prompted for file or directory paths, simply **drag and drop** the file or folder directly from File Explorer or Finder into the Terminal window and press **Enter**.

---

## 💻 Developer Guide

### 1. Requirements
- **.NET SDK 10.0** (or **.NET SDK 8.0 LTS**).
- Recommended IDE: Visual Studio 2022/2025, JetBrains Rider, or VS Code (with C# Dev Kit).

### 2. Clone & Run from Source
```bash
# Clone the repository
git clone https://github.com/GiaHuyNguyen1/ParamToolbox.git
cd ParamToolbox

# Restore dependencies and run
dotnet run
```

*(If you only have .NET 8 SDK installed, you can simply change `<TargetFramework>net10.0</TargetFramework>` in `ParamToolbox.csproj` to `net8.0`).*

### 3. Sample Datasets
The project includes a ready-to-test `Samples/` directory:
- `Samples/XMLs/`: Sample XML templates (`SingleApp_merchant_1.xml`, `SingleApp_system.xml`, `SingleApp_cashiers.xml`...).
- `Samples/JSONs/`: Corresponding configuration JSON files.

---

## 📋 Feature Menu

When launched, the interactive CLI displays the menu:

```text
==========================================================
                   === PARAM TOOLBOX ===                  
==========================================================
1. Generate default JSON from XML <Parameter> tags (Feature 1)
2. Consolidate XML & JSON to CSV/Excel (Feature 2)
3. Clone Merchant XML Templates to ZIP (Feature 3)
4. Generate C# Binding Code from PIDs (Feature 4)
5. Export XML Template Metadata to Excel (Feature 5)
9. Switch language / Đổi ngôn ngữ (Current: English)
0. Exit
----------------------------------------------------------
Select an option (0-5, 9): 
```

| Key | Feature | Description |
| :---: | :--- | :--- |
| **`1`** | **XML to JSON Generator** | Parses `<Parameter>` tags from XML and generates `{name}.json` (DefaultValues) and `{name}.type.json` ("T" for Text, "P" for Password). |
| **`2`** | **XML & JSON to CSV Consolidation** | Combines XML parameter definitions with JSON values to export a unified configuration matrix into CSV for QA auditing. |
| **`3`** | **Merchant Template Cloner** | Clones merchant templates (`merchant_1` to `merchant_N`), increments group IDs (`sys_G`), field IDs (`sys_F`), and PIDs, packaging them into zipped batches. |
| **`4`** | **C# Model Binding Generator** | Generates strongly-typed C# property mapping logic from a list of PIDs, saving output to `GeneratedCode.txt`. |
| **`5`** | **Template XML Metadata to Excel** | Deeply inspects all XML attributes, hierarchy, validation, and styling, exporting a styled `.xlsx` workbook (ClosedXML) with auto-filters and frozen header panes. |
| **`9`** | **Language Switcher** | Toggles user interface language dynamically between **English** and **Tiếng Việt**. |
| **`0`** | **Exit** | Closes the application. |

---

## 📦 Automated Multi-Platform Packaging

To package standalone self-contained single-file binaries without requiring .NET runtime on client machines:

- **On macOS / Linux:**
  ```bash
  chmod +x build-all.sh
  ./build-all.sh
  ```
- **On Windows:**
  Execute `build-all.bat`

Output ZIP archives will be generated in `dist/` for:
- `ParamToolbox_Win64.zip`
- `ParamToolbox_MacArm64.zip`
- `ParamToolbox_MacX64.zip`

---

## 📖 Technical Specifications
For full architectural details, data flow contracts, and XML schema definitions, see [SPECS.md](SPECS.md).
