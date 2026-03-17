"use client";

import type { ComponentType } from "react";
import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthState } from "@/providers/auth-provider";

interface WithAuthOptions {
  requiredRoles?: string[];
  redirectTo?: string;
  unauthorizedRedirectTo?: string;
}

export const withAuth = <P extends object>(
  WrappedComponent: ComponentType<P>,
  options: WithAuthOptions = {}
) => {
  const {
    requiredRoles,
    redirectTo = "/auth",
    unauthorizedRedirectTo = "/dashboard",
  } = options;

  const ComponentWithAuth = (props: P) => {
    const router = useRouter();
    const { isAuthenticated, user } = useAuthState();
    const hasRequiredRole =
      !requiredRoles?.length ||
      requiredRoles.some((role) => user?.roleNames?.includes(role));

    useEffect(() => {
      if (!isAuthenticated) {
        router.replace(redirectTo);
        return;
      }

      if (!hasRequiredRole) {
        router.replace(unauthorizedRedirectTo);
      }
    }, [hasRequiredRole, isAuthenticated, redirectTo, router, unauthorizedRedirectTo]);

    if (!isAuthenticated || !hasRequiredRole) {
      return null;
    }

    return <WrappedComponent {...props} />;
  };

  ComponentWithAuth.displayName = `withAuth(${
    WrappedComponent.displayName || WrappedComponent.name || "Component"
  })`;

  return ComponentWithAuth;
};
