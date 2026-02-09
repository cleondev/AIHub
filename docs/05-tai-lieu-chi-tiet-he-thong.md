# Tài liệu chi tiết hệ thống AIHub (MVP)

## 1. Mục đích và phạm vi
AIHub là nền tảng AI cho doanh nghiệp tập trung vào 2 nhóm năng lực chính:
1. **Quản lý tri thức nội bộ & hỏi đáp theo nguồn đã phê duyệt**.
2. **AI đọc hiểu API & tạo dữ liệu có quy trình phê duyệt (human-in-the-loop)**.

Phạm vi tài liệu này mở rộng các phần trong `01-tong-quan.md` và `02-kien-truc.md` để mô tả chi tiết yêu cầu, hành vi, và cấu hình MVP.

## 2. Đối tượng sử dụng và quyền hạn
### 2.1 Vai trò
- **Knowledge Curator**: upload tài liệu, quản lý glossary, thiết lập mức truy cập.
- **Business User**: tìm kiếm tri thức, hỏi đáp.
- **Data Approver**: duyệt dữ liệu do AI tạo (draft).
- **Admin/IT**: cấu hình hệ thống, quản lý API, model profile, policy, audit.

### 2.2 Ma trận quyền cơ bản
| Hành động | Curator | Business User | Approver | Admin |
| --- | --- | --- | --- | --- |
| Upload tài liệu | ✅ | ❌ | ❌ | ✅ |
| Hỏi đáp (RAG) | ✅ | ✅ | ✅ | ✅ |
| Tạo glossary | ✅ | ❌ | ❌ | ✅ |
| Đăng ký API | ❌ | ❌ | ❌ | ✅ |
| Tạo dữ liệu AI | ✅ | ✅ | ❌ | ✅ |
| Phê duyệt dữ liệu | ❌ | ❌ | ✅ | ✅ |
| Cấu hình policy/model | ❌ | ❌ | ❌ | ✅ |

## 3. Thành phần hệ thống và trách nhiệm
### 3.1 Web Portal
- Dashboard theo vai trò.
- Trang quản lý tri thức, glossary, và trạng thái ingestion.
- Trang hỏi đáp và lịch sử truy vấn.
- Trang phê duyệt dữ liệu (diff, chỉnh sửa trực tiếp trước khi approve).

### 3.2 API Gateway (.NET 10)
- Xác thực/ phân quyền (JWT + RBAC).
- Gắn `trace_id` cho mọi request.
- Thực thi policy trước khi gọi LLM.
- Điều phối đến các service chuyên biệt.

### 3.3 Knowledge Service
- CRUD tài liệu/ glossary.
- Tạo chunk/ embedding.
- Truy vấn semantic search (pgvector).

### 3.4 Agent Tooling Service
- Đọc schema OpenAPI/JSON Schema.
- Sinh dữ liệu theo template, constraint, và rule.
- Chuẩn hóa dữ liệu theo yêu cầu API.

### 3.5 Approval Service
- Quản lý trạng thái: `draft → approved/rejected`.
- Lưu lịch sử chỉnh sửa, lý do từ chối.

### 3.6 LLM Gateway
- Model registry, prompt registry, cache.
- Thống nhất quota/rate-limit.
- Hỗ trợ nhiều nhà cung cấp model.

## 4. Luồng nghiệp vụ chi tiết
### 4.1 Upload & Ingestion tri thức
1. Curator upload tài liệu.
2. Hệ thống validate định dạng và scan bảo mật (nếu có).
3. Ingestion service tách đoạn, gắn metadata: nguồn, phòng ban, tag.
4. Tạo embedding và lưu `knowledge_chunks`.
5. Trạng thái ingestion hiển thị trên portal.

### 4.2 Hỏi đáp (RAG)
1. Business User gửi câu hỏi.
2. Policy Engine kiểm tra scope và quyền truy cập.
3. Query vector + lọc theo metadata + RLS.
4. LLM trả lời, kèm citations (document + đoạn).
5. Log truy vấn vào `audit_logs`.

### 4.3 Đăng ký API & sinh dữ liệu
1. Admin đăng ký API và schema.
2. Người dùng yêu cầu tạo dữ liệu (số lượng, constraints, tiêu chí).
3. LLM phân tích schema, sinh payload `draft`.
4. Draft được lưu vào `ai_generated_records`.
5. Approver xem, chỉnh sửa, và approve.
6. Khi approve, hệ thống gọi API thật và cập nhật trạng thái.

## 5. Mô hình dữ liệu mở rộng (gợi ý)
### 5.1 Bảng bổ sung
- **document_ingestion_jobs**: theo dõi trạng thái ingestion.
  - `id`, `document_id`, `status`, `started_at`, `finished_at`, `error_message`
- **query_sessions**: lịch sử hỏi đáp.
  - `id`, `user_id`, `question`, `answer`, `citations`, `created_at`
- **approval_comments**: ý kiến người duyệt.
  - `id`, `record_id`, `comment`, `created_by`, `created_at`

### 5.2 Quy ước metadata cho chunk
- `source_file`, `page_number`, `section_title`, `tags`, `department`.

## 6. Chính sách & bảo mật
### 6.1 Policy Engine
- Rule dạng: allow/deny theo keyword, tag, department, hoặc regex.
- Ví dụ: từ chối câu hỏi ngoài domain tài chính.

### 6.2 Bảo mật dữ liệu
- Mã hóa dữ liệu nhạy cảm ở DB (column-level encryption).
- Row-level Security theo đội nhóm.
- Audit log cho: upload, query, approval.

## 7. Quan sát & vận hành
- Log tập trung: request/response + trace_id.
- Metrics: số lượng query, thời gian phản hồi, tỉ lệ approve.
- Alert khi ingestion fail, hoặc API schema lỗi.

## 8. SLA & giới hạn
- Thời gian phản hồi query: mục tiêu < 5s.
- Số lượng chunk tối đa trên query: 10–20.
- Giới hạn kích thước file upload: 50–100MB.

## 9. API chi tiết (ví dụ)
### 9.1 `POST /knowledge/documents`
**Request** (multipart)
- `file`: PDF/DOCX/CSV
- `tags`: string[]

**Response**
```
{
  "id": "uuid",
  "status": "uploaded",
  "trace_id": "trace-123"
}
```

### 9.2 `POST /knowledge/query`
**Request**
```
{
  "question": "Chính sách hoàn tiền là gì?",
  "top_k": 5
}
```

**Response**
```
{
  "answer": "...",
  "citations": [
    {"document_id": "uuid", "chunk_id": "uuid"}
  ],
  "trace_id": "trace-456"
}
```

### 9.3 `POST /ai/data-generation`
**Request**
```
{
  "api_catalog_id": "uuid",
  "quantity": 10,
  "constraints": {
    "country": "VN"
  }
}
```

**Response**
```
{
  "record_id": "uuid",
  "status": "draft",
  "trace_id": "trace-789"
}
```

## 10. Roadmap mở rộng (ngoài MVP)
- Versioning tài liệu và schema.
- Agent tool để tự động cập nhật glossary.
- Đánh giá chất lượng câu trả lời (feedback loop).
- Auto test API payload trước khi gửi thật.
