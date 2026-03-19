import { beforeEach, describe, expect, it, vi } from "vitest";
import { act, renderHook } from "@testing-library/react";
import { TemplateProvider, useTemplateAction, useTemplateState } from "./index";
import { TemplateCategory } from "./context";

const getMock = vi.hoisted(() => vi.fn());
const postMock = vi.hoisted(() => vi.fn());
const putMock = vi.hoisted(() => vi.fn());
const deleteMock = vi.hoisted(() => vi.fn());

vi.mock("@/utils/axiosInstance", () => ({
  getAxiosInstance: () => ({
    get: getMock,
    post: postMock,
    put: putMock,
    delete: deleteMock,
  }),
}));

describe("TemplateProvider actions", () => {
  beforeEach(() => {
    getMock.mockReset();
    postMock.mockReset();
    putMock.mockReset();
    deleteMock.mockReset();
  });

  it("fetchAll loads templates into state", async () => {
    getMock.mockResolvedValueOnce({
      data: {
        result: {
          items: [
            {
              id: 11,
              name: "Admin Dashboard",
              slug: "admin-dashboard",
              category: "Internal",
            },
          ],
          totalCount: 1,
        },
      },
    });

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <TemplateProvider>{children}</TemplateProvider>
    );

    const { result } = renderHook(
      () => ({
        actions: useTemplateAction(),
        state: useTemplateState(),
      }),
      {
        wrapper,
      },
    );

    await act(async () => {
      await result.current.actions.fetchAll();
    });

    expect(getMock).toHaveBeenCalledWith("/api/services/app/Template/GetList", {
      params: undefined,
    });
    expect(result.current.state.items).toHaveLength(1);
    expect(result.current.state.totalCount).toBe(1);
    expect(result.current.state.isSuccess).toBe(true);
  });

  it("create posts data and refreshes list", async () => {
    postMock.mockResolvedValueOnce({ data: { result: {} } });
    getMock.mockResolvedValueOnce({
      data: {
        result: {
          items: [
            {
              id: 3,
              name: "Marketing Site",
              category: TemplateCategory.LandingPages,
            },
          ],
          totalCount: 1,
        },
      },
    });

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <TemplateProvider>{children}</TemplateProvider>
    );

    const { result } = renderHook(
      () => ({
        actions: useTemplateAction(),
        state: useTemplateState(),
      }),
      { wrapper },
    );

    await act(async () => {
      await result.current.actions.create({
        name: "Marketing Site",
        description: "A marketing landing page template",
        category: TemplateCategory.LandingPages,
        framework: 1,
        language: 1,
        database: 1,
        includesAuth: false,
        tags: "landing,marketing",
        status: 2,
        version: "1.0.0",
        isFeatured: false,
      });
    });

    expect(postMock).toHaveBeenCalledTimes(1);
    expect(getMock).toHaveBeenCalledWith("/api/services/app/Template/GetList", {
      params: undefined,
    });
    expect(result.current.state.items).toHaveLength(1);
    expect(result.current.state.totalCount).toBe(1);
    expect(result.current.state.isSuccess).toBe(true);
  });

  it("sets error state when fetchAll request fails", async () => {
    getMock.mockRejectedValueOnce(new Error("network error"));

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <TemplateProvider>{children}</TemplateProvider>
    );

    const { result } = renderHook(
      () => ({
        actions: useTemplateAction(),
        state: useTemplateState(),
      }),
      {
        wrapper,
      },
    );

    await act(async () => {
      await result.current.actions.fetchAll();
    });

    expect(result.current.state.isError).toBe(true);
    expect(result.current.state.isPending).toBe(false);
  });
});
