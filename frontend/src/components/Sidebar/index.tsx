"use client";

import {
  FolderIcon,
  LayoutGridIcon,
  LayoutTemplateIcon,
  SettingsIcon,
  PlusIcon,
  BarChart3Icon,
  UsersIcon,
  LayersIcon,
  ServerIcon,
  ActivityIcon,
  LogOutIcon,
  SparklesIcon,
} from "lucide-react";
import { useStyles } from "./styles";
import { useAuthAction, useAuthState } from "@/providers/auth-provider";
import { useRouter } from "next/navigation";

interface SidebarProps {
  currentPage: string;
  onNavigate: (page: string) => void;
}

export function Sidebar({ currentPage, onNavigate }: SidebarProps) {
  const { styles, cx } = useStyles();
  const router = useRouter();
  const { user } = useAuthState();
  const { logout } = useAuthAction();
  const isAdmin = user?.roleNames?.includes("PlatformAdministrator");
  const displayName =
    [user?.name, user?.surname].filter(Boolean).join(" ").trim() ||
    user?.userName ||
    "User";
  const roles = user?.roleNames ?? [];
  const rolesLabel = roles.length > 0 ? roles.join(", ") : "No role assigned";
  const initials =
    displayName
      .split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join("") || "U";
  const mainNav = [
    {
      id: "projects",
      label: "My Projects",
      icon: FolderIcon,
    },
    {
      id: "generate",
      label: "Generate",
      icon: SparklesIcon,
    },
    {
      id: "templates",
      label: "Templates",
      icon: LayoutTemplateIcon,
    },
    {
      id: "settings",
      label: "Settings",
      icon: SettingsIcon,
    },
  ];

  const adminNav = [
    {
      id: "admin",
      label: "Overview",
      icon: BarChart3Icon,
    },
    {
      id: "admin-users",
      label: "Users",
      icon: UsersIcon,
    },
    {
      id: "admin-projects",
      label: "Projects",
      icon: LayersIcon,
    },
    {
      id: "admin-deployments",
      label: "Deployments",
      icon: ServerIcon,
    },
    {
      id: "admin-health",
      label: "System Health",
      icon: ActivityIcon,
    },
  ];

  const handleLogout = async () => {
    await logout();
    router.push("/auth");
  };

  return (
    <div className={styles.sidebar}>
      <div className={styles.content}>
        <h1 className={styles.brand}>PromptForge</h1>

        <button
          type="button"
          onClick={() => onNavigate("generate")}
          className={cx(styles.newButton, styles.focusRing)}
        >
          <PlusIcon className={styles.newIcon} />
          New project
        </button>

        <nav className={styles.nav}>
          {mainNav.map((item) => {
            const isActive = currentPage === item.id;
            return (
              <button
                key={item.id}
                type="button"
                onClick={() => onNavigate(item.id)}
                className={cx(
                  styles.navButton,
                  styles.focusRing,
                  isActive && styles.navButtonActive,
                )}
              >
                <item.icon
                  className={cx(
                    styles.navIcon,
                    isActive && styles.navIconActive,
                  )}
                />
                {item.label}
              </button>
            );
          })}
        </nav>

        {isAdmin && (
          <>
            <div className={styles.adminLabel}>Admin</div>
            <div className={styles.divider} />

            <nav className={styles.nav}>
              {adminNav.map((item) => {
                const isActive = currentPage === item.id;
                return (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() =>
                      onNavigate(
                        item.id.startsWith("admin") ? "admin" : item.id,
                      )
                    }
                    className={cx(
                      styles.navButton,
                      styles.focusRing,
                      isActive && styles.navButtonActive,
                    )}
                  >
                    <item.icon
                      className={cx(
                        styles.navIcon,
                        isActive && styles.navIconActive,
                      )}
                    />
                    {item.label}
                  </button>
                );
              })}
            </nav>
          </>
        )}

        <div className={styles.footer}>
          <button
            type="button"
            className={cx(styles.profileCard, styles.focusRing)}
          >
            <div className={styles.profileInfo}>
              <div className={styles.avatar}>{initials}</div>
              <div className={styles.profileTextBlock}>
                <span className={styles.profileName}>{displayName}</span>
                {/* <span className={styles.profileMeta}>{identityLabel}</span> */}
                <span className={styles.roleBadge}>{rolesLabel}</span>
              </div>
            </div>
          </button>

          <button
            type="button"
            onClick={handleLogout}
            className={cx(styles.logoutButton, styles.focusRing)}
          >
            <LogOutIcon className={styles.logoutIcon} />
            Log out
          </button>
        </div>
      </div>
    </div>
  );
}
