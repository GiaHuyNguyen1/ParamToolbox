# 📋 QA & QC TESTER USER GUIDE - PARAMTOOLBOX

🌐 Language: **English** | [Tiếng Việt](GUIDE_FOR_QA.vi.md)

This handbook is designed specifically for QA / QC / BA team members to inspect, consolidate, and audit configuration templates without needing any programming knowledge or runtime installation.

---

## 🚀 1. Download & Launch (Takes Under 1 Minute)

### Step 1: Download the Package for Your OS
Go to the [GitHub Releases](https://github.com/GiaHuyNguyen1/ParamToolbox/releases) page and download the ZIP file for your machine:
- **Windows PC**: Download `ParamToolbox_Win64.zip`
- **Mac with Apple Silicon (M1/M2/M3/M4 chip)**: Download `ParamToolbox_MacArm64.zip`
- **Mac with Intel chip**: Download `ParamToolbox_MacX64.zip`

### Step 2: Extract & Run
- **On Windows:**
  1. Extract the `.zip` file.
  2. Double-click **`Run_Tool.bat`** (or `ParamToolbox.exe`). The command window will open.
- **On macOS:**
  1. Extract the `.zip` file.
  2. Double-click **`Run_Tool.command`**.
  3. *(If macOS displays a security prompt preventing launch)*: Open Terminal, type `xattr -d com.apple.quarantine ` (with a trailing space), drag the `ParamToolbox` file into the Terminal window, and press Enter. Then run the tool normally.

> [!TIP]
> **Drag & Drop Paths:** When prompted for file or folder paths, you **do not need to type paths manually**. Simply drag and drop the folder or file from Finder or File Explorer directly into the Terminal window and press **Enter**. Quotes are automatically handled.

> [!NOTE]
> **Language Toggle:** The tool defaults to **English**. If you prefer Vietnamese, simply press **`9`** on the main menu to toggle languages at any time.

---

## 🎯 2. Primary Features for QA / Testers

### 🌟 Feature 2: Consolidate XML & JSON into a CSV Matrix
- **When to use:** When auditing parameters across multiple merchants, checking default values, and distinguishing Text (`T`) vs Password (`P`) fields.
- **How to use:**
  1. From the main menu, press **`2`** and hit Enter.
  2. **XML folder:** Drag & drop the directory containing template XML files (e.g. `Samples/XMLs`).
  3. **JSON folder:** Drag & drop the directory containing configuration JSON files (e.g. `Samples/JSONs`).
  4. **Output CSV file:** Enter a destination path (e.g. `Desktop/Merchant_Audit.csv`) or press Enter to save to Desktop automatically.
  5. **Merchant count:** Enter the number of merchants to consolidate (e.g. `6`) and hit Enter.
- **Result:** Open the generated `.csv` in Excel to filter, inspect, and verify parameter consistency.

---

### 🌟 Feature 5: Export XML Metadata to Styled Excel (.xlsx)
- **When to use:** When you need a comprehensive, hierarchical breakdown of all template attributes (GroupID, GroupTitle, DisplayStyle, DataType, Readonly, Required, InputType, etc.).
- **How to use:**
  1. From the main menu, press **`5`** and hit Enter.
  2. **XML folder:** Drag & drop the folder containing your template XML files.
  3. **Output Excel file:** Specify the destination path (e.g. `Desktop/template-parameter-metadata.xlsx`).
- **Result:** Generates an Excel spreadsheet with:
  - Clean navy blue headers with bold white text.
  - Pre-activated **Auto-Filter** on all columns for filtering by Group or DataType.
  - **Freeze Panes** on the header row so column names stay visible while scrolling large datasets.

---

### 🌟 Feature 3: Clone Merchant XML Templates for Test Automation
- **When to use:** When preparing test fixtures for load or multi-merchant testing (generating `merchant_1.xml` through `merchant_30.xml`).
- **How to use:**
  1. Press **`3`** and hit Enter.
  2. Drag & drop the base merchant template file (e.g. `SingleApp_merchant_1.xml`).
  3. Enter output directory and total quantity to generate (e.g. `30`).
- **Result:** Automatically increments group IDs, field IDs, and PIDs, packaging them into neat ZIP batches (10 files per zip).

---

## ❓ 3. Troubleshooting & FAQs

| Issue | Cause | Solution |
| :--- | :--- | :--- |
| **"Error: Folder does not exist!"** | Path was mistyped or directory moved. | Use **drag & drop** directly from Finder/Explorer into Terminal. |
| **"No valid XML files found"** | Directory has no `.xml` files or only hidden files (starting with `.`). | Verify that the folder contains unhidden `.xml` files. |
| **Vietnamese characters garbled in CSV** | Excel opened CSV without UTF-8 recognition. | The tool writes UTF-8 with BOM. If your Excel version fails to detect it, open Excel -> **Data** tab -> **From Text/CSV** and select **UTF-8** encoding. |
| **Window closes immediately on Mac** | Execution permission missing. | Run `chmod +x ParamToolbox Run_Tool.command` in Terminal. |
