import { createAction } from "redux-actions";
import { IProjectItem, IProjectStateContext } from "./context";

export enum ProjectStateEnums {
  PROJECT_FETCH_ALL_PENDING = "PROJECT_FETCH_ALL_PENDING",
  PROJECT_FETCH_ALL_SUCCESS = "PROJECT_FETCH_ALL_SUCCESS",
  PROJECT_FETCH_ALL_ERROR = "PROJECT_FETCH_ALL_ERROR",
  PROJECT_FETCH_ONE_PENDING = "PROJECT_FETCH_ONE_PENDING",
  PROJECT_FETCH_ONE_SUCCESS = "PROJECT_FETCH_ONE_SUCCESS",
  PROJECT_FETCH_ONE_ERROR = "PROJECT_FETCH_ONE_ERROR",
  PROJECT_CREATE_PENDING = "PROJECT_CREATE_PENDING",
  PROJECT_CREATE_SUCCESS = "PROJECT_CREATE_SUCCESS",
  PROJECT_CREATE_ERROR = "PROJECT_CREATE_ERROR",
  PROJECT_UPDATE_PENDING = "PROJECT_UPDATE_PENDING",
  PROJECT_UPDATE_SUCCESS = "PROJECT_UPDATE_SUCCESS",
  PROJECT_UPDATE_ERROR = "PROJECT_UPDATE_ERROR",
  PROJECT_DELETE_PENDING = "PROJECT_DELETE_PENDING",
  PROJECT_DELETE_SUCCESS = "PROJECT_DELETE_SUCCESS",
  PROJECT_DELETE_ERROR = "PROJECT_DELETE_ERROR",
}

export const fetchAllPending = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_FETCH_ALL_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const fetchAllSuccess = createAction<
  IProjectStateContext,
  { items: IProjectItem[]; totalCount: number }
>(ProjectStateEnums.PROJECT_FETCH_ALL_SUCCESS, ({ items, totalCount }) => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  items,
  totalCount,
}));

export const fetchAllError = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_FETCH_ALL_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const fetchOnePending = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_FETCH_ONE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const fetchOneSuccess = createAction<IProjectStateContext, IProjectItem>(
  ProjectStateEnums.PROJECT_FETCH_ONE_SUCCESS,
  selected => ({ isPending: false, isSuccess: true, isError: false, selected })
);

export const fetchOneError = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_FETCH_ONE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const createPending = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_CREATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createSuccess = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_CREATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const createError = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_CREATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const updatePending = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_UPDATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSuccess = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_UPDATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const updateError = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_UPDATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const deletePending = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_DELETE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteSuccess = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_DELETE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const deleteError = createAction<IProjectStateContext>(
  ProjectStateEnums.PROJECT_DELETE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
