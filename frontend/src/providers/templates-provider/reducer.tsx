import { handleActions } from "redux-actions";
import { INITIAL_STATE, ITemplateStateContext } from "./context";
import { TemplateStateEnums } from "./actions";

export const TemplateReducer = handleActions<
  ITemplateStateContext,
  Partial<ITemplateStateContext>
>(
  {
    [TemplateStateEnums.TEMPLATE_FETCH_ALL_PENDING]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_FETCH_ALL_SUCCESS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_FETCH_ALL_ERROR]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_FETCH_ONE_PENDING]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_FETCH_ONE_SUCCESS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_FETCH_ONE_ERROR]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_CREATE_PENDING]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_CREATE_SUCCESS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_CREATE_ERROR]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_UPDATE_PENDING]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_UPDATE_SUCCESS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_UPDATE_ERROR]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_DELETE_PENDING]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_DELETE_SUCCESS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_DELETE_ERROR]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
    [TemplateStateEnums.TEMPLATE_CLEAR_ITEMS]: (state, { payload }) => ({
      ...state,
      ...payload,
    }),
  },
  INITIAL_STATE,
);
