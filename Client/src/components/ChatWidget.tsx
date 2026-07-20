import { useState, useRef, useEffect, type FormEvent } from "react";
import * as chatService from "@/services/chat";

type Message = { role: "user" | "assistant"; content: string };

export function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  async function handleSend(e: FormEvent) {
    e.preventDefault();
    const text = input.trim();
    if (!text || loading) return;

    setInput("");
    setMessages((prev) => [...prev, { role: "user", content: text }]);
    setLoading(true);

    try {
      const r = await chatService.sendMessage(text);
      setMessages((prev) => [...prev, { role: "assistant", content: r.reply ?? "No response." }]);
    } catch {
      setMessages((prev) => [...prev, { role: "assistant", content: "Connection error. Try again." }]);
    }
    setLoading(false);
  }

  return (
    <>
      {!open && (
        <button className="chat-fab" onClick={() => setOpen(true)} title="BankApp Assistant">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
          </svg>
        </button>
      )}
      {open && (
        <div className="chat-window">
          <div className="chat-header">
            <span>BankApp Assistant</span>
            <button onClick={() => setOpen(false)}>×</button>
          </div>
          <div className="chat-body">
            {messages.length === 0 && (
              <div className="chat-empty">
                Ask me about your accounts, loans, or exchange rates.
              </div>
            )}
            {messages.map((m, i) => (
              <div key={i} className={`chat-msg ${m.role}`}>
                <div className="chat-bubble">{m.content}</div>
              </div>
            ))}
            {loading && <div className="chat-msg assistant"><div className="chat-bubble">...</div></div>}
            <div ref={bottomRef} />
          </div>
          <form className="chat-input-row" onSubmit={handleSend}>
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Ask a banking question..."
              disabled={loading}
            />
            <button type="submit" disabled={loading}>Send</button>
          </form>
        </div>
      )}
    </>
  );
}
