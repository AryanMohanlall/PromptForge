import axios, { type AxiosInstance } from "axios";
import Cookies from "js-cookie";

const AUTH_TOKEN_KEY = "auth_token";
const AUTH_USER_KEY = "auth_user";
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:44311";

let axiosInstance: AxiosInstance | null = null;

const getAccessToken = (): string | undefined => {
  // Primary source: cookie (set during login/GitHub callback)
  const tokenFromCookie = Cookies.get(AUTH_TOKEN_KEY);
  if (tokenFromCookie) return tokenFromCookie;

  // Fallback: sessionStorage (avoids race conditions where cookie hasn't been written yet)
  if (typeof window === "undefined") return undefined;

  try {
    const raw = sessionStorage.getItem(AUTH_USER_KEY);
    if (!raw) return undefined;
    const parsed = JSON.parse(raw) as { accessToken?: unknown };
    if (typeof parsed?.accessToken === "string" && parsed.accessToken.length > 0) {
      return parsed.accessToken;
    }
  } catch {
    // Ignore parse/storage issues; request will proceed without auth header.
  }

  return undefined;
};

export const getAxiosInstance = () => {
  if (!axiosInstance) {
    axiosInstance = axios.create({
      baseURL: API_BASE_URL,
    });

    axiosInstance.interceptors.request.use(config => {
      const token = getAccessToken();
      if (token) {
        config.headers = config.headers ?? {};
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  return axiosInstance;
};

export const setAuthToken = (token: string) => {
  Cookies.set(AUTH_TOKEN_KEY, token, {
    path: "/",
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
  });
};

export const removeAuthToken = () => {
  Cookies.remove(AUTH_TOKEN_KEY, { path: "/" });
};
