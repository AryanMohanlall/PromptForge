"use client";

import { useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import { Sidebar } from "@/components/Sidebar";
import { useStyles } from "./styles/style";
import { useAuthState } from "@/providers/auth-provider";

export default function MainLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  const { styles } = useStyles();
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, isInitialized } = useAuthState();

  useEffect(() => {
    if (isInitialized && !isAuthenticated) {
      router.replace("/auth");
    }
  }, [isAuthenticated, isInitialized, router]);

  if (!isInitialized || !isAuthenticated) {
    return null;
  }

  const segments = pathname.split("/").filter(Boolean);
  const section = segments[0] ?? "dashboard";
  const subSection = segments[1];

  const currentPage =
    section === "create-project"
      ? "create"
      : section === "admin" && subSection
        ? `admin-${subSection}`
        : section;

  const handleNavigate = (page: string) => {
    if (page === "create") {
      router.push("/create-project");
      return;
    }
    router.push(`/${page}`);
  };

  return (
    <div className={styles.layout}>
      <Sidebar currentPage={currentPage} onNavigate={handleNavigate} />
      <main className={styles.content}>{children}</main>
    </div>
  );
}
