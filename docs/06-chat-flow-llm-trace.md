# Chat → LLM flow trace (current implementation)

Tài liệu này làm rõ message hiện đang đi qua các lớp nào trước khi tới LLM và các điểm có thể làm request không tới được Minimax.

## 1) Entry points

UI gửi message theo 2 đường:

1. **SignalR realtime** (ưu tiên):
   - `app.js` gọi `chatConnection.invoke("SendMessage", message)` nếu kết nối hub thành công.
2. **HTTP fallback**:
   - `app.js` gọi `POST /module/chatbox/message` khi realtime chưa kết nối được.

## 2) Runtime flow

Cả 2 đường đều gặp `IChatBoxService.SendAsync(...)`:

1. Tạo `AIRequestEnvelope`.
2. Gọi `SemanticKernelRuntime.ExecuteAsync(...)`.
3. Runtime classify intent và chọn plugin theo thứ tự DI:
   - `KnowledgePlugin`
   - `RequestWorkflowPlugin`
   - `ExternalChatPlugin`
4. Với intent `GeneralChat` + `AllowExternalModel = true` thì vào `ExternalChatPlugin`.
5. Plugin gọi `IExternalChatService.ReplyAsync(...)`.
6. Adapter gọi `IMinimaxChatService.TrySendAsync(...)`.
7. `MinimaxChatService` đọc env vars, gọi HTTP tới Minimax endpoint.

## 3) Điểm nghẽn thường gặp (request không tới LLM)

### A. Thiếu biến môi trường Minimax

`MinimaxChatService` trả về `null` ngay nếu thiếu `MINIMAX_API_KEY` hoặc `MINIMAX_GROUP_ID`.

Hệ quả:
- `ExternalChatPlugin` nhận `null` và trả `null` cho runtime.
- Runtime không có plugin nào khác xử lý `GeneralChat` nên rơi vào `fallback`.

### B. Lỗi HTTP/non-2xx từ Minimax

Nếu Minimax trả lỗi HTTP, service log error và trả `null`.
Flow cũng rơi fallback như trên.

### C. Parse response không ra text

Nếu payload từ Minimax không match các field parser đang đọc (`reply`, `choices[*].message.content`, `message`...), service sẽ warning và trả `null`.
Flow rơi fallback.

### D. Khác biệt giữa realtime và fallback

- Realtime (`ChatHub`) hard-code `AllowExternalModel = true`.
- HTTP controller dùng request body `ChatMessageRequest` (default cũng là `true`, nhưng vẫn có thể bị client gửi đè thành `false`).

## 4) Cách xác định flow đang dừng ở đâu

1. Kiểm tra UI đang đi đường nào:
   - Nếu status là "Đã kết nối realtime." thì request đi qua `ChatHub`.
   - Ngược lại đi qua `POST /module/chatbox/message`.
2. Kiểm tra response `source`:
   - `model:minimax` => đã tới LLM.
   - `fallback` => chưa có plugin trả kết quả, thường là external trả `null`.
3. Xem log service:
   - Warning thiếu env vars.
   - Error HTTP status từ Minimax.
   - Warning parse response.

## 5) Kết luận nhanh

Với code hiện tại, trường hợp "không tới được LLM" thường xảy ra nhất khi:
- Chưa set `MINIMAX_API_KEY`/`MINIMAX_GROUP_ID`, hoặc
- Minimax API trả lỗi / format phản hồi khác parser mong đợi.

