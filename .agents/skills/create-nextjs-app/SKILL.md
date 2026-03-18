---
name: create-nextjs-app
description: Use this skill when scaffolding any full-stack Next.js app from scratch. Covers project init, dependencies, folder structure, auth, state management, API layer, styling, protected routes, CRUD pages, and testing — in a strict, repeatable step-by-step order.
---

# Full-Stack Next.js App — Code Generation Guide

## Overview

This guide is **project-agnostic**. Replace every `<AppName>`, `<Entity>`, and `<API_BASE_URL>` placeholder with values from the actual project spec. Follow every phase **in order** — do not skip or reorder steps.

---

## Stack Decisions (non-negotiable)

| Concern | Tool | Why |
|---|---|---|
| Framework | Next.js 15+ (App Router) | File-based routing, RSC, layouts |
| Language | TypeScript (strict) | Type safety across the full stack |
| UI Library | Ant Design (`antd`) | Comprehensive, production-ready components |
| CSS-in-JS | `antd-style` (`createStyles`) | Co-located styles, design-token-aware |
| State | React Context + `useReducer` + `redux-actions` | Predictable, no extra runtime |
| HTTP | `axios` (singleton instance) | Interceptors for auth, base URL |
| Cookies | `js-cookie` | Token persistence across tabs |
| Icons | `lucide-react` | Tree-shakeable, consistent |
| Testing | Vitest (unit) + Playwright (E2E) | Fast unit tests, real browser E2E |

---

## Phase 1 — Scaffold the Project

```bash
npx create-next-app@latest <app-name> \
  --typescript \
  --app \
  --src-dir \
  --no-tailwind \
  --eslint \
  --import-alias "@/*"

cd <app-name>
```

> **Checkpoint:** `npm run dev` shows the default Next.js page at `localhost:3000`.

---

## Phase 2 — Install Dependencies

Run all installs in one shot to avoid lockfile conflicts.

```bash
# Production
npm install \
  antd \
  antd-style \
  @ant-design/nextjs-registry \
  axios \
  js-cookie \
  redux-actions \
  lucide-react

# Dev
npm install --save-dev \
  @types/js-cookie \
  @types/redux-actions \
  vitest \
  @vitejs/plugin-react \
  @vitest/coverage-v8 \
  @testing-library/react \
  @testing-library/jest-dom \
  @testing-library/user-event \
  jsdom \
  @playwright/test
```

> **Checkpoint:** `npm ls antd antd-style axios redux-actions` shows no missing peer deps.

---

## Phase 3 — Folder Structure

Create this exact structure. Every folder listed must exist before writing any files.

```
src/
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   │   ├── page.tsx
│   │   │   └── styles/
│   │   │       └── style.ts
│   │   └── register/
│   │       ├── page.tsx
│   │       └── styles/
│   │           └── style.ts
│   ├── (dashboard)/
│   │   ├── layout.tsx          ← protected layout (withAuth)
│   │   ├── dashboard/
│   │   │   ├── page.tsx
│   │   │   └── styles/
│   │   │       └── style.ts
│   │   └── <entity>/           ← one folder per entity (plural noun)
│   │       ├── page.tsx
│   │       └── styles/
│   │           └── style.ts
│   ├── layout.tsx              ← root layout (AntdRegistry + AppProviders)
│   └── globals.css
├── components/
│   ├── layout/
│   │   ├── AppShell.tsx        ← sidebar + header shell
│   │   └── PageHeader.tsx
│   └── <entity>/               ← one folder per entity
│       ├── <Entity>Table.tsx
│       └── <Entity>Modal.tsx
├── hoc/
│   └── withAuth.tsx            ← HOC protecting dashboard routes
├── providers/
│   ├── index.tsx               ← composes all providers
│   ├── auth-provider/
│   │   ├── context.tsx
│   │   ├── actions.tsx
│   │   ├── reducer.tsx
│   │   └── index.tsx
│   └── <entity>-provider/     ← one folder per entity (kebab-case)
│       ├── context.tsx
│       ├── actions.tsx
│       ├── reducer.tsx
│       └── index.tsx
├── types/
│   └── index.ts                ← shared interfaces / enums
└── utils/
    └── axiosInstance.ts        ← singleton axios + token helpers
```

```bash
# Create all directories
mkdir -p src/app/\(auth\)/login/styles
mkdir -p src/app/\(auth\)/register/styles
mkdir -p src/app/\(dashboard\)/dashboard/styles
mkdir -p src/components/layout
mkdir -p src/hoc
mkdir -p src/providers/auth-provider
mkdir -p src/types
mkdir -p src/utils
```

---

## Phase 4 — Environment Variables

```env
# .env.local  (never commit — git-ignored by default)
NEXT_PUBLIC_API_URL=<API_BASE_URL>
```

> Rule: All client-visible env vars **must** be prefixed `NEXT_PUBLIC_`. Server-only secrets have no prefix.

---

## Phase 5 — Axios Instance

This is the **single** HTTP client for the entire app. Never create inline `axios.create()` elsewhere.

```typescript
// src/utils/axiosInstance.ts
import axios from 'axios';
import Cookies from 'js-cookie';

const BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3001';
const TOKEN_KEY = '<app-name>_token';  // ← replace <app-name>

let instance: ReturnType<typeof axios.create> | null = null;

export const getAxiosInstance = () => {
  if (!instance) {
    instance = axios.create({
      baseURL: BASE_URL,
      headers: { 'Content-Type': 'application/json' },
    });

    // Inject auth header on every request
    instance.interceptors.request.use((config) => {
      const token = Cookies.get(TOKEN_KEY);
      if (token) config.headers.Authorization = `Bearer ${token}`;
      return config;
    });

    // Global 401 handler → redirect to login
    instance.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          Cookies.remove(TOKEN_KEY);
          if (typeof window !== 'undefined') {
            window.location.href = '/login';
          }
        }
        return Promise.reject(error);
      }
    );
  }
  return instance;
};

export const setAuthToken = (token: string) =>
  Cookies.set(TOKEN_KEY, token, { expires: 1, secure: true, sameSite: 'strict' });

export const removeAuthToken = () => Cookies.remove(TOKEN_KEY);
export const getAuthToken    = () => Cookies.get(TOKEN_KEY);
```

---

## Phase 6 — Auth Provider (4-file pattern)

### 6a. context.tsx

```typescript
// src/providers/auth-provider/context.tsx
"use client";
import { createContext } from "react";

export interface IUser {
  userId: string;
  accessToken: string;
  name: string;
  email: string;
}

export interface IAuthStateContext {
  isPending:       boolean;
  isSuccess:       boolean;
  isError:         boolean;
  isAuthenticated: boolean;
  user?:           IUser;
  errorMessage?:   string;
}

export interface IAuthActionContext {
  login:    (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string) => Promise<void>;
  logout:   () => void;
}

export const INITIAL_AUTH_STATE: IAuthStateContext = {
  isPending:       false,
  isSuccess:       false,
  isError:         false,
  isAuthenticated: false,
};

export const AuthStateContext  = createContext<IAuthStateContext>(INITIAL_AUTH_STATE);
export const AuthActionContext = createContext<IAuthActionContext>({
  login:    async () => {},
  register: async () => {},
  logout:   () => {},
});
```

### 6b. actions.tsx

```typescript
// src/providers/auth-provider/actions.tsx
import { createAction } from "redux-actions";
import { IAuthStateContext, IUser } from "./context";

export enum AuthStateEnums {
  AUTH_PENDING          = "AUTH_PENDING",
  AUTH_LOGIN_SUCCESS    = "AUTH_LOGIN_SUCCESS",
  AUTH_REGISTER_SUCCESS = "AUTH_REGISTER_SUCCESS",
  AUTH_ERROR            = "AUTH_ERROR",
  AUTH_LOGOUT           = "AUTH_LOGOUT",
  AUTH_LOAD_LOCAL       = "AUTH_LOAD_LOCAL",
}

type AuthAction = { type: string; payload: Partial<IAuthStateContext> };

const createAuthAction = (type: AuthStateEnums, payload: Partial<IAuthStateContext>): AuthAction =>
  createAction<Partial<IAuthStateContext>>(type)(payload) as AuthAction;

export const authPending      = () => createAuthAction(AuthStateEnums.AUTH_PENDING, { isPending: true, isSuccess: false, isError: false });
export const loginSuccess     = (user: IUser) => createAuthAction(AuthStateEnums.AUTH_LOGIN_SUCCESS, { isPending: false, isSuccess: true, isAuthenticated: true, user });
export const registerSuccess  = () => createAuthAction(AuthStateEnums.AUTH_REGISTER_SUCCESS, { isPending: false, isSuccess: true });
export const authError        = (errorMessage: string) => createAuthAction(AuthStateEnums.AUTH_ERROR, { isPending: false, isSuccess: false, isError: true, errorMessage });
export const authLogout       = () => createAuthAction(AuthStateEnums.AUTH_LOGOUT, { ...INITIAL_AUTH_STATE });
export const loadLocalState   = (user: IUser) => createAuthAction(AuthStateEnums.AUTH_LOAD_LOCAL, { isAuthenticated: true, user });

// Re-export INITIAL_AUTH_STATE so reducer can import from one place
export { INITIAL_AUTH_STATE } from "./context";
```

### 6c. reducer.tsx

```typescript
// src/providers/auth-provider/reducer.tsx
import { handleActions } from "redux-actions";
import { INITIAL_AUTH_STATE, IAuthStateContext } from "./context";
import { AuthStateEnums } from "./actions";

export const AuthReducer = handleActions<IAuthStateContext, Partial<IAuthStateContext>>(
  {
    [AuthStateEnums.AUTH_PENDING]:          (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.AUTH_LOGIN_SUCCESS]:    (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.AUTH_REGISTER_SUCCESS]: (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.AUTH_ERROR]:            (state, { payload }) => ({ ...state, ...payload }),
    [AuthStateEnums.AUTH_LOGOUT]:           (_state, { payload }) => ({ ...INITIAL_AUTH_STATE, ...payload }),
    [AuthStateEnums.AUTH_LOAD_LOCAL]:       (state, { payload }) => ({ ...state, ...payload }),
  },
  INITIAL_AUTH_STATE
);
```

### 6d. index.tsx

```typescript
// src/providers/auth-provider/index.tsx
"use client";
import { useContext, useEffect, useReducer } from "react";
import { getAxiosInstance, setAuthToken, removeAuthToken, getAuthToken } from "@/utils/axiosInstance";
import { AuthReducer } from "./reducer";
import { INITIAL_AUTH_STATE, AuthStateContext, AuthActionContext, IUser } from "./context";
import { authPending, loginSuccess, registerSuccess, authError, authLogout, loadLocalState } from "./actions";

const SESSION_KEY = "auth_user";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const api = getAxiosInstance();
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_AUTH_STATE);

  // Rehydrate from sessionStorage on mount
  useEffect(() => {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (raw && getAuthToken()) {
      try {
        const user: IUser = JSON.parse(raw);
        dispatch(loadLocalState(user));
      } catch { /* ignore corrupt data */ }
    }
  }, []);

  const login = async (email: string, password: string) => {
    dispatch(authPending());
    try {
      // ← Adapt endpoint to your backend
      const res = await api.post('/api/auth/login', { email, password });
      const user: IUser = res.data.result ?? res.data;
      setAuthToken(user.accessToken);
      sessionStorage.setItem(SESSION_KEY, JSON.stringify(user));
      dispatch(loginSuccess(user));
    } catch (err: any) {
      dispatch(authError(err?.response?.data?.error?.message ?? 'Login failed'));
    }
  };

  const register = async (name: string, email: string, password: string) => {
    dispatch(authPending());
    try {
      await api.post('/api/auth/register', { name, email, password });
      dispatch(registerSuccess());
    } catch (err: any) {
      dispatch(authError(err?.response?.data?.error?.message ?? 'Registration failed'));
    }
  };

  const logout = () => {
    removeAuthToken();
    sessionStorage.removeItem(SESSION_KEY);
    dispatch(authLogout());
  };

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider value={{ login, register, logout }}>
        {children}
      </AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
};

export const useAuthState  = () => {
  const ctx = useContext(AuthStateContext);
  if (!ctx) throw new Error("useAuthState must be used inside AuthProvider");
  return ctx;
};

export const useAuthAction = () => {
  const ctx = useContext(AuthActionContext);
  if (!ctx) throw new Error("useAuthAction must be used inside AuthProvider");
  return ctx;
};
```

---

## Phase 7 — Entity Provider (repeat per entity)

For every CRUD entity, create `src/providers/<entity>-provider/` with the same 4-file split.
Replace `Entity` / `entity` / `ENTITY` with the real noun (PascalCase, camelCase, UPPER_SNAKE).

### context.tsx

```typescript
// src/providers/<entity>-provider/context.tsx
"use client";
import { createContext } from "react";

export interface I<Entity> {
  id: string;
  // ... entity-specific fields
}

export interface I<Entity>StateContext {
  isPending:  boolean;
  isSuccess:  boolean;
  isError:    boolean;
  items:      I<Entity>[];
  selected?:  I<Entity>;
  totalCount: number;
}

export interface I<Entity>ActionContext {
  fetchAll:  () => void;
  fetchById: (id: string) => void;
  create:    (data: Omit<I<Entity>, 'id'>) => Promise<void>;
  update:    (data: I<Entity>) => Promise<void>;
  remove:    (id: string) => Promise<void>;
}

export const INITIAL_<ENTITY>_STATE: I<Entity>StateContext = {
  isPending: false, isSuccess: false, isError: false,
  items: [], totalCount: 0,
};

export const <Entity>StateContext  = createContext<I<Entity>StateContext>(INITIAL_<ENTITY>_STATE);
export const <Entity>ActionContext = createContext<I<Entity>ActionContext>({
  fetchAll:  () => {},
  fetchById: () => {},
  create:    async () => {},
  update:    async () => {},
  remove:    async () => {},
});
```

### actions.tsx

```typescript
// src/providers/<entity>-provider/actions.tsx
import { createAction } from "redux-actions";
import { I<Entity>StateContext, I<Entity> } from "./context";

export enum <Entity>StateEnums {
  <ENTITY>_FETCH_ALL_PENDING  = "<ENTITY>_FETCH_ALL_PENDING",
  <ENTITY>_FETCH_ALL_SUCCESS  = "<ENTITY>_FETCH_ALL_SUCCESS",
  <ENTITY>_FETCH_ALL_ERROR    = "<ENTITY>_FETCH_ALL_ERROR",
  <ENTITY>_FETCH_ONE_PENDING  = "<ENTITY>_FETCH_ONE_PENDING",
  <ENTITY>_FETCH_ONE_SUCCESS  = "<ENTITY>_FETCH_ONE_SUCCESS",
  <ENTITY>_FETCH_ONE_ERROR    = "<ENTITY>_FETCH_ONE_ERROR",
  <ENTITY>_CREATE_PENDING     = "<ENTITY>_CREATE_PENDING",
  <ENTITY>_CREATE_SUCCESS     = "<ENTITY>_CREATE_SUCCESS",
  <ENTITY>_CREATE_ERROR       = "<ENTITY>_CREATE_ERROR",
  <ENTITY>_UPDATE_PENDING     = "<ENTITY>_UPDATE_PENDING",
  <ENTITY>_UPDATE_SUCCESS     = "<ENTITY>_UPDATE_SUCCESS",
  <ENTITY>_UPDATE_ERROR       = "<ENTITY>_UPDATE_ERROR",
  <ENTITY>_DELETE_PENDING     = "<ENTITY>_DELETE_PENDING",
  <ENTITY>_DELETE_SUCCESS     = "<ENTITY>_DELETE_SUCCESS",
  <ENTITY>_DELETE_ERROR       = "<ENTITY>_DELETE_ERROR",
}

const make = (type: <Entity>StateEnums, payload: Partial<I<Entity>StateContext>) =>
  createAction<Partial<I<Entity>StateContext>>(type)(payload);

export const <entity>FetchAllPending  = () => make(<Entity>StateEnums.<ENTITY>_FETCH_ALL_PENDING, { isPending: true, isSuccess: false, isError: false });
export const <entity>FetchAllSuccess  = (items: I<Entity>[], totalCount: number) => make(<Entity>StateEnums.<ENTITY>_FETCH_ALL_SUCCESS, { isPending: false, isSuccess: true, items, totalCount });
export const <entity>FetchAllError    = () => make(<Entity>StateEnums.<ENTITY>_FETCH_ALL_ERROR, { isPending: false, isError: true });
export const <entity>FetchOnePending  = () => make(<Entity>StateEnums.<ENTITY>_FETCH_ONE_PENDING, { isPending: true, isSuccess: false, isError: false });
export const <entity>FetchOneSuccess  = (selected: I<Entity>) => make(<Entity>StateEnums.<ENTITY>_FETCH_ONE_SUCCESS, { isPending: false, isSuccess: true, selected });
export const <entity>FetchOneError    = () => make(<Entity>StateEnums.<ENTITY>_FETCH_ONE_ERROR, { isPending: false, isError: true });
export const <entity>CreatePending    = () => make(<Entity>StateEnums.<ENTITY>_CREATE_PENDING, { isPending: true, isSuccess: false, isError: false });
export const <entity>CreateSuccess    = () => make(<Entity>StateEnums.<ENTITY>_CREATE_SUCCESS, { isPending: false, isSuccess: true });
export const <entity>CreateError      = () => make(<Entity>StateEnums.<ENTITY>_CREATE_ERROR, { isPending: false, isError: true });
export const <entity>UpdatePending    = () => make(<Entity>StateEnums.<ENTITY>_UPDATE_PENDING, { isPending: true, isSuccess: false, isError: false });
export const <entity>UpdateSuccess    = () => make(<Entity>StateEnums.<ENTITY>_UPDATE_SUCCESS, { isPending: false, isSuccess: true });
export const <entity>UpdateError      = () => make(<Entity>StateEnums.<ENTITY>_UPDATE_ERROR, { isPending: false, isError: true });
export const <entity>DeletePending    = () => make(<Entity>StateEnums.<ENTITY>_DELETE_PENDING, { isPending: true, isSuccess: false, isError: false });
export const <entity>DeleteSuccess    = () => make(<Entity>StateEnums.<ENTITY>_DELETE_SUCCESS, { isPending: false, isSuccess: true });
export const <entity>DeleteError      = () => make(<Entity>StateEnums.<ENTITY>_DELETE_ERROR, { isPending: false, isError: true });
```

### reducer.tsx

```typescript
// src/providers/<entity>-provider/reducer.tsx
import { handleActions } from "redux-actions";
import { INITIAL_<ENTITY>_STATE, I<Entity>StateContext } from "./context";
import { <Entity>StateEnums } from "./actions";

export const <Entity>Reducer = handleActions<I<Entity>StateContext, Partial<I<Entity>StateContext>>(
  {
    [<Entity>StateEnums.<ENTITY>_FETCH_ALL_PENDING]:  (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_FETCH_ALL_SUCCESS]:  (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_FETCH_ALL_ERROR]:    (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_FETCH_ONE_PENDING]:  (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_FETCH_ONE_SUCCESS]:  (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_FETCH_ONE_ERROR]:    (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_CREATE_PENDING]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_CREATE_SUCCESS]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_CREATE_ERROR]:       (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_UPDATE_PENDING]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_UPDATE_SUCCESS]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_UPDATE_ERROR]:       (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_DELETE_PENDING]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_DELETE_SUCCESS]:     (s, { payload }) => ({ ...s, ...payload }),
    [<Entity>StateEnums.<ENTITY>_DELETE_ERROR]:       (s, { payload }) => ({ ...s, ...payload }),
  },
  INITIAL_<ENTITY>_STATE
);
```

### index.tsx

```typescript
// src/providers/<entity>-provider/index.tsx
"use client";
import { useContext, useReducer } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import { <Entity>Reducer } from "./reducer";
import { INITIAL_<ENTITY>_STATE, <Entity>StateContext, <Entity>ActionContext, I<Entity> } from "./context";
import {
  <entity>FetchAllPending, <entity>FetchAllSuccess, <entity>FetchAllError,
  <entity>FetchOnePending, <entity>FetchOneSuccess, <entity>FetchOneError,
  <entity>CreatePending, <entity>CreateSuccess, <entity>CreateError,
  <entity>UpdatePending, <entity>UpdateSuccess, <entity>UpdateError,
  <entity>DeletePending, <entity>DeleteSuccess, <entity>DeleteError,
} from "./actions";

// ← Adapt this endpoint to your backend's routing convention
const ENDPOINT = '/api/<entities>';

export const <Entity>Provider = ({ children }: { children: React.ReactNode }) => {
  const api = getAxiosInstance();
  const [state, dispatch] = useReducer(<Entity>Reducer, INITIAL_<ENTITY>_STATE);

  const fetchAll = async () => {
    dispatch(<entity>FetchAllPending());
    try {
      const res = await api.get(ENDPOINT);
      // Adapt to your API's response shape:
      const items = res.data.result?.items ?? res.data;
      const totalCount = res.data.result?.totalCount ?? items.length;
      dispatch(<entity>FetchAllSuccess(items, totalCount));
    } catch {
      dispatch(<entity>FetchAllError());
    }
  };

  const fetchById = async (id: string) => {
    dispatch(<entity>FetchOnePending());
    try {
      const res = await api.get(`${ENDPOINT}/${id}`);
      dispatch(<entity>FetchOneSuccess(res.data.result ?? res.data));
    } catch {
      dispatch(<entity>FetchOneError());
    }
  };

  const create = async (data: Omit<I<Entity>, 'id'>) => {
    dispatch(<entity>CreatePending());
    try {
      await api.post(ENDPOINT, data);
      dispatch(<entity>CreateSuccess());
      await fetchAll();
    } catch {
      dispatch(<entity>CreateError());
    }
  };

  const update = async (data: I<Entity>) => {
    dispatch(<entity>UpdatePending());
    try {
      await api.put(`${ENDPOINT}/${data.id}`, data);
      dispatch(<entity>UpdateSuccess());
      await fetchAll();
    } catch {
      dispatch(<entity>UpdateError());
    }
  };

  const remove = async (id: string) => {
    dispatch(<entity>DeletePending());
    try {
      await api.delete(`${ENDPOINT}/${id}`);
      dispatch(<entity>DeleteSuccess());
      await fetchAll();
    } catch {
      dispatch(<entity>DeleteError());
    }
  };

  return (
    <<Entity>StateContext.Provider value={state}>
      <<Entity>ActionContext.Provider value={{ fetchAll, fetchById, create, update, remove }}>
        {children}
      </<Entity>ActionContext.Provider>
    </<Entity>StateContext.Provider>
  );
};

export const use<Entity>State  = () => {
  const ctx = useContext(<Entity>StateContext);
  if (!ctx) throw new Error("use<Entity>State must be inside <Entity>Provider");
  return ctx;
};

export const use<Entity>Action = () => {
  const ctx = useContext(<Entity>ActionContext);
  if (!ctx) throw new Error("use<Entity>Action must be inside <Entity>Provider");
  return ctx;
};
```

---

## Phase 8 — Compose All Providers

Add each new entity provider here. **AuthProvider must always be outermost.**

```typescript
// src/providers/index.tsx
"use client";
import { AuthProvider }   from "./auth-provider";
import { <Entity>Provider } from "./<entity>-provider";
// import more providers here...

export const AppProviders = ({ children }: { children: React.ReactNode }) => (
  <AuthProvider>
    {/* Nest entity providers inside auth — they may need auth state */}
    <<Entity>Provider>
      {children}
    </<Entity>Provider>
  </AuthProvider>
);
```

---

## Phase 9 — Root Layout

```typescript
// src/app/layout.tsx
import type { Metadata } from "next";
import { AntdRegistry } from "@ant-design/nextjs-registry";
import { AppProviders } from "@/providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "<AppName>",
  description: "<App description>",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AntdRegistry>
          <AppProviders>
            {children}
          </AppProviders>
        </AntdRegistry>
      </body>
    </html>
  );
}
```

```css
/* src/app/globals.css — minimal reset only */
*, *::before, *::after { box-sizing: border-box; }
body { margin: 0; padding: 0; }
```

---

## Phase 10 — withAuth HOC (Route Protection)

```typescript
// src/hoc/withAuth.tsx
"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { Spin } from "antd";
import { useAuthState } from "@/providers/auth-provider";
import { getAuthToken } from "@/utils/axiosInstance";

export function withAuth<T extends object>(WrappedComponent: React.ComponentType<T>) {
  return function ProtectedComponent(props: T) {
    const router = useRouter();
    const { isAuthenticated, isPending } = useAuthState();
    const token = getAuthToken();

    useEffect(() => {
      if (!isPending && !isAuthenticated && !token) {
        router.replace('/login');
      }
    }, [isAuthenticated, isPending, token, router]);

    if (isPending) {
      return (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
          <Spin size="large" />
        </div>
      );
    }

    if (!isAuthenticated && !token) return null;

    return <WrappedComponent {...props} />;
  };
}
```

---

## Phase 11 — Dashboard Layout (Protected)

```typescript
// src/app/(dashboard)/layout.tsx
"use client";
import { withAuth } from "@/hoc/withAuth";
import { AppShell }  from "@/components/layout/AppShell";

function DashboardLayout({ children }: { children: React.ReactNode }) {
  return <AppShell>{children}</AppShell>;
}

export default withAuth(DashboardLayout);
```

### AppShell (Ant Design layout)

```typescript
// src/components/layout/AppShell.tsx
"use client";
import { useState } from "react";
import { Layout, Menu, Button, theme } from "antd";
import { useRouter, usePathname } from "next/navigation";
import { LogOut, LayoutDashboard } from "lucide-react";
import { useAuthAction } from "@/providers/auth-provider";
import { useStyles } from "./styles/style";  // ← antd-style

const { Header, Sider, Content } = Layout;

const NAV_ITEMS = [
  { key: "/dashboard",  label: "Dashboard",  icon: <LayoutDashboard size={16} /> },
  // Add entity nav items here:
  // { key: "/<entities>", label: "<Entities>", icon: <SomeIcon size={16} /> },
];

export const AppShell = ({ children }: { children: React.ReactNode }) => {
  const router = useRouter();
  const pathname = usePathname();
  const { logout } = useAuthAction();
  const [collapsed, setCollapsed] = useState(false);
  const { styles } = useStyles();
  const { token } = theme.useToken();

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Sider collapsible collapsed={collapsed} onCollapse={setCollapsed}>
        <div className={styles.logo}>{!collapsed && "<AppName>"}</div>
        <Menu
          theme="dark"
          selectedKeys={[pathname]}
          items={NAV_ITEMS}
          onClick={({ key }) => router.push(key)}
        />
      </Sider>
      <Layout>
        <Header className={styles.header} style={{ background: token.colorBgContainer }}>
          <Button type="text" icon={<LogOut size={16} />} onClick={logout}>
            Logout
          </Button>
        </Header>
        <Content className={styles.content}>{children}</Content>
      </Layout>
    </Layout>
  );
};
```

---

## Phase 12 — Styling Pattern (antd-style — MANDATORY)

Every page and shared component **must** define its styles using `createStyles` in a co-located `styles/style.ts` file. **No inline `style={}` for layout** — only for antd token-driven overrides.

```typescript
// src/app/(dashboard)/dashboard/styles/style.ts
import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    padding: ${token.paddingLG}px;
  `,
  header: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: ${token.marginLG}px;
  `,
  card: css`
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorBgContainer};
  `,
}));
```

```typescript
// src/app/(dashboard)/dashboard/page.tsx
"use client";
import { useStyles } from "./styles/style";

export default function DashboardPage() {
  const { styles } = useStyles();
  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2>Dashboard</h2>
      </div>
    </div>
  );
}
```

---

## Phase 13 — Login Page

```typescript
// src/app/(auth)/login/page.tsx
"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { Form, Input, Button, Alert, Typography } from "antd";
import { useAuthState, useAuthAction } from "@/providers/auth-provider";
import { useStyles } from "./styles/style";

const { Title } = Typography;

export default function LoginPage() {
  const router = useRouter();
  const { isPending, isError, isAuthenticated, errorMessage } = useAuthState();
  const { login } = useAuthAction();
  const { styles } = useStyles();

  useEffect(() => {
    if (isAuthenticated) router.replace("/dashboard");
  }, [isAuthenticated, router]);

  const onFinish = ({ email, password }: { email: string; password: string }) => {
    login(email, password);
  };

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <Title level={3}>Sign In</Title>
        {isError && <Alert message={errorMessage ?? "Login failed"} type="error" showIcon style={{ marginBottom: 16 }} />}
        <Form layout="vertical" onFinish={onFinish} autoComplete="off">
          <Form.Item label="Email" name="email" rules={[{ required: true, type: "email" }]}>
            <Input />
          </Form.Item>
          <Form.Item label="Password" name="password" rules={[{ required: true, min: 6 }]}>
            <Input.Password />
          </Form.Item>
          <Form.Item>
            <Button type="primary" htmlType="submit" loading={isPending} block>
              Sign In
            </Button>
          </Form.Item>
        </Form>
      </div>
    </div>
  );
}
```

```typescript
// src/app/(auth)/login/styles/style.ts
import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background: ${token.colorBgLayout};
  `,
  card: css`
    width: 100%;
    max-width: 400px;
    padding: ${token.paddingXL}px;
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadow};
  `,
}));
```

---

## Phase 14 — Entity List Page + CRUD Modal

### Page

```typescript
// src/app/(dashboard)/<entities>/page.tsx
"use client";
import { useEffect, useState } from "react";
import { Button } from "antd";
import { Plus } from "lucide-react";
import { use<Entity>State, use<Entity>Action } from "@/providers/<entity>-provider";
import { <Entity>Table }  from "@/components/<entity>/<Entity>Table";
import { <Entity>Modal }  from "@/components/<entity>/<Entity>Modal";
import { I<Entity> }      from "@/providers/<entity>-provider/context";
import { useStyles }      from "./styles/style";

export default function <Entity>Page() {
  const { styles } = useStyles();
  const { items, isPending, totalCount } = use<Entity>State();
  const { fetchAll, create, update, remove } = use<Entity>Action();
  const [modalOpen, setModalOpen]     = useState(false);
  const [selected, setSelected]        = useState<I<Entity> | null>(null);

  useEffect(() => { fetchAll(); }, []);

  const handleSave = async (data: Omit<I<Entity>, 'id'> | I<Entity>) => {
    if ("id" in data) await update(data as I<Entity>);
    else              await create(data);
    setModalOpen(false);
    setSelected(null);
  };

  const handleEdit = (record: I<Entity>) => {
    setSelected(record);
    setModalOpen(true);
  };

  const handleAdd = () => {
    setSelected(null);
    setModalOpen(true);
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2><Entities></h2>
        <Button type="primary" icon={<Plus size={16} />} onClick={handleAdd}>
          Add <Entity>
        </Button>
      </div>
      <<Entity>Table
        items={items}
        loading={isPending}
        totalCount={totalCount}
        onEdit={handleEdit}
        onDelete={remove}
      />
      <<Entity>Modal
        open={modalOpen}
        initial={selected}
        onSave={handleSave}
        onCancel={() => { setModalOpen(false); setSelected(null); }}
      />
    </div>
  );
}
```

### Table Component

```typescript
// src/components/<entity>/<Entity>Table.tsx
"use client";
import { Table, Button, Popconfirm, Space } from "antd";
import type { ColumnsType } from "antd/es/table";
import { Pencil, Trash2 } from "lucide-react";
import { I<Entity> } from "@/providers/<entity>-provider/context";

interface Props {
  items:      I<Entity>[];
  loading:    boolean;
  totalCount: number;
  onEdit:     (record: I<Entity>) => void;
  onDelete:   (id: string) => void;
}

export const <Entity>Table = ({ items, loading, onEdit, onDelete }: Props) => {
  const columns: ColumnsType<I<Entity>> = [
    // ← Add entity-specific columns here
    { title: "Name", dataIndex: "name", key: "name" },
    {
      title: "Actions",
      key: "actions",
      width: 120,
      render: (_, record) => (
        <Space>
          <Button
            size="small"
            icon={<Pencil size={14} />}
            onClick={() => onEdit(record)}
          />
          <Popconfirm title="Delete this item?" onConfirm={() => onDelete(record.id)}>
            <Button size="small" danger icon={<Trash2 size={14} />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <Table
      rowKey="id"
      columns={columns}
      dataSource={items}
      loading={loading}
      pagination={{ pageSize: 10 }}
    />
  );
};
```

### Modal Component

```typescript
// src/components/<entity>/<Entity>Modal.tsx
"use client";
import { useEffect } from "react";
import { Modal, Form, Input, Button } from "antd";
import { I<Entity> } from "@/providers/<entity>-provider/context";

interface Props {
  open:     boolean;
  initial?: I<Entity> | null;
  onSave:   (data: Omit<I<Entity>, 'id'> | I<Entity>) => void;
  onCancel: () => void;
}

export const <Entity>Modal = ({ open, initial, onSave, onCancel }: Props) => {
  const [form] = Form.useForm();
  const isEdit = !!initial;

  useEffect(() => {
    if (open) {
      if (initial) form.setFieldsValue(initial);
      else form.resetFields();
    }
  }, [open, initial, form]);

  const handleOk = async () => {
    const values = await form.validateFields();
    onSave(isEdit ? { ...initial!, ...values } : values);
  };

  return (
    <Modal
      open={open}
      title={isEdit ? "Edit <Entity>" : "Add <Entity>"}
      onCancel={onCancel}
      footer={[
        <Button key="cancel" onClick={onCancel}>Cancel</Button>,
        <Button key="save" type="primary" onClick={handleOk}>Save</Button>,
      ]}
    >
      <Form form={form} layout="vertical">
        {/* ← Add form fields matching the entity shape */}
        <Form.Item name="name" label="Name" rules={[{ required: true }]}>
          <Input />
        </Form.Item>
      </Form>
    </Modal>
  );
};
```

---

## Phase 15 — Testing Setup

### Vitest config

```typescript
// vitest.config.ts
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/tests/setup.ts"],
  },
  resolve: {
    alias: { "@": path.resolve(__dirname, "./src") },
  },
});
```

### Setup file

```typescript
// src/tests/setup.ts
import "@testing-library/jest-dom";
```

### Playwright config

```typescript
// playwright.config.ts
import { defineConfig } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/e2e",
  use: {
    baseURL: "http://localhost:3000",
  },
  webServer: {
    command: "npm run dev",
    url: "http://localhost:3000",
    reuseExistingServer: !process.env.CI,
  },
});
```

### package.json scripts (add to existing)

```json
{
  "scripts": {
    "test":       "vitest",
    "test:unit":  "vitest run",
    "test:e2e":   "playwright test",
    "test:cover": "vitest run --coverage"
  }
}
```

> **Checkpoint:** `npm test` runs without errors. At least one smoke test passes.

---

## Phase 16 — Final Checklist

Work through this checklist **in order** before declaring the scaffold complete.

### Structure
- [ ] All folders from Phase 3 exist
- [ ] No component imports directly from another component's styles folder
- [ ] No `axios.create()` outside `axiosInstance.ts`

### Auth
- [ ] Login redirects to `/dashboard` on success
- [ ] Dashboard redirects to `/login` when unauthenticated
- [ ] Logout clears cookie + sessionStorage and redirects to `/login`
- [ ] `withAuth` HOC applied to `(dashboard)/layout.tsx`

### State Management
- [ ] Every provider follows the 4-file split (context / actions / reducer / index)
- [ ] Enum values are prefixed with the entity name (no collisions)
- [ ] `fetchAll()` is called after every create / update / delete

### Styling
- [ ] Every page has a co-located `styles/style.ts` using `createStyles`
- [ ] No raw CSS files (except `globals.css` for reset only)
- [ ] No Tailwind classes
- [ ] Design tokens used for spacing, colors, radius (no magic numbers)

### API
- [ ] `NEXT_PUBLIC_API_URL` set in `.env.local`
- [ ] Axios interceptor injects `Authorization: Bearer <token>`
- [ ] 401 responses clear the token and redirect to `/login`
- [ ] API response shape adapted (`res.data.result` vs `res.data`) per backend

### Testing
- [ ] Vitest configured with jsdom + `@testing-library/jest-dom`
- [ ] Playwright configured with local dev server
- [ ] `npm run test:unit` exits 0

---

## Placeholder Reference

| Placeholder | Example substitution |
|---|---|
| `<AppName>` | `TaskFlow` |
| `<app-name>` | `task-flow` |
| `<API_BASE_URL>` | `http://localhost:3001` |
| `<Entity>` | `Project` |
| `<entity>` | `project` |
| `<entities>` | `projects` |
| `<ENTITY>` | `PROJECT` |
| `<Entities>` | `Projects` |

---

## Common Mistakes to Avoid

| Mistake | Correct approach |
|---|---|
| Calling `axios.create()` in a component | Always use `getAxiosInstance()` |
| Auto-fetching in provider `useEffect` | Let the page call `fetchAll()` on mount |
| Using `style={{}}` for layout | Use `createStyles` in a co-located `styles/style.ts` |
| Skipping the 4-file provider split | Always: context / actions / reducer / index |
| Wrapping page with `withAuth` directly | Wrap the **layout** — all child pages inherit protection |
| Hardcoding colors/spacing | Use `token.colorBgContainer`, `token.paddingLG`, etc. |
| Storing tokens in `localStorage` | Always use `js-cookie` with `secure` + `sameSite` |
| Duplicate enum values across providers | Prefix every enum value with the entity name |
