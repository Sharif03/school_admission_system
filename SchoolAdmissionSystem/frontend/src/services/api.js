
const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:8080";

async function request(endpoint, options = {}) {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options.headers,
    },
  });

  if (response.status === 204) {
    return null;
  }

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(
      data?.title || data?.message || `Request failed (${response.status})`
    );
  }

  return data;
}

export const api = {
  getClasses: () => request("/classes"),

  createClass: (classData) =>
    request("/classes", {
      method: "POST",
      body: JSON.stringify(classData),
    }),

  createHold: (classId, studentId) =>
    request(`/classes/${classId}/holds`, {
      method: "POST",
      headers: { "X-Student-Id": studentId },
    }),

  getMyHolds: (studentId) =>
    request("/me/holds", {
      headers: { "X-Student-Id": studentId },
    }),

  confirmHold: (holdId, studentId) =>
    request(`/holds/${holdId}/confirm`, {
      method: "POST",
      headers: { "X-Student-Id": studentId },
    }),

  cancelHold: (holdId, studentId) =>
    request(`/holds/${holdId}`, {
      method: "DELETE",
      headers: { "X-Student-Id": studentId },
    }),
};
