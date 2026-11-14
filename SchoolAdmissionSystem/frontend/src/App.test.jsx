
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import App from "./App";
import { api } from "./services/api";

vi.mock("./services/api", () => ({
  api: {
    getClasses: vi.fn(),
    getMyHolds: vi.fn(),
    createClass: vi.fn(),
    createHold: vi.fn(),
    confirmHold: vi.fn(),
    cancelHold: vi.fn(),
  },
}));

const mockClass = {
  id: "class-001",
  name: "Class Six",
  totalSeats: 100,
  heldSeats: 5,
  confirmedSeats: 20,
  availableSeats: 75,
};

const mockHold = {
  holdId: "hold-001",
  classId: "class-001",
  className: "Class Six",
  status: "HELD",
  expiresAt: "2026-11-15T12:00:00Z",
};

const validStudentId = "12345678901234567";

describe("App", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    api.getClasses.mockResolvedValue([mockClass]);
    api.getMyHolds.mockResolvedValue([]);
    api.createClass.mockResolvedValue({});
    api.createHold.mockResolvedValue({});
    api.confirmHold.mockResolvedValue({});
    api.cancelHold.mockResolvedValue({});
  });

  it("should load and display admission classes", async () => {
    render(<App />);

    expect(
      screen.getByRole("heading", { name: "Admission Portal" })
    ).toBeInTheDocument();

    expect(
      await screen.findByRole("heading", { name: "Class Six" })
    ).toBeInTheDocument();

    expect(api.getClasses).toHaveBeenCalled();
    expect(screen.getByText("1 classes")).toBeInTheDocument();
  });

  it("should show an empty state when no classes exist", async () => {
    api.getClasses.mockResolvedValue([]);

    render(<App />);

    expect(
      await screen.findByText("No classes have been created yet.")
    ).toBeInTheDocument();
  });

  it("should not allow reserving a seat without a valid student ID", async () => {
    const user = userEvent.setup();

    render(<App />);

    const reserveButton = await screen.findByRole("button", {
      name: "Reserve a Seat",
    });

    expect(reserveButton).toBeDisabled();
    expect(api.createHold).not.toHaveBeenCalled();

    const studentInput = screen.getByLabelText("17-digit Student ID");

    await user.type(studentInput, "12345");

    expect(
      screen.getByText("5/17 digits")
    ).toBeInTheDocument();

    expect(reserveButton).toBeDisabled();
  });

  it("should load applications after entering a valid 17-digit student ID", async () => {
    const user = userEvent.setup();

    api.getMyHolds.mockResolvedValue([mockHold]);

    render(<App />);

    await user.type(
      screen.getByLabelText("17-digit Student ID"),
      validStudentId
    );

    expect(
      await screen.findByText("Application: hold-001")
    ).toBeInTheDocument();

    expect(api.getMyHolds).toHaveBeenCalledWith(validStudentId);
  });

  it("should create a class and refresh the class list", async () => {
    const user = userEvent.setup();

    render(<App />);

    await screen.findByRole("heading", { name: "Class Six" });

    await user.clear(screen.getByLabelText("Class Name"));
    await user.type(
      screen.getByLabelText("Class Name"),
      "Class Seven"
    );

    await user.clear(screen.getByLabelText("Total Seats"));
    await user.type(screen.getByLabelText("Total Seats"), "80");

    await user.click(
      screen.getByRole("button", { name: "Create Class" })
    );

    await waitFor(() => {
      expect(api.createClass).toHaveBeenCalledWith({
        name: "Class Seven",
        totalSeats: 80,
      });
    });

    expect(
      await screen.findByText("Class created successfully.")
    ).toBeInTheDocument();

    expect(api.getClasses.mock.calls.length).toBeGreaterThanOrEqual(2);
  });

  it("should create a seat hold for a valid student ID", async () => {
    const user = userEvent.setup();

    render(<App />);

    await screen.findByRole("heading", { name: "Class Six" });

    await user.type(
      screen.getByLabelText("17-digit Student ID"),
      validStudentId
    );

    const reserveButton = await screen.findByRole("button", {
      name: "Reserve a Seat",
    });

    expect(reserveButton).toBeEnabled();

    await user.click(reserveButton);

    await waitFor(() => {
      expect(api.createHold).toHaveBeenCalledWith(
        "class-001",
        validStudentId
      );
    });

    expect(
      await screen.findByText("Your seat has been held successfully.")
    ).toBeInTheDocument();
  });

  it("should display an error when loading classes fails", async () => {
    api.getClasses.mockRejectedValue(new Error("Server unavailable"));

    render(<App />);

    expect(
      await screen.findByText("Server unavailable")
    ).toBeInTheDocument();
  });


  it("should confirm an admission application", async () => {
    const user = userEvent.setup();

    api.getMyHolds.mockResolvedValue([mockHold]);

    render(<App />);

    await user.type(
      screen.getByLabelText("17-digit Student ID"),
      validStudentId
    );

    expect(
      await screen.findByText("Application: hold-001")
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole("button", { name: "ভর্তি ফি দিন" })
    );

    await waitFor(() => {
      expect(api.confirmHold).toHaveBeenCalledWith(
        "hold-001",
        validStudentId
      );
    });

    expect(
      await screen.findByText("Your admission has been confirmed.")
    ).toBeInTheDocument();
  });

  it("should cancel an admission reservation", async () => {
    const user = userEvent.setup();

    api.getMyHolds.mockResolvedValue([mockHold]);

    render(<App />);

    await user.type(
      screen.getByLabelText("17-digit Student ID"),
      validStudentId
    );

    expect(
      await screen.findByText("Application: hold-001")
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole("button", { name: "বাতিল" })
    );

    await waitFor(() => {
      expect(api.cancelHold).toHaveBeenCalledWith(
        "hold-001",
        validStudentId
      );
    });

    expect(
      await screen.findByText("Your reservation has been cancelled.")
    ).toBeInTheDocument();
  });

});
