import { describe, expect, it, beforeEach, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { CodeGenProvider, useCodeGenAction, useCodeGenState } from "./index";

const postMock = vi.hoisted(() => vi.fn());
const getMock = vi.hoisted(() => vi.fn());
const putMock = vi.hoisted(() => vi.fn());

vi.mock("@/utils/axiosInstance", () => ({
  getAxiosInstance: () => ({ post: postMock, get: getMock, put: putMock }),
}));

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <CodeGenProvider>{children}</CodeGenProvider>
);

describe("CodeGenProvider actions", () => {
  beforeEach(() => {
    postMock.mockReset();
    getMock.mockReset();
    putMock.mockReset();
  });

  it("createSession calls API and stores session", async () => {
    const mockSession = {
      id: "session-1",
      projectName: "my-app",
      prompt: "Build an app",
      normalizedRequirement: "An app",
      detectedFeatures: ["auth"],
      detectedEntities: ["User"],
      status: 1,
    };

    postMock.mockResolvedValueOnce({
      data: { result: mockSession },
    });

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    let session;
    await act(async () => {
      session = await result.current.actions.createSession("Build an app");
    });

    expect(session).toMatchObject({ id: "session-1", projectName: "my-app" });
    expect(postMock).toHaveBeenCalledWith(
      "/api/services/app/CodeGen/CreateSession",
      { prompt: "Build an app" }
    );
    expect(result.current.state.isSuccess).toBe(true);
    expect(result.current.state.session).toMatchObject({ id: "session-1" });
  });

  it("recommendStack stores recommendation", async () => {
    const mockRec = {
      framework: "Next.js",
      language: "TypeScript",
      styling: "Ant Design",
      database: "PostgreSQL",
      orm: "Prisma",
      auth: "NextAuth.js",
      reasoning: {},
    };

    postMock.mockResolvedValueOnce({
      data: { result: mockRec },
    });

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    await act(async () => {
      await result.current.actions.recommendStack("session-1");
    });

    expect(result.current.state.recommendation).toMatchObject({
      framework: "Next.js",
    });
  });

  it("resetSession clears all state", async () => {
    postMock.mockResolvedValueOnce({
      data: {
        result: {
          id: "session-1",
          projectName: "my-app",
          prompt: "test",
          status: 1,
        },
      },
    });

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    await act(async () => {
      await result.current.actions.createSession("test");
    });

    expect(result.current.state.session).toBeTruthy();

    act(() => {
      result.current.actions.resetSession();
    });

    expect(result.current.state.session).toBeNull();
    expect(result.current.state.isPending).toBe(false);
  });

  it("sets error state when API call fails", async () => {
    postMock.mockRejectedValueOnce(new Error("Network error"));

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    let thrown: Error | undefined;
    await act(async () => {
      try {
        await result.current.actions.createSession("test");
      } catch (e) {
        thrown = e as Error;
      }
    });

    expect(thrown?.message).toBe("Network error");
    expect(result.current.state.isError).toBe(true);
    expect(result.current.state.errorMessage).toBe("Network error");
  });

  it("generateSpec deduplicates concurrent calls for the same session", async () => {
    const mockSpec = {
      entities: [],
      pages: [],
      apiRoutes: [],
      validations: [],
      fileManifest: [],
    };

    let resolveRequest: ((value: unknown) => void) | undefined;
    postMock.mockImplementationOnce(
      () =>
        new Promise((resolve) => {
          resolveRequest = resolve;
        })
    );

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    await act(async () => {
      const first = result.current.actions.generateSpec("session-1");
      const second = result.current.actions.generateSpec("session-1");

      resolveRequest?.({
        data: {
          result: {
            id: "session-1",
            spec: mockSpec,
          },
        },
      });

      const [specA, specB] = await Promise.all([first, second]);
      expect(specA).toEqual(mockSpec);
      expect(specB).toEqual(mockSpec);
    });

    expect(postMock).toHaveBeenCalledTimes(1);
  });

  it("generateSpec returns an empty spec shape when backend returns null spec", async () => {
    postMock.mockResolvedValueOnce({
      data: {
        result: {
          id: "session-1",
          spec: null,
        },
      },
    });

    const { result } = renderHook(
      () => ({ state: useCodeGenState(), actions: useCodeGenAction() }),
      { wrapper }
    );

    let spec;
    await act(async () => {
      spec = await result.current.actions.generateSpec("session-1");
    });

    expect(spec).toEqual({
      entities: [],
      pages: [],
      apiRoutes: [],
      validations: [],
      fileManifest: [],
    });
  });
});
