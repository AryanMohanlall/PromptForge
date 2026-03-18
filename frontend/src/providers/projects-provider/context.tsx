"use client";

import { createContext } from "react";

export enum ProjectFramework {
  NextJS = 1,
  ReactVite = 2,
  Angular = 3,
  Vue = 4,
  DotNetBlazor = 5,
}

export enum ProjectProgrammingLanguage {
  TypeScript = 1,
  JavaScript = 2,
  CSharp = 3,
}

export enum ProjectDatabaseOption {
  RenderPostgres = 1,
  NeonPostgres = 2,
  MongoCloud = 3,
}

export enum ProjectStatus {
  Draft = 1,
  PromptSubmitted = 2,
  CodeGenerationInProgress = 3,
  CodeGenerationCompleted = 8,
  RepositoryPushInProgress = 4,
  Deployed = 5,
  Failed = 6,
  Archived = 7,
}

export interface IProjectItem {
  id: number;
  workspaceId: number;
  promptId?: number | null;
  name: string;
  prompt: string;
  promptVersion: number;
  promptSubmittedAt?: string | null;
  framework: ProjectFramework;
  language: ProjectProgrammingLanguage;
  databaseOption: ProjectDatabaseOption;
  includeAuth: boolean;
  status: ProjectStatus;
  statusMessage?: string | null;
  templateId?: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface IProjectCreateInput {
  workspaceId?: number;
  promptId?: number | null;
  name: string;
  prompt: string;
  promptVersion?: number;
  promptSubmittedAt?: string | null;
  framework: ProjectFramework;
  language: ProjectProgrammingLanguage;
  databaseOption: ProjectDatabaseOption;
  includeAuth: boolean;
  status?: ProjectStatus;
  templateId?: number;
}

export interface IProjectUpdateInput extends IProjectCreateInput {
  id: number;
}

export interface IProjectStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  items: IProjectItem[];
  selected?: IProjectItem;
  totalCount: number;
}

export interface IProjectActionContext {
  fetchAll: () => Promise<void>;
  fetchById: (id: number) => Promise<void>;
  create: (data: IProjectCreateInput) => Promise<IProjectItem>;
  update: (data: IProjectUpdateInput) => Promise<void>;
  remove: (id: number) => Promise<void>;
}

export const INITIAL_STATE: IProjectStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  items: [],
  totalCount: 0,
};

export const ProjectStateContext = createContext<IProjectStateContext>(INITIAL_STATE);
export const ProjectActionContext = createContext<IProjectActionContext>({
  fetchAll: async () => {},
  fetchById: async () => {},
  create: async () => ({} as IProjectItem),
  update: async () => {},
  remove: async () => {},
});
