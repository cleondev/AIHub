# AIHub

Tài liệu mô tả hệ thống (MVP) nằm trong thư mục `docs/`:
- [Tổng quan](docs/01-tong-quan.md)
- [Kiến trúc](docs/02-kien-truc.md)
- [Mô hình dữ liệu](docs/03-mo-hinh-du-lieu.md)
- [API MVP](docs/04-api-mvp.md)
- [Tài liệu chi tiết hệ thống](docs/05-tai-lieu-chi-tiet-he-thong.md)
- [Thiết kế điều chỉnh cho kịch bản FoxTS](docs/07-thiet-ke-foxts-enterprise.md)

## Cấu trúc tách module/project
Solution đã tách thành các project riêng biệt:
- `AIHub.Modules.Management`: quản lý tri thức, cấu hình LLM (mặc định minimax), thông tin tool.
- `AIHub.Modules.ChatBox`: xử lý hội thoại, gọi module quản lý + tool.
- `AIHub.Modules.MockApi`: API giả lập với flow `query/create/approve`.
- `AIHub.Modules.Tooling`: module tool để đọc/ghi sang mock API.
- `AIHub.Modules.SemanticKernel`: orchestration runtime trung tâm (envelope, policy guards, plugin contract).
- `AIHub.Api`: host Web API + dashboard để chạy kịch bản end-to-end.

## Kịch bản demo mong muốn
1. Mở Management, thêm khái niệm `abc`.
2. Qua ChatBox nhập `liệt kê abc` để AI trả danh sách tri thức.
3. Nhập `tạo def` để tạo request trạng thái `Pending`.
4. Approve request bằng chat `approve <request-id>` hoặc nút approve để chuyển `Approved`.


## Bật chat AI thật với Minimax
Để ChatBox phản hồi bằng AI thật (thay vì chỉ rule-based), cấu hình biến môi trường trước khi chạy API:
- `MINIMAX_API_KEY`: API key từ Minimax.
- `MINIMAX_GROUP_ID`: Group ID trong tài khoản Minimax.
- `MINIMAX_MODEL` (tuỳ chọn): mặc định `abab6.5s-chat`.
- `MINIMAX_BASE_URL` (tuỳ chọn): mặc định `https://api.minimax.chat`.

Ví dụ:
```bash
export MINIMAX_API_KEY=your_key
export MINIMAX_GROUP_ID=your_group_id
export MINIMAX_MODEL=abab6.5s-chat
dotnet run --project src/AIHub.Api
```

Luồng xử lý chat hiện tại:
1. Ưu tiên các lệnh nghiệp vụ (`liệt kê`, `tạo`, `approve`) như trước.
2. Nếu không khớp rule, hệ thống gọi Minimax để trả lời realtime.


## Demo gọi API bằng LLM (Minimax)
Sau khi nhập `API key` + `Group ID` ở tab **Quản lý** (hoặc set env), ChatBox có thể tự gọi API nội bộ:
- Hỏi danh sách sản phẩm: `liệt kê sản phẩm category accessories`
- Lọc theo tên/category: `xem sản phẩm name chuột category accessories`
- Tạo order: `tạo order productId <guid> quantity 1`

LLM sẽ thực hiện tool-call đến mock API:
- `list_products(keyword,name,category)`
- `create_order(productId,quantity)`

## Aspire orchestration (chatbot + product API)
- `AIHub.AppHost`: Aspire host để chạy 2 service riêng biệt:
  - `AIHub.Api` (chat bot service)
  - `AIHub.ProductApi` (product/order API service)
- `AIHub.ProductApi` đã bật Swagger tại `/swagger`.

Các API chính của `AIHub.ProductApi`:
- Nhóm `products`:
  - `GET /api/products`
  - `GET /api/products/{id}`
  - `POST /api/products`
  - `PUT /api/products/{id}`
- Nhóm `orders`:
  - `GET /api/orders`
  - `GET /api/orders/{id}`
  - `POST /api/orders`
  - `PUT /api/orders/{id}`
