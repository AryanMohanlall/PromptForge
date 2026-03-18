"use client";

import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import {
  CheckIcon,
  RocketIcon,
  CodeIcon,
  CopyIcon,
  ExternalLinkIcon,
  GithubIcon,
  RefreshCwIcon,
  ChevronRightIcon,
  FolderIcon,
  FileIcon,
  LayersIcon,
  GitBranchIcon,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import type { AxiosError } from "axios";
import { useStyles } from "./styles";
import {
  ProjectStatus,
  useProjectAction,
  useProjectState,
} from "@/providers/projects-provider";
import { getAxiosInstance } from "@/utils/axiosInstance";

interface GenerationPageProps {
  onNavigate: (page: string) => void;
}

type StepStatus = "completed" | "active" | "pending";

type StatusBadgeState = "Generating" | "Generated" | "Deploying" | "Live";

const StatusBadge = ({ status }: { status: StatusBadgeState }) => {
  const { styles, cx } = useStyles();
  const statusClassMap: Record<StatusBadgeState, string> = {
    Generating: styles.statusGenerating,
    Generated: styles.statusGenerated,
    Deploying: styles.statusDeploying,
    Live: styles.statusLive,
  };

  return (
    <span className={cx(styles.statusBadge, statusClassMap[status])}>
      {status}
    </span>
  );
};

interface PipelineStepProps {
  name: string;
  status: StepStatus;
  duration?: string;
  isLast?: boolean;
}

const PipelineStep = ({ name, status, duration, isLast }: PipelineStepProps) => {
  const { styles, cx } = useStyles();
  const isCompleted = status === "completed";
  const isActive = status === "active";

  return (
    <div className={styles.stepRow}>
      <div className={styles.stepRail}>
        <div
          className={cx(
            styles.stepDot,
            isCompleted && styles.stepDotCompleted,
            isActive && styles.stepDotActive
          )}
        >
          {isCompleted && <CheckIcon className={styles.stepCheck} />}
        </div>
        {!isLast && (
          <div
            className={cx(
              styles.stepLine,
              isCompleted && styles.stepLineCompleted
            )}
          />
        )}
      </div>
      <div className={styles.stepContent}>
        <span className={styles.stepTitle}>{name}</span>
        {duration && <span className={styles.stepDuration}>{duration}</span>}
      </div>
    </div>
  );
};

export function GenerationPage({ onNavigate }: GenerationPageProps) {
  const { styles, cx } = useStyles();
  const searchParams = useSearchParams();

  const { selected: project } = useProjectState();
  const { fetchById } = useProjectAction();

  // Resolve project ID from state, URL param, or sessionStorage (set by CreateProjectPage)
  const [projectId, setProjectId] = useState<number | null>(null);
  useEffect(() => {
    if (projectId) return;
    const fromState = project?.id ?? null;
    const urlParam = searchParams.get("id");
    const fromUrl = urlParam ? parseInt(urlParam, 10) : NaN;
    const stored = typeof window !== "undefined"
      ? parseInt(sessionStorage.getItem("generatingProjectId") ?? "", 10)
      : NaN;
    const resolved = fromState
      ?? (!Number.isNaN(fromUrl) ? fromUrl : null)
      ?? (!Number.isNaN(stored) ? stored : null);
    if (resolved) setProjectId(resolved);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.id]);

  const [isDeploying, setIsDeploying] = useState(false);
  const [deploymentStep, setDeploymentStep] = useState(-1);
  const [repoName, setRepoName] = useState("promptforge-app");
  const [branchName, setBranchName] = useState("main");
  const [autoDeploy, setAutoDeploy] = useState(true);
  const [isCreatingRepo, setIsCreatingRepo] = useState(false);
  const [deployError, setDeployError] = useState<string | null>(null);
  const [githubRepoUrl, setGithubRepoUrl] = useState<string | null>(null);
  const [githubRepoFullName, setGithubRepoFullName] = useState<string | null>(null);

  // Track generation steps dynamically from real backend status messages
  const [completedMessages, setCompletedMessages] = useState<string[]>([]);
  const [activeMessage, setActiveMessage] = useState<string | null>(null);

  const deploymentSteps = useMemo(
    () => [
      { name: "Creating GitHub repository" },
      { name: "Committing generated code" },
      { name: "Starting BuildJob" },
      { name: "Running deployment" },
      { name: "Publishing LiveUrl" },
    ],
    []
  );

  // Poll for real project status every 3 seconds
  useEffect(() => {
    if (!projectId) return;
    fetchById(projectId);
    const interval = setInterval(() => {
      fetchById(projectId);
    }, 3000);
    return () => clearInterval(interval);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projectId]);

  const projectTitle = project?.name ?? "New PromptForge build";
  const configuredOwner = (process.env.NEXT_PUBLIC_GITHUB_OWNER ?? "").trim();
  const displayOwner = configuredOwner || "owner";

  // Derive real codegen state from backend status
  const isCodeGenInProgress = !project ||
    project.status === ProjectStatus.Draft ||
    project.status === ProjectStatus.PromptSubmitted ||
    project.status === ProjectStatus.CodeGenerationInProgress;
  const isCodeGenFailed = project?.status === ProjectStatus.Failed;
  const isCodeGenDone = project?.status === ProjectStatus.CodeGenerationCompleted ||
    project?.status === ProjectStatus.RepositoryPushInProgress ||
    project?.status === ProjectStatus.Deployed;

  // Build dynamic step list from real backend statusMessage updates
  useEffect(() => {
    if (!project?.statusMessage) return;
    const msg = project.statusMessage.replace(/\.{2,}$/, "").trim();
    if (!msg) return;

    setActiveMessage((prev) => {
      // If we got a new message, push the old one to completed
      if (prev && prev !== msg) {
        setCompletedMessages((list) =>
          list.includes(prev) ? list : [...list, prev]
        );
      }
      return msg;
    });
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.statusMessage]);

  // When codegen finishes, move active message to completed
  useEffect(() => {
    if (isCodeGenDone && activeMessage) {
      setCompletedMessages((list) =>
        list.includes(activeMessage) ? list : [...list, activeMessage]
      );
      setActiveMessage(null);
    }
  }, [isCodeGenDone, activeMessage]);

  // Build the visual step list: completed steps + active step
  const generationSteps = useMemo(() => {
    const steps = completedMessages.map((name) => ({ name, status: "completed" as StepStatus }));
    if (activeMessage) {
      steps.push({ name: activeMessage, status: "active" as StepStatus });
    }
    if (isCodeGenDone && steps.length === 0) {
      steps.push({ name: "Code generation completed", status: "completed" as StepStatus });
    }
    return steps;
  }, [completedMessages, activeMessage, isCodeGenDone]);

  // Update default repo name once project loads
  useEffect(() => {
    if (project?.name && repoName === "promptforge-app") {
      setRepoName(
        project.name.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 50) || "promptforge-app"
      );
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [project?.name]);

  const isGenerated = isCodeGenDone;
  const isDeployed = isDeploying && deploymentStep >= deploymentSteps.length;

  const generationStatus: StatusBadgeState = isDeployed
    ? "Live"
    : isDeploying || isCreatingRepo
      ? "Deploying"
      : isGenerated
        ? "Generated"
        : "Generating";

  const progressPercentage = isGenerated
    ? 100
    : generationSteps.length > 0 && activeMessage
      ? Math.round((completedMessages.length / (completedMessages.length + 1)) * 100)
      : 0;

  const getStepStatus = (index: number, currentStep: number): StepStatus => {
    if (index < currentStep) return "completed";
    if (index === currentStep) return "active";
    return "pending";
  };

  const handleDeploy = async () => {
    const sanitizedRepoName = repoName.trim();
    if (!sanitizedRepoName || isDeploying || isCreatingRepo) {
      return;
    }

    if (!project?.id) {
      setDeployError("No generated project was found to commit.");
      return;
    }

    setDeployError(null);
    setIsCreatingRepo(true);
    setIsDeploying(true);
    setDeploymentStep(0);

    try {
      const instance = getAxiosInstance();

      // Step 0: Creating GitHub repository
      const response = await instance.post<{
        repository?: {
          name?: string;
          fullName?: string;
          htmlUrl?: string;
        };
      }>("/api/github-app/repositories", {
        name: sanitizedRepoName,
        description: `Generated from PromptForge: ${projectTitle}`,
        isPrivate: true,
        autoInit: true,
        owner: configuredOwner || undefined,
      });

      const repository = response.data.repository;
      const fullName = repository?.fullName ?? "";
      const ownerFromFullName = fullName.includes("/") ? fullName.split("/")[0] : configuredOwner || undefined;
      const repoFromFullName = fullName.includes("/") ? fullName.split("/")[1] : repository?.name ?? sanitizedRepoName;

      // Step 1: Committing generated code
      setDeploymentStep(1);

      await instance.post("/api/github-app/commit-generated", {
        projectId: project.id,
        owner: ownerFromFullName,
        repositoryName: repoFromFullName,
        repositoryFullName: fullName || undefined,
        branch: branchName,
        commitMessage: `feat: initial generated project commit (${projectTitle})`,
      });

      // Step 2–4: Mark remaining steps as complete (no real build/deploy pipeline yet)
      setDeploymentStep(2);
      setDeploymentStep(deploymentSteps.length);

      setGithubRepoUrl(repository?.htmlUrl ?? null);
      setGithubRepoFullName(repository?.fullName ?? repository?.name ?? sanitizedRepoName);
    } catch (error) {
      setIsDeploying(false);
      setDeploymentStep(-1);
      const axiosError = error as AxiosError<{
        result?: {
          message?: string;
          details?: string;
          error?: string;
        };
        message?: string;
        details?: string;
        error?: string;
      }>;

      const payload = axiosError.response?.data;
      const backendMessage =
        payload?.result?.details ||
        payload?.result?.error ||
        payload?.result?.message ||
        payload?.details ||
        payload?.error ||
        payload?.message;

      setDeployError(
        backendMessage ||
          "Repository creation failed. Verify GitHub App credentials and installation access."
      );
    } finally {
      setIsCreatingRepo(false);
    }
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.titleRow}>
          <div>
            <p className={styles.eyebrow}>Generation workspace</p>
            <h1 className={styles.title}>{projectTitle}</h1>
            <p className={styles.subtitle}>Tracking your AppRequest through PromptSession, GeneratedProject, BuildJob, and Deployment.</p>
          </div>
          <StatusBadge status={generationStatus} />
        </div>

        <div className={styles.progressWrap}>
          <div className={styles.progressTrack}>
            <motion.div
              className={styles.progressFill}
              initial={{ width: 0 }}
              animate={{ width: `${progressPercentage}%` }}
              transition={{ duration: 0.5 }}
            />
            <div className={styles.progressShimmer} />
          </div>
          <div className={styles.progressMeta}>
            <span>Overall progress</span>
            <span>{progressPercentage}%</span>
          </div>
        </div>
      </div>

      {isCodeGenFailed && (
        <div className={styles.card} style={{ borderColor: "var(--ant-color-error)", marginBottom: 16 }}>
          <p style={{ color: "var(--ant-color-error)", margin: 0, fontWeight: 600 }}>
            Code generation failed. Please go back and create a new project.
          </p>
        </div>
      )}

      <div className={styles.grid}>
        <div className={styles.card}>
          <div className={styles.cardHeader}>
            <div>
              <h3 className={styles.cardTitle}>Generation pipeline</h3>
              <p className={styles.cardSubtitle}>From AppRequest to GeneratedProject</p>
            </div>
            <div className={styles.cardBadge}>PromptSession</div>
          </div>

          <div className={styles.stepStack}>
            {generationSteps.length > 0 ? (
              generationSteps.map((step, index) => (
                <PipelineStep
                  key={step.name}
                  name={step.name}
                  status={step.status}
                  isLast={index === generationSteps.length - 1}
                />
              ))
            ) : isCodeGenInProgress ? (
              <div style={{
                display: "flex",
                alignItems: "center",
                gap: 8,
                padding: "10px 16px",
                background: "var(--ant-color-fill-quaternary)",
                borderRadius: 8,
                fontSize: 13,
                color: "var(--ant-color-text-secondary)",
              }}>
                <motion.span
                  animate={{ opacity: [1, 0.3, 1] }}
                  transition={{ duration: 1.2, repeat: Infinity }}
                  style={{ width: 8, height: 8, borderRadius: "50%", background: "var(--ant-color-primary)", display: "inline-block", flexShrink: 0 }}
                />
                Waiting for generation to start...
              </div>
            ) : null}
          </div>
        </div>

        <div className={styles.panelStack}>
          <div className={styles.codePanel}>
            <div className={styles.codeHeader}>
              <div className={styles.windowDots}>
                <span className={styles.dot} />
                <span className={styles.dot} />
                <span className={styles.dot} />
              </div>
              <span className={styles.codeTitle}>GeneratedProject.tsx</span>
              <div className={styles.codeActions}>
                <button className={cx(styles.iconButton, styles.focusRing)} title="Copy path">
                  <CopyIcon className={styles.iconSmall} />
                </button>
              </div>
            </div>

            <div className={styles.codeBody}>
              <div className={styles.fileTree}>
                <div className={styles.treeItem}>
                  <ChevronRightIcon className={styles.treeChevron} />
                  <FolderIcon className={styles.treeFolder} />
                  src
                </div>
                <div className={styles.treeItemIndented}>
                  <ChevronRightIcon className={styles.treeChevron} />
                  <FolderIcon className={styles.treeFolder} />
                  generation
                </div>
                <div className={styles.treeItemIndented}>
                  <ChevronRightIcon className={styles.treeChevron} />
                  <FolderIcon className={styles.treeFolder} />
                  deployment
                </div>
                <div className={cx(styles.treeItemIndented, styles.treeItemActive)}>
                  <FileIcon className={styles.treeFile} />
                  GeneratedProject.tsx
                </div>
                <div className={styles.treeItemIndented}>
                  <FileIcon className={styles.treeFile} />
                  BuildJob.ts
                </div>
                <div className={styles.treeItemIndented}>
                  <FileIcon className={styles.treeFile} />
                  LiveUrl.ts
                </div>
              </div>

              <div className={styles.codeContent}>
                <pre className={styles.codeBlock}>
                  <code>
                    <span className={styles.codeKeyword}>import</span> React, {" "}
                    {"{"} useState {"}"} {" "}
                    <span className={styles.codeKeyword}>from</span>{" "}
                    <span className={styles.codeString}>&quot;react&quot;</span>;
                    {"\n"}
                    <span className={styles.codeKeyword}>import</span> {"{"} BuildJob {"}"} {" "}
                    <span className={styles.codeKeyword}>from</span>{" "}
                    <span className={styles.codeString}>&quot;@/generation&quot;</span>;
                    {"\n\n"}
                    <span className={styles.codeKeyword}>export</span> <span className={styles.codeKeyword}>function</span>{" "}
                    <span className={styles.codeFunction}>GeneratedProject</span>() {"{"}
                    {"\n"}
                    {"  "}<span className={styles.codeKeyword}>const</span> [status] = <span className={styles.codeFunction}>useState</span>(
                    <span className={styles.codeString}>&quot;InProgress&quot;</span>);
                    {"\n\n"}
                    {"  "}<span className={styles.codeKeyword}>return</span> (
                    {"\n    "}&lt;<span className={styles.codeTag}>section</span>{" "}
                    <span className={styles.codeAttr}>aria-label</span>=
                    <span className={styles.codeString}>&quot;Generated project&quot;</span>
                    &gt;
                    {"\n      "}&lt;<span className={styles.codeTag}>h2</span>&gt;PromptSession summary&lt;/{" "}
                    <span className={styles.codeTag}>h2</span>&gt;
                    {"\n      "}&lt;<span className={styles.codeTag}>BuildJob</span>{" "}
                    <span className={styles.codeAttr}>status</span>=
                    <span className={styles.codeString}>&quot;{"{status}"}&quot;</span>
                    /&gt;
                    {"\n    "}&lt;/{" "}
                    <span className={styles.codeTag}>section</span>&gt;
                    {"\n  "});
                    {"\n"}
                    {"}"}
                  </code>
                </pre>
              </div>
            </div>
          </div>

          <details className={styles.logDetails}>
            <summary className={cx(styles.logSummary, styles.focusRing)}>
              Generation log
            </summary>
            <div className={styles.logBody}>
              {completedMessages.map((msg) => (
                <span key={msg}>
                  ✓ {msg}
                  <br />
                </span>
              ))}
              {activeMessage && (
                <span style={{ color: "var(--ant-color-primary)" }}>
                  ▸ {activeMessage}
                  <br />
                </span>
              )}
              {generationSteps.length === 0 && isCodeGenInProgress && (
                <span style={{ color: "var(--ant-color-text-tertiary)" }}>
                  Waiting for generation to start...
                </span>
              )}
            </div>
          </details>

          <div className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <h3 className={styles.cardTitle}>Output review</h3>
                <p className={styles.cardSubtitle}>Architecture snapshot + Generated modules</p>
              </div>
              <div className={styles.cardBadge}>GeneratedProject</div>
            </div>

            <div className={styles.reviewGrid}>
              <div>
                <div className={styles.reviewLabel}>Architecture snapshot</div>
                <ul className={styles.reviewList}>
                  <li className={styles.reviewItem}>
                    <LayersIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>Frontend</span>
                      <span className={styles.reviewText}>Next.js + Ant Design + antd-style</span>
                    </div>
                  </li>
                  <li className={styles.reviewItem}>
                    <LayersIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>Backend</span>
                      <span className={styles.reviewText}>ABP Framework services + DTO layer</span>
                    </div>
                  </li>
                  <li className={styles.reviewItem}>
                    <LayersIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>Data</span>
                      <span className={styles.reviewText}>Postgres + migrations scaffold</span>
                    </div>
                  </li>
                </ul>
              </div>

              <div>
                <div className={styles.reviewLabel}>Generated modules</div>
                <ul className={styles.reviewList}>
                  <li className={styles.reviewItem}>
                    <GitBranchIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>AppRequest</span>
                      <span className={styles.reviewText}>Submission + validation</span>
                    </div>
                  </li>
                  <li className={styles.reviewItem}>
                    <GitBranchIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>BuildJob</span>
                      <span className={styles.reviewText}>Queued build pipeline</span>
                    </div>
                  </li>
                  <li className={styles.reviewItem}>
                    <GitBranchIcon className={styles.reviewIcon} />
                    <div>
                      <span className={styles.reviewTitle}>Deployment</span>
                      <span className={styles.reviewText}>LiveUrl publishing</span>
                    </div>
                  </li>
                </ul>
              </div>
            </div>

            <div className={styles.reviewFooter}>
              <div>
                <div className={styles.reviewLabel}>LiveUrl</div>
                <p className={styles.liveUrlMuted}>
                  LiveUrl is published after Deployment succeeds.
                </p>
              </div>
              <button
                type="button"
                onClick={() => onNavigate("dashboard")}
                className={cx(styles.secondaryButton, styles.focusRing)}
              >
                View history
              </button>
            </div>
          </div>
        </div>
      </div>

      <AnimatePresence>
        {isGenerated && !isDeploying && !isDeployed && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className={styles.ctaCard}
          >
            <div className={styles.ctaHeader}>
              <div className={styles.ctaIcon}>
                <CheckIcon className={styles.iconMedium} />
              </div>
              <div>
                <h2 className={styles.ctaTitle}>GeneratedProject ready</h2>
                <p className={styles.ctaSubtitle}>52 files produced in 38 seconds</p>
              </div>
            </div>

            <div className={styles.formGrid}>
              <div>
                <label className={styles.inputLabel}>Repository name</label>
                <div className={styles.inputWrap}>
                  <span className={styles.inputPrefix}>{displayOwner}/</span>
                  <input
                    type="text"
                    value={repoName}
                    onChange={(event) => setRepoName(event.target.value)}
                    className={styles.input}
                  />
                </div>
              </div>
              <div>
                <label className={styles.inputLabel}>Branch</label>
                <select
                  value={branchName}
                  onChange={(event) => setBranchName(event.target.value)}
                  className={cx(styles.select, styles.focusRing)}
                >
                  <option value="main">main</option>
                  <option value="dev">dev</option>
                  <option value="release">release</option>
                </select>
              </div>
            </div>

            <div className={styles.ctaActions}>
              <button
                type="button"
                className={cx(styles.secondaryButton, styles.focusRing)}
              >
                <CodeIcon className={styles.iconSmall} />
                Preview code
              </button>
              <button
                type="button"
                onClick={handleDeploy}
                className={cx(styles.primaryButton, styles.focusRing)}
                disabled={isCreatingRepo || !repoName.trim()}
              >
                <RocketIcon className={styles.iconSmall} />
                {isCreatingRepo ? "Creating repository..." : "Commit and deploy"}
              </button>
            </div>

            {deployError && (
              <p className={styles.errorText} role="alert">
                {deployError}
              </p>
            )}

            <div className={styles.checkboxRow}>
              <input
                type="checkbox"
                id="auto-deploy"
                checked={autoDeploy}
                onChange={(event) => setAutoDeploy(event.target.checked)}
                className={styles.checkbox}
              />
              <label htmlFor="auto-deploy" className={styles.checkboxLabel}>
                Automatically deploy after commit
              </label>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {(isDeploying || isDeployed) && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: "auto" }}
            className={styles.deployWrap}
          >
            <div className={styles.sectionDivider}>
              <span>Deployment</span>
            </div>

            <div className={styles.card}>
              <div className={styles.cardHeader}>
                <div>
                  <h3 className={styles.cardTitle}>Deployment pipeline</h3>
                  <p className={styles.cardSubtitle}>RepositoryContext + DeploymentContext</p>
                </div>
                <div className={styles.cardBadge}>BuildJob</div>
              </div>

              <div className={styles.stepStack}>
                {deploymentSteps.map((step, index) => (
                  <PipelineStep
                    key={step.name}
                    name={step.name}
                    status={getStepStatus(index, deploymentStep)}
                    isLast={index === deploymentSteps.length - 1}
                  />
                ))}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <AnimatePresence>
        {isDeployed && (
          <motion.div
            initial={{ opacity: 0, scale: 0.96, y: 20 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            transition={{ type: "spring", damping: 24, stiffness: 320 }}
            className={styles.successCard}
          >
            <div className={styles.successOverlay} />
            <div className={styles.successContent}>
              <div className={styles.successIcon}>
                <CheckIcon className={styles.iconLarge} />
              </div>

              <h2 className={styles.successTitle}>LiveUrl is ready</h2>

              <div className={styles.successUrl}>
                <span className={styles.successUrlText}>
                  https://live.promptforge.app/{repoName}
                </span>
                <button
                  className={cx(styles.iconButton, styles.focusRing)}
                  title="Copy LiveUrl"
                >
                  <CopyIcon className={styles.iconSmall} />
                </button>
              </div>

              <div className={styles.successActions}>
                <button className={cx(styles.successPrimary, styles.focusRing)}>
                  <ExternalLinkIcon className={styles.iconSmall} />
                  Open live site
                </button>
                <button
                  className={cx(styles.successGhost, styles.focusRing)}
                  onClick={() => {
                    if (githubRepoUrl) {
                      window.open(githubRepoUrl, "_blank", "noopener,noreferrer");
                    }
                  }}
                  disabled={!githubRepoUrl}
                >
                  <GithubIcon className={styles.iconSmall} />
                  {githubRepoFullName ? `View ${githubRepoFullName}` : "View on GitHub"}
                </button>
                <button className={cx(styles.successGhost, styles.focusRing)}>
                  <RefreshCwIcon className={styles.iconSmall} />
                  Redeploy
                </button>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
