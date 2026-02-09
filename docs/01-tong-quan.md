# Tổng quan hệ thống AIHub (MVP)

## Mục tiêu
AIHub là platform AI cho doanh nghiệp với hai demo tối thiểu:
1. **Quản lý tri thức nội bộ & ngôn ngữ chuyên ngành**: thu thập/chuẩn hoá tri thức; AI đọc hiểu và trả lời truy vấn dựa trên nguồn tri thức đã được phê duyệt.
2. **AI đọc hiểu API & tạo dữ liệu + Human-in-the-loop**: AI hiểu mô tả API, gọi API tạo dữ liệu; dữ liệu do AI tạo phải được người phê duyệt trước khi ghi chính thức.

## Giá trị
- Tập trung hoá tri thức doanh nghiệp, giảm rủi ro rò rỉ.
- Tăng tốc tạo dữ liệu mẫu/đầu vào từ AI nhưng vẫn đảm bảo kiểm soát.
- Dễ mở rộng theo module và kết nối hệ thống hiện hữu.

## Phạm vi demo (MVP)
- **Kho tri thức**: tài liệu dạng PDF/DOCX/HTML/CSV, glossary thuật ngữ chuyên ngành.
- **Tìm kiếm & hỏi đáp**: RAG trên kho tri thức; trích dẫn nguồn.
- **Quản lý API**: đăng ký API với schema (OpenAPI/JSON Schema) và mô tả nghiệp vụ.
- **Công cụ tạo dữ liệu**: AI đề xuất dữ liệu, tạo bản nháp và gửi workflow phê duyệt.
- **Quy trình phê duyệt**: người duyệt xem bản nháp, sửa/đồng ý/huỷ.
- **Rule & Policy**: cấu hình phạm vi trả lời, hạn chế nội dung ngoài miền tri thức.
- **Quản lý model**: cấu hình và lựa chọn LLM linh hoạt theo use-case.

## Persona chính
- **Knowledge Curator**: quản lý tri thức, glossary, phân quyền tài liệu.
- **Business User**: đặt câu hỏi và nhận câu trả lời.
- **Data Approver**: xem xét & phê duyệt dữ liệu do AI tạo.
- **Admin/IT**: cấu hình hệ thống, API, logging.

## Kiến trúc tổng quan (mô tả ngắn)
- **Frontend**: Web portal (React + TypeScript) cho người dùng, curator, approver.
- **Backend**: .NET 10 (Minimal API hoặc ASP.NET Core) cung cấp API nội bộ.
- **LLM Gateway**: module tích hợp LLM, quản lý prompt, token, caching.
- **Policy Engine**: rule phạm vi tri thức, kiểm soát nội dung trả lời.
- **Data Layer**: PostgreSQL + pgvector + PostgREST.
- **Ingestion Service**: xử lý tài liệu, tách đoạn, embedding.
- **Approval Service**: workflow phê duyệt dữ liệu do AI tạo.

## Phi chức năng
- **Bảo mật**: RBAC, audit log, mã hoá dữ liệu nhạy cảm.
- **Giám sát**: logging, tracing, metrics.
- **Khả dụng**: hỗ trợ triển khai on-prem hoặc cloud.
