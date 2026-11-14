
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import HoldCard from "./HoldCard";

const mockHold = {
  holdId: "hold-001",
  classId: "class-001",
  className: "Class One",
  status: "HELD",
  expiresAt: "2026-11-15T12:00:00Z",
};

describe("HoldCard", () => {
  it("should render application information and HELD status", () => {
    render(
      <HoldCard
        hold={mockHold}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        busy={false}
      />
    );

    expect(
      screen.getByRole("heading", { name: "Class One" })
    ).toBeInTheDocument();

    expect(screen.getByText("Application: hold-001"))
      .toBeInTheDocument();

    expect(screen.getByText("HELD")).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "ভর্তি ফি দিন" })
    ).toBeEnabled();

    expect(
      screen.getByRole("button", { name: "বাতিল" })
    ).toBeEnabled();
  });

  it("should call onConfirm with hold ID when confirm button is clicked", async () => {
    const user = userEvent.setup();
    const onConfirmMock = vi.fn();

    render(
      <HoldCard
        hold={mockHold}
        onConfirm={onConfirmMock}
        onCancel={vi.fn()}
        busy={false}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "ভর্তি ফি দিন" })
    );

    expect(onConfirmMock).toHaveBeenCalledTimes(1);
    expect(onConfirmMock).toHaveBeenCalledWith("hold-001");
  });

  it("should call onCancel with hold ID when cancel button is clicked", async () => {
    const user = userEvent.setup();
    const onCancelMock = vi.fn();

    render(
      <HoldCard
        hold={mockHold}
        onConfirm={vi.fn()}
        onCancel={onCancelMock}
        busy={false}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "বাতিল" })
    );

    expect(onCancelMock).toHaveBeenCalledTimes(1);
    expect(onCancelMock).toHaveBeenCalledWith("hold-001");
  });

  it("should show CONFIRMED status and hide cancel button", () => {
    const confirmedHold = {
      ...mockHold,
      status: "CONFIRMED",
    };

    render(
      <HoldCard
        hold={confirmedHold}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        busy={false}
      />
    );

    expect(screen.getByText("CONFIRMED")).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "Confirmed" })
    ).toBeDisabled();

    expect(
      screen.queryByRole("button", { name: "বাতিল" })
    ).not.toBeInTheDocument();
  });

  it("should disable confirm button and hide cancel button for EXPIRED application", () => {
    const expiredHold = {
      ...mockHold,
      status: "EXPIRED",
    };

    render(
      <HoldCard
        hold={expiredHold}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        busy={false}
      />
    );

    expect(screen.getByText("EXPIRED")).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "ভর্তি ফি দিন" })
    ).toBeDisabled();

    expect(
      screen.queryByRole("button", { name: "বাতিল" })
    ).not.toBeInTheDocument();
  });

  it("should disable both action buttons when busy", () => {
    render(
      <HoldCard
        hold={mockHold}
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        busy={true}
      />
    );

    expect(
      screen.getByRole("button", { name: "ভর্তি ফি দিন" })
    ).toBeDisabled();

    expect(
      screen.getByRole("button", { name: "বাতিল" })
    ).toBeDisabled();
  });
});
