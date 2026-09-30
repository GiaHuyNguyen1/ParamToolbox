# 📋 CẨM NANG HƯỚNG DẪN SỬ DỤNG DÀNH CHO QA / QC TESTER

Tài liệu này dành riêng cho các bạn Tester/QA/BA để kiểm tra, xuất dữ liệu và thẩm định tham số cấu hình mà **hoàn toàn không cần biết lập trình hay cài đặt môi trường phức tạp**.

---

## 🚀 1. Tải và Khởi động Tool (Trong 1 phút)

### Bước 1: Tải bộ cài đặt phù hợp với máy của bạn
Truy cập vào mục **Releases** trên GitHub của dự án và tải file `.zip` tương ứng:
- Máy tính **Windows**: Tải file `ParamToolbox_Win64.zip`
- Máy Mac chip **Apple Silicon (M1/M2/M3/M4)**: Tải file `ParamToolbox_MacArm64.zip`
- Máy Mac chip **Intel**: Tải file `ParamToolbox_MacX64.zip`

### Bước 2: Giải nén và Chạy
- **Trên Windows:**
  1. Giải nén file `.zip`.
  2. Click đúp vào file **`Run_Tool.bat`** (hoặc `ParamToolbox.exe`). Cửa sổ công cụ màu đen sẽ hiện ra.
- **Trên macOS:**
  1. Giải nén file `.zip`.
  2. Click đúp vào file **`Run_Tool.command`**.
  3. *(Nếu Mac báo file không mở được do bảo mật)*: Mở Terminal, gõ `xattr -d com.apple.quarantine ` rồi kéo file `ParamToolbox` vào nhấn Enter. Sau đó mở lại bình thường.

> [!TIP]
> **Mẹo kéo thả (Drag & Drop):** Khi tool yêu cầu nhập đường dẫn thư mục hoặc file, bạn **chỉ cần kéo và thả (drag & drop)** thư mục đó từ màn hình máy tính vào thẳng cửa sổ dòng lệnh rồi nhấn **Enter**. Không cần phải gõ tay đường dẫn!

---

## 🎯 2. Các Tính năng QA thường dùng nhất

### 🌟 Tính năng 2: Tổng hợp XML và JSON xuất ra file CSV
- **Khi nào dùng:** Khi cần kiểm tra ma trận tham số cấu hình giữa các Merchant, đối chiếu xem tham số nào có giá trị mặc định, tham số nào là dạng Password (`P`) hay Text (`T`).
- **Cách thao tác:**
  1. Tại Menu chính, gõ số **`2`** rồi nhấn Enter.
  2. **Thư mục XML:** Kéo thả folder chứa các file XML template (ví dụ `Samples/XMLs`).
  3. **Thư mục JSON:** Kéo thả folder chứa các file JSON cấu hình (ví dụ `Samples/JSONs`).
  4. **Tên file xuất CSV:** Gõ tên file muốn lưu (ví dụ: `Desktop/KiemTra_Merchant.csv`) hoặc bấm Enter để tool tự lưu ra Desktop.
  5. **Số lượng Merchant:** Nhập số lượng Merchant cần kiểm tra (ví dụ: `6`), bấm Enter.
- **Kết quả:** Mở file `.csv` bằng Excel để lọc và đối chiếu số liệu.

---

### 🌟 Tính năng 5: Xuất toàn bộ Metadata XML ra file Excel (.xlsx)
- **Khi nào dùng:** Khi cần xem cấu trúc phân cấp chi tiết nhất của template XML (GroupID, Title, DisplayStyle, DataType, Readonly, Required, InputType...).
- **Cách thao tác:**
  1. Tại Menu chính, gõ số **`5`** rồi nhấn Enter.
  2. **Thư mục template XML:** Kéo thả folder chứa các file XML.
  3. **Đường dẫn file Excel xuất ra:** Nhập đường dẫn (ví dụ: `Desktop/template-parameter-metadata.xlsx`).
- **Kết quả:** Tool xuất ra 1 file Excel chuẩn hóa:
  - Dòng tiêu đề có màu xanh dương chuyên nghiệp, chữ trắng in đậm.
  - Đã bật sẵn **Auto-Filter** trên tất cả các cột để bạn dễ dàng lọc theo từng Group hoặc DataType.
  - Đã cố định dòng tiêu đề (Freeze Pane) để khi cuộn hàng trăm tham số vẫn thấy tên cột.

---

### 🌟 Tính năng 3: Nhân bản Merchant Template XML phục vụ Test tải/Test nhiều Merchant
- **Khi nào dùng:** Khi cần tạo ra hàng loạt template Merchant mẫu (`merchant_1` đến `merchant_30`) để nạp vào hệ thống kiểm thử tự động.
- **Cách thao tác:**
  1. Gõ số **`3`**, bấm Enter.
  2. Kéo thả file XML mẫu (ví dụ: `SingleApp_merchant_1.xml`).
  3. Nhập thư mục xuất và tổng số lượng cần tạo (ví dụ: `30`).
- **Kết quả:** Tool tự động tăng tiến các ID và đóng gói sẵn thành các file `.zip` (mỗi file 10 template).

---

## ❓ 3. Các lỗi thường gặp và cách xử lý (Troubleshooting)

| Lỗi | Nguyên nhân | Cách xử lý |
| :--- | :--- | :--- |
| **"Folder không tồn tại"** | Đường dẫn gõ sai hoặc thư mục đã bị xóa/đổi tên. | Hãy sử dụng thao tác **kéo thả** folder vào cửa sổ terminal để đảm bảo đường dẫn chính xác 100%. |
| **Không tìm thấy file XML nào** | Thư mục chọn không chứa file `.xml` hoặc các file bắt đầu bằng dấu chấm `.` (file ẩn). | Kiểm tra lại xem folder đã giải nén chưa và file có đuôi `.xml` hay không. |
| **Font chữ tiếng Việt trong CSV bị lỗi khi mở bằng Excel** | Excel chưa nhận diện UTF-8. | Tool đã tích hợp sẵn chuẩn UTF-8 có BOM. Nếu máy bạn bị lỗi font, hãy mở Excel -> vào tab **Data** -> chọn **From Text/CSV** và chọn File Origin là **UTF-8**. |
| **Cửa sổ tắt ngay khi bấm đúp** | Thiếu quyền thực thi trên macOS. | Chạy lệnh `chmod +x ParamToolbox Run_Tool.command` trong Terminal. |
