# API MVP (đề xuất)

## Knowledge
- `POST /knowledge/documents` upload tài liệu
- `GET /knowledge/documents` danh sách tài liệu
- `POST /knowledge/query` hỏi đáp (RAG)

## Glossary
- `POST /glossary/terms`
- `GET /glossary/terms`

## API Catalog
- `POST /api-catalog` đăng ký API
- `GET /api-catalog` danh sách API

## AI Data Generation
- `POST /ai/data-generation` tạo bản nháp dữ liệu từ AI
- `GET /ai/data-generation/{id}` xem trạng thái

## Policy & Model
- `GET /policies` danh sách rule/policy
- `POST /policies` tạo rule/policy
- `GET /models` danh sách model profile
- `POST /models` tạo/cập nhật model profile

## Approval
- `POST /approval/{id}/approve` phê duyệt
- `POST /approval/{id}/reject` từ chối

## Lưu ý
- Các endpoint trả về `trace_id` để audit.
- Các API quan trọng yêu cầu RBAC và audit log.
