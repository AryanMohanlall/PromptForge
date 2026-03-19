import { describe, expect, it } from "vitest";
import { CodeGenReducer } from "./reducer";
import { INITIAL_STATE, CodeGenStatus } from "./context";
import {
  createSessionPending,
  createSessionSuccess,
  createSessionError,
  recommendStackSuccess,
  resetSession,
} from "./actions";
import type { ICodeGenSession, IStackRecommendation } from "./context";

const mockSession: ICodeGenSession = {
  id: "abc-123",
  userId: 1,
  projectId: null,
  projectName: "test-app",
  prompt: "Build a todo app",
  normalizedRequirement: "A todo application",
  detectedFeatures: ["authentication", "crud"],
  detectedEntities: ["User", "Todo"],
  confirmedStack: null,
  spec: null,
  specConfirmedAt: null,
  generationStartedAt: null,
  generationCompletedAt: null,
  status: CodeGenStatus.Captured,
  validationResults: [],
  scaffoldTemplate: "next-ts-antd-prisma",
  generatedFiles: [],
  repairAttempts: 0,
  createdAt: "2026-01-01T00:00:00Z",
  updatedAt: "2026-01-01T00:00:00Z",
};

describe("CodeGenReducer", () => {
  it("sets pending state on createSession pending", () => {
    const next = CodeGenReducer(INITIAL_STATE, createSessionPending());
    expect(next.isPending).toBe(true);
    expect(next.isSuccess).toBe(false);
    expect(next.isError).toBe(false);
  });

  it("stores session on createSession success", () => {
    const next = CodeGenReducer(INITIAL_STATE, createSessionSuccess(mockSession));
    expect(next.isPending).toBe(false);
    expect(next.isSuccess).toBe(true);
    expect(next.session).toEqual(mockSession);
  });

  it("sets error on createSession error", () => {
    const next = CodeGenReducer(INITIAL_STATE, createSessionError("Network error"));
    expect(next.isError).toBe(true);
    expect(next.errorMessage).toBe("Network error");
  });

  it("stores recommendation on recommendStack success", () => {
    const rec: IStackRecommendation = {
      framework: "Next.js",
      language: "TypeScript",
      styling: "Ant Design",
      database: "PostgreSQL",
      orm: "Prisma",
      auth: "NextAuth.js",
      reasoning: { framework: "Best for SSR" },
    };
    const next = CodeGenReducer(INITIAL_STATE, recommendStackSuccess(rec));
    expect(next.recommendation).toEqual(rec);
  });

  it("resets state on resetSession", () => {
    const dirty = {
      ...INITIAL_STATE,
      isPending: true,
      session: mockSession,
    };
    const next = CodeGenReducer(dirty, resetSession());
    expect(next.isPending).toBe(false);
    expect(next.session).toBeNull();
    expect(next.recommendation).toBeNull();
  });
});
