import { handleActions } from "redux-actions";
import { INITIAL_STATE, ICodeGenStateContext } from "./context";
import { CodeGenStateEnums } from "./actions";

export const CodeGenReducer = handleActions<ICodeGenStateContext, Partial<ICodeGenStateContext>>(
  {
    [CodeGenStateEnums.CREATE_SESSION_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.CREATE_SESSION_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.CREATE_SESSION_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.RECOMMEND_STACK_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.RECOMMEND_STACK_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.RECOMMEND_STACK_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.SAVE_STACK_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.SAVE_STACK_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.SAVE_STACK_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.GENERATE_SPEC_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.GENERATE_SPEC_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.GENERATE_SPEC_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.SAVE_SPEC_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.SAVE_SPEC_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.SAVE_SPEC_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.CONFIRM_SPEC_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.CONFIRM_SPEC_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.CONFIRM_SPEC_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.START_GENERATION_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.START_GENERATION_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.START_GENERATION_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.POLL_STATUS_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.TRIGGER_REPAIR_PENDING]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.TRIGGER_REPAIR_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [CodeGenStateEnums.TRIGGER_REPAIR_ERROR]: (state, { payload }) => ({ ...state, ...payload }),

    [CodeGenStateEnums.RESET_SESSION]: (state, { payload }) => ({ ...state, ...payload }),
  },
  INITIAL_STATE
);
