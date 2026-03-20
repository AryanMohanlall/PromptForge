import { describe, expect, it } from "vitest";
import { AuthReducer } from "./reducer";
import { INITIAL_STATE } from "./context";
import {
  loginPending,
  loginSuccess,
  loginError,
  loadLocalState,
  githubConnect,
  logoutSuccess,
  authInitialized,
} from "./actions";

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

  it("sets isGithubConnected via loadLocalState", () => {
    const next = AuthReducer(
      INITIAL_STATE,
      loadLocalState({ isGithubConnected: true, hasCreatedProject: false })
    );
    expect(next.isGithubConnected).toBe(true);
    expect(next.hasCreatedProject).toBe(false);
  });

  it("does not set isGithubConnected when loadLocalState passes false", () => {
    const next = AuthReducer(
      INITIAL_STATE,
      loadLocalState({ isGithubConnected: false, hasCreatedProject: false })
    );
    expect(next.isGithubConnected).toBe(false);
  });

  it("sets isGithubConnected via githubConnect action", () => {
    const next = AuthReducer(INITIAL_STATE, githubConnect());
    expect(next.isGithubConnected).toBe(true);
  });

  it("resets isGithubConnected on logout", () => {
    const connected = AuthReducer(INITIAL_STATE, githubConnect());
    expect(connected.isGithubConnected).toBe(true);

    const next = AuthReducer(connected, logoutSuccess());
    expect(next.isGithubConnected).toBe(false);
    expect(next.hasCreatedProject).toBe(false);
    expect(next.isAuthenticated).toBe(false);
  });

  it("sets isInitialized via authInitialized", () => {
    const next = AuthReducer(INITIAL_STATE, authInitialized());
    expect(next.isInitialized).toBe(true);
  });
});
