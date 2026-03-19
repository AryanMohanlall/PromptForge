"use client";

import { createContext } from "react";

export interface ITemplateItem {
  id: number;
  name: string;
  slug: string;
  description?: string | null;
  previewImageUrl?: string | null;
  category: string;
  tags?: string | null;
  author?: string | null;
  sourceUrl?: string | null;
  lastUpdatedAt?: string | null;
  likeCount?: number | null;
  viewCount?: number | null;
}

export interface ITemplateCreateInput {
  name: string;
  slug: string;
  description?: string;
  previewImageUrl?: string;
  category: string;
  tags?: string;
  author?: string;
  sourceUrl?: string;
  lastUpdatedAt?: string | null;
  likeCount?: number | null;
  viewCount?: number | null;
}

export interface ITemplateUpdateInput extends ITemplateCreateInput {
  id: number;
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
  fetchAll: () => Promise<void>;
  fetchById: (id: number) => Promise<void>;
  create: (data: ITemplateCreateInput) => Promise<void>;
  update: (data: ITemplateUpdateInput) => Promise<void>;
  remove: (id: number) => Promise<void>;
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
});
