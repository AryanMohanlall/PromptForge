"use client";

import { ReactNode, useCallback, useContext, useReducer, useRef } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import { CodeGenReducer } from "./reducer";
import {
  CodeGenActionContext,
  CodeGenStateContext,
  IAppSpec,
  ICodeGenSession,
  IGenerationStatus,
  IStackConfig,
  IStackRecommendation,
  IValidationResult,
  INITIAL_STATE,
} from "./context";
import {
  confirmSpecError,
  confirmSpecPending,
  confirmSpecSuccess,
  createSessionError,
  createSessionPending,
  createSessionSuccess,
  generateSpecError,
  generateSpecPending,
  generateSpecSuccess,
  pollStatusSuccess,
  recommendStackError,
  recommendStackPending,
  recommendStackSuccess,
  resetSession as resetSessionAction,
  saveSpecError,
  saveSpecPending,
  saveSpecSuccess,
  saveStackError,
  saveStackPending,
  saveStackSuccess,
  startGenerationError,
  startGenerationPending,
  startGenerationSuccess,
  triggerRepairError,
  triggerRepairPending,
  triggerRepairSuccess,
} from "./actions";

const ENDPOINT = "/api/services/app/CodeGen";

interface AbpResult<T> {
  result: T;
}

export const CodeGenProvider = ({ children }: { children: ReactNode }) => {
  const instance = getAxiosInstance();
  const [state, dispatch] = useReducer(CodeGenReducer, INITIAL_STATE);
  const inFlightSpecRequests = useRef<Record<string, Promise<IAppSpec>>>({});

  const normalizeSpec = useCallback((spec: IAppSpec | null | undefined): IAppSpec => ({
    entities: spec?.entities ?? [],
    pages: spec?.pages ?? [],
    apiRoutes: spec?.apiRoutes ?? [],
    validations: spec?.validations ?? [],
    fileManifest: spec?.fileManifest ?? [],
  }), []);

  const createSession = useCallback(async (prompt: string): Promise<ICodeGenSession> => {
    dispatch(createSessionPending());
    try {
      const res = await instance.post<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/CreateSession`,
        { prompt }
      );
      const session = res.data.result;
      dispatch(createSessionSuccess(session));
      return session;
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to create session";
      dispatch(createSessionError(msg));
      throw error;
    }
  }, [instance]);

  const recommendStack = useCallback(async (sessionId: string): Promise<IStackRecommendation> => {
    dispatch(recommendStackPending());
    try {
      const res = await instance.post<AbpResult<IStackRecommendation>>(
        `${ENDPOINT}/RecommendStack`,
        null,
        { params: { sessionId } }
      );
      const recommendation = res.data.result;
      dispatch(recommendStackSuccess(recommendation));
      return recommendation;
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to recommend stack";
      dispatch(recommendStackError(msg));
      throw error;
    }
  }, [instance]);

  const saveStack = useCallback(async (sessionId: string, stack: IStackConfig): Promise<void> => {
    dispatch(saveStackPending());
    try {
      const res = await instance.put<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/SaveStack`,
        { sessionId, stack }
      );
      dispatch(saveStackSuccess(res.data.result));
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to save stack";
      dispatch(saveStackError(msg));
      throw error;
    }
  }, [instance]);

  const generateSpec = useCallback(async (sessionId: string): Promise<IAppSpec> => {
    const existingRequest = inFlightSpecRequests.current[sessionId];
    if (existingRequest !== undefined) {
      return existingRequest;
    }

    dispatch(generateSpecPending());
    const request = instance
      .post<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/GenerateSpec`,
        null,
        { params: { sessionId } }
      )
      .then((res) => {
        const session = res.data.result;
        dispatch(generateSpecSuccess(session));
        return normalizeSpec(session.spec);
      })
      .catch((error) => {
        const msg = error instanceof Error ? error.message : "Failed to generate spec";
        dispatch(generateSpecError(msg));
        throw error;
      })
      .finally(() => {
        delete inFlightSpecRequests.current[sessionId];
      });

    inFlightSpecRequests.current[sessionId] = request;
    return request;
  }, [instance, normalizeSpec]);

  const saveSpec = useCallback(async (sessionId: string, spec: IAppSpec): Promise<void> => {
    dispatch(saveSpecPending());
    try {
      const res = await instance.put<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/SaveSpec`,
        { sessionId, spec }
      );
      dispatch(saveSpecSuccess(res.data.result));
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to save spec";
      dispatch(saveSpecError(msg));
      throw error;
    }
  }, [instance]);

  const confirmSpec = useCallback(async (sessionId: string): Promise<void> => {
    dispatch(confirmSpecPending());
    try {
      const res = await instance.post<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/ConfirmSpec`,
        null,
        { params: { sessionId } }
      );
      dispatch(confirmSpecSuccess(res.data.result));
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to confirm spec";
      dispatch(confirmSpecError(msg));
      throw error;
    }
  }, [instance]);

  const startGeneration = useCallback(async (sessionId: string): Promise<void> => {
    dispatch(startGenerationPending());
    try {
      const res = await instance.post<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/Generate`,
        null,
        { params: { sessionId } }
      );
      dispatch(startGenerationSuccess(res.data.result));
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to start generation";
      dispatch(startGenerationError(msg));
      throw error;
    }
  }, [instance]);

  const pollStatus = useCallback(async (sessionId: string): Promise<IGenerationStatus> => {
    const res = await instance.get<AbpResult<IGenerationStatus>>(
      `${ENDPOINT}/GetStatus`,
      { params: { sessionId } }
    );
    const status = res.data.result;
    dispatch(pollStatusSuccess(status));
    return status;
  }, [instance]);

  const triggerRepair = useCallback(async (
    sessionId: string,
    failures: IValidationResult[]
  ): Promise<void> => {
    dispatch(triggerRepairPending());
    try {
      const res = await instance.post<AbpResult<ICodeGenSession>>(
        `${ENDPOINT}/Repair`,
        { sessionId, failures }
      );
      dispatch(triggerRepairSuccess(res.data.result));
    } catch (error) {
      const msg = error instanceof Error ? error.message : "Failed to trigger repair";
      dispatch(triggerRepairError(msg));
      throw error;
    }
  }, [instance]);

  const resetSessionFn = useCallback(() => {
    dispatch(resetSessionAction());
  }, []);

  return (
    <CodeGenStateContext.Provider value={state}>
      <CodeGenActionContext.Provider
        value={{
          createSession,
          recommendStack,
          saveStack,
          generateSpec,
          saveSpec,
          confirmSpec,
          startGeneration,
          pollStatus,
          triggerRepair,
          resetSession: resetSessionFn,
        }}
      >
        {children}
      </CodeGenActionContext.Provider>
    </CodeGenStateContext.Provider>
  );
};

export const useCodeGenState = () => {
  const context = useContext(CodeGenStateContext);
  if (!context) {
    throw new Error("useCodeGenState must be used within CodeGenProvider");
  }
  return context;
};

export const useCodeGenAction = () => {
  const context = useContext(CodeGenActionContext);
  if (!context) {
    throw new Error("useCodeGenAction must be used within CodeGenProvider");
  }
  return context;
};

export { CodeGenStatus } from "./context";
export type {
  IAppSpec,
  ICodeGenSession,
  ICodeGenStateContext,
  IGenerationStatus,
  IStackConfig,
  IStackRecommendation,
  IValidationResult,
  IValidationRule,
  IEntitySpec,
  IFieldSpec,
  IRelationSpec,
  IPageSpec,
  IApiRouteSpec,
  IFileEntry,
  IGeneratedFile,
} from "./context";
