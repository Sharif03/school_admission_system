
function ClassCard({ admissionClass, onHold, disabled }) {
  const {
    id,
    name,
    totalSeats,
    heldSeats,
    confirmedSeats,
    availableSeats,
  } = admissionClass;

  return (
    <article className="class-card">
      <div className="card-heading">
        <h3>{name}</h3>
        <span className="seat-count">
          {availableSeats} available
        </span>
      </div>

      <div className="seat-summary">
        <div>
          <strong>{totalSeats}</strong>
          <span>Total</span>
        </div>

        <div>
          <strong>{heldSeats}</strong>
          <span>Held</span>
        </div>

        <div>
          <strong>{confirmedSeats}</strong>
          <span>Confirmed</span>
        </div>

        <div>
          <strong>{availableSeats}</strong>
          <span>Available</span>
        </div>
      </div>

      <div className="seat-progress">
        <div
          className="seat-progress-fill"
          style={{
            width: `${Math.min(
              100,
              ((totalSeats - availableSeats) / totalSeats) * 100
            )}%`,
          }}
        />
      </div>

      <button
        className="primary-button"
        disabled={disabled || availableSeats <= 0}
        onClick={() => onHold(id)}
      >
        {availableSeats > 0 ? "Reserve a Seat" : "No Seats Available"}
      </button>
    </article>
  );
}

export default ClassCard;
