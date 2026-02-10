const pretty = (value) => JSON.stringify(value, null, 2);

async function callApi(path, options = {}) {
  const response = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  const body = await response.json();

  if (!response.ok) {
    throw new Error(pretty(body));
  }

  return body;
}

const bind = (id) => document.getElementById(id);

const tabButtons = document.querySelectorAll(".tab-button");
const tabPanels = document.querySelectorAll(".tab-panel");

const activateTab = (tabName) => {
  tabButtons.forEach((button) => {
    button.classList.toggle("active", button.dataset.tab === tabName);
  });

  tabPanels.forEach((panel) => {
    panel.classList.toggle("active", panel.dataset.panel === tabName);
  });
};

tabButtons.forEach((button) => {
  button.addEventListener("click", () => activateTab(button.dataset.tab));
});

activateTab("chat");

bind("btn-add-concept").addEventListener("click", async () => {
  const name = bind("concept-name").value.trim();
  const target = bind("management-result");
  if (!name) {
    target.textContent = "Vui lòng nhập tên khái niệm.";
    return;
  }

  try {
    const data = await callApi("/module/management/concepts", {
      method: "POST",
      body: JSON.stringify({ name })
    });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-list-concepts").addEventListener("click", async () => {
  const target = bind("management-result");
  try {
    const data = await callApi("/module/management/concepts");
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-set-llm").addEventListener("click", async () => {
  const model = bind("llm-model").value.trim();
  const target = bind("management-result");

  try {
    const data = await callApi("/module/management/llm-config", {
      method: "POST",
      body: JSON.stringify({
        provider: "minimax",
        model,
        apiBaseUrl: "https://api.minimax.chat",
        apiKey: bind("llm-api-key").value.trim() || null,
        groupId: bind("llm-group-id").value.trim() || null
      })
    });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-get-llm").addEventListener("click", async () => {
  const target = bind("management-result");
  try {
    const data = await callApi("/module/management/llm-config");
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

const chatResult = bind("chat-result");
const chatStatus = bind("chat-status");
const chatThread = bind("chat-thread");

const appendChat = (item) => {
  const line = document.createElement("div");
  line.className = "chat-item";
  const sentAt = item.sentAt ? new Date(item.sentAt).toLocaleTimeString("vi-VN") : "";
  line.innerHTML = `<strong>${item.sender}</strong> <small>${sentAt}</small><div>${item.message}</div>`;
  chatThread.appendChild(line);
  chatThread.scrollTop = chatThread.scrollHeight;
};

let chatConnection = null;

const startRealtimeChat = async () => {
  if (!window.signalR) {
    chatStatus.textContent = "Không tải được thư viện realtime. Sử dụng fallback API thường.";
    return;
  }

  chatConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/chat")
    .withAutomaticReconnect()
    .build();

  chatConnection.on("chat_history", (history) => {
    chatThread.innerHTML = "";
    history.forEach(appendChat);
  });

  chatConnection.on("chat_message", (item) => {
    appendChat(item);
  });

  try {
    await chatConnection.start();
    await chatConnection.invoke("JoinRoom");
    chatStatus.textContent = "Đã kết nối realtime.";
  } catch (error) {
    chatStatus.textContent = "Kết nối realtime thất bại. Sử dụng fallback API thường.";
    chatResult.textContent = error.message;
    chatConnection = null;
  }
};

startRealtimeChat();

bind("btn-send-chat").addEventListener("click", async () => {
  const message = bind("chat-input").value.trim();
  if (!message) {
    chatResult.textContent = "Vui lòng nhập nội dung chat.";
    return;
  }

  bind("chat-input").value = "";

  if (chatConnection) {
    try {
      await chatConnection.invoke("SendMessage", message);
      chatResult.textContent = "Đã gửi message realtime.";
      return;
    } catch (error) {
      chatResult.textContent = `Realtime lỗi, fallback API: ${error.message}`;
    }
  }

  try {
    const data = await callApi("/module/chatbox/message", {
      method: "POST",
      body: JSON.stringify({ message })
    });
    chatResult.textContent = pretty(data);
  } catch (error) {
    chatResult.textContent = error.message;
  }
});

const loadProducts = async () => {
  const keyword = bind("mock-keyword").value.trim();
  const name = bind("mock-name").value.trim();
  const category = bind("mock-category").value.trim();
  const target = bind("mock-result");

  const query = new URLSearchParams();
  if (keyword) query.set("keyword", keyword);
  if (name) query.set("name", name);
  if (category) query.set("category", category);

  try {
    const suffix = query.toString();
    const data = await callApi(`/module/mock-api/products${suffix ? `?${suffix}` : ""}`);
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
};

const loadOrders = async () => {
  const target = bind("orders-result");

  try {
    const data = await callApi("/module/mock-api/purchase-requests");
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
};

bind("btn-mock-list-products").addEventListener("click", loadProducts);
bind("btn-mock-list-orders").addEventListener("click", loadOrders);
bind("btn-mock-refresh-orders").addEventListener("click", loadOrders);

bind("btn-mock-create-order").addEventListener("click", async () => {
  const productId = bind("mock-product-id").value.trim();
  const quantity = Number(bind("mock-quantity").value);
  const target = bind("mock-result");

  if (!productId || !quantity) {
    target.textContent = "Vui lòng nhập Product ID và số lượng mua.";
    return;
  }

  try {
    const data = await callApi("/module/mock-api/purchase-requests", {
      method: "POST",
      body: JSON.stringify({ productId, quantity })
    });
    target.textContent = pretty(data);
    await loadOrders();
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-tool-read").addEventListener("click", async () => {
  const keyword = bind("tool-keyword").value.trim();
  const target = bind("tool-result");

  try {
    const data = await callApi(`/module/tool/read?keyword=${encodeURIComponent(keyword)}`);
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-tool-create").addEventListener("click", async () => {
  const name = bind("tool-keyword").value.trim() || "def";
  const target = bind("tool-result");

  try {
    const data = await callApi("/module/tool/write/create", {
      method: "POST",
      body: JSON.stringify({ name })
    });
    target.textContent = pretty(data);
    bind("tool-approve-id").value = data.data.id;
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-tool-approve").addEventListener("click", async () => {
  const id = bind("tool-approve-id").value.trim();
  const target = bind("tool-result");

  try {
    const data = await callApi(`/module/tool/write/approve/${id}`, { method: "POST" });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});
