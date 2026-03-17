"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { setAuthToken } from "@/utils/axiosInstance";

const AUTH_USER_KEY = "auth_user";

export default function GitHubCallbackPage() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    const accessToken = searchParams.get("accessToken");
    const userId = searchParams.get("userId");
    const expireInSeconds = searchParams.get("expireInSeconds");
    const error = searchParams.get("error");

    if (error) {
      router.replace(`/auth?error=${encodeURIComponent(error)}`);
      return;
    }

    if (!accessToken || !userId) {
      router.replace("/auth?error=missing_token");
      return;
    }

    setAuthToken(accessToken);

    sessionStorage.setItem(
      AUTH_USER_KEY,
      JSON.stringify({
        userId: Number(userId),
        accessToken,
        expireInSeconds: Number(expireInSeconds ?? 86400),
      }),
    );

    router.replace("/");
  }, [router, searchParams]);

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
      }}
    >
      Signing you in…
    </div>
  );
}
