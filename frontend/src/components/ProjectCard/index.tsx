"use client";

import { useStyles } from "./styles";

export interface ProjectData {
  id: string;
  name: string;
  status: "Draft" | "Generating" | "Generated" | "Deploying" | "Live" | "Failed";
  framework: string;
  language: string;
  updatedAt: string;
  url?: string;
}

interface ProjectCardProps {
  project: ProjectData;
  onView: () => void;
}

export function ProjectCard({ project, onView }: ProjectCardProps) {
  const { styles, cx } = useStyles();

  const statusClassMap: Record<ProjectData["status"], string> = {
    Draft: styles.statusDraft,
    Generating: styles.statusGenerating,
    Generated: styles.statusGenerated,
    Deploying: styles.statusDeploying,
    Live: styles.statusLive,
    Failed: styles.statusFailed,
  };

  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <div>
          <h3 className={styles.title}>{project.name}</h3>
          <p className={styles.meta}>
            {project.framework} · {project.language}
          </p>
        </div>
        <span className={cx(styles.statusBadge, statusClassMap[project.status])}>
          {project.status}
        </span>
      </div>

      <div className={styles.body}>
        <p className={styles.updatedAt}>{project.updatedAt}</p>
        {project.url ? (
          <a
            href={`https://${project.url}`}
            target="_blank"
            rel="noreferrer"
            className={styles.url}
          >
            {project.url}
          </a>
        ) : (
          <span className={styles.urlMuted}>No live URL yet</span>
        )}
      </div>

      <div className={styles.footer}>
        <span className={styles.details}>View build details</span>
        <button
          type="button"
          onClick={onView}
          className={cx(styles.viewButton, styles.focusRing)}
        >
          Open
        </button>
      </div>
    </div>
  );
}
