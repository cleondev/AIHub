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

document.getElementById("btn-health").addEventListener("click", async () => {
  const target = document.getElementById("health-result");
  try {
    const data = await callApi("/health");
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

document.getElementById("btn-query").addEventListener("click", async () => {
  const target = document.getElementById("query-result");
  const query = document.getElementById("knowledge-query").value.trim();

  if (!query) {
    target.textContent = "Vui lòng nhập câu hỏi.";
    return;
  }

  try {
    const data = await callApi("/knowledge/query", {
      method: "POST",
      body: JSON.stringify({ query, topK: 3, constraints: {} })
    });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

document.getElementById("btn-term").addEventListener("click", async () => {
  const target = document.getElementById("term-result");
  const term = document.getElementById("term-name").value.trim();
  const definition = document.getElementById("term-definition").value.trim();

  if (!term || !definition) {
    target.textContent = "Vui lòng nhập đầy đủ thuật ngữ và định nghĩa.";
    return;
  }

  try {
    const data = await callApi("/glossary/terms", {
      method: "POST",
      body: JSON.stringify({ term, definition, tags: [] })
    });
    target.textContent = pretty(data);
  } catch (error) {
    target.textContent = error.message;
  }
});

document.getElementById("btn-load-terms").addEventListener("click", async () => {
  const list = document.getElementById("term-list");
  list.innerHTML = "";

  try {
    const data = await callApi("/glossary/terms");
    const terms = data.data ?? [];

    if (!terms.length) {
      list.innerHTML = "<li>Chưa có thuật ngữ nào.</li>";
      return;
    }

    terms.forEach((item) => {
      const li = document.createElement("li");
      li.textContent = `${item.term}: ${item.definition}`;
      list.appendChild(li);
    });
  } catch (error) {
    const li = document.createElement("li");
    li.textContent = error.message;
    list.appendChild(li);
  }
});


async function loadToolMock(type) {
  const result = document.getElementById("tool-result");

  try {
    const response = await fetch("/mock-tool-data.txt");

    if (!response.ok) {
      throw new Error(`Không đọc được file mock-tool-data.txt (${response.status}).`);
    }

    const content = await response.text();
    const lines = content
      .split("\n")
      .map((line) => line.trim())
      .filter((line) => line && !line.startsWith("#"));

    const records = lines
      .map((line) => {
        const [recordType, ...segments] = line.split("|");
        const item = { type: recordType };

        segments.forEach((segment) => {
          const [key, ...rest] = segment.split("=");
          item[key] = rest.join("=");
        });

        return item;
      })
      .filter((item) => item.type === type)
      .map((item) => ({
        ...item,
        sample_result: item.sample_result ? JSON.parse(item.sample_result) : undefined,
        sample_response: item.sample_response ? JSON.parse(item.sample_response) : undefined
      }));

    if (!records.length) {
      result.textContent = `Không có mock cho nhóm ${type}.`;
      return;
    }

    result.textContent = pretty(records);
  } catch (error) {
    result.textContent = error.message;
  }
}

document.getElementById("btn-tool-db").addEventListener("click", async () => {
  await loadToolMock("DB");
});

document.getElementById("btn-tool-api").addEventListener("click", async () => {
  await loadToolMock("API");
});
