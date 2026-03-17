"use client";

import { useContext, useReducer } from "react";
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
} from "./actions";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const instance = getAxiosInstance();
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);

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
      const payload = tenantId === undefined ? rest : input;
      await instance.post("/api/services/app/Account/Register", payload);
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
      dispatch(logoutSuccess());
    } catch {
      dispatch(logoutError());
    }
  };

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider value={{ login, register, logout }}>
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
