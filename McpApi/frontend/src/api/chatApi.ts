import axios from "axios";

const client = axios.create({ baseURL: "/api/chat" });

export interface ConversationMessage {
  id: string;
  role: "System" | "User" | "Assistant";
  content: string;
  createdAt: string;
}

export interface Conversation {
  id: string;
  title: string | null;
  userId: string | null;
  status: "Active" | "Archived";
  createdAt: string;
  updatedAt: string | null;
  messages: ConversationMessage[];
}

export interface ConversationSummary {
  id: string;
  title: string | null;
  userId: string | null;
  status: "Active" | "Archived";
  createdAt: string;
  messageCount: number;
}

export interface PagedResult<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export async function createConversation(
  title?: string,
  initialMessage?: string
): Promise<Conversation> {
  const { data } = await client.post<Conversation>("/conversations", {
    title,
    initialMessage,
  });
  return data;
}

export async function getConversation(id: string): Promise<Conversation> {
  const { data } = await client.get<Conversation>(`/conversations/${id}`);
  return data;
}

export async function listConversations(
  page = 1,
  pageSize = 50
): Promise<PagedResult<ConversationSummary>> {
  const { data } = await client.get<PagedResult<ConversationSummary>>(
    "/conversations",
    { params: { page, pageSize } }
  );
  return data;
}

/** Streams a message via SSE. Calls onChunk for each text chunk, returns full response. */
export async function sendMessage(
  conversationId: string,
  content: string,
  onChunk: (chunk: string) => void,
  signal?: AbortSignal
): Promise<string> {
  const response = await fetch(
    `/api/chat/conversations/${conversationId}/messages`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ content }),
      signal,
    }
  );

  if (!response.ok) {
    throw new Error(`Send message failed: ${response.status}`);
  }

  const reader = response.body!.getReader();
  const decoder = new TextDecoder();
  let full = "";
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    // SSE events are separated by double newlines
    const events = buffer.split("\n\n");
    // Keep the last (possibly incomplete) chunk in the buffer
    buffer = events.pop()!;

    for (const event of events) {
      if (!event.trim()) continue;

      // Reassemble multi-line data fields:
      // A chunk with newlines becomes multiple lines within one event block.
      // Lines starting with "data: " are data; bare lines are continuations.
      const lines = event.split("\n");
      let payload = "";
      for (const line of lines) {
        if (line.startsWith("data: ")) {
          payload += line.slice(6);
        } else {
          // Continuation of a multi-line chunk — restore the newline
          payload += "\n" + line;
        }
      }

      if (payload === "[DONE]") continue;
      full += payload;
      onChunk(payload);
    }
  }

  return full;
}
