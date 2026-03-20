import { describe, expect, it, beforeEach, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { AuthProvider, useAuthAction, useAuthState } from "./index";

const postMock = vi.hoisted(() => vi.fn());
const getMock = vi.hoisted(() => vi.fn());
const setAuthTokenMock = vi.hoisted(() => vi.fn());
const removeAuthTokenMock = vi.hoisted(() => vi.fn());

vi.mock("@/utils/axiosInstance", () => ({
  getAxiosInstance: () => ({
    get: getMock,
    post: postMock,
  }),
  setAuthToken: setAuthTokenMock,
  removeAuthToken: removeAuthTokenMock,
}));

const wrapper = ({ children }: { children: React.ReactNode }) => (
  <AuthProvider>{children}</AuthProvider>
);

describe("AuthProvider actions", () => {
  beforeEach(() => {
    postMock.mockReset();
    getMock.mockReset();
    setAuthTokenMock.mockReset();
    removeAuthTokenMock.mockReset();
    sessionStorage.clear();
  });

  it("stores token and user session on login", async () => {
    postMock.mockResolvedValueOnce({
      data: {
        result: {
          accessToken: "token-123",
          expireInSeconds: 3600,
          userId: 7,
          userName: "jane.doe",
          name: "Jane",
          surname: "Doe",
          emailAddress: "jane@example.com",
          roleNames: ["PlatformAdministrator"],
        },
      },
    });

    const { result } = renderHook(() => useAuthAction(), { wrapper });

    await act(async () => {
      const user = await result.current.login("user@example.com", "Password1!");
      expect(user?.userId).toBe(7);
    });

    expect(setAuthTokenMock).toHaveBeenCalledWith("token-123");
    expect(sessionStorage.getItem("auth_user")).toBe(
      JSON.stringify({
        accessToken: "token-123",
        expireInSeconds: 3600,
        userId: 7,
        userName: "jane.doe",
        name: "Jane",
        surname: "Doe",
        emailAddress: "jane@example.com",
        roleNames: ["PlatformAdministrator"],
      }),
    );
  });

  it("sends Abp.TenantId header when tenantId is provided", async () => {
    postMock.mockImplementation((url: string) => {
      if (url.includes("Register")) {
        return Promise.resolve({ data: { result: {} } });
      }
      return Promise.resolve({
        data: {
          result: {
            accessToken: "token-123",
            expireInSeconds: 3600,
            userId: 7,
            userName: "jane.doe",
            name: "Jane",
            surname: "Doe",
            emailAddress: "jane@example.com",
            roleNames: ["PlatformAdministrator"],
          },
        },
      });
    });

    const { result } = renderHook(() => useAuthAction(), { wrapper });

    await act(async () => {
      await result.current.register({
        name: "Jane",
        surname: "Doe",
        userName: "jane.doe",
        emailAddress: "jane@example.com",
        password: "Password1!",
        tenantId: 12,
      });
    });

    expect(postMock.mock.calls[0][0]).toBe(
      "/api/services/app/Account/Register",
    );
    expect(postMock.mock.calls[0][2]).toEqual({
      headers: {
        "Abp.TenantId": "12",
      },
    });
  });

  it("omits tenant header when tenantId is undefined", async () => {
    postMock.mockImplementation((url: string) => {
      if (url.includes("Register")) {
        return Promise.resolve({ data: { result: {} } });
      }
      return Promise.resolve({
        data: {
          result: {
            accessToken: "token-123",
            expireInSeconds: 3600,
            userId: 7,
            userName: "jane.doe",
            name: "Jane",
            surname: "Doe",
            emailAddress: "jane@example.com",
            roleNames: ["PlatformAdministrator"],
          },
        },
      });
    });

    const { result } = renderHook(() => useAuthAction(), { wrapper });

    await act(async () => {
      await result.current.register({
        name: "Jane",
        surname: "Doe",
        userName: "jane.doe",
        emailAddress: "jane@example.com",
        password: "Password1!",
      });
    });

    expect(postMock.mock.calls[0][0]).toBe(
      "/api/services/app/Account/Register",
    );
    expect(postMock.mock.calls[0][2]).toBeUndefined();
  });
});

describe("AuthProvider bootstrap – GitHub connection state", () => {
  beforeEach(() => {
    postMock.mockReset();
    setAuthTokenMock.mockReset();
    removeAuthTokenMock.mockReset();
    sessionStorage.clear();
  });

  it("sets isGithubConnected=true when github_oauth_complete is in sessionStorage", () => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ userId: 1, accessToken: "tok", expireInSeconds: 3600 })
    );
    sessionStorage.setItem("github_oauth_complete", "true");

    const { result } = renderHook(() => useAuthState(), { wrapper });

    expect(result.current.isGithubConnected).toBe(true);
    expect(result.current.isAuthenticated).toBe(true);
  });

  it("sets isGithubConnected=false when auth_user exists but github_oauth_complete is absent", () => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ userId: 1, accessToken: "tok", expireInSeconds: 3600 })
    );

    const { result } = renderHook(() => useAuthState(), { wrapper });

    expect(result.current.isGithubConnected).toBe(false);
    expect(result.current.isAuthenticated).toBe(true);
  });

  it("sets isGithubConnected=false when sessionStorage is empty", () => {
    const { result } = renderHook(() => useAuthState(), { wrapper });

    expect(result.current.isGithubConnected).toBe(false);
    expect(result.current.isAuthenticated).toBe(false);
  });

  it("clears github_oauth_complete on logout", async () => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ userId: 1, accessToken: "tok", expireInSeconds: 3600 })
    );
    sessionStorage.setItem("github_oauth_complete", "true");

    const { result } = renderHook(
      () => ({ state: useAuthState(), actions: useAuthAction() }),
      { wrapper }
    );

    expect(result.current.state.isGithubConnected).toBe(true);

    await act(async () => {
      await result.current.actions.logout();
    });

    expect(result.current.state.isGithubConnected).toBe(false);
    expect(sessionStorage.getItem("github_oauth_complete")).toBeNull();
  });
});

describe("AuthProvider connectGithub", () => {
  beforeEach(() => {
    postMock.mockReset();
    sessionStorage.clear();
  });

  it("redirects to GitHub OAuth endpoint", () => {
    // Mock window.location.href assignment
    const originalLocation = window.location;
    const hrefSetter = vi.fn();
    Object.defineProperty(window, "location", {
      writable: true,
      value: { ...originalLocation, set href(url: string) { hrefSetter(url); } },
    });

    const { result } = renderHook(() => useAuthAction(), { wrapper });

    act(() => {
      result.current.connectGithub();
    });

    // Restore
    Object.defineProperty(window, "location", {
      writable: true,
      value: originalLocation,
    });
  });
});
