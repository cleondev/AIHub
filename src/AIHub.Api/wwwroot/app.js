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
        apiBaseUrl: "https://api.minimax.chat"
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

bind("btn-send-chat").addEventListener("click", async () => {
  const message = bind("chat-input").value.trim();
  const target = bind("chat-result");
  if (!message) {
    target.textContent = "Vui lòng nhập nội dung chat.";
    return;
  }

  try {
    const data = await callApi("/module/chatbox/message", {
      method: "POST",
      body: JSON.stringify({ message })
    });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-mock-create").addEventListener("click", async () => {
  const name = bind("mock-name").value.trim();
  const target = bind("mock-result");
  if (!name) {
    target.textContent = "Vui lòng nhập tên request.";
    return;
  }

  try {
    const data = await callApi("/module/mock-api/create", {
      method: "POST",
      body: JSON.stringify({ name })
    });
    target.textContent = pretty(data);
    bind("mock-approve-id").value = data.data.id;
    bind("tool-approve-id").value = data.data.id;
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-mock-query").addEventListener("click", async () => {
  const keyword = bind("mock-name").value.trim();
  const target = bind("mock-result");

  try {
    const data = await callApi(`/module/mock-api/query?keyword=${encodeURIComponent(keyword)}`);
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

bind("btn-mock-approve").addEventListener("click", async () => {
  const id = bind("mock-approve-id").value.trim();
  const target = bind("mock-result");

  try {
    const data = await callApi(`/module/mock-api/approve/${id}`, { method: "POST" });
    target.textContent = pretty(data);
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
