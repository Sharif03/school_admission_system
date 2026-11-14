
function HoldCard({ hold, onConfirm, onCancel, busy }) {
  const isHeld = hold.status === "HELD";
  const isConfirmed = hold.status === "CONFIRMED";

  return (
    <article className="hold-card">
      <div className="card-heading">
        <div>
          <h3>{hold.className}</h3>
          <small>Application: {hold.holdId}</small>
        </div>

        <span className={`status-badge ${hold.status.toLowerCase()}`}>
          {hold.status}
        </span>
      </div>

      <p>
        Expires: {new Date(hold.expiresAt).toLocaleString()}
      </p>

      <div className="hold-actions">
        <button
          className="primary-button"
          disabled={!isHeld || busy}
          onClick={() => onConfirm(hold.holdId)}
        >
          {isConfirmed ? "Confirmed" : "ভর্তি ফি দিন"}
        </button>

        {isHeld && (
          <button
            className="secondary-button"
            disabled={busy}
            onClick={() => onCancel(hold.holdId)}
          >
            বাতিল
          </button>
        )}
      </div>
    </article>
  );
}

export default HoldCard;
