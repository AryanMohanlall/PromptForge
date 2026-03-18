import { createAction } from "redux-actions";
import { ITemplateItem, ITemplateStateContext } from "./context";

type TemplateStatePayload = Partial<ITemplateStateContext>;

export enum TemplateStateEnums {
  TEMPLATE_FETCH_ALL_PENDING = "TEMPLATE_FETCH_ALL_PENDING",
  TEMPLATE_FETCH_ALL_SUCCESS = "TEMPLATE_FETCH_ALL_SUCCESS",
  TEMPLATE_FETCH_ALL_ERROR = "TEMPLATE_FETCH_ALL_ERROR",
  TEMPLATE_FETCH_ONE_PENDING = "TEMPLATE_FETCH_ONE_PENDING",
  TEMPLATE_FETCH_ONE_SUCCESS = "TEMPLATE_FETCH_ONE_SUCCESS",
  TEMPLATE_FETCH_ONE_ERROR = "TEMPLATE_FETCH_ONE_ERROR",
  TEMPLATE_CREATE_PENDING = "TEMPLATE_CREATE_PENDING",
  TEMPLATE_CREATE_SUCCESS = "TEMPLATE_CREATE_SUCCESS",
  TEMPLATE_CREATE_ERROR = "TEMPLATE_CREATE_ERROR",
  TEMPLATE_UPDATE_PENDING = "TEMPLATE_UPDATE_PENDING",
  TEMPLATE_UPDATE_SUCCESS = "TEMPLATE_UPDATE_SUCCESS",
  TEMPLATE_UPDATE_ERROR = "TEMPLATE_UPDATE_ERROR",
  TEMPLATE_DELETE_PENDING = "TEMPLATE_DELETE_PENDING",
  TEMPLATE_DELETE_SUCCESS = "TEMPLATE_DELETE_SUCCESS",
  TEMPLATE_DELETE_ERROR = "TEMPLATE_DELETE_ERROR",
}

export const fetchAllPending = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_FETCH_ALL_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const fetchAllSuccess = createAction(
  TemplateStateEnums.TEMPLATE_FETCH_ALL_SUCCESS,
  ({
    items,
    totalCount,
  }: {
    items: ITemplateItem[];
    totalCount: number;
  }): TemplateStatePayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    items,
    totalCount,
  }),
);

export const fetchAllError = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_FETCH_ALL_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const fetchOnePending = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_FETCH_ONE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const fetchOneSuccess = createAction(
  TemplateStateEnums.TEMPLATE_FETCH_ONE_SUCCESS,
  (selected: ITemplateItem): TemplateStatePayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    selected,
  }),
);

export const fetchOneError = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_FETCH_ONE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const createPending = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_CREATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const createSuccess = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_CREATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false }),
);

export const createError = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_CREATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const updatePending = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_UPDATE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const updateSuccess = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_UPDATE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false }),
);

export const updateError = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_UPDATE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);

export const deletePending = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_DELETE_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false }),
);

export const deleteSuccess = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_DELETE_SUCCESS,
  () => ({ isPending: false, isSuccess: true, isError: false }),
);

export const deleteError = createAction<TemplateStatePayload>(
  TemplateStateEnums.TEMPLATE_DELETE_ERROR,
  () => ({ isPending: false, isSuccess: false, isError: true }),
);
