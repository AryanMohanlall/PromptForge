import axios, { type AxiosInstance } from "axios";
import Cookies from "js-cookie";

const AUTH_TOKEN_KEY = "auth_token";
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "https://localhost:44311";

let axiosInstance: AxiosInstance | null = null;

export const getAxiosInstance = () => {
  if (!axiosInstance) {
    axiosInstance = axios.create({
      baseURL: API_BASE_URL,
    });

    axiosInstance.interceptors.request.use(config => {
      const token = Cookies.get(AUTH_TOKEN_KEY);
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  return axiosInstance;
};

export const setAuthToken = (token: string) => {
  Cookies.set(AUTH_TOKEN_KEY, token, {
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
  });
};

export const removeAuthToken = () => {
  Cookies.remove(AUTH_TOKEN_KEY);
};
