# AIHub

Tài liệu mô tả hệ thống (MVP) nằm trong thư mục `docs/`:
- [Tổng quan](docs/01-tong-quan.md)
- [Kiến trúc](docs/02-kien-truc.md)
- [Mô hình dữ liệu](docs/03-mo-hinh-du-lieu.md)
- [API MVP](docs/04-api-mvp.md)
- [Tài liệu chi tiết hệ thống](docs/05-tai-lieu-chi-tiet-he-thong.md)

## Cấu trúc tách module/project
Solution đã tách thành các project riêng biệt:
- `AIHub.Modules.Management`: quản lý tri thức, cấu hình LLM (mặc định minimax), thông tin tool.
- `AIHub.Modules.ChatBox`: xử lý hội thoại, gọi module quản lý + tool.
- `AIHub.Modules.MockApi`: API giả lập với flow `query/create/approve`.
- `AIHub.Modules.Tooling`: module tool để đọc/ghi sang mock API.
- `AIHub.Api`: host Web API + dashboard để chạy kịch bản end-to-end.

## Kịch bản demo mong muốn
1. Mở Management, thêm khái niệm `abc`.
2. Qua ChatBox nhập `liệt kê abc` để AI trả danh sách tri thức.
3. Nhập `tạo def` để tạo request trạng thái `Pending`.
4. Approve request bằng chat `approve <request-id>` hoặc nút approve để chuyển `Approved`.
