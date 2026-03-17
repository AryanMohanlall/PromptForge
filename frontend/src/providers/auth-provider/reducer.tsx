import { INITIAL_STATE, type IAuthStateContext } from "./context";
import { AuthStateEnums, type AuthAction } from "./actions";

const handledActions = new Set<AuthStateEnums>([
  AuthStateEnums.LOGIN_PENDING,
  AuthStateEnums.LOGIN_SUCCESS,
  AuthStateEnums.LOGIN_ERROR,
  AuthStateEnums.REGISTER_PENDING,
  AuthStateEnums.REGISTER_SUCCESS,
  AuthStateEnums.REGISTER_ERROR,
  AuthStateEnums.LOGOUT_PENDING,
  AuthStateEnums.LOGOUT_SUCCESS,
  AuthStateEnums.LOGOUT_ERROR,
  AuthStateEnums.LOAD_LOCAL_STATE,
  AuthStateEnums.GITHUB_CONNECT,
  AuthStateEnums.PROJECT_CREATED,
]);

export const AuthReducer = (
  state: IAuthStateContext = INITIAL_STATE,
  action: AuthAction
): IAuthStateContext => {
  if (!handledActions.has(action.type)) {
    return state;
  }

  return {
    ...state,
    ...action.payload,
  };
};
