# 📘 TECHNICAL SPECIFICATIONS - PARAMTOOLBOX

🌐 Language: **English** | [Tiếng Việt](SPECS.vi.md)

## 1. System Overview
`ParamToolbox` is a cross-platform Command-Line Interface (CLI) application built with **C# / .NET** to automate the management, standardization, transformation, code generation, and reporting of XML/JSON parameter configuration templates (POS / Cashier / Merchant / Surcharge templates).

### Technology Stack
- **Language & Runtime:** C# 12 / .NET 10.0 (fully backwards-compatible with .NET 8.0 LTS).
- **Dependencies:**
  - `ClosedXML (v0.104.2)`: Advanced OpenXML spreadsheet generation (`.xlsx`), custom styling, auto-filtering, and column auto-fit.
  - `Newtonsoft.Json (v13.0.4)` & `System.Text.Json`: High-performance JSON serialization and schema formatting.
  - `System.Xml.Linq` (`XDocument`, `XElement`): Hierarchical XML parsing and manipulation.
  - `System.IO.Compression`: Native ZIP archive packaging.

---

## 2. Directory Structure

```text
ParamToolbox/
├── Program.cs                                  # Entry point & interactive CLI navigation (0-5, 9)
├── I18n.cs                                     # Bilingual internationalization engine (English default)
├── PathHelper.cs                               # Terminal drag-and-drop path sanitizer (strips quotes)
├── Feature1_JsonGenerator.cs                   # [Feature 1] XML -> JSON (DefaultValue & Type)
├── Feature2_CsvConverter.cs                    # [Feature 2] Consolidate XML + JSON -> CSV matrix
├── Feature3_MerchantCloner.cs                  # [Feature 3] Clone Merchant templates -> ZIP batches
├── Feature4_CSharpBindingGenerator.cs          # [Feature 4] Generate C# property bindings from PIDs
├── Feature5_TemplateMetadataCsvExporter.cs     # [Feature 5] Full XML metadata export -> Excel (.xlsx)
├── ParamToolbox.csproj                         # Project file & package dependencies
├── ParamToolbox.sln                            # Standalone Visual Studio / Rider solution
├── README.md                                   # Primary user documentation (English)
├── README.vi.md                                # Vietnamese documentation
├── GUIDE_FOR_QA.md                             # QA Tester handbook (English)
├── GUIDE_FOR_QA.vi.md                          # QA Tester handbook (Vietnamese)
├── GUIDE_FOR_DEV.md                            # Developer contribution guide (English)
├── GUIDE_FOR_DEV.vi.md                         # Developer contribution guide (Vietnamese)
├── SPECS.md                                    # Technical specifications (English)
├── SPECS.vi.md                                 # Technical specifications (Vietnamese)
├── build-all.sh                                # Automated packaging script for macOS / Linux
├── build-all.bat                               # Automated packaging script for Windows
└── Samples/                                    # Ready-to-test sample fixtures
    ├── XMLs/                                   # Template XML fixtures (Merchant, System, Cashier...)
    └── JSONs/                                  # Matching JSON configuration fixtures
```

---

## 3. Detailed Feature Specifications

### 3.1. Feature 1: Generate Default JSON from XML `<Parameter>` Tags
- **Class:** `Feature1_JsonGenerator`
- **Objective:** Parses template XML files and extracts parameters to generate two corresponding JSON configuration files: one for default values (`DefaultValue`) and one for field types (`Type`).
- **Input:**
  - Directory path containing XML files.
  - Filter: Ignores hidden and metadata files (starting with `.` or `_`).
- **Processing Logic:**
  1. Recursively traverses `<Parameter>` elements containing child `<PID>`.
  2. Discards entries with empty PIDs.
  3. Reads `<Defaultvalue>` content.
  4. Reads `<InputType>`. If `InputType.Equals("password", StringComparison.OrdinalIgnoreCase)`, assigns `Type = "P"`, otherwise `Type = "T"` (Text).
- **Output:**
  - `{xmlFileNameWithoutExt}.json`: Serialized Dictionary `<string, object>` (Key: PID, Value: DefaultValue).
  - `{xmlFileNameWithoutExt}.type.json`: Serialized Dictionary `<string, string>` (Key: PID, Value: "T" | "P").
  - `{xmlFileNameWithoutExt}.pids.txt`: Line-delimited list of distinct PIDs.

---

### 3.2. Feature 2: Consolidate XML & JSON into CSV Matrix
- **Class:** `Feature2_CsvConverter`
- **Objective:** Merges parameter definitions from XML templates with concrete runtime configuration values from JSON files, exporting a consolidated matrix into CSV for QA auditing.
- **Input:**
  1. Path to folder containing XML files.
  2. Path to folder containing JSON files.
  3. Output `.csv` file path (defaults to Desktop if blank).
  4. Total number of Merchants to consolidate (default: 6).
- **Template Categorization:**
  - Automatically identifies: `System`, `Cashier`, `Surcharge`, and `Merchant` (dynamically resolves `merchant_1` through `merchant_N`).
- **Output Schema:**
  | Column | Description | Data Source |
  | :--- | :--- | :--- |
  | `PID` | Parameter identifier | `<PID>` tag in XML |
  | `Title` | Display title | `<Title>` tag in XML |
  | `Type` | Input classification (T: Text, P: Password) | From `{name}.type.json` |
  | `Group` | Parameter grouping | `<GroupTitle>` / `<GroupID>` |
  | `DefaultValue` | Baseline value | JSON or XML default |
  | `Merchant 1` ... `Merchant N` | Per-merchant configuration values | Dynamic merchant JSON files |
- **Format:** Encoded in UTF-8 with BOM (`Encoding.UTF8`), strictly compliant with RFC 4180 delimiter and escaping rules.

---

### 3.3. Feature 3: Clone Merchant XML Templates to ZIP Batches
- **Class:** `Feature3_MerchantCloner`
- **Objective:** Generates multiple distinct Merchant XML files from a baseline template (`SingleApp_merchant_1.xml` -> `merchant_2.xml` ... `merchant_30.xml`), incrementing group and field identifiers.
- **Input:**
  1. Source XML template path.
  2. Output directory path (defaults to `Merchant_Zips/`).
  3. Total number of clones (default: 30).
  4. Maximum files per `.zip` batch (default: 10).
- **Transformation Rules:**
  - **Group ID:** Increments `sys_G1` -> `sys_G{i}`.
  - **File ID:** Increments `sys_F1` -> `sys_F{i}`.
  - **PID Suffix/Prefix:** Updates numbered PIDs (e.g. `merchantProcessor.mid1` -> `mid{i}`).
  - **DefaultValue:** Rewrites dynamic macro references (`#{basePid1}` -> `#{basePid{i}}`).
- **Output:**
  - Single uncompressed `SingleApp_merchant_1.xml` (baseline).
  - Packaged ZIP archives:
    - `Merchant_2_8.zip`
    - `Merchant_9_16.zip`
    - `Merchant_17_30.zip`

---

### 3.4. Feature 4: Generate C# Model Binding Code from PIDs
- **Class:** `Feature4_CSharpBindingGenerator`
- **Objective:** Assists backend engineers by generating strongly-typed C# property mapping code from XML elements to C# model objects.
- **Input:**
  1. XML file path.
  2. Target PID list (entered via console, terminated with 'DONE' or double Enter).
- **Code Generation Logic:**
  - Matches each PID against `<Parameter>` tags in the XML.
  - Maps XML `DataType` to C# types (`string`, `uint`, `DateTime`, `bool`).
  - Converts camelCase/dot.notation PID keys to PascalCase property names.
- **Output:**
  - Prints generated C# binding code to console.
  - Writes code to `GeneratedCode.txt` for immediate developer clipboard copy.

---

### 3.5. Feature 5: Export Full XML Metadata to Styled Excel (.xlsx)
- **Class:** `Feature5_TemplateMetadataCsvExporter`
- **Objective:** Deeply inspects and flattens XML template structures (Group -> File -> Header -> Parameter) and outputs a presentation-ready Excel spreadsheet using ClosedXML.
- **Input:**
  1. Directory path containing XML templates.
  2. Destination `.xlsx` file path.
- **Extracted Column Schema:**
  - **Context:** `Template File`
  - **Group:** `GroupID`, `GroupTitle`, `GroupOrder`, `GroupDescription`
  - **File:** `FileID`, `FileName`, `FileDescription`
  - **Header:** `HeaderTitle`, `HeaderDisplayStyle`, `HeaderDefaultStyle`, `HeaderDisplay`
  - **Parameter:** `PID`, `Title`, `Defaultvalue`, `DataType`, `Readonly`, `Required`, `Display`, `InputType`, `Select`, `Type`, `Description`, `ValueType`...
  - **Dynamic Attributes:** Automatically captures custom schema elements.
- **Styling Features:**
  - Navy blue styled header (`Color.FromArgb(41, 128, 185)`) with bold white text.
  - Active Excel Auto-Filter across all data columns.
  - Auto-fitted column widths (`AdjustToContents()`).
  - Frozen header pane for convenient navigation across thousands of rows.

---

## 4. Fault Tolerance & User Experience
1. **Bilingual Support (`I18n.cs`):** English is default; dynamically switchable to Vietnamese via option `9`.
2. **Drag & Drop Path Cleaning (`PathHelper.cs`):** Automatically removes enclosing single quotes `'` and double quotes `"` created when dragging files from macOS Finder or Windows File Explorer.
3. **Graceful Error Handling:** Iterative processors wrap each file in individual `try...catch` blocks to prevent batch aborts on corrupt files.

---

## 5. Adding New Features
To add a new feature (e.g. Feature 6):
1. Create `Feature6_{Name}.cs` in the `ParamToolbox` namespace.
2. Define `public static void Execute()`.
3. Register the option in `Program.cs` under the interactive menu loop.
