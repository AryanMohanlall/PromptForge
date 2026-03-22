"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeftOutlined,
  BranchesOutlined,
  CloudServerOutlined,
  DeploymentUnitOutlined,
  GithubOutlined,
  LoadingOutlined,
} from "@ant-design/icons";
import { Button, Card, Empty, Spin, Tag, Timeline, Typography } from "antd";
import { useStyles } from "./styles/style";
import {
  ProjectStatus,
  useProjectAction,
  useProjectState,
} from "@/providers/projects-provider";
import { Link } from "lucide-react";

const { Title, Text, Paragraph } = Typography;

interface DeploymentItem {
  id: number;
  target: string;
  environmentName: string;
  status: string;
  url: string | null;
  triggeredAt: string | null;
  completedAt: string | null;
  errorMessage: string | null;
}

interface RepositoryInfo {
  id: number;
  provider: string;
  owner: string;
  name: string;
  fullName: string;
  defaultBranch: string;
  visibility: string;
  htmlUrl: string;
  createdAt: string;
  updatedAt: string;
}

const statusColorMap: Record<string, string> = {
  Draft: "default",
  PromptSubmitted: "processing",
  CodeGenerationInProgress: "processing",
  CodeGenerationCompleted: "success",
  RepositoryPushInProgress: "processing",
  Deployed: "success",
  Failed: "error",
  Archived: "default",
};

const statusLabelMap: Record<number, string> = {
  1: "Draft",
  2: "Prompt Submitted",
  3: "Code Generation In Progress",
  4: "Repository Push In Progress",
  5: "Deployed",
  6: "Failed",
  7: "Archived",
  8: "Code Generation Completed",
};

const deploymentStatusColorMap: Record<string, string> = {
  Pending: "default",
  InProgress: "processing",
  Succeeded: "success",
  Failed: "error",
  Cancelled: "default",
};

const frameworkLabelMap: Record<number, string> = {
  1: "Next.js",
  2: "React + Vite",
  3: "Angular",
  4: "Vue",
  5: ".NET Blazor",
};

const languageLabelMap: Record<number, string> = {
  1: "TypeScript",
  2: "JavaScript",
  3: "C#",
};

const databaseLabelMap: Record<number, string> = {
  1: "Render Postgres",
  2: "Neon Postgres",
  3: "MongoDB Cloud",
};

export default function ProjectDetailPage() {
  const { styles, cx } = useStyles();
  const params = useParams();
  const router = useRouter();
  const projectId = Number(params.id);

  const { selected, isPending, isError } = useProjectState();
  const { fetchById } = useProjectAction();

  const [deployments, setDeployments] = useState<DeploymentItem[]>([]);
  const [repository, setRepository] = useState<RepositoryInfo | null>(null);
  const [isLoadingDeployments, setIsLoadingDeployments] = useState(false);
  const [isLoadingRepo, setIsLoadingRepo] = useState(false);

  useEffect(() => {
    if (Number.isFinite(projectId)) {
      fetchById(projectId);
    }
  }, [projectId, fetchById]);

  useEffect(() => {
    if (!selected) return;

    const fetchDeployments = async () => {
      setIsLoadingDeployments(true);
      try {
        const axiosInstance = (
          await import("@/utils/axiosInstance")
        ).getAxiosInstance();
        const res = await axiosInstance.get(
          `/api/services/app/Deployment/GetByProjectId`,
          { params: { projectId: selected.id } },
        );
        setDeployments(res.data.result ?? []);
      } catch {
        setDeployments([]);
      } finally {
        setIsLoadingDeployments(false);
      }
    };

    const fetchRepository = async () => {
      setIsLoadingRepo(true);
      try {
        const axiosInstance = (
          await import("@/utils/axiosInstance")
        ).getAxiosInstance();
        const res = await axiosInstance.get(
          `/api/services/app/ProjectRepository/GetByProjectId`,
          { params: { projectId: selected.id } },
        );
        const repos = res.data.result ?? [];
        setRepository(repos.length > 0 ? repos[0] : null);
      } catch {
        setRepository(null);
      } finally {
        setIsLoadingRepo(false);
      }
    };

    fetchDeployments();
    fetchRepository();
  }, [selected]);

  const handleBack = () => {
    router.push("/projects");
  };

  if (isPending && !selected) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingState}>
          <Spin indicator={<LoadingOutlined style={{ fontSize: 36 }} spin />} />
          <Text className={styles.loadingText}>Loading project...</Text>
        </div>
      </div>
    );
  }

  if (isError || !selected) {
    return (
      <div className={styles.container}>
        <div className={styles.errorState}>
          <Title level={4} className={styles.errorTitle}>
            Project not found
          </Title>
          <Paragraph className={styles.errorDescription}>
            The project you are looking for does not exist or you do not have
            permission to view it.
          </Paragraph>
          <Button
            type="primary"
            icon={<ArrowLeftOutlined />}
            onClick={handleBack}
            className={styles.backButton}
          >
            Back to Projects
          </Button>
        </div>
      </div>
    );
  }

  const statusLabel = statusLabelMap[selected.status] ?? "Unknown";
  const statusColor = statusColorMap[statusLabel] ?? "default";
  const frameworkLabel = frameworkLabelMap[selected.framework] ?? "Unknown";
  const languageLabel = languageLabelMap[selected.language] ?? "Unknown";
  const databaseLabel = databaseLabelMap[selected.databaseOption] ?? "Unknown";

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Button
          type="text"
          icon={<ArrowLeftOutlined />}
          onClick={handleBack}
          className={styles.backLink}
        >
          Back to projects
        </Button>

        <div className={styles.titleRow}>
          <div>
            <Title level={2} className={styles.projectName}>
              {selected.name}
            </Title>
            <Tag color={statusColor} className={styles.statusTag}>
              {statusLabel}
            </Tag>
          </div>
        </div>

        {selected.statusMessage && (
          <Paragraph className={styles.statusMessage}>
            {selected.statusMessage}
          </Paragraph>
        )}
      </div>

      <div className={styles.content}>
        <div className={styles.mainColumn}>
          <Card className={styles.card} styles={{ body: { padding: 0 } }}>
            <div className={styles.cardHeader}>
              <BranchesOutlined className={styles.cardIcon} />
              <span className={styles.cardTitle}>Project Details</span>
            </div>
            <div className={styles.cardBody}>
              <div className={styles.detailGrid}>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Framework</span>
                  <span className={styles.detailValue}>{frameworkLabel}</span>
                </div>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Language</span>
                  <span className={styles.detailValue}>{languageLabel}</span>
                </div>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Database</span>
                  <span className={styles.detailValue}>{databaseLabel}</span>
                </div>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Auth</span>
                  <span className={styles.detailValue}>
                    {selected.includeAuth ? "Enabled" : "Disabled"}
                  </span>
                </div>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Created</span>
                  <span className={styles.detailValue}>
                    {new Date(selected.createdAt).toLocaleString()}
                  </span>
                </div>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Updated</span>
                  <span className={styles.detailValue}>
                    {new Date(selected.updatedAt).toLocaleString()}
                  </span>
                </div>
              </div>
            </div>
          </Card>

          {selected.prompt && (
            <Card className={styles.card} styles={{ body: { padding: 0 } }}>
              <div className={styles.cardHeader}>
                <span className={styles.cardTitle}>Prompt</span>
              </div>
              <div className={styles.cardBody}>
                <Paragraph className={styles.promptText}>
                  {selected.prompt}
                </Paragraph>
              </div>
            </Card>
          )}

          <Card className={styles.card} styles={{ body: { padding: 0 } }}>
            <div className={styles.cardHeader}>
              <DeploymentUnitOutlined className={styles.cardIcon} />
              <span className={styles.cardTitle}>Deployments</span>
            </div>
            <div className={styles.cardBody}>
              {isLoadingDeployments ? (
                <div className={styles.loadingInline}>
                  <Spin size="small" />
                  <span>Loading deployments...</span>
                </div>
              ) : deployments.length > 0 ? (
                <Timeline
                  items={deployments.map((dep) => ({
                    children: (
                      <div className={styles.deploymentItem}>
                        <div className={styles.deploymentHeader}>
                          <Tag
                            color={
                              deploymentStatusColorMap[dep.status] ?? "default"
                            }
                          >
                            {dep.status}
                          </Tag>
                          <span className={styles.deploymentEnv}>
                            {dep.environmentName}
                          </span>
                          <span className={styles.deploymentTarget}>
                            {dep.target}
                          </span>
                        </div>
                        {dep.url && (
                          <a
                            href={dep.url}
                            target="_blank"
                            rel="noopener noreferrer"
                            className={styles.deploymentUrl}
                          >
                            <Link /> {dep.url}
                          </a>
                        )}
                        {dep.triggeredAt && (
                          <div className={styles.deploymentTime}>
                            Triggered:{" "}
                            {new Date(dep.triggeredAt).toLocaleString()}
                          </div>
                        )}
                        {dep.completedAt && (
                          <div className={styles.deploymentTime}>
                            Completed:{" "}
                            {new Date(dep.completedAt).toLocaleString()}
                          </div>
                        )}
                        {dep.errorMessage && (
                          <div className={styles.deploymentError}>
                            {dep.errorMessage}
                          </div>
                        )}
                      </div>
                    ),
                  }))}
                />
              ) : (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No deployments yet"
                  className={styles.emptyState}
                />
              )}
            </div>
          </Card>
        </div>

        <div className={styles.sideColumn}>
          <Card className={styles.card} styles={{ body: { padding: 0 } }}>
            <div className={styles.cardHeader}>
              <GithubOutlined className={styles.cardIcon} />
              <span className={styles.cardTitle}>Repository</span>
            </div>
            <div className={styles.cardBody}>
              {isLoadingRepo ? (
                <div className={styles.loadingInline}>
                  <Spin size="small" />
                  <span>Loading repository...</span>
                </div>
              ) : repository ? (
                <div className={styles.repoInfo}>
                  <div className={styles.repoName}>
                    <GithubOutlined className={styles.repoIcon} />
                    <a
                      href={repository.htmlUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className={styles.repoLink}
                    >
                      {repository.fullName}
                    </a>
                  </div>
                  <div className={styles.repoDetails}>
                    <div className={styles.repoDetailRow}>
                      <span className={styles.repoLabel}>Provider</span>
                      <span className={styles.repoValue}>
                        {repository.provider}
                      </span>
                    </div>
                    <div className={styles.repoDetailRow}>
                      <span className={styles.repoLabel}>Branch</span>
                      <span className={styles.repoValue}>
                        {repository.defaultBranch}
                      </span>
                    </div>
                    <div className={styles.repoDetailRow}>
                      <span className={styles.repoLabel}>Visibility</span>
                      <Tag>{repository.visibility}</Tag>
                    </div>
                    <div className={styles.repoDetailRow}>
                      <span className={styles.repoLabel}>Created</span>
                      <span className={styles.repoValue}>
                        {new Date(repository.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                </div>
              ) : (
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description="No repository connected"
                  className={styles.emptyState}
                />
              )}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
