"use client";

import { Suspense, useEffect, useMemo } from "react";
import { setAuthToken } from "@/utils/axiosInstance";

const AUTH_USER_KEY = "auth_user";
const GITHUB_OAUTH_COMPLETE_KEY = "github_oauth_complete";

function GitHubCallback() {
  const oauthParams = useMemo(() => {
    if (typeof window === "undefined") return null;
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");
    const userId = params.get("userId");

    if (!token || !userId) return null;
    return {
      token,
      userId,
      expireInSeconds: params.get("expireInSeconds"),
      tenantId: params.get("tenantId"),
      userName: params.get("userName"),
      name: params.get("name"),
      surname: params.get("surname"),
      email: params.get("email"),
      avatarUrl: params.get("avatarUrl"),
      githubUsername: params.get("githubUsername"),
      roleNames: params.get("roleNames"),
      roleName: params.get("roleName"),
    };
  }, []);

  useEffect(() => {
    if (!oauthParams) return;

    setAuthToken(oauthParams.token);

    sessionStorage.setItem(
      AUTH_USER_KEY,
      JSON.stringify({
        userId: Number(oauthParams.userId),
        tenantId: oauthParams.tenantId ? Number(oauthParams.tenantId) : null,
        accessToken: oauthParams.token,
        expireInSeconds: Number(oauthParams.expireInSeconds ?? 86400),
        userName: oauthParams.userName,
        name: oauthParams.name,
        surname: oauthParams.surname,
        email: oauthParams.email,
        avatarUrl: oauthParams.avatarUrl,
        githubUsername: oauthParams.githubUsername,
        roleNames: oauthParams.roleNames?.split(",").filter(Boolean) ?? [],
        roleName: oauthParams.roleName,
      }),
    );

    sessionStorage.setItem(GITHUB_OAUTH_COMPLETE_KEY, "true");

    // Full page navigation so AuthProvider re-bootstraps with the updated sessionStorage.
    // Client-side router.replace would skip the bootstrap useEffect since AuthProvider is already mounted.
    window.location.replace("/dashboard");
  }, [oauthParams]);

  if (!oauthParams) {
    return (
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          height: "100vh",
          gap: "16px",
          padding: "24px",
          textAlign: "center",
        }}
      >
        <p style={{ color: "#ef4444", fontWeight: 600, maxWidth: 440 }}>
          GitHub sign-in failed: missing or invalid OAuth parameters. Please try
          again.
        </p>
        <a
          href="/login"
          style={{ color: "#6366f1", textDecoration: "underline" }}
        >
          Back to login
        </a>
      </div>
    );
  }

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

export default function GitHubCallbackPage() {
  return (
    <Suspense>
      <GitHubCallback />
    </Suspense>
  );
}
