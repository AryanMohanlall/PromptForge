"use client";

import { createContext } from "react";

export interface IUser {
  userId: number;
  accessToken: string;
  expireInSeconds: number;
  userName?: string;
  name?: string;
  surname?: string;
  emailAddress?: string;
  roleNames: string[];
  roleName?: string;

  avatarUrl?: string;
}

export interface IAuthStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  isAuthenticated: boolean;
  isInitialized: boolean;
  isGithubConnected: boolean;
  hasCreatedProject: boolean;
  user?: IUser;
}

export interface IAuthActionContext {
  login: (
    userNameOrEmailAddress: string,
    password: string,
  ) => Promise<IUser | null>;
  register: (input: IRegisterInput) => Promise<void>;
  logout: () => Promise<void>;
  connectGithub: () => void;
  markProjectCreated: () => void;
}

export interface IRegisterInput {
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
  tenantId?: number;
  roleName?: string;
}

export const INITIAL_STATE: IAuthStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  isAuthenticated: false,
  isInitialized: false,
  isGithubConnected: false,
  hasCreatedProject: false,
};

export const AuthStateContext = createContext<IAuthStateContext>(INITIAL_STATE);
export const AuthActionContext = createContext<IAuthActionContext>({
  login: async () => null,
  register: async () => {},
  logout: async () => {},
  connectGithub: () => {},
  markProjectCreated: () => {},
});
