// src/components/Toast.jsx
import { useEffect } from "react";

const TOAST_DURATION = 4000; // ms

export default function Toast({ id, type = "info", message, onClose }) {
  useEffect(() => {
    const timerId = setTimeout(() => onClose(id), TOAST_DURATION);
    return () => clearTimeout(timerId);
  }, [id, onClose]);

  return (
    <div className={`toast toast-${type}`} role="status" aria-live="polite">
      <span className="toast-icon" aria-hidden="true">
        {type === "error" ? "!" : "✓"}
      </span>

      <p className="toast-message">{message}</p>

      <button
        type="button"
        className="toast-close"
        onClick={() => onClose(id)}
        aria-label="Dismiss notification"
      >
        ×
      </button>
    </div>
  );
}