"use client";

import { useEffect, useRef, useState } from "react";
import { Spin } from "antd";
import {
  CheckCircle2Icon,
  XCircleIcon,
  ClockIcon,
  Loader2Icon,
} from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import type { IGenerationStatus, IValidationResult } from "@/providers/codegen-provider";
import { useStyles } from "./GenerationProgress.styles";

interface GenerationProgressProps {
  sessionId: string;
  onComplete: (status: IGenerationStatus) => void;
}

export function GenerationProgress({ sessionId, onComplete }: GenerationProgressProps) {
  const { styles, cx } = useStyles();
  const { generationStatus } = useCodeGenState();
  const { startGeneration, pollStatus } = useCodeGenAction();

  const [started, setStarted] = useState(false);
  const [activityLog, setActivityLog] = useState<string[]>([]);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    let mounted = true;

    startGeneration(sessionId)
      .then(() => {
        if (mounted) setStarted(true);
      })
      .catch(() => {
        if (mounted)
          setActivityLog((prev) => [...prev, "Failed to start generation."]);
      });

    return () => {
      mounted = false;
    };
  }, [sessionId, startGeneration]);

  useEffect(() => {
    if (!started) return;

    pollRef.current = setInterval(async () => {
      try {
        const status = await pollStatus(sessionId);

        // Update local activity log based on current status
        setActivityLog((prev) => {
          if (status.completedSteps.length > prev.length) {
            const newSteps = status.completedSteps.slice(prev.length);
            return [...prev, ...newSteps];
          }
          return prev;
        });

        if (status.isComplete || status.errorMessage) {
          if (pollRef.current) clearInterval(pollRef.current);
          onComplete(status);
        }
      } catch {
        // poll silently fails
      }
    }, 3000);

    return () => {
      if (pollRef.current) clearInterval(pollRef.current);
    };
  }, [started, sessionId, pollStatus, onComplete]);

  const validations = generationStatus?.validationResults ?? [];
  const isComplete = generationStatus?.isComplete ?? false;
  const hasError = !!generationStatus?.errorMessage;

  const currentStepsCount = generationStatus?.completedSteps?.length ?? activityLog.length;
  const estimatedTotalSteps = 8;
  
  let progressPercent = 0;

  if (isComplete && !hasError) {
    progressPercent = 100;
  } else if (hasError) {
    progressPercent = Math.min(Math.round((currentStepsCount / estimatedTotalSteps) * 100), 100);
  } else {
    progressPercent = Math.min(Math.round((currentStepsCount / estimatedTotalSteps) * 90), 95);
    progressPercent = Math.max(5, progressPercent); // Guarantee at least 5% starting visual
  }

  const getValidationIcon = (status: IValidationResult["status"]) => {
    switch (status) {
      case "passed":
        return <CheckCircle2Icon className={cx(styles.iconSmall, styles.validationPassed)} />;
      case "failed":
        return <XCircleIcon className={cx(styles.iconSmall, styles.validationFailed)} />;
      case "running":
        return <Loader2Icon className={cx(styles.iconSmall, styles.validationRunning)} />;
      default:
        return <ClockIcon className={cx(styles.iconSmall, styles.validationPending)} />;
    }
  };

  return (
    <div className={styles.container}>
      <motion.div
        className={styles.header}
        initial={{ opacity: 0, y: -12 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <div className={styles.titleRow}>
          <h2 className={styles.title}>Generating Application</h2>
          <span
            className={cx(
              styles.statusBadge,
              isComplete && !hasError && styles.statusComplete,
              hasError && styles.statusFailed
            )}
          >
            {hasError ? "Failed" : isComplete ? "Complete" : "Generating..."}
          </span>
        </div>

        <div className={styles.progressWrap}>
          <div className={styles.progressTrack}>
            <div
              className={styles.progressFill}
              style={{ width: `${isComplete && !hasError ? 100 : progressPercent}%` }}
            />
            {!isComplete && <div className={styles.progressShimmer} />}
          </div>
          <div className={styles.progressMeta}>
            <span>{generationStatus?.currentPhase ?? "Initializing..."}</span>
            <span>{isComplete && !hasError ? 100 : progressPercent}%</span>
          </div>
        </div>
      </motion.div>

      <div className={styles.grid}>
        <div className={styles.card}>
          <h3 className={styles.cardTitle}>Validations</h3>
          <div className={styles.validationStack}>
            {validations.map((val) => (
              <div key={val.id} className={styles.validationItem}>
                {getValidationIcon(val.status)}
                <span className={styles.validationText}>
                  {val.id} {val.message ? `— ${val.message}` : ""}
                </span>
              </div>
            ))}
            {validations.length === 0 && (
              <div className={styles.validationItem}>
                <Spin size="small" />
                <span className={styles.validationText}>Waiting for validations...</span>
              </div>
            )}
          </div>
        </div>

        <div className={styles.card}>
          <h3 className={styles.cardTitle}>Activity</h3>
          <div className={styles.activityFeed}>
            {activityLog.map((log, i) => (
              <div key={i} className={styles.activityItem}>
                <div
                  className={cx(
                    styles.activityDot,
                    i === activityLog.length - 1 && styles.activityDotActive
                  )}
                />
                <span className={styles.activityText}>{log}</span>
              </div>
            ))}
            {activityLog.length === 0 && (
              <div className={styles.activityItem}>
                <Spin size="small" />
                <span className={styles.activityText}>Starting generation pipeline...</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
