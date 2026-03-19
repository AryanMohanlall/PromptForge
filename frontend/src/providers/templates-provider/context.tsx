"use client";

import { createContext } from "react";
import {
  ProjectFramework,
  ProjectProgrammingLanguage,
  ProjectDatabaseOption,
} from "../projects-provider/context";

export enum TemplateCategory {
  AppsAndGames = 1,
  LandingPages = 2,
  Dashboards = 3,
  ECommerce = 4,
  Components = 5,
  AI = 6,
  Portfolio = 7,
  Blog = 8,
  SaaS = 9,
  Other = 99,
}

export enum TemplateStatus {
  Draft = 1,
  Active = 2,
  Deprecated = 3,
}

export interface ITemplateItem {
  id: number;
  name: string;
  description?: string | null;
  author?: string | null;
  category: TemplateCategory;
  categoryName?: string | null;
  framework: ProjectFramework;
  language: ProjectProgrammingLanguage;
  database: ProjectDatabaseOption;
  includesAuth: boolean;
  tags: string[];
  thumbnailUrl?: string | null;
  previewUrl?: string | null;
  status: TemplateStatus;
  version: string;
  isFeatured: boolean;
  forkCount: number;
  isFavorite: boolean;
  createdAt: string;
}

export interface ITemplateCreateInput {
  name: string;
  description?: string;
  author?: string;
  category: TemplateCategory;
  framework: ProjectFramework;
  language: ProjectProgrammingLanguage;
  database: ProjectDatabaseOption;
  includesAuth: boolean;
  tags?: string; // Comma-separated
  thumbnailUrl?: string;
  previewUrl?: string;
  status: TemplateStatus;
  version: string;
  isFeatured: boolean;
  scaffoldConfig?: string;
}

export interface ITemplateUpdateInput extends ITemplateCreateInput {
  id: number;
}

export interface ITemplateListInput {
  category?: TemplateCategory;
  framework?: ProjectFramework;
  database?: ProjectDatabaseOption;
  includesAuth?: boolean;
  status?: TemplateStatus;
  searchTerm?: string;
  isFeatured?: boolean;
  isMyTemplates?: boolean;
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
}

export interface ITemplateStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  items: ITemplateItem[];
  selected?: ITemplateItem;
  totalCount: number;
}

export interface ITemplateActionContext {
  fetchAll: (input?: ITemplateListInput) => Promise<void>;
  fetchById: (id: number) => Promise<void>;
  create: (data: ITemplateCreateInput) => Promise<void>;
  update: (data: ITemplateUpdateInput) => Promise<void>;
  remove: (id: number) => Promise<void>;
  publish: (id: number) => Promise<void>;
  deprecate: (id: number) => Promise<void>;
  setFeatured: (id: number, featured: boolean) => Promise<void>;
  toggleFavorite: (id: number) => Promise<void>;
}

export const INITIAL_STATE: ITemplateStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  items: [],
  totalCount: 0,
};

export const TemplateStateContext =
  createContext<ITemplateStateContext>(INITIAL_STATE);
export const TemplateActionContext = createContext<ITemplateActionContext>({
  fetchAll: async () => {},
  fetchById: async () => {},
  create: async () => {},
  update: async () => {},
  remove: async () => {},
  publish: async () => {},
  deprecate: async () => {},
  setFeatured: async () => {},
  toggleFavorite: async () => {},
});
