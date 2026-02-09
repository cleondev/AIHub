# Mô hình dữ liệu (đề xuất)

## Bảng chính
### knowledge_documents
- `id` (uuid)
- `title`
- `source_type` (pdf/docx/html/csv)
- `owner_team`
- `access_level` (public/team/private)
- `created_at`

### knowledge_chunks
- `id` (uuid)
- `document_id` (fk)
- `content`
- `embedding` (vector)
- `metadata` (jsonb)

### glossary_terms
- `id` (uuid)
- `term`
- `definition`
- `examples` (text)
- `tags` (text[])

### api_catalog
- `id` (uuid)
- `name`
- `description`
- `openapi_spec` (jsonb)
- `auth_type`
- `allowed_scopes` (text[])
- `created_at`

### ai_generated_records
- `id` (uuid)
- `api_catalog_id` (fk)
- `payload_draft` (jsonb)
- `status` (draft/approved/rejected)
- `created_by`
- `approved_by`
- `approved_at`

### audit_logs
- `id` (uuid)
- `actor_id`
- `action`
- `resource_type`
- `resource_id`
- `timestamp`

### policy_rules
- `id` (uuid)
- `name`
- `scope` (knowledge/api)
- `rule_type` (allow/deny/regex/keyword)
- `rule_value`
- `is_active`
- `created_at`

### model_profiles
- `id` (uuid)
- `name`
- `provider` (openai/azure/local)
- `model_name`
- `temperature`
- `max_tokens`
- `is_active`

## Ghi chú
- Dùng **Row-level Security** để phân quyền trên bảng tri thức.
- Tạo index `ivfflat` trên cột `embedding`.
