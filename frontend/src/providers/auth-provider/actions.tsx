import { createAction } from "redux-actions";
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

export const loginPending = createAction<IAuthStateContext>(
  AuthStateEnums.LOGIN_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, isAuthenticated: false })
);

export const loginSuccess = createAction<IAuthStateContext, IUser>(
  AuthStateEnums.LOGIN_SUCCESS,
  (user: IUser) => ({ isPending: false, isSuccess: true, isError: false, isAuthenticated: true, user })
);

export const loginError = createAction<IAuthStateContext>(
  AuthStateEnums.LOGIN_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true, isAuthenticated: false })
);

export const registerPending = createAction<IAuthStateContext>(
  AuthStateEnums.REGISTER_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, isAuthenticated: false })
);

export const registerSuccess = createAction<IAuthStateContext, IUser>(
  AuthStateEnums.REGISTER_SUCCESS,
  (user: IUser) => ({ isPending: false, isSuccess: true, isError: false, isAuthenticated: true, user })
);

export const registerError = createAction<IAuthStateContext>(
  AuthStateEnums.REGISTER_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true, isAuthenticated: false })
);

export const logoutPending = createAction<IAuthStateContext>(
  AuthStateEnums.LOGOUT_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, isAuthenticated: false })
);

export const logoutSuccess = createAction<IAuthStateContext>(
  AuthStateEnums.LOGOUT_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false, isAuthenticated: false, user: undefined })
);

export const logoutError = createAction<IAuthStateContext>(
  AuthStateEnums.LOGOUT_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true, isAuthenticated: false })
);
