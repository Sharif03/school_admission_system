
import { useCallback, useEffect, useRef, useState } from "react";
import { api } from "./services/api";
import ClassCard from "./components/ClassCard";
import HoldCard from "./components/HoldCard";
import Toast from "./components/Toast";
import "./App.css";

function App() {
  const [classes, setClasses] = useState([]);
  const [holds, setHolds] = useState([]);

  const [studentId, setStudentId] = useState("");
  const [className, setClassName] = useState("");
  const [totalSeats, setTotalSeats] = useState(120);

  const [loading, setLoading] = useState(false);
  const [busyHoldId, setBusyHoldId] = useState(null);

  // --- toaster state ---
  const [toasts, setToasts] = useState([]);
  const toastIdRef = useRef(0);

  const dismissToast = useCallback((id) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback((type, text) => {
    const message = String(text ?? "").trim();
    if (!message) return;

    toastIdRef.current += 1;
    const id = toastIdRef.current;

    setToasts((prev) => {
      // Don't stack the same message twice (e.g. a failing poll every 5s)
      if (prev.some((toast) => toast.type === type && toast.message === message)) {
        return prev;
      }
      return [...prev, { id, type, message }];
    });
  }, []);

  const validStudentId = /^[0-9]{17}$/.test(studentId.trim());

  const refreshData = useCallback(async () => {
    try {
      const classData = await api.getClasses();
      setClasses(classData);

      if (validStudentId) {
        const holdData = await api.getMyHolds(studentId.trim());
        setHolds(holdData);
      } else {
        setHolds([]);
      }
    } catch (err) {
      showToast("error", err.message);
    }
  }, [studentId, validStudentId, showToast]);

  useEffect(() => {
    const initialFetchId = setTimeout(refreshData, 0);

    const intervalId = setInterval(refreshData, 5000);

    return () => {
      clearTimeout(initialFetchId);
      clearInterval(intervalId);
    };
  }, [refreshData]);

  async function handleCreateClass(event) {
    event.preventDefault();
    setLoading(true);

    try {
      await api.createClass({
        name: className.trim(),
        totalSeats: Number(totalSeats),
      });

      setClassName("");
      setTotalSeats(120);
      showToast("success", "Class created successfully.");

      await refreshData();
    } catch (err) {
      showToast("error", err.message);
    } finally {
      setLoading(false);
    }
  }

  async function handleCreateHold(classId) {
    if (!validStudentId) {
      showToast("error", "Enter a valid 17-digit Student ID first.");
      return;
    }

    setLoading(true);

    try {
      await api.createHold(classId, studentId.trim());

      showToast("success", "Your seat has been held successfully.");
      await refreshData();
    } catch (err) {
      showToast("error", err.message);
      await refreshData();
    } finally {
      setLoading(false);
    }
  }

  async function handleConfirm(holdId) {
    setBusyHoldId(holdId);

    try {
      await api.confirmHold(holdId, studentId.trim());

      showToast("success", "Your admission has been confirmed.");
      await refreshData();
    } catch (err) {
      showToast("error", err.message);
      await refreshData();
    } finally {
      setBusyHoldId(null);
    }
  }

  async function handleCancel(holdId) {
    setBusyHoldId(holdId);

    try {
      await api.cancelHold(holdId, studentId.trim());

      showToast("success", "Your reservation has been cancelled.");
      await refreshData();
    } catch (err) {
      showToast("error", err.message);
      await refreshData();
    } finally {
      setBusyHoldId(null);
    }
  }

  return (
    <main className="app-container">
      <header className="app-header">
        <div>
          <p className="eyebrow">SCHOOL ADMISSION SYSTEM</p>
          <h1>Admission Portal</h1>
          <p className="subtitle">
            Manage classes and reserve admission seats.
          </p>
        </div>

        <button className="refresh-button" onClick={refreshData}>
          Refresh
        </button>
      </header>

      <section className="panel">
        <div className="section-heading">
          <div>
            <p className="eyebrow">ADMINISTRATION</p>
            <h2>Create a Class</h2>
          </div>
        </div>

        <form className="class-form" onSubmit={handleCreateClass}>
          <div className="form-group">
            <label htmlFor="className">Class Name</label>
            <input
              id="className"
              value={className}
              onChange={(event) => setClassName(event.target.value)}
              placeholder="e.g. Class 6 - Morning Shift"
              maxLength={50}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="totalSeats">Total Seats</label>
            <input
              id="totalSeats"
              type="number"
              min="1"
              max="200"
              value={totalSeats}
              onChange={(event) => setTotalSeats(event.target.value)}
              required
            />
          </div>

          <button
            className="primary-button create-button"
            type="submit"
            disabled={loading}
          >
            Create Class
          </button>
        </form>
      </section>

      <section className="panel">
        <div className="section-heading">
          <div>
            <p className="eyebrow">AVAILABLE CLASSES</p>
            <h2>Admission Classes</h2>
          </div>

          <span className="section-count">
            {classes.length} classes
          </span>
        </div>

        {classes.length === 0 ? (
          <p className="empty-state">No classes have been created yet.</p>
        ) : (
          <div className="class-grid">
            {classes.map((admissionClass) => (
              <ClassCard
                key={admissionClass.id}
                admissionClass={admissionClass}
                onHold={handleCreateHold}
                disabled={!validStudentId || loading}
              />
            ))}
          </div>
        )}
      </section>

      <section className="panel">
        <div className="section-heading">
          <div>
            <p className="eyebrow">PARENT PORTAL</p>
            <h2>My Applications</h2>
          </div>
        </div>

        <div className="form-group student-id-group">
          <label htmlFor="studentId">17-digit Student ID</label>
          <input
            id="studentId"
            value={studentId}
            onChange={(event) => {
              setStudentId(event.target.value.replace(/\D/g, "").slice(0, 17));
            }}
            placeholder="Enter your 17-digit Student ID"
            inputMode="numeric"
            maxLength={17}
          />

          <small>
            {studentId.length}/17 digits
          </small>
        </div>

        {!validStudentId ? (
          <p className="empty-state">
            Enter your complete Student ID to view your applications and
            reserve a seat.
          </p>
        ) : holds.length === 0 ? (
          <p className="empty-state">
            You have no admission applications yet.
          </p>
        ) : (
          <div className="hold-list">
            {holds.map((hold) => (
              <HoldCard
                key={hold.holdId}
                hold={hold}
                onConfirm={handleConfirm}
                onCancel={handleCancel}
                busy={busyHoldId === hold.holdId}
              />
            ))}
          </div>
        )}
      </section>

      <footer className="app-footer">
        School Admission System · Seat availability refreshes automatically.
      </footer>

      <div className="toast-container">
        {toasts.map((toast) => (
          <Toast
            key={toast.id}
            id={toast.id}
            type={toast.type}
            message={toast.message}
            onClose={dismissToast}
          />
        ))}
      </div>
    </main>
  );
}

export default App;
