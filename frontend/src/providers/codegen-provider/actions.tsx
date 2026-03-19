import { createAction } from "redux-actions";
import {
  ICodeGenSession,
  ICodeGenStateContext,
  IGenerationStatus,
  IStackRecommendation,
} from "./context";

type CodeGenPayload = Partial<ICodeGenStateContext>;

export enum CodeGenStateEnums {
  CREATE_SESSION_PENDING = "CODEGEN_CREATE_SESSION_PENDING",
  CREATE_SESSION_SUCCESS = "CODEGEN_CREATE_SESSION_SUCCESS",
  CREATE_SESSION_ERROR = "CODEGEN_CREATE_SESSION_ERROR",

  RECOMMEND_STACK_PENDING = "CODEGEN_RECOMMEND_STACK_PENDING",
  RECOMMEND_STACK_SUCCESS = "CODEGEN_RECOMMEND_STACK_SUCCESS",
  RECOMMEND_STACK_ERROR = "CODEGEN_RECOMMEND_STACK_ERROR",

  SAVE_STACK_PENDING = "CODEGEN_SAVE_STACK_PENDING",
  SAVE_STACK_SUCCESS = "CODEGEN_SAVE_STACK_SUCCESS",
  SAVE_STACK_ERROR = "CODEGEN_SAVE_STACK_ERROR",

  GENERATE_SPEC_PENDING = "CODEGEN_GENERATE_SPEC_PENDING",
  GENERATE_SPEC_SUCCESS = "CODEGEN_GENERATE_SPEC_SUCCESS",
  GENERATE_SPEC_ERROR = "CODEGEN_GENERATE_SPEC_ERROR",

  SAVE_SPEC_PENDING = "CODEGEN_SAVE_SPEC_PENDING",
  SAVE_SPEC_SUCCESS = "CODEGEN_SAVE_SPEC_SUCCESS",
  SAVE_SPEC_ERROR = "CODEGEN_SAVE_SPEC_ERROR",

  CONFIRM_SPEC_PENDING = "CODEGEN_CONFIRM_SPEC_PENDING",
  CONFIRM_SPEC_SUCCESS = "CODEGEN_CONFIRM_SPEC_SUCCESS",
  CONFIRM_SPEC_ERROR = "CODEGEN_CONFIRM_SPEC_ERROR",

  START_GENERATION_PENDING = "CODEGEN_START_GENERATION_PENDING",
  START_GENERATION_SUCCESS = "CODEGEN_START_GENERATION_SUCCESS",
  START_GENERATION_ERROR = "CODEGEN_START_GENERATION_ERROR",

  POLL_STATUS_SUCCESS = "CODEGEN_POLL_STATUS_SUCCESS",

  TRIGGER_REPAIR_PENDING = "CODEGEN_TRIGGER_REPAIR_PENDING",
  TRIGGER_REPAIR_SUCCESS = "CODEGEN_TRIGGER_REPAIR_SUCCESS",
  TRIGGER_REPAIR_ERROR = "CODEGEN_TRIGGER_REPAIR_ERROR",

  RESET_SESSION = "CODEGEN_RESET_SESSION",
}

// ─── Create Session ──────────────────────────────────────────────────────────

export const createSessionPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.CREATE_SESSION_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const createSessionSuccess = createAction(
  CodeGenStateEnums.CREATE_SESSION_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const createSessionError = createAction(
  CodeGenStateEnums.CREATE_SESSION_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Recommend Stack ─────────────────────────────────────────────────────────

export const recommendStackPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.RECOMMEND_STACK_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const recommendStackSuccess = createAction(
  CodeGenStateEnums.RECOMMEND_STACK_SUCCESS,
  (recommendation: IStackRecommendation): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    recommendation,
  })
);

export const recommendStackError = createAction(
  CodeGenStateEnums.RECOMMEND_STACK_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Save Stack ──────────────────────────────────────────────────────────────

export const saveStackPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.SAVE_STACK_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const saveStackSuccess = createAction(
  CodeGenStateEnums.SAVE_STACK_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const saveStackError = createAction(
  CodeGenStateEnums.SAVE_STACK_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Generate Spec ───────────────────────────────────────────────────────────

export const generateSpecPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.GENERATE_SPEC_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const generateSpecSuccess = createAction(
  CodeGenStateEnums.GENERATE_SPEC_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const generateSpecError = createAction(
  CodeGenStateEnums.GENERATE_SPEC_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Save Spec ───────────────────────────────────────────────────────────────

export const saveSpecPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.SAVE_SPEC_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const saveSpecSuccess = createAction(
  CodeGenStateEnums.SAVE_SPEC_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const saveSpecError = createAction(
  CodeGenStateEnums.SAVE_SPEC_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Confirm Spec ────────────────────────────────────────────────────────────

export const confirmSpecPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.CONFIRM_SPEC_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const confirmSpecSuccess = createAction(
  CodeGenStateEnums.CONFIRM_SPEC_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const confirmSpecError = createAction(
  CodeGenStateEnums.CONFIRM_SPEC_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Start Generation ────────────────────────────────────────────────────────

export const startGenerationPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.START_GENERATION_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const startGenerationSuccess = createAction(
  CodeGenStateEnums.START_GENERATION_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const startGenerationError = createAction(
  CodeGenStateEnums.START_GENERATION_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Poll Status ─────────────────────────────────────────────────────────────

export const pollStatusSuccess = createAction(
  CodeGenStateEnums.POLL_STATUS_SUCCESS,
  (generationStatus: IGenerationStatus): CodeGenPayload => ({
    generationStatus,
  })
);

// ─── Repair ──────────────────────────────────────────────────────────────────

export const triggerRepairPending = createAction<CodeGenPayload>(
  CodeGenStateEnums.TRIGGER_REPAIR_PENDING,
  () => ({ isPending: true, isSuccess: false, isError: false, errorMessage: "" })
);

export const triggerRepairSuccess = createAction(
  CodeGenStateEnums.TRIGGER_REPAIR_SUCCESS,
  (session: ICodeGenSession): CodeGenPayload => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    session,
  })
);

export const triggerRepairError = createAction(
  CodeGenStateEnums.TRIGGER_REPAIR_ERROR,
  (errorMessage: string): CodeGenPayload => ({
    isPending: false,
    isSuccess: false,
    isError: true,
    errorMessage,
  })
);

// ─── Reset ───────────────────────────────────────────────────────────────────

export const resetSession = createAction<CodeGenPayload>(
  CodeGenStateEnums.RESET_SESSION,
  () => ({
    isPending: false,
    isSuccess: false,
    isError: false,
    session: null,
    recommendation: null,
    generationStatus: null,
    errorMessage: "",
  })
);
