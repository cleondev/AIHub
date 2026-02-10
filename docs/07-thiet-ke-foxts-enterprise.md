# Thiết kế điều chỉnh cho kịch bản doanh nghiệp FoxTS

## 1) Phân tích lại bài toán
Kịch bản của FoxTS không chỉ là chatbot hỏi đáp, mà là một hệ thống **AI orchestration có kiểm soát** gồm 3 trục chính:

1. **Knowledge/Data Hub**
   - Khai báo nhiều kho tri thức khác nhau.
   - Phân loại tri thức theo **tính chất**, **vòng đời**, **nguồn sinh ra**.
   - Import dữ liệu thành tri thức, tự động đề xuất tri thức mới từ tài liệu.
   - Tri thức do AI đề xuất phải có quy trình **human approval**.
   - Tri thức có TTL/hết hạn, cần cơ chế tái xác thực hoặc archive.

2. **Tool/Skill Hub**
   - Khai báo và quản lý skill/tool theo miền nghiệp vụ (query sản phẩm, đặt hàng...).
   - Tool cần metadata: risk level, quyền chạy, timeout, idempotency.
   - Tool có version + chính sách phê duyệt trước khi chạy các hành động ghi.

3. **Chatbot Studio + Runtime**
   - Tạo chatbot theo từng domain (ví dụ: FoxTS Assistance).
   - Chọn nhiều nguồn tri thức + model + skill để chạy.
   - Hỗ trợ cả **Q&A** lẫn **Command execution**.
   - Command gọi API bên ngoài phải có **approval gate**.

=> Vì vậy, kiến trúc nên chuyển từ "chatbot + vài endpoint" sang mô hình **governed AI platform**.

---

## 2) Thiết kế mục tiêu (Target Architecture)

## 2.1. Logical Architecture
```text
[Portal/Admin UI]
  ├─ Knowledge Console
  ├─ Tool/Skill Console
  ├─ Chatbot Studio
  └─ Approval Center
           |
           v
[AI Gateway / BFF]
  ├─ AuthN/AuthZ + RBAC + Tenant Context
  ├─ Policy Decision Point
  ├─ Request/Response Redaction
  └─ Audit Correlation
           |
           v
[Orchestrator Runtime]
  ├─ Intent Classifier (Q&A | Action | Clarification)
  ├─ Planner (tool chain + guardrails)
  ├─ Tool Router (allowlist, timeout, retry)
  ├─ Approval Hook (pre-action)
  └─ Citation + Trace Builder
     |                 |
     |                 +----> [Observability: Logs/Trace/Metrics]
     v
[Knowledge Services]        [Tool Services]
  ├─ Ingestion pipeline      ├─ Product Query Skill
  ├─ Classification engine   ├─ Order Draft Skill
  ├─ Vector + keyword store  ├─ External API connectors
  ├─ TTL lifecycle jobs      └─ Risk control policies
  └─ Glossary service

[Workflow Services]
  ├─ Approval requests
  ├─ 4-eyes approval
  └─ SLA/escalation
```

## 2.2. Domain module đề xuất

### A. Knowledge/Data Domain
Các thực thể cốt lõi:
- `KnowledgeSpace`: một kho tri thức theo domain (FoxTS Manual, Product Policy...).
- `KnowledgeItem`: đơn vị tri thức đã chuẩn hoá.
- `KnowledgeClass`: phân loại theo:
  - `Nature`: Structured / Unstructured / Procedural / Glossary.
  - `Lifecycle`: Draft / Active / Expired / Archived.
  - `Origin`: HumanAuthored / Imported / AIProposed / APIIngested.
- `KnowledgeVersion`: quản lý phiên bản tri thức.
- `KnowledgeValidity`: `effective_from`, `expires_at`, `review_after`.

Quy tắc vận hành:
- Tri thức `AIProposed` mặc định vào trạng thái `PendingApproval`.
- Tri thức quá hạn (`expires_at`) không được dùng để trả lời nếu policy yêu cầu "fresh-only".
- Tri thức procedural (ví dụ "vét kho") nên đưa vào glossary + intent expansion để chatbot hiểu ngữ nghĩa nghiệp vụ.

### B. Tool/Skill Domain
Các thực thể:
- `SkillDefinition`: mô tả skill, input/output schema, owner.
- `ToolEndpoint`: thông tin API đích, auth, rate limit.
- `SkillPolicy`:
  - `risk_level`: low/medium/high.
  - `approval_required`: true/false.
  - `allowed_roles`.
  - `dry_run_supported`.
- `SkillVersion`: versioned rollout để tránh breaking.

Quy tắc:
- Skill query (chỉ đọc) có thể auto-run.
- Skill tạo đơn hàng/ghi dữ liệu luôn chạy theo 2 bước: `simulate/query` -> `create_draft` -> `human_approve`.

### C. Chatbot Domain
Các thực thể:
- `ChatbotProfile`: tên bot, persona, policy set.
- `ModelProfile`: provider/model, cost/latency profile.
- `BotKnowledgeBinding`: bot gắn 1..n knowledge spaces.
- `BotSkillBinding`: bot gắn 1..n skills.
- `ConversationSession`: lưu trace hội thoại + tool calls + approval links.

Quy tắc:
- Mỗi response phải có `response_mode` (direct answer / tool result / awaiting approval).
- Mọi action phải trả về `approval_link` khi cần người duyệt.

---

## 3) Luồng nghiệp vụ đã điều chỉnh cho kịch bản FoxTS

## 3.1. Khởi tạo hệ thống
1. Admin tạo `KnowledgeSpace = FoxTS_UserGuide`.
2. Import tài liệu hướng dẫn sử dụng hệ thống mua sắm FoxTS.
3. Ingestion pipeline tách chunk + phân loại tri thức.
4. Các tri thức mới/chuyên ngành được AI đề xuất vào hàng chờ duyệt.
5. Curator duyệt để kích hoạt tri thức.

## 3.2. Khai báo skill
- `Skill_QueryProductFoxTS` (read-only): gọi API sản phẩm/tồn kho.
- `Skill_CreateOrderFoxTS` (write): luôn tạo đơn nháp + yêu cầu duyệt.

## 3.3. Tạo chatbot
- Tạo bot `FoxTS Assistance`.
- Bind:
  - Knowledge: `FoxTS_UserGuide`, `FoxTS_Glossary`.
  - Skills: query sản phẩm, tạo order.
  - Model: `MiniMax`.

## 3.4. Runtime chat (chuẩn hoá)
- Câu hỏi định danh bot: trả lời từ persona.
- Câu hỏi thông tin sản phẩm: gọi `Skill_QueryProductFoxTS`.
- Câu lệnh đặt hàng: kiểm tra tồn kho trước, sau đó tạo draft order nếu hợp lệ.
- Câu có thuật ngữ nghiệp vụ ("vét kho"): resolve qua glossary -> chuyển thành intent mua toàn bộ tồn kho -> tạo draft + approval link.

---

## 4) Điều chỉnh kỹ thuật quan trọng

1. **Tách rõ Query vs Command**
   - Query: cho phép thực thi trực tiếp.
   - Command: luôn đi qua approval workflow nếu có side effect.

2. **Bổ sung Approval-as-a-Service**
   - Dùng chung cho cả:
     - Tri thức AI đề xuất.
     - Command do chatbot khởi tạo (đặt hàng, cập nhật dữ liệu).

3. **Áp dụng policy theo ngữ cảnh hội thoại**
   - Policy gắn theo bot + role user + risk skill.
   - Chặn execute nếu thiếu quyền hoặc thiếu approval.

4. **Kiểm soát vòng đời tri thức (Knowledge TTL)**
   - Scheduler đánh dấu sắp hết hạn.
   - Expired knowledge không tham gia RAG (hoặc bị giảm trọng số theo policy).

5. **Observability bắt buộc**
   - Lưu: prompt hash, retrieved docs, tool calls, approval state, latency/cost.
   - Bắt buộc correlation id xuyên suốt từ chat -> tool -> approval.

---

## 5) Lộ trình triển khai đề xuất

### Phase 1 (MVP+)
- Chuẩn hoá model dữ liệu Knowledge/Skill/Approval.
- Bổ sung `Origin`, `Lifecycle`, `expires_at` cho knowledge.
- Tạo skill registry + bot binding.
- Cứng hoá flow `query -> draft -> approve` cho order.

### Phase 2
- Thêm background agent phát hiện tri thức mới từ import.
- Nâng cấp planner để hỗ trợ multi-step tool chain.
- SLA/escalation cho approval quá hạn.

### Phase 3
- Multi-tenant isolation + quota.
- Cost-aware model routing.
- Policy-as-code (OPA/Rego), compliance reporting.

---

## 6) Checklist chấp nhận theo đúng kịch bản người dùng
- [ ] Tạo được kho tri thức FoxTS và import tài liệu.
- [ ] Tự động phát hiện tri thức mới/chuyên ngành và tạo request phê duyệt.
- [ ] Khai báo được 2 skill: query sản phẩm, đặt hàng.
- [ ] Tạo chatbot FoxTS Assistance, bind model MiniMax + knowledge + skills.
- [ ] Chatbot trả lời đúng Q&A và gọi API query.
- [ ] Chatbot tạo order ở chế độ draft + gửi link phê duyệt.
- [ ] Thuật ngữ "vét kho" được hiểu đúng nhờ knowledge/glossary.
- [ ] Mọi command ghi dữ liệu đều có approval trace.
