"use client";

import { useEffect, useState } from "react";
import { Skeleton, Spin, message } from "antd";
import { RocketIcon, ArrowLeftIcon, FileTextIcon } from "lucide-react";
import { motion } from "framer-motion";
import {
  useCodeGenAction,
  useCodeGenState,
} from "@/providers/codegen-provider";
import type { IReadmeResult } from "@/providers/codegen-provider";
import { useStyles } from "./SpecReviewStep.styles";

interface SpecReviewStepProps {
  sessionId: string;
  onConfirm: () => void;
  onBack: () => void;
}

const LOADING_STAGES = [
  {
    title: "Drafting the README",
    description:
      "Turning your prompt and selected stack into a clear application brief.",
  },
  {
    title: "Deriving the build plan",
    description:
      "Recovering entities, routes, APIs, and package needs from that README.",
  },
  {
    title: "Preparing the review",
    description:
      "Organizing everything into a preview you can sanity-check before generation.",
  },
];

function isNonEmpty(value: string | null | undefined): value is string {
  return Boolean(value && value.trim());
}

function describeCount(
  count: number,
  singular: string,
  plural = `${singular}s`,
) {
  return `${count} ${count === 1 ? singular : plural}`;
}

function buildPlanHeadline(result: IReadmeResult | null): string {
  const plan = result?.plan;
  if (!plan) {
    return "";
  }

  const packageCount = [
    ...(plan.dependencyPlan?.dependencies ?? []),
    ...(plan.dependencyPlan?.devDependencies ?? []),
  ].filter((dependency) => !dependency.isExisting).length;

  return [
    describeCount(plan.entities.length, "entity"),
    describeCount(plan.pages.length, "page"),
    describeCount(plan.apiRoutes.length, "API route"),
    describeCount(packageCount, "package addition"),
  ].join(", ");
}

function buildPlannedPackages(result: IReadmeResult | null) {
  const plan = result?.plan;
  if (!plan) {
    return [];
  }

  return [
    ...(plan.dependencyPlan?.dependencies ?? []).map((dependency) => ({
      ...dependency,
      kind: "runtime",
    })),
    ...(plan.dependencyPlan?.devDependencies ?? []).map((dependency) => ({
      ...dependency,
      kind: "dev",
    })),
  ].filter(
    (dependency) => !dependency.isExisting && isNonEmpty(dependency.name),
  );
}

export function SpecReviewStep({
  sessionId,
  onConfirm,
  onBack,
}: SpecReviewStepProps) {
  const { styles } = useStyles();
  const { isPending } = useCodeGenState();
  const { generateReadme, confirmReadme } = useCodeGenAction();

  const [readmeResult, setReadmeResult] = useState<IReadmeResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [loadingStageIndex, setLoadingStageIndex] = useState(0);

  const plan = readmeResult?.plan ?? null;
  const planHeadline = buildPlanHeadline(readmeResult);
  const plannedPackages = buildPlannedPackages(readmeResult);
  const entityPreview = (plan?.entities ?? []).slice(0, 4);
  const pagePreview = (plan?.pages ?? []).slice(0, 6);
  const apiPreview = (plan?.apiRoutes ?? []).slice(0, 4);
  const packagePreview = plannedPackages.slice(0, 6);
  const hasHomePage = (plan?.pages ?? []).some((page) => page.route === "/");
  const previewMetrics = plan
    ? [
        {
          label: "Entities",
          value: plan.entities.length.toString(),
          detail:
            plan.entities.length > 0
              ? "Core data objects inferred from the README."
              : "No persistent entities planned yet.",
        },
        {
          label: "Pages",
          value: plan.pages.length.toString(),
          detail: hasHomePage
            ? "Includes a root homepage route."
            : "Homepage route still needs attention.",
        },
        {
          label: "API Routes",
          value: plan.apiRoutes.length.toString(),
          detail:
            plan.apiRoutes.length > 0
              ? "Backend endpoints the app expects."
              : "Client-side flow with no backend routes planned.",
        },
        {
          label: "Packages",
          value: plannedPackages.length.toString(),
          detail:
            plannedPackages.length > 0
              ? "New packages that scaffolding may add."
              : "No extra packages planned beyond the scaffold.",
        },
      ]
    : [];

  useEffect(() => {
    generateReadme(sessionId)
      .then((result) => {
        setReadmeResult(result);
        setLoading(false);
      })
      .catch(() => {
        setLoadError("Failed to generate README. Please retry.");
        message.error("Failed to generate README.");
        setLoading(false);
      });
  }, [generateReadme, sessionId]);

  useEffect(() => {
    if (!loading) {
      setLoadingStageIndex(0);
      return;
    }

    const interval = window.setInterval(() => {
      setLoadingStageIndex((current) => (current + 1) % LOADING_STAGES.length);
    }, 1600);

    return () => window.clearInterval(interval);
  }, [loading]);

  const handleConfirm = async () => {
    try {
      await confirmReadme(sessionId);
      onConfirm();
    } catch {
      message.error("Failed to confirm README.");
    }
  };

  const handleRetry = async () => {
    setLoading(true);
    setLoadError("");

    try {
      const result = await generateReadme(sessionId);
      setReadmeResult(result);
    } catch {
      setLoadError("Failed to generate README. Please retry.");
      message.error("Failed to generate README.");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingCard}>
          <div className={styles.loadingStageRow}>
            <Spin size="large" />
            <div className={styles.loadingStageCopy}>
              <span className={styles.loadingEyebrow}>
                Preparing your review
              </span>
              <span className={styles.loadingTitle}>
                {LOADING_STAGES[loadingStageIndex]?.title}
              </span>
              <span className={styles.loadingText}>
                {LOADING_STAGES[loadingStageIndex]?.description}
              </span>
            </div>
          </div>

          <div className={styles.loadingTimeline}>
            {LOADING_STAGES.map((stage, index) => {
              const stateClassName =
                index === loadingStageIndex
                  ? styles.loadingStageActive
                  : index < loadingStageIndex
                    ? styles.loadingStageComplete
                    : styles.loadingStagePending;

              return (
                <div
                  key={stage.title}
                  className={`${styles.loadingStagePill} ${stateClassName}`}
                >
                  <span>{stage.title}</span>
                </div>
              );
            })}
          </div>

          <div className={styles.loadingPreviewGrid}>
            {[0, 1, 2].map((cardIndex) => (
              <div key={cardIndex} className={styles.loadingPreviewCard}>
                <Skeleton
                  active
                  title={{ width: cardIndex === 0 ? "45%" : "55%" }}
                  paragraph={{ rows: cardIndex === 2 ? 4 : 3 }}
                />
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (!readmeResult) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingWrap}>
          <span className={styles.loadingText}>
            {loadError || "README is unavailable."}
          </span>
          <div className={styles.actionRow}>
            <button
              type="button"
              className={styles.backButton}
              onClick={onBack}
            >
              Back
            </button>
            <button
              type="button"
              className={styles.confirmButton}
              onClick={handleRetry}
            >
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.headerIcon}>
          <FileTextIcon size={24} />
        </div>
        <h2 className={styles.title}>Review Application Spec</h2>
        <p className={styles.subtitle}>
          Review the AI-generated README below. A structured build plan is
          derived from this exact README and reused during scaffolding and
          generation.
        </p>
      </div>

      {readmeResult.summary && (
        <motion.div
          className={styles.summaryCard}
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h3 className={styles.summaryTitle}>Summary</h3>
          <p className={styles.summaryText}>{readmeResult.summary}</p>
        </motion.div>
      )}

      {readmeResult.plan && (
        <motion.div
          className={styles.planCard}
          initial={{ opacity: 0, y: 12 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3, delay: 0.05 }}
        >
          <div className={styles.planHeader}>
            <div className={styles.planHeaderCopy}>
              <h3 className={styles.summaryTitle}>Plan Preview</h3>
              <p className={styles.summaryText}>
                {planHeadline}. This preview is generated from the approved
                README and becomes the implementation plan we scaffold from.
              </p>
            </div>
            <span className={styles.planStatusBadge}>
              {hasHomePage ? "Homepage included" : "Homepage missing"}
            </span>
          </div>

          <div className={styles.metricGrid}>
            {previewMetrics.map((metric) => (
              <div key={metric.label} className={styles.metricCard}>
                <span className={styles.metricLabel}>{metric.label}</span>
                <span className={styles.metricValue}>{metric.value}</span>
                <span className={styles.metricDetail}>{metric.detail}</span>
              </div>
            ))}
          </div>

          <div className={styles.previewGrid}>
            <div className={styles.previewSection}>
              <div className={styles.previewSectionHeader}>
                <h4 className={styles.previewSectionTitle}>Entities</h4>
                <span className={styles.previewSectionHint}>
                  What the app tracks
                </span>
              </div>
              {entityPreview.length > 0 ? (
                <div className={styles.previewList}>
                  {entityPreview.map((entity) => {
                    const fieldNames = entity.fields
                      .map((field) => field.name)
                      .filter(isNonEmpty)
                      .slice(0, 4)
                      .join(", ");

                    return (
                      <div key={entity.name} className={styles.previewListItem}>
                        <div className={styles.previewItemRow}>
                          <span className={styles.previewItemPrimary}>
                            {entity.name}
                          </span>
                          <span className={styles.previewBadge}>
                            {describeCount(entity.fields.length, "field")}
                          </span>
                        </div>
                        <span className={styles.previewItemSecondary}>
                          {fieldNames ||
                            "Fields will be refined during generation."}
                        </span>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <p className={styles.emptyState}>
                  No persistent entities are planned yet. That can still be fine
                  for a small client-side app.
                </p>
              )}
            </div>

            <div className={styles.previewSection}>
              <div className={styles.previewSectionHeader}>
                <h4 className={styles.previewSectionTitle}>Pages</h4>
                <span className={styles.previewSectionHint}>
                  Routes the user can visit
                </span>
              </div>
              {pagePreview.length > 0 ? (
                <div className={styles.previewList}>
                  {pagePreview.map((page) => (
                    <div
                      key={`${page.route}-${page.name}`}
                      className={styles.previewListItem}
                    >
                      <div className={styles.previewItemRow}>
                        <span className={styles.previewItemPrimary}>
                          {page.route}
                        </span>
                        <span className={styles.previewBadge}>
                          {page.layout}
                        </span>
                      </div>
                      <span className={styles.previewItemSecondary}>
                        {page.description || `${page.name} page`}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className={styles.emptyState}>
                  No pages are planned yet. The generator will struggle without
                  a visible route map.
                </p>
              )}
            </div>

            <div className={styles.previewSection}>
              <div className={styles.previewSectionHeader}>
                <h4 className={styles.previewSectionTitle}>API Routes</h4>
                <span className={styles.previewSectionHint}>
                  Server work expected
                </span>
              </div>
              {apiPreview.length > 0 ? (
                <div className={styles.previewList}>
                  {apiPreview.map((route) => (
                    <div
                      key={`${route.method}-${route.path}`}
                      className={styles.previewListItem}
                    >
                      <div className={styles.previewItemRow}>
                        <span className={styles.previewItemPrimary}>
                          {route.method} {route.path}
                        </span>
                        <span className={styles.previewBadge}>
                          {route.auth ? "auth" : "public"}
                        </span>
                      </div>
                      <span className={styles.previewItemSecondary}>
                        {route.description || "Recovered from the README plan."}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className={styles.emptyState}>
                  No backend API routes are planned. For a local-only app, that
                  is completely okay.
                </p>
              )}
            </div>

            <div className={styles.previewSection}>
              <div className={styles.previewSectionHeader}>
                <h4 className={styles.previewSectionTitle}>Packages</h4>
                <span className={styles.previewSectionHint}>
                  New additions beyond the scaffold
                </span>
              </div>
              {packagePreview.length > 0 ? (
                <div className={styles.previewList}>
                  {packagePreview.map((dependency) => (
                    <div
                      key={`${dependency.kind}-${dependency.name}`}
                      className={styles.previewListItem}
                    >
                      <div className={styles.previewItemRow}>
                        <span className={styles.previewItemPrimary}>
                          {dependency.name}
                        </span>
                        <span className={styles.previewBadge}>
                          {dependency.kind}
                        </span>
                      </div>
                      <span className={styles.previewItemSecondary}>
                        {dependency.purpose ||
                          `Planned package addition (${dependency.version}).`}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className={styles.emptyState}>
                  No package additions are currently planned. The base scaffold
                  should be enough.
                </p>
              )}
            </div>
          </div>
        </motion.div>
      )}

      <motion.div
        className={styles.readmeCard}
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3, delay: 0.1 }}
      >
        <div className={styles.readmeHeader}>
          <FileTextIcon size={16} />
          <span>README.md</span>
        </div>
        <div className={styles.readmeContent}>
          <pre className={styles.readmePre}>{readmeResult.readmeMarkdown}</pre>
        </div>
      </motion.div>

      <div className={styles.actionRow}>
        <button type="button" className={styles.backButton} onClick={onBack}>
          <ArrowLeftIcon size={16} />
          Back
        </button>
        <button
          type="button"
          className={styles.confirmButton}
          onClick={handleConfirm}
          disabled={isPending}
        >
          {isPending ? <Spin size="small" /> : <RocketIcon size={16} />}
          Approve & Generate
        </button>
      </div>
    </div>
  );
}
