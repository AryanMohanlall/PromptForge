"use client";

import { createContext } from "react";

// ─── Enums ───────────────────────────────────────────────────────────────────

export enum CodeGenStatus {
  Captured = 1,
  StackConfirmed = 2,
  SpecGenerated = 3,
  SpecConfirmed = 4,
  Generating = 5,
  Generated = 6,
  ValidationRunning = 7,
  ValidationPassed = 8,
  ValidationFailed = 9,
  Committed = 10,
  Deployed = 11,
}

export enum GenerationMode {
  Full = 1,
  Refinement = 2,
  Repair = 3,
}

// ─── Stack Types ─────────────────────────────────────────────────────────────

export interface IStackConfig {
  framework: string;
  language: string;
  styling: string;
  database: string;
  orm: string;
  auth: string;
  reasoning: Record<string, string>;
}

// ─── Spec Types ──────────────────────────────────────────────────────────────

export interface IFieldSpec {
  name: string;
  type: "string" | "int" | "float" | "boolean" | "datetime" | "enum" | "json";
  required: boolean;
  unique?: boolean;
  default?: unknown;
  maxLength?: number;
  enumValues?: string[];
  description: string;
}

export interface IRelationSpec {
  type: "one-to-one" | "one-to-many" | "many-to-many";
  target: string;
  foreignKey?: string;
}

export interface IEntitySpec {
  name: string;
  tableName: string;
  fields: IFieldSpec[];
  relations: IRelationSpec[];
}

export interface IPageSpec {
  route: string;
  name: string;
  layout: "authenticated" | "public" | "admin";
  components: string[];
  dataRequirements: string[];
  description: string;
}

export interface IApiRouteSpec {
  method: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  path: string;
  handler: string;
  requestBody?: unknown;
  responseShape: unknown;
  auth: boolean;
  description: string;
}

export interface IValidationRule {
  id: string;
  category:
    | "file-exists"
    | "entity-schema"
    | "route-exists"
    | "build-passes"
    | "lint-passes"
    | "env-vars"
    | "test-passes"
    | "auth-guard"
    | "type-check"
    | "api-returns";
  description: string;
  target: string;
  assertion: string;
  automatable: boolean;
  script?: string;
}

export interface IFileEntry {
  path: string;
  type: "scaffold" | "generated" | "static";
  description: string;
}

// ─── Dependency Plan Types ───────────────────────────────────────────────────

export interface IDependencyItem {
  name: string;
  version: string;
  purpose: string;
  isExisting: boolean;
}

export interface IDependencyPlan {
  dependencies: IDependencyItem[];
  devDependencies: IDependencyItem[];
  envVars: Record<string, string>;
}

export interface IAppSpec {
  entities: IEntitySpec[];
  pages: IPageSpec[];
  apiRoutes: IApiRouteSpec[];
  validations: IValidationRule[];
  fileManifest: IFileEntry[];
  dependencyPlan: IDependencyPlan;
  architectureNotes: string;
}

// ─── Refinement Types ────────────────────────────────────────────────────────

export interface IRefinementInput {
  sessionId: string;
  changeRequest: string;
  affectedFiles: string[];
}

export interface IRefinementResult {
  changedFiles: IGeneratedFile[];
  deletedFiles: string[];
  summary: string;
  validationResults: IValidationResult[];
}

// ─── Session ─────────────────────────────────────────────────────────────────

export interface IGeneratedFile {
  path: string;
  content: string;
}

export interface IValidationResult {
  id: string;
  status: "pending" | "running" | "passed" | "failed";
  message?: string;
}

export interface ICodeGenSession {
  id: string;
  userId: number;
  projectId: number | null;
  projectName: string;
  prompt: string;
  normalizedRequirement: string;
  detectedFeatures: string[];
  detectedEntities: string[];
  confirmedStack: IStackConfig | null;
  spec: IAppSpec | null;
  specConfirmedAt: string | null;
  generationStartedAt: string | null;
  generationCompletedAt: string | null;
  status: CodeGenStatus;
  validationResults: IValidationResult[];
  scaffoldTemplate: string;
  generatedFiles: IGeneratedFile[];
  repairAttempts: number;
  isPublic: boolean;
  generationMode: string | null;
  createdAt: string;
  updatedAt: string;
}

// ─── Recommendation ──────────────────────────────────────────────────────────

export interface IStackRecommendation {
  framework: string;
  language: string;
  styling: string;
  database: string;
  orm: string;
  auth: string;
  reasoning: Record<string, string>;
}

// ─── Generation Status ───────────────────────────────────────────────────────

export interface IGenerationStatus {
  currentPhase: string;
  completedSteps: string[];
  validationResults: IValidationResult[];
  isComplete: boolean;
  errorMessage?: string;
}

// ─── README Result ───────────────────────────────────────────────────────────

export interface IReadmeResult {
  readmeMarkdown: string;
  summary: string;
  plan?: IAppSpec | null;
}

// ─── State & Action Contexts ─────────────────────────────────────────────────

export interface ICodeGenStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  session: ICodeGenSession | null;
  recommendation: IStackRecommendation | null;
  generationStatus: IGenerationStatus | null;
  refinementResult: IRefinementResult | null;
  errorMessage: string;
}

export interface ICodeGenActionContext {
  createSession: (prompt: string) => Promise<ICodeGenSession>;
  recommendStack: (sessionId: string) => Promise<IStackRecommendation>;
  saveStack: (sessionId: string, stack: IStackConfig) => Promise<void>;
  generateSpec: (sessionId: string) => Promise<IAppSpec>;
  saveSpec: (sessionId: string, spec: IAppSpec) => Promise<void>;
  confirmSpec: (sessionId: string) => Promise<void>;
  generateReadme: (sessionId: string) => Promise<IReadmeResult>;
  confirmReadme: (sessionId: string) => Promise<void>;
  startGeneration: (sessionId: string) => Promise<void>;
  pollStatus: (sessionId: string) => Promise<IGenerationStatus>;
  triggerRepair: (sessionId: string, failures: IValidationResult[]) => Promise<void>;
  refineSession: (input: IRefinementInput) => Promise<IRefinementResult>;
  resetSession: () => void;
}

export const INITIAL_STATE: ICodeGenStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  session: null,
  recommendation: null,
  generationStatus: null,
  refinementResult: null,
  errorMessage: "",
};

export const CodeGenStateContext = createContext<ICodeGenStateContext>(INITIAL_STATE);
export const CodeGenActionContext = createContext<ICodeGenActionContext>({
  createSession: async () => ({} as ICodeGenSession),
  recommendStack: async () => ({} as IStackRecommendation),
  saveStack: async () => {},
  generateSpec: async () => ({} as IAppSpec),
  saveSpec: async () => {},
  confirmSpec: async () => {},
  generateReadme: async () => ({} as IReadmeResult),
  confirmReadme: async () => {},
  startGeneration: async () => {},
  pollStatus: async () => ({} as IGenerationStatus),
  triggerRepair: async () => {},
  refineSession: async () => ({} as IRefinementResult),
  resetSession: () => {},
});
