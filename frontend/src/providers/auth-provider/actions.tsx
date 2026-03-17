import { type IAuthStateContext, type IUser } from "./context";

export enum AuthStateEnums {
  LOGIN_PENDING = "LOGIN_PENDING",
  LOGIN_SUCCESS = "LOGIN_SUCCESS",
  LOGIN_ERROR = "LOGIN_ERROR",
  REGISTER_PENDING = "REGISTER_PENDING",
  REGISTER_SUCCESS = "REGISTER_SUCCESS",
  REGISTER_ERROR = "REGISTER_ERROR",
  LOGOUT_PENDING = "LOGOUT_PENDING",
  LOGOUT_SUCCESS = "LOGOUT_SUCCESS",
  LOGOUT_ERROR = "LOGOUT_ERROR",
}

export interface AuthAction {
  type: AuthStateEnums;
  payload: IAuthStateContext;
}

const createAuthAction = (type: AuthStateEnums, payload: IAuthStateContext): AuthAction => ({
  type,
  payload,
});

export const loginPending = (): AuthAction =>
  createAuthAction(AuthStateEnums.LOGIN_PENDING, {
    isPending: true,
    isSuccess: false,
    isError: false,
    isAuthenticated: false,
  });

export const loginSuccess = (user: IUser): AuthAction =>
  createAuthAction(AuthStateEnums.LOGIN_SUCCESS, {
    isPending: false,
    isSuccess: true,
    isError: false,
    isAuthenticated: true,
    user,
  });

export const loginError = (): AuthAction =>
  createAuthAction(AuthStateEnums.LOGIN_ERROR, {
    isPending: false,
    isSuccess: false,
    isError: true,
    isAuthenticated: false,
  });

export const registerPending = (): AuthAction =>
  createAuthAction(AuthStateEnums.REGISTER_PENDING, {
    isPending: true,
    isSuccess: false,
    isError: false,
    isAuthenticated: false,
  });

export const registerSuccess = (user: IUser): AuthAction =>
  createAuthAction(AuthStateEnums.REGISTER_SUCCESS, {
    isPending: false,
    isSuccess: true,
    isError: false,
    isAuthenticated: true,
    user,
  });

export const registerError = (): AuthAction =>
  createAuthAction(AuthStateEnums.REGISTER_ERROR, {
    isPending: false,
    isSuccess: false,
    isError: true,
    isAuthenticated: false,
  });

export const logoutPending = (): AuthAction =>
  createAuthAction(AuthStateEnums.LOGOUT_PENDING, {
    isPending: true,
    isSuccess: false,
    isError: false,
    isAuthenticated: false,
  });

export const logoutSuccess = (): AuthAction =>
  createAuthAction(AuthStateEnums.LOGOUT_SUCCESS, {
    isPending: false,
    isSuccess: true,
    isError: false,
    isAuthenticated: false,
    user: undefined,
  });

export const logoutError = (): AuthAction =>
  createAuthAction(AuthStateEnums.LOGOUT_ERROR, {
    isPending: false,
    isSuccess: false,
    isError: true,
    isAuthenticated: false,
  });
