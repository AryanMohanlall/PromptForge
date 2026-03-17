import { handleActions } from "redux-actions";
import { INITIAL_STATE, type IAuthStateContext } from "./context";
import { AuthStateEnums } from "./actions";

export const AuthReducer = handleActions<IAuthStateContext, IAuthStateContext>(
  {
    [AuthStateEnums.LOGIN_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.LOGIN_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.LOGIN_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.REGISTER_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.REGISTER_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.REGISTER_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.LOGOUT_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.LOGOUT_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.LOGOUT_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
  },
  INITIAL_STATE
);
