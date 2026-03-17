"use client";

import { useContext, useEffect, useReducer } from "react";
import { getAxiosInstance, setAuthToken, removeAuthToken } from "@/utils/axiosInstance";
import { AuthReducer } from "./reducer";
import { INITIAL_STATE, AuthStateContext, AuthActionContext, type IRegisterInput, type IUser } from "./context";
import {
  loginPending,
  loginSuccess,
  loginError,
  registerPending,
  registerSuccess,
  registerError,
  logoutPending,
  logoutSuccess,
  logoutError,
  loadLocalState,
  githubConnect,
  projectCreated,
} from "./actions";

const AUTH_USER_KEY = "auth_user";
const GITHUB_CONNECTED_KEY = "github_connected";
const PROJECT_STORAGE_KEY = "project_created";
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:44311";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const instance = getAxiosInstance();
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);

  useEffect(() => {
    try {
      const stored = sessionStorage.getItem(AUTH_USER_KEY);
      const hasCreatedProject = sessionStorage.getItem(PROJECT_STORAGE_KEY) === "true";

      if (stored) {
        const user: IUser = JSON.parse(stored);
        if (user?.accessToken && user?.userId) {
          setAuthToken(user.accessToken);
          dispatch(loginSuccess(user));
          // OAuth callback stores auth_user in sessionStorage; treat that as GitHub-connected.
          sessionStorage.setItem(GITHUB_CONNECTED_KEY, "true");
          dispatch(loadLocalState({ isGithubConnected: true, hasCreatedProject }));
          return;
        }
      }

      const isGithubConnected = sessionStorage.getItem(GITHUB_CONNECTED_KEY) === "true";
      if (isGithubConnected || hasCreatedProject) {
        dispatch(loadLocalState({ isGithubConnected, hasCreatedProject }));
      }
    } catch {
      // ignore parse errors
    }
  }, []);

  const login = async (userNameOrEmailAddress: string, password: string) => {
    dispatch(loginPending());
    try {
      const res = await instance.post("/api/TokenAuth/Authenticate", {
        userNameOrEmailAddress,
        password,
        rememberClient: true,
      });
      const { accessToken, expireInSeconds, userId } = res.data.result;
      const user: IUser = { accessToken, expireInSeconds, userId };
      setAuthToken(accessToken);
      dispatch(loginSuccess(user));
      return user;
    } catch {
      dispatch(loginError());
      return null;
    }
  };

  const register = async (input: IRegisterInput) => {
    dispatch(registerPending());
    try {
      const { tenantId, ...rest } = input;
      await instance.post(
        "/api/services/app/Account/Register",
        rest,
        tenantId === undefined
          ? undefined
          : {
              headers: {
                "Abp.TenantId": String(tenantId),
              },
            }
      );
      const user = await login(input.userName || input.emailAddress, input.password);
      if (user) {
        dispatch(registerSuccess(user));
      } else {
        dispatch(registerError());
      }
    } catch {
      dispatch(registerError());
    }
  };

  const logout = async () => {
    dispatch(logoutPending());
    try {
      removeAuthToken();
      sessionStorage.removeItem(AUTH_USER_KEY);
      sessionStorage.removeItem(GITHUB_CONNECTED_KEY);
      sessionStorage.removeItem(PROJECT_STORAGE_KEY);
      dispatch(logoutSuccess());
    } catch {
      dispatch(logoutError());
    }
  };

  const connectGithub = () => {
    try {
      const stored = sessionStorage.getItem(AUTH_USER_KEY);
      if (stored) {
        const user: IUser = JSON.parse(stored);
        if (user?.accessToken && user?.userId) {
          sessionStorage.setItem(GITHUB_CONNECTED_KEY, "true");
          dispatch(githubConnect());
          return;
        }
      }
    } catch {
      // Fallback to OAuth redirect below.
    }

    window.location.href = `${API_BASE_URL}/api/TokenAuth/GitHubLogin`;
  };

  const markProjectCreated = () => {
    sessionStorage.setItem(PROJECT_STORAGE_KEY, "true");
    dispatch(projectCreated());
  };

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider
        value={{ login, register, logout, connectGithub, markProjectCreated }}
      >
        {children}
      </AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
};

export const useAuthState = () => {
  const context = useContext(AuthStateContext);
  if (!context) {
    throw new Error("useAuthState must be used within AuthProvider");
  }
  return context;
};

export const useAuthAction = () => {
  const context = useContext(AuthActionContext);
  if (!context) {
    throw new Error("useAuthAction must be used within AuthProvider");
  }
  return context;
};
