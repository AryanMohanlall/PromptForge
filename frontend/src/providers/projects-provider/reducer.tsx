import { handleActions } from "redux-actions";
import { INITIAL_STATE, IProjectStateContext } from "./context";
import { ProjectStateEnums } from "./actions";

export const ProjectReducer = handleActions<IProjectStateContext, IProjectStateContext>(
  {
    [ProjectStateEnums.PROJECT_FETCH_ALL_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_FETCH_ALL_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_FETCH_ALL_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_FETCH_ONE_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_FETCH_ONE_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_FETCH_ONE_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_CREATE_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_CREATE_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_CREATE_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_UPDATE_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_UPDATE_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_UPDATE_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_DELETE_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_DELETE_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [ProjectStateEnums.PROJECT_DELETE_ERROR]: (state, { payload }) => ({ ...state, ...payload }),
  },
  INITIAL_STATE
);
