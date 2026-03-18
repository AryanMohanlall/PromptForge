import { describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { ProjectCard, type ProjectData } from "./index";

const mockProject: ProjectData = {
  id: "101",
  name: "Demo App",
  status: "Generated",
  framework: "Next.js",
  language: "TypeScript",
  updatedAt: "Updated today",
};

describe("ProjectCard", () => {
  it("calls onDelete when delete is clicked", async () => {
    const onView = vi.fn();
    const onDelete = vi.fn();

    render(<ProjectCard project={mockProject} onView={onView} onDelete={onDelete} />);

    fireEvent.click(screen.getByRole("button", { name: "Delete" }));

    expect(onDelete).toHaveBeenCalledTimes(1);
  });

  it("disables delete button while deleting", async () => {
    const onView = vi.fn();
    const onDelete = vi.fn();

    render(
      <ProjectCard
        project={mockProject}
        onView={onView}
        onDelete={onDelete}
        isDeleting
      />
    );

    expect(screen.getByRole("button", { name: "Deleting..." })).toBeDisabled();
  });
});
