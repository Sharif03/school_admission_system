
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import ClassCard from "./ClassCard";

const mockClass = {
  id: "class-001",
  name: "Class One",
  totalSeats: 10,
  heldSeats: 2,
  confirmedSeats: 3,
  availableSeats: 5,
};

describe("ClassCard", () => {
  it("should render class name and seat information", () => {
    render(
      <ClassCard
        admissionClass={mockClass}
        onHold={vi.fn()}
        disabled={false}
      />
    );

    expect(
      screen.getByRole("heading", { name: "Class One" })
    ).toBeInTheDocument();

    expect(screen.getByText("10")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("5 available")).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "Reserve a Seat" })
    ).toBeEnabled();
  });

  it("should call onHold with class ID when reserve button is clicked", async () => {
    const user = userEvent.setup();
    const onHoldMock = vi.fn();

    render(
      <ClassCard
        admissionClass={mockClass}
        onHold={onHoldMock}
        disabled={false}
      />
    );

    await user.click(
      screen.getByRole("button", { name: "Reserve a Seat" })
    );

    expect(onHoldMock).toHaveBeenCalledTimes(1);
    expect(onHoldMock).toHaveBeenCalledWith("class-001");
  });

  it("should disable reserve button when no seats are available", () => {
    const fullClass = {
      ...mockClass,
      heldSeats: 7,
      confirmedSeats: 3,
      availableSeats: 0,
    };

    render(
      <ClassCard
        admissionClass={fullClass}
        onHold={vi.fn()}
        disabled={false}
      />
    );

    expect(
      screen.getByRole("button", { name: "No Seats Available" })
    ).toBeDisabled();
  });

  it("should disable reserve button when parent disables it", () => {
    render(
      <ClassCard
        admissionClass={mockClass}
        onHold={vi.fn()}
        disabled={true}
      />
    );

    expect(
      screen.getByRole("button", { name: "Reserve a Seat" })
    ).toBeDisabled();
  });

  it("should display correct seat progress percentage", () => {
    const { container } = render(
      <ClassCard
        admissionClass={mockClass}
        onHold={vi.fn()}
        disabled={false}
      />
    );

    const progressBar = container.querySelector(
      ".seat-progress-fill"
    );

    // 5 occupied seats out of 10 = 50%
    expect(progressBar).toHaveStyle({ width: "50%" });
  });
});
