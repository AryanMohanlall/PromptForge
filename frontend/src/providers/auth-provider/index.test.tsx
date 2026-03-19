import { describe, expect, it, beforeEach, vi } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { AuthProvider, useAuthAction } from "./index";

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

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    );

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

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    );

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

    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <AuthProvider>{children}</AuthProvider>
    );

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
