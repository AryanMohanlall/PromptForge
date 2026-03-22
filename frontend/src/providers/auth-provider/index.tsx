"use client";

import { useContext, useEffect, useReducer } from "react";
import {
  getAxiosInstance,
  setAuthToken,
  removeAuthToken,
} from "@/utils/axiosInstance";
import { AuthReducer } from "./reducer";
import {
  INITIAL_STATE,
  AuthStateContext,
  AuthActionContext,
  type IRegisterInput,
  type IUser,
} from "./context";
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
  projectCreated,
  authInitialized,
} from "./actions";
import { useRouter } from "next/navigation";

const AUTH_USER_KEY = "auth_user";
const GITHUB_OAUTH_COMPLETE_KEY = "github_oauth_complete";
const PROJECT_STORAGE_KEY = "project_created";
const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:44311";

const clearStoredAuth = () => {
  removeAuthToken();
  sessionStorage.removeItem(AUTH_USER_KEY);
  //sessionStorage.removeItem(GITHUB_CONNECTED_KEY);
};

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const instance = getAxiosInstance();
  const router = useRouter();
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);

  useEffect(() => {
    try {
      const stored = sessionStorage.getItem(AUTH_USER_KEY);
      const hasCreatedProject =
        sessionStorage.getItem(PROJECT_STORAGE_KEY) === "true";
      const isGithubConnected =
        sessionStorage.getItem(GITHUB_OAUTH_COMPLETE_KEY) === "true";

      if (stored) {
        const user: IUser = JSON.parse(stored);
        if (user?.accessToken && user?.userId) {
          setAuthToken(user.accessToken);
          dispatch(loginSuccess(user));
          dispatch(loadLocalState({ isGithubConnected, hasCreatedProject }));
          dispatch(authInitialized());
          return;
        }
      }

      if (isGithubConnected || hasCreatedProject) {
        dispatch(loadLocalState({ isGithubConnected, hasCreatedProject }));
      }
    } catch {
      // ignore parse errors
    }
    dispatch(authInitialized());
  }, []);

  const login = async (userNameOrEmailAddress: string, password: string) => {
    dispatch(loginPending());
    try {
      const res = await instance.post("/api/TokenAuth/Authenticate", {
        userNameOrEmailAddress,
        password,
        rememberClient: true,
      });
      const {
        accessToken,
        expireInSeconds,
        userId,
        userName,
        name,
        surname,
        emailAddress,
        roleNames,
        roleName,
      } = res.data.result;
      setAuthToken(accessToken);
      const user: IUser = {
        accessToken,
        expireInSeconds,
        userId,
        userName,
        name,
        surname,
        emailAddress,
        roleNames: Array.isArray(roleNames) ? roleNames : [],
        roleName,
      };
      sessionStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));
      dispatch(loginSuccess(user));
      return user;
    } catch {
      dispatch(loginError());
      return null;
    }
  };

  const register = async (input: IRegisterInput) => {
    dispatch(registerPending());
    const roles = ["Admin", "Developer", "ProductBuilder"];
    try {
      const { tenantId, roleName, ...rest } = input;
      let role;
      if (roleName) {
        role = roles.indexOf(roleName);
      }

      await instance.post(
        "/api/services/app/Account/Register",
        { ...rest, role },
        tenantId === undefined
          ? undefined
          : {
              headers: {
                "Abp.TenantId": String(tenantId),
              },
            },
      );
      const user = await login(
        input.userName || input.emailAddress,
        input.password,
      );
      if (user) {
        dispatch(registerSuccess(user));
        router.push("/projects");
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
      sessionStorage.removeItem(GITHUB_OAUTH_COMPLETE_KEY);
      sessionStorage.removeItem(PROJECT_STORAGE_KEY);
      dispatch(logoutSuccess());
    } catch {
      dispatch(logoutError());
    }
  };

  const connectGithub = () => {
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
