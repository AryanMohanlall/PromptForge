import { describe, expect, it } from "vitest";
import { AuthReducer } from "./reducer";
import { INITIAL_STATE } from "./context";
import { loginPending, loginSuccess, loginError } from "./actions";

describe("AuthReducer", () => {
  it("sets pending state on login pending", () => {
    const next = AuthReducer(INITIAL_STATE, loginPending());
    expect(next.isPending).toBe(true);
    expect(next.isAuthenticated).toBe(false);
    expect(next.isError).toBe(false);
  });

  it("sets authenticated user on login success", () => {
    const next = AuthReducer(
      INITIAL_STATE,
      loginSuccess({ accessToken: "token", expireInSeconds: 120, userId: 5, roleNames: [] })
    );
    expect(next.isAuthenticated).toBe(true);
    expect(next.user?.userId).toBe(5);
  });

  it("sets error state on login error", () => {
    const next = AuthReducer(INITIAL_STATE, loginError());
    expect(next.isError).toBe(true);
    expect(next.isAuthenticated).toBe(false);
  });
});
