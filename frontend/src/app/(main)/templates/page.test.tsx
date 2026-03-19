import { beforeEach, describe, expect, it, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import TemplatesPage from "./page";

const fetchAllMock = vi.hoisted(() => vi.fn());
const templateStateMock = vi.hoisted(() => ({
  items: [] as Array<{
    id: number;
    name: string;
    slug: string;
    category: string;
    description?: string | null;
    tags?: string[] | null;
    categoryName?: string | null;
    author?: string | null;
    likeCount?: number | null;
    viewCount?: number | null;
    sourceUrl?: string | null;
  }>,
  isPending: false,
  isError: false,
}));

vi.mock("@/providers/templates-provider", () => ({
  useTemplateAction: () => ({
    fetchAll: fetchAllMock,
  }),
  useTemplateState: () => templateStateMock,
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}));

vi.mock("./styles/style", () => ({
  useStyles: () => ({
    styles: new Proxy(
      {},
      {
        get: (_target, key) => String(key),
      },
    ),
  }),
}));

describe("TemplatesPage", () => {
  beforeEach(() => {
    fetchAllMock.mockReset();
    templateStateMock.items = [];
    templateStateMock.isPending = false;
    templateStateMock.isError = false;
  });

  it("loads templates on mount and filters by search", () => {
    templateStateMock.items = [
      {
        id: 1,
        name: "Admin Dashboard",
        slug: "admin-dashboard",
        category: "Internal",
        categoryName: "Internal",
        description: "Internal analytics dashboard template",
        tags: ["admin", "analytics"],
      },
      {
        id: 2,
        name: "Marketing Landing",
        slug: "marketing-landing",
        category: "Marketing",
        categoryName: "Marketing",
        description: "Landing page starter",
        tags: ["landing", "seo"],
      },
    ];

    render(<TemplatesPage />);

    expect(fetchAllMock).toHaveBeenCalledTimes(1);
    expect(screen.getByText("Admin Dashboard")).toBeInTheDocument();
    expect(screen.getByText("Marketing Landing")).toBeInTheDocument();

    fireEvent.change(screen.getByPlaceholderText("Search templates"), {
      target: { value: "marketing" },
    });

    expect(screen.getByText("Marketing Landing")).toBeInTheDocument();
    expect(screen.queryByText("Admin Dashboard")).not.toBeInTheDocument();
  });

  it("shows error empty state when loading fails", () => {
    templateStateMock.isError = true;

    render(<TemplatesPage />);

    expect(fetchAllMock).toHaveBeenCalledTimes(1);
    expect(screen.getByText("Could not load templates.")).toBeInTheDocument();
  });
});
