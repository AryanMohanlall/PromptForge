import { createAction } from "redux-actions";
import { IProjectItem, IProjectStateContext } from "./context";

type ProjectStatePayload = Partial<IProjectStateContext>;

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

export const fetchAllPending = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_FETCH_ALL_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const fetchAllSuccess = createAction(
  ProjectStateEnums.PROJECT_FETCH_ALL_SUCCESS,
  ({ items, totalCount }: { items: IProjectItem[]; totalCount: number }): ProjectStatePayload => ({
  isPending: false,
  isSuccess: true,
  isError: false,
  items,
  totalCount,
  })
);

export const fetchAllError = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_FETCH_ALL_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const fetchOnePending = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_FETCH_ONE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const fetchOneSuccess = createAction(
  ProjectStateEnums.PROJECT_FETCH_ONE_SUCCESS,
  (selected: IProjectItem): ProjectStatePayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    selected,
  })
);

export const fetchOneError = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_FETCH_ONE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const createPending = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_CREATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createSuccess = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_CREATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const createError = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_CREATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const updatePending = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_UPDATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateSuccess = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_UPDATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const updateError = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_UPDATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const deletePending = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_DELETE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteSuccess = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_DELETE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false })
);

export const deleteError = createAction<ProjectStatePayload>(
  ProjectStateEnums.PROJECT_DELETE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
