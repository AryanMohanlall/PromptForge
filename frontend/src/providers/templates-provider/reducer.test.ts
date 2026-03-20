import { describe, expect, it } from "vitest";
import { TemplateCategory } from "./context";
import { TemplateReducer } from "./reducer";
import { INITIAL_STATE } from "./context";
import { fetchAllPending, fetchAllSuccess, fetchAllError } from "./actions";

describe("TemplateReducer", () => {
  it("sets pending state on fetch all pending", () => {
    const next = TemplateReducer(INITIAL_STATE, fetchAllPending());

    expect(next.isPending).toBe(true);
    expect(next.isSuccess).toBe(false);
    expect(next.isError).toBe(false);
  });

  it("stores templates on fetch all success", () => {
    const next = TemplateReducer(
      INITIAL_STATE,
      fetchAllSuccess({
        items: [
          {
            id: 1,
            name: "Landing Page",
            category: TemplateCategory.LandingPages,
            framework: 1 as const,
            language: 1 as const,
            database: 1 as const,
            includesAuth: false,
            tags: [],
            thumbnailUrl: null,
            previewUrl: null,
            status: 2,
            version: "1.0.0",
            isFeatured: false,
            forkCount: 0,
            isFavorite: false,
            createdAt: "2024-01-01",
          },
        ],
        totalCount: 1,
      }),
    );

    expect(next.isPending).toBe(false);
    expect(next.isSuccess).toBe(true);
    expect(next.items).toHaveLength(1);
    expect(next.totalCount).toBe(1);
  });

  it("sets error state on fetch all error", () => {
    const next = TemplateReducer(INITIAL_STATE, fetchAllError());

    expect(next.isPending).toBe(false);
    expect(next.isSuccess).toBe(false);
    expect(next.isError).toBe(true);
  });
});
