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
  SearchIcon,
  FolderPlusIcon,
  MonitorIcon,
  ServerIcon,
  DatabaseIcon,
  LoaderIcon,
} from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import type { AxiosError } from "axios";
import { useStyles } from "./styles";
import {
  ProjectStatus,
  useProjectAction,
  useProjectState,
} from "@/providers/projects-provider";
import { useAuthState } from "@/providers/auth-provider";
import { getAxiosInstance } from "@/utils/axiosInstance";
import { ClaimDeployment } from "@/components/blocks/claim-deployment";

interface GenerationPageProps {
  onNavigate: (page: string) => void;
}

type StepStatus = "completed" | "active" | "pending";

type StatusBadgeState = "Generating" | "Generated" | "Deploying" | "Live";

// ─── Fixed 5-step pipeline definition ────────────────────────────────────────

interface PipelinePhase {
  id: number;
  name: string;
  description: string;
  icon: React.ElementType;
}

const PIPELINE_PHASES: PipelinePhase[] = [
  {
    id: 1,
    name: "Requirements Analysis",
    description: "AI analyzes your prompt and extracts features",
    icon: SearchIcon,
  },
  {
    id: 2,
    name: "Scaffolding",
    description: "Setting up project structure from your template",
    icon: FolderPlusIcon,
  },
  {
    id: 3,
    name: "Frontend",
    description: "Building UI pages and components",
    icon: MonitorIcon,
  },
  {
    id: 4,
    name: "Backend",
    description: "Generating API routes and services",
    icon: ServerIcon,
  },
  {
    id: 5,
    name: "Database",
    description: "Creating schemas, models, and migrations",
    icon: DatabaseIcon,
  },
];

const TOTAL_PHASES = PIPELINE_PHASES.length;

// ─── Parse [N/5] prefix from statusMessage ───────────────────────────────────

function parsePhase(
  msg: string | null | undefined,
): { phase: number; detail: string } | null {
  if (!msg) return null;
  const match = msg.match(/^\[(\d+)\/\d+\]\s*(.*)/);
  if (!match) return null;
  return { phase: parseInt(match[1], 10), detail: match[2] };
}

// ─── Sub-components ──────────────────────────────────────────────────────────

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
      {status === "Generating" && (
        <motion.span
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
          style={{ display: "inline-flex", marginRight: 6 }}
        >
          <LoaderIcon style={{ width: 12, height: 12 }} />
        </motion.span>
      )}
      {status}
    </span>
  );
};

interface PipelineStepProps {
  phase: PipelinePhase;
  status: StepStatus;
  detail?: string;
  isLast?: boolean;
}

const PipelineStep = ({ phase, status, detail, isLast }: PipelineStepProps) => {
  const { styles, cx } = useStyles();
  const isCompleted = status === "completed";
  const isActive = status === "active";
  const Icon = phase.icon;

  return (
    <div className={styles.stepRow}>
      <div className={styles.stepRail}>
        <motion.div
          className={cx(
            styles.stepDot,
            isCompleted && styles.stepDotCompleted,
            isActive && styles.stepDotActive,
          )}
          animate={isActive ? { scale: [1, 1.12, 1] } : {}}
          transition={
            isActive
              ? { duration: 1.5, repeat: Infinity, ease: "easeInOut" }
              : {}
          }
        >
          {isCompleted ? (
            <motion.div
              initial={{ scale: 0, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              transition={{ type: "spring", stiffness: 400, damping: 15 }}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
              }}
            >
              <CheckIcon className={styles.stepCheck} />
            </motion.div>
          ) : (
            <Icon className={styles.stepIcon} />
          )}
        </motion.div>
        {!isLast && (
          <div
            className={cx(
              styles.stepLine,
              isCompleted && styles.stepLineCompleted,
            )}
          />
        )}
      </div>
      <div className={styles.stepContent}>
        <div className={styles.stepTitleRow}>
          <span
            className={cx(
              styles.stepTitle,
              isActive && styles.stepTitleActive,
              status === "pending" && styles.stepTitlePending,
            )}
          >
            {phase.name}
          </span>
          {isActive && (
            <motion.span
              className={styles.stepActiveBadge}
              initial={{ opacity: 0, x: -8 }}
              animate={{ opacity: 1, x: 0 }}
            >
              In progress
            </motion.span>
          )}
        </div>
        <span className={styles.stepDescription}>{phase.description}</span>
        <AnimatePresence>
          {isActive && detail && (
            <motion.span
              className={styles.stepDetail}
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: "auto" }}
              exit={{ opacity: 0, height: 0 }}
            >
              {detail}
            </motion.span>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
};

// ─── Main page ───────────────────────────────────────────────────────────────

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function GenerationPage({ onNavigate }: GenerationPageProps) {
  const { styles, cx } = useStyles();
  const searchParams = useSearchParams();

  const { selected: project } = useProjectState();
  const { fetchById } = useProjectAction();
  const { isGithubConnected } = useAuthState();

  // Resolve project ID from state, URL param, or sessionStorage
  const [projectId, setProjectId] = useState<number | null>(null);
  useEffect(() => {
    if (projectId) return;
    const fromState = project?.id ?? null;
    const urlParam = searchParams.get("id");
    const fromUrl = urlParam ? parseInt(urlParam, 10) : NaN;
    const stored =
      typeof window !== "undefined"
        ? parseInt(sessionStorage.getItem("generatingProjectId") ?? "", 10)
        : NaN;
    const resolved =
      fromState ??
      (!Number.isNaN(fromUrl) ? fromUrl : null) ??
      (!Number.isNaN(stored) ? stored : null);
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
  const [githubRepoFullName, setGithubRepoFullName] = useState<string | null>(
    null,
  );

  // Track the latest phase detail from statusMessage
  const [phaseDetails, setPhaseDetails] = useState<Record<number, string>>({});
  const [currentPhase, setCurrentPhase] = useState(0);
  const [completedMessages, setCompletedMessages] = useState<string[]>([]);

  const deploymentSteps = useMemo(
    () => [
      { name: "Creating GitHub repository" },
      { name: "Committing generated code" },
      { name: "Starting BuildJob" },
      { name: "Running deployment" },
      { name: "Publishing LiveUrl" },
    ],
    [],
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
  const isCodeGenInProgress =
    !project ||
    project.status === ProjectStatus.Draft ||
    project.status === ProjectStatus.PromptSubmitted ||
    project.status === ProjectStatus.CodeGenerationInProgress;
  const isCodeGenFailed = project?.status === ProjectStatus.Failed;
  const isCodeGenDone =
    project?.status === ProjectStatus.CodeGenerationCompleted ||
    project?.status === ProjectStatus.RepositoryPushInProgress ||
    project?.status === ProjectStatus.Deployed;

  // Parse [N/7] prefix from statusMessage to drive pipeline steps
  useEffect(() => {
    if (!project?.statusMessage) return;
    const msg = project.statusMessage.replace(/\.{2,}$/, "").trim();
    if (!msg) return;

    const parsed = parsePhase(msg);
    if (parsed) {
      setCurrentPhase(parsed.phase);
      setPhaseDetails((prev) => ({
        ...prev,
        [parsed.phase]: parsed.detail,
      }));
    }

    // Also track raw messages for the log
    setCompletedMessages((prev) => {
      if (prev.includes(msg)) return prev;
      return [...prev, msg];
    });
  }, [project?.statusMessage]);

  // When codegen finishes, mark all phases done
  useEffect(() => {
    if (isCodeGenDone) {
      setCurrentPhase(TOTAL_PHASES + 1);
    }
  }, [isCodeGenDone]);

  // Build step statuses
  const pipelineSteps = useMemo(() => {
    return PIPELINE_PHASES.map((phase) => {
      let status: StepStatus;
      if (isCodeGenDone || phase.id < currentPhase) {
        status = "completed";
      } else if (phase.id === currentPhase) {
        status = "active";
      } else {
        status = "pending";
      }
      return {
        ...phase,
        status,
        detail: phaseDetails[phase.id],
      };
    });
  }, [currentPhase, phaseDetails, isCodeGenDone]);

  const progressPercentage = isCodeGenDone
    ? 100
    : currentPhase > 0
      ? Math.min(
          Math.round(
            ((currentPhase - 1) / TOTAL_PHASES) * 100 + (1 / TOTAL_PHASES) * 50,
          ),
          95,
        )
      : 0;

  // Update default repo name once project loads
  useEffect(() => {
    if (project?.name && repoName === "promptforge-app") {
      setRepoName(
        project.name
          .toLowerCase()
          .replace(/[^a-z0-9]+/g, "-")
          .replace(/^-|-$/g, "")
          .slice(0, 50) || "promptforge-app",
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

  const getStepStatus = (index: number, currentStep: number): StepStatus => {
    if (index < currentStep) return "completed";
    if (index === currentStep) return "active";
    return "pending";
  };

  const handleDeploy = async () => {
    if (!isGithubConnected) {
      setDeployError("GitHub must be connected before deploying. Please complete GitHub OAuth first.");
      return;
    }

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
      const repoUrl = repository?.htmlUrl ?? "";
      const fullName = repository?.fullName ?? "";
      const ownerFromFullName = fullName.includes("/")
        ? fullName.split("/")[0]
        : configuredOwner || undefined;
      const repoFromFullName = fullName.includes("/")
        ? fullName.split("/")[1]
        : (repository?.name ?? sanitizedRepoName);

      setDeploymentStep(1);

      await instance.post("/api/github-app/commit-generated", {
        projectId: project.id,
        owner: ownerFromFullName,
        repositoryName: repoFromFullName,
        repositoryFullName: fullName || undefined,
        branch: branchName,
        commitMessage: `feat: initial generated project commit (${projectTitle})`,
      });

      setDeploymentStep(2);
      setDeploymentStep(deploymentSteps.length);

      setGithubRepoUrl(repository?.htmlUrl ?? null);
      setGithubRepoFullName(
        repository?.fullName ?? repository?.name ?? sanitizedRepoName,
      );
    } catch (error) {
      setIsDeploying(false);
      setDeploymentStep(-1);
      const axiosError = error as AxiosError<{
        result?: { message?: string; details?: string; error?: string };
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
          "Repository creation failed. Verify GitHub App credentials and installation access.",
      );
    } finally {
      setIsCreatingRepo(false);
    }
  };

  return (
    <div className={styles.page}>
      {/* ── Header with progress ───────────────────────────────────────── */}
      <div className={styles.header}>
        <div className={styles.titleRow}>
          <div>
            <p className={styles.eyebrow}>Generation workspace</p>
            <h1 className={styles.title}>{projectTitle}</h1>
            <p className={styles.subtitle}>
              Building your app step by step — sit back while we handle the
              heavy lifting.
            </p>
          </div>
          <StatusBadge status={generationStatus} />
        </div>

        <div className={styles.progressWrap}>
          <div className={styles.progressTrack}>
            <motion.div
              className={styles.progressFill}
              initial={{ width: 0 }}
              animate={{ width: `${progressPercentage}%` }}
              transition={{ duration: 0.6, ease: "easeOut" }}
            />
            {isCodeGenInProgress && <div className={styles.progressShimmer} />}
          </div>
          <div className={styles.progressMeta}>
            <span>
              {isCodeGenDone
                ? "All steps completed"
                : currentPhase > 0
                  ? `Step ${Math.min(currentPhase, TOTAL_PHASES)} of ${TOTAL_PHASES}`
                  : "Preparing..."}
            </span>
            <span>{progressPercentage}%</span>
          </div>
        </div>
      </div>

      {/* ── Error state ────────────────────────────────────────────────── */}
      {isCodeGenFailed && (
        <div
          className={styles.card}
          style={{ borderColor: "var(--ant-color-error)", marginBottom: 16 }}
        >
          <p
            style={{
              color: "var(--ant-color-error)",
              margin: 0,
              fontWeight: 600,
            }}
          >
            Code generation failed. Please go back and create a new project.
          </p>
        </div>
      )}

      {/* ── Main grid: Pipeline + right panel ──────────────────────────── */}
      <div className={styles.grid}>
        {/* Left: Fixed 7-step pipeline */}
        <div className={styles.card}>
          <div className={styles.cardHeader}>
            <div>
              <h3 className={styles.cardTitle}>Build pipeline</h3>
              <p className={styles.cardSubtitle}>
                Your app is being built in {TOTAL_PHASES} steps
              </p>
            </div>
            <div className={styles.cardBadge}>
              {isCodeGenDone
                ? "Complete"
                : currentPhase > 0
                  ? `Phase ${currentPhase}`
                  : "Starting"}
            </div>
          </div>

          <div className={styles.stepStack}>
            {pipelineSteps.map((step, index) => (
              <PipelineStep
                key={step.id}
                phase={step}
                status={step.status}
                detail={step.detail}
                isLast={index === pipelineSteps.length - 1}
              />
            ))}
          </div>
        </div>

        {/* Right: Live activity + output review */}
        <div className={styles.panelStack}>
          {/* Live activity card */}
          <div className={styles.card}>
            <div className={styles.cardHeader}>
              <div>
                <h3 className={styles.cardTitle}>Live activity</h3>
                <p className={styles.cardSubtitle}>
                  Real-time updates from the build
                </p>
              </div>
            </div>

            <div className={styles.activityFeed}>
              <AnimatePresence>
                {completedMessages.length === 0 && isCodeGenInProgress && (
                  <motion.div
                    className={styles.activityItem}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                  >
                    <motion.span
                      className={styles.activityDot}
                      animate={{ opacity: [1, 0.3, 1] }}
                      transition={{ duration: 1.2, repeat: Infinity }}
                    />
                    <span className={styles.activityText}>
                      Initializing generation pipeline...
                    </span>
                  </motion.div>
                )}
                {completedMessages.map((msg, i) => {
                  const parsed = parsePhase(msg);
                  const isLatest =
                    i === completedMessages.length - 1 && isCodeGenInProgress;
                  return (
                    <motion.div
                      key={msg}
                      className={styles.activityItem}
                      initial={{ opacity: 0, y: 8 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: 0.05 }}
                    >
                      {isLatest ? (
                        <motion.span
                          className={styles.activityDotActive}
                          animate={{ scale: [1, 1.3, 1] }}
                          transition={{ duration: 1, repeat: Infinity }}
                        />
                      ) : (
                        <span className={styles.activityDotDone} />
                      )}
                      <span
                        className={
                          isLatest
                            ? styles.activityTextActive
                            : styles.activityText
                        }
                      >
                        {parsed ? parsed.detail : msg}
                      </span>
                    </motion.div>
                  );
                })}
              </AnimatePresence>
            </div>
          </div>

          {/* Output review */}
          <AnimatePresence>
            {isCodeGenDone && (
              <motion.div
                className={styles.card}
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
              >
                <div className={styles.cardHeader}>
                  <div>
                    <h3 className={styles.cardTitle}>Build summary</h3>
                    <p className={styles.cardSubtitle}>
                      What was generated for your app
                    </p>
                  </div>
                  <div className={styles.cardBadge}>Complete</div>
                </div>

                <div className={styles.summaryGrid}>
                  <div className={styles.summaryItem}>
                    <MonitorIcon className={styles.summaryIcon} />
                    <div>
                      <span className={styles.summaryLabel}>Frontend</span>
                      <span className={styles.summaryValue}>
                        Pages, components & routing
                      </span>
                    </div>
                  </div>
                  <div className={styles.summaryItem}>
                    <ServerIcon className={styles.summaryIcon} />
                    <div>
                      <span className={styles.summaryLabel}>Backend</span>
                      <span className={styles.summaryValue}>
                        API routes & services
                      </span>
                    </div>
                  </div>
                  <div className={styles.summaryItem}>
                    <DatabaseIcon className={styles.summaryIcon} />
                    <div>
                      <span className={styles.summaryLabel}>Database</span>
                      <span className={styles.summaryValue}>
                        Schema & migrations
                      </span>
                    </div>
                  </div>
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>

      {/* ── Deploy CTA ─────────────────────────────────────────────────── */}
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
                <h2 className={styles.ctaTitle}>Your app is ready</h2>
                <p className={styles.ctaSubtitle}>
                  All steps completed successfully. Deploy to GitHub to go live.
                </p>
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
                {isCreatingRepo
                  ? "Creating repository..."
                  : "Commit and deploy"}
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

      {/* ── Deployment pipeline ────────────────────────────────────────── */}
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
                  <p className={styles.cardSubtitle}>
                    Pushing to GitHub and deploying
                  </p>
                </div>
                <div className={styles.cardBadge}>BuildJob</div>
              </div>

              <div className={styles.stepStack}>
                {deploymentSteps.map((step, index) => (
                  <PipelineStep
                    key={step.name}
                    phase={{
                      id: index,
                      name: step.name,
                      description: "",
                      icon: RocketIcon,
                    }}
                    status={getStepStatus(index, deploymentStep)}
                    isLast={index === deploymentSteps.length - 1}
                  />
                ))}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── Success state ──────────────────────────────────────────────── */}
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

              <h2 className={styles.successTitle}>Your app is live</h2>

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
                      window.open(
                        githubRepoUrl,
                        "_blank",
                        "noopener,noreferrer",
                      );
                    }
                  }}
                  disabled={!githubRepoUrl}
                >
                  <GithubIcon className={styles.iconSmall} />
                  {githubRepoFullName
                    ? `View ${githubRepoFullName}`
                    : "View on GitHub"}
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

      <AnimatePresence>
        {isDeployed && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5 }}
            className={styles.claimWrap}
          >
            <ClaimDeployment
              url={`https://live.promptforge.app/${repoName}`}
              onClaimClick={() => {
                const claimUrl = `https://vercel.com/oauth/authorize?client_id=YOUR_CLIENT_ID&redirect_uri=${encodeURIComponent(
                  window.location.origin,
                )}/vercel/callback&response_type=code`;
                window.location.href = claimUrl;
              }}
            />
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
