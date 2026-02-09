# Kiến trúc chi tiết

## Sơ đồ thành phần (textual)
```
[Web Portal]
   | REST/GraphQL
[.NET 10 API Gateway]
   |-- Auth/RBAC
   |-- Policy Engine (scope rules)
   |-- Knowledge Service
   |-- Agent Tooling Service
   |-- Approval Service
   |-- Audit/Observability
   |
   |--> [LLM Gateway]
   |       |-- Model Router (OpenAI/Azure/Local)
   |       |-- Model Registry (per use-case)
   |       |-- Prompt Registry
   |       |-- Response Cache
   |
   |--> [PostgreSQL + pgvector]
   |       |-- PostgREST (data API)
   |       |-- Row-level Security
   |
   |--> [Object Storage]
   |       |-- raw documents
```

## Luồng 1: Quản lý tri thức & hỏi đáp
1. Curator tải tài liệu lên (PDF/DOCX/CSV).
2. Ingestion Service trích xuất nội dung, chia chunk, gắn metadata.
3. Tạo embedding, lưu vào Postgres (pgvector).
4. Người dùng đặt câu hỏi, hệ thống truy vấn vector + lọc theo quyền.
5. Policy Engine kiểm tra phạm vi, từ chối câu hỏi ngoài domain nếu cần.
6. LLM trả lời kèm trích dẫn nguồn (citations).

## Luồng 2: AI đọc hiểu API & tạo dữ liệu có phê duyệt
1. Admin đăng ký API + schema (OpenAPI/JSON Schema) + mô tả nghiệp vụ.
2. Người dùng yêu cầu tạo dữ liệu (ví dụ: tạo khách hàng mẫu).
3. Policy Engine xác thực request theo rule và giới hạn phạm vi.
4. Agent Tooling Service dùng LLM đọc schema, sinh dữ liệu.
5. Dữ liệu được tạo ở trạng thái *draft*.
6. Approver xem, chỉnh sửa, phê duyệt.
7. Khi phê duyệt, gọi API thật để tạo dữ liệu chính thức.

## Các dịch vụ chính
- **Knowledge Service**: tài liệu, glossary, embedding, search.
- **Agent Tooling Service**: gọi API bên thứ 3, tạo dữ liệu.
- **Approval Service**: workflow phê duyệt, lưu lịch sử.
- **LLM Gateway**: quản lý mô hình, prompt, rate-limit.
- **Policy Engine**: quản lý rule/policy trả lời và phạm vi.

## Công nghệ đề xuất
- **Backend**: .NET 10, ASP.NET Core Minimal API.
- **Database**: PostgreSQL + pgvector.
- **API Layer**: PostgREST cho dữ liệu read-heavy.
- **Frontend**: React + Vite + Tailwind.
- **Observability**: OpenTelemetry + Grafana/Prometheus.
- **Policy** (tuỳ chọn): OPA (Open Policy Agent) hoặc custom rules engine.
- **Queue** (tuỳ chọn): RabbitMQ/Redis Stream cho ingestion.
