"use client";

import { useEffect, useMemo, useState } from "react";
import { SearchIcon, ChevronDownIcon } from "lucide-react";
import { motion, type Variants } from "framer-motion";
import { message } from "antd";
import { ProjectCard, ProjectData } from "../ProjectCard";
import { useStyles } from "./styles";
import {
  ProjectFramework,
  ProjectProgrammingLanguage,
  ProjectStatus,
  useProjectAction,
  useProjectState,
} from "@/providers/projects-provider";

interface DashboardPageProps {
  onNavigate: (page: string) => void;
}

export function DashboardPage({ onNavigate }: DashboardPageProps) {
  const { styles, cx } = useStyles();
  const { items, isPending, isError } = useProjectState();
  const { fetchAll, remove } = useProjectAction();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [isFilterOpen, setIsFilterOpen] = useState(false);
  const [deletingProjectId, setDeletingProjectId] = useState<number | null>(
    null,
  );

  const statuses = [
    "All",
    "Draft",
    "Generating",
    "Generated",
    "Deploying",
    "Live",
    "Failed",
  ];

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  const dashboardProjects = useMemo<ProjectData[]>(() => {
    const statusMap: Record<ProjectStatus, ProjectData["status"]> = {
      [ProjectStatus.Draft]: "Draft",
      [ProjectStatus.PromptSubmitted]: "Generating",
      [ProjectStatus.CodeGenerationInProgress]: "Generating",
      [ProjectStatus.CodeGenerationCompleted]: "Generated",
      [ProjectStatus.RepositoryPushInProgress]: "Deploying",
      [ProjectStatus.Deployed]: "Live",
      [ProjectStatus.Failed]: "Failed",
      [ProjectStatus.Archived]: "Generated",
    };

    const frameworkMap: Record<ProjectFramework, string> = {
      [ProjectFramework.NextJS]: "Next.js",
      [ProjectFramework.ReactVite]: "React + Vite",
      [ProjectFramework.Angular]: "Angular",
      [ProjectFramework.Vue]: "Vue",
      [ProjectFramework.DotNetBlazor]: ".NET Blazor",
    };

    const languageMap: Record<ProjectProgrammingLanguage, string> = {
      [ProjectProgrammingLanguage.TypeScript]: "TypeScript",
      [ProjectProgrammingLanguage.JavaScript]: "JavaScript",
      [ProjectProgrammingLanguage.CSharp]: "C#",
    };

    return items.map((item) => ({
      id: String(item.id),
      name: item.name,
      status: statusMap[item.status] ?? "Draft",
      framework: frameworkMap[item.framework] ?? "Next.js",
      language: languageMap[item.language] ?? "TypeScript",
      updatedAt: `Updated ${new Date(item.updatedAt).toLocaleString()}`,
      url: item.lastDeploymentUrl || undefined,
    }));
  }, [items]);

  const filteredProjects = dashboardProjects.filter((project) => {
    const matchesSearch = project.name
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "All" || project.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleDeleteProject = async (project: ProjectData) => {
    const projectId = Number(project.id);
    if (!Number.isFinite(projectId)) {
      return;
    }

    const confirmed = window.confirm(
      `Delete \"${project.name}\"? This action cannot be undone.`,
    );
    if (!confirmed) {
      return;
    }

    setDeletingProjectId(projectId);
    try {
      await remove(projectId);
      message.success(`Deleted \"${project.name}\".`);
    } catch {
      message.error("Could not delete the project. Please try again.");
    } finally {
      setDeletingProjectId(null);
    }
  };

  const handleClaimProject = (project: ProjectData) => {
    const projectId = Number(project.id);
    if (!Number.isFinite(projectId)) {
      return;
    }
    const claimUrl = `https://vercel.com/oauth/authorize?client_id=YOUR_CLIENT_ID&redirect_uri=${encodeURIComponent(
      window.location.origin,
    )}/vercel/callback&response_type=code`;
    window.location.href = claimUrl;
  };

  const containerVariants: Variants = {
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: { staggerChildren: 0.1 },
    },
  };

  const itemVariants: Variants = {
    hidden: { opacity: 0, y: 20 },
    show: {
      opacity: 1,
      y: 0,
      transition: {
        type: "spring",
        stiffness: 300,
        damping: 24,
      },
    },
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <h1 className={styles.title}>My projects</h1>

        <div className={styles.actions}>
          <div className={styles.searchWrap}>
            <SearchIcon className={styles.searchIcon} />
            <input
              type="text"
              placeholder="Search projects..."
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              className={cx(styles.searchInput, styles.focusRing)}
            />
          </div>

          <div className={styles.filterWrap}>
            <button
              type="button"
              onClick={() => setIsFilterOpen(!isFilterOpen)}
              className={cx(styles.filterButton, styles.focusRing)}
            >
              {statusFilter}
              <ChevronDownIcon className={styles.filterIcon} />
            </button>

            {isFilterOpen && (
              <div className={styles.filterMenu}>
                {statuses.map((status) => (
                  <button
                    key={status}
                    type="button"
                    onClick={() => {
                      setStatusFilter(status);
                      setIsFilterOpen(false);
                    }}
                    className={cx(
                      styles.filterItem,
                      statusFilter === status && styles.filterItemActive,
                    )}
                  >
                    {status}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {isPending ? (
        <div className={styles.emptyState}>Loading projects...</div>
      ) : isError ? (
        <div className={styles.emptyState}>
          Failed to load projects. Please try again.
        </div>
      ) : filteredProjects.length > 0 ? (
        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="show"
          className={styles.grid}
        >
          {filteredProjects.map((project) => (
            <motion.div key={project.id} variants={itemVariants}>
              <ProjectCard
                project={project}
                onView={() => onNavigate("generation")}
                onDelete={() => handleDeleteProject(project)}
                onClaim={() => handleClaimProject(project)}
                isDeleting={deletingProjectId === Number(project.id)}
              />
            </motion.div>
          ))}
        </motion.div>
      ) : (
        <div className={styles.emptyState}>
          No projects found matching your criteria.
        </div>
      )}
    </div>
  );
}
