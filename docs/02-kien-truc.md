# Kiến trúc chi tiết

## Định hướng kiến trúc (bản cập nhật)
AIHub được refactor theo hướng **Semantic Kernel Centric Runtime** để gom orchestration vào một lõi thống nhất, đồng thời vẫn giữ mô hình **Modular Monolith** cho giai đoạn MVP.

Mỗi bounded context được biểu diễn dưới dạng plugin/module:
- **KnowledgePlugin**: truy vấn khái niệm/tri thức.
- **RequestWorkflowPlugin**: tạo request, approve, query mock workflow.
- **ExternalChatPlugin**: route sang model provider (Minimax) khi cần.
- **Policy guards**: request policy + response policy tách riêng khỏi prompt.

## Sơ đồ logic mới
```
[Channels]
Web/Portal | Mobile | Chat (Teams/Zalo/Slack) | Internal Apps
    |
    v
[AI Gateway/BFF]
AuthN/AuthZ, tenant routing, rate limit, redaction cơ bản
    |
    v
[Semantic Kernel Runtime]
- Plugin Registry (versioned)
- Intent classification
- Tool allowlist + tool trace
- Pre-hook: policy guard
- Post-hook: response guard + citation enforcement
    |
    +--------------------+--------------------+
    |                    |                    |
    v                    v                    v
[Knowledge Plugins]   [Domain Tool Plugins] [Model Gateway]
```

## Cấu trúc tầng trong API
```
AIHub.Api
├── Controllers/                # HTTP contract, nhận request envelope
├── Application/                # Service nghiệp vụ module management
├── Models/                     # DTO/record dùng chung cho API
├── Services/                   # Adapter hạ tầng
└── Program.cs                  # DI wiring cho Semantic Kernel runtime + plugins

AIHub.Modules.SemanticKernel
├── AIRequestEnvelope           # User/Tenant/Policy/Trace context
├── SemanticKernelRuntime       # Orchestration core
├── IRequestPolicyGuard         # Pre-execution guard
├── IResponsePolicyGuard        # Post-execution guard
└── ISemanticKernelPlugin       # Hợp đồng plugin

AIHub.Modules.ChatBox
├── ChatBoxService              # Bridge từ controller vào runtime
├── KnowledgePlugin             # Bounded context: Management knowledge
├── RequestWorkflowPlugin       # Bounded context: workflow tools
└── ExternalChatPlugin          # Bounded context: external LLM
```

## Runtime flow chuẩn hoá
1. **Controller** chuẩn hoá request thành `AIRequestEnvelope` (User/Tenant/Policy/Trace).
2. **Pre-hook** (`IRequestPolicyGuard`) xác thực policy và quyền theo role.
3. **SemanticKernelRuntime** classify intent + chọn plugin phù hợp.
4. **Plugin execution** gọi tool/domain service, ghi nhận `ToolCallTrace`.
5. **Post-hook** (`IResponsePolicyGuard`) enforce citation/policy response.
6. Trả về `ChatReply` có `source`, `citations`, `tool calls` để audit/observability.

## Nguyên tắc triển khai enterprise-ready
1. **Không nhét policy vào prompt**: policy chạy bằng guard/hook độc lập.
2. **Mọi tool call đều trace được**: tên tool, input params, success/failure.
3. **Plugin allowlist**: chỉ plugin đã đăng ký trong DI mới được thực thi.
4. **Tenant/User context first**: mọi request phải có context trước khi vào runtime.
5. **Fallback rõ ràng**: nếu plugin không xử lý được thì về fallback response.

## Roadmap mở rộng sau refactor
- Bổ sung plugin cho LOS/ECM/CoreBanking/Workflow thật theo bounded context.
- Tách Model Gateway riêng: routing theo tenant/cost/sensitivity + fallback model.
- Bổ sung ACL-aware RAG orchestration và citation store.
- Bật observability chuẩn OpenTelemetry + replay session.
- Đưa policy lên policy-as-code (OPA/Rego) cho governance đầy đủ.
