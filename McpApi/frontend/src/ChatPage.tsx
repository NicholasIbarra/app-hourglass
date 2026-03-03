import { useState, useEffect, useRef, useCallback } from "react";
import {
  Layout,
  Menu,
  Button,
  Input,
  Typography,
  List,
  Spin,
  Empty,
  theme,
} from "antd";
import {
  PlusOutlined,
  SendOutlined,
  MessageOutlined,
  LoadingOutlined,
} from "@ant-design/icons";
import {
  createConversation,
  getConversation,
  listConversations,
  sendMessage,
  type ConversationSummary,
  type ConversationMessage,
} from "./api/chatApi";

const { Sider, Content } = Layout;
const { Text } = Typography;

interface DisplayMessage {
  id: string;
  role: "User" | "Assistant" | "System";
  content: string;
  createdAt: string;
  streaming?: boolean;
}

export default function ChatPage() {
  const { token } = theme.useToken();
  const [conversations, setConversations] = useState<ConversationSummary[]>([]);
  const [activeId, setActiveId] = useState<string | null>(null);
  const [messages, setMessages] = useState<DisplayMessage[]>([]);
  const [input, setInput] = useState("");
  const [sending, setSending] = useState(false);
  const [loadingConvos, setLoadingConvos] = useState(true);
  const [loadingMessages, setLoadingMessages] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const abortRef = useRef<AbortController | null>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  // Load conversations on mount
  useEffect(() => {
    loadConversations();
  }, []);

  useEffect(() => {
    scrollToBottom();
  }, [messages, scrollToBottom]);

  async function loadConversations() {
    setLoadingConvos(true);
    try {
      const result = await listConversations();
      setConversations(result.items);
    } catch (e) {
      console.error("Failed to load conversations", e);
    } finally {
      setLoadingConvos(false);
    }
  }

  async function selectConversation(id: string) {
    setActiveId(id);
    setLoadingMessages(true);
    try {
      const convo = await getConversation(id);
      setMessages(
        convo.messages
          .filter((m) => m.role !== "System")
          .map((m) => ({ ...m, streaming: false }))
      );
    } catch (e) {
      console.error("Failed to load conversation", e);
    } finally {
      setLoadingMessages(false);
    }
  }

  async function handleNewConversation() {
    try {
      const convo = await createConversation();
      setActiveId(convo.id);
      setMessages([]);
      await loadConversations();
    } catch (e) {
      console.error("Failed to create conversation", e);
    }
  }

  async function handleSend() {
    const text = input.trim();
    if (!text || sending) return;

    let conversationId = activeId;

    // Auto-create conversation if none selected
    if (!conversationId) {
      try {
        const convo = await createConversation();
        conversationId = convo.id;
        setActiveId(convo.id);
        loadConversations();
      } catch (e) {
        console.error("Failed to create conversation", e);
        return;
      }
    }

    const userMsg: DisplayMessage = {
      id: `temp-user-${Date.now()}`,
      role: "User",
      content: text,
      createdAt: new Date().toISOString(),
    };

    const assistantMsg: DisplayMessage = {
      id: `temp-assistant-${Date.now()}`,
      role: "Assistant",
      content: "",
      createdAt: new Date().toISOString(),
      streaming: true,
    };

    setMessages((prev) => [...prev, userMsg, assistantMsg]);
    setInput("");
    setSending(true);

    abortRef.current = new AbortController();

    try {
      await sendMessage(
        conversationId,
        text,
        (chunk) => {
          setMessages((prev) => {
            const updated = [...prev];
            const last = updated[updated.length - 1];
            if (last.role === "Assistant" && last.streaming) {
              updated[updated.length - 1] = {
                ...last,
                content: last.content + chunk,
              };
            }
            return updated;
          });
        },
        abortRef.current.signal
      );

      // Mark streaming complete
      setMessages((prev) => {
        const updated = [...prev];
        const last = updated[updated.length - 1];
        if (last.role === "Assistant") {
          updated[updated.length - 1] = { ...last, streaming: false };
        }
        return updated;
      });

      // Refresh conversation list to get updated title/count
      loadConversations();
    } catch (e) {
      if ((e as Error).name !== "AbortError") {
        console.error("Failed to send message", e);
        setMessages((prev) => {
          const updated = [...prev];
          const last = updated[updated.length - 1];
          if (last.role === "Assistant" && last.streaming) {
            updated[updated.length - 1] = {
              ...last,
              content: last.content || "Error: Failed to get response.",
              streaming: false,
            };
          }
          return updated;
        });
      }
    } finally {
      setSending(false);
      abortRef.current = null;
    }
  }

  const menuItems = conversations.map((c) => ({
    key: c.id,
    icon: <MessageOutlined />,
    label: c.title || `Conversation`,
  }));

  return (
    <Layout style={{ height: "100vh" }}>
      <Sider
        width={280}
        style={{
          background: token.colorBgContainer,
          borderRight: `1px solid ${token.colorBorderSecondary}`,
          display: "flex",
          flexDirection: "column",
        }}
      >
        <div
          style={{
            padding: "16px",
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
          }}
        >
          <Button
            type="primary"
            icon={<PlusOutlined />}
            block
            onClick={handleNewConversation}
          >
            New Chat
          </Button>
        </div>
        <div style={{ flex: 1, overflow: "auto" }}>
          {loadingConvos ? (
            <div style={{ textAlign: "center", padding: 24 }}>
              <Spin />
            </div>
          ) : (
            <Menu
              mode="inline"
              selectedKeys={activeId ? [activeId] : []}
              items={menuItems}
              onClick={({ key }) => selectConversation(key)}
              style={{ border: "none" }}
            />
          )}
        </div>
      </Sider>
      <Content
        style={{
          display: "flex",
          flexDirection: "column",
          background: token.colorBgLayout,
        }}
      >
        {/* Messages area */}
        <div
          style={{
            flex: 1,
            overflow: "auto",
            padding: "24px 24px 0",
          }}
        >
          {!activeId && messages.length === 0 ? (
            <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                height: "100%",
              }}
            >
              <Empty description="Start a new conversation or select one from the sidebar" />
            </div>
          ) : loadingMessages ? (
            <div style={{ textAlign: "center", padding: 48 }}>
              <Spin size="large" />
            </div>
          ) : (
            <List
              dataSource={messages}
              renderItem={(msg) => (
                <div
                  style={{
                    display: "flex",
                    justifyContent:
                      msg.role === "User" ? "flex-end" : "flex-start",
                    marginBottom: 12,
                  }}
                >
                  <div
                    style={{
                      maxWidth: "70%",
                      padding: "10px 16px",
                      borderRadius: 12,
                      background:
                        msg.role === "User"
                          ? token.colorPrimary
                          : token.colorBgElevated,
                      color:
                        msg.role === "User"
                          ? "#fff"
                          : token.colorText,
                      boxShadow: token.boxShadowTertiary,
                      whiteSpace: "pre-wrap",
                      wordBreak: "break-word",
                    }}
                  >
                    {msg.content}
                    {msg.streaming && !msg.content && (
                      <LoadingOutlined style={{ marginLeft: 4 }} />
                    )}
                  </div>
                </div>
              )}
            />
          )}
          <div ref={messagesEndRef} />
        </div>

        {/* Input area */}
        <div
          style={{
            padding: "16px 24px",
            borderTop: `1px solid ${token.colorBorderSecondary}`,
            background: token.colorBgContainer,
          }}
        >
          <div style={{ display: "flex", gap: 8 }}>
            <Input.TextArea
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onPressEnter={(e) => {
                if (!e.shiftKey) {
                  e.preventDefault();
                  handleSend();
                }
              }}
              placeholder="Type a message..."
              autoSize={{ minRows: 1, maxRows: 4 }}
              disabled={sending}
              style={{ flex: 1 }}
            />
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={handleSend}
              loading={sending}
              style={{ alignSelf: "flex-end" }}
            >
              Send
            </Button>
          </div>
          <Text
            type="secondary"
            style={{ fontSize: 12, marginTop: 4, display: "block" }}
          >
            Press Enter to send, Shift+Enter for new line
          </Text>
        </div>
      </Content>
    </Layout>
  );
}
