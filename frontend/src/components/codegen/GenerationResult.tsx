"use client";

import { Spin } from "antd";
import {
  CheckCircle2Icon,
  XCircleIcon,
  RocketIcon,
  RefreshCwIcon,
  ArrowLeftIcon,
  SaveIcon,
} from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import type { IGenerationStatus } from "@/providers/codegen-provider";
import { useStyles } from "./GenerationResult.styles";

interface GenerationResultProps {
  sessionId: string;
  status: IGenerationStatus;
  onDeploy: () => void;
  onSaveOnly?: () => void;
  onRetry: () => void;
  onBack: () => void;
  isDeploying?: boolean;
  isSaving?: boolean;
}

export function GenerationResult({
  sessionId,
  status,
  onDeploy,
  onSaveOnly,
  onRetry,
  onBack,
  isDeploying = false,
  isSaving = false,
}: GenerationResultProps) {
  const { styles } = useStyles();
  const { isPending, session } = useCodeGenState();
  const { triggerRepair } = useCodeGenAction();

  const failures = status.validationResults.filter(
    (v) => v.status === "failed"
  );
  const totalValidations = status.validationResults.length;
  const passedValidations = status.validationResults.filter((v) => v.status === "passed").length;
  const successSubtitle = totalValidations === 0
    ? "Generation completed. Validation results were not reported by the backend."
    : passedValidations === totalValidations
      ? `All ${totalValidations} validations passed. Your app is ready to deploy.`
      : `${passedValidations} of ${totalValidations} validations passed. Your app is ready to deploy.`;
  const isSuccess = !status.errorMessage && failures.length === 0;
  const repairAttempts = session?.repairAttempts ?? 0;
  const maxRepairAttempts = 5;
  const canRepair = failures.length > 0 && failures.length <= 5 && repairAttempts < maxRepairAttempts;
  const canUseRefinement = failures.length > 0 && repairAttempts >= maxRepairAttempts;

  const handleRepair = async () => {
    try {
      await triggerRepair(sessionId, failures);
      onRetry();
    } catch {
      // error handled by provider
    }
  };

  if (isSuccess) {
    return (
      <div className={styles.container}>
        <motion.div
          className={styles.successCard}
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.4 }}
        >
          <div className={styles.successOverlay} />
          <div className={styles.successContent}>
            <div className={styles.successIcon}>
              <CheckCircle2Icon className={styles.iconLarge} />
            </div>
            <h2 className={styles.successTitle}>Application Generated Successfully!</h2>
            <p className={styles.successSubtitle}>
              {successSubtitle}
            </p>
            <div className={styles.successActions}>
              <button
                type="button"
                className={styles.primaryButton}
                onClick={onDeploy}
                disabled={isDeploying}
              >
                {isDeploying ? <Spin size="small" /> : <RocketIcon size={16} />}
                {isDeploying ? "Deploying..." : "Commit & Deploy"}
              </button>
              <button
                type="button"
                className={styles.ghostButton}
                onClick={onBack}
              >
                <ArrowLeftIcon size={16} />
                Back to Spec
              </button>
            </div>
          </div>
        </motion.div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <motion.div
        className={styles.failedCard}
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        <div className={styles.failedHeader}>
          <div className={styles.failedIcon}>
            <XCircleIcon className={styles.iconLarge} />
          </div>
          <div>
            <h3 className={styles.failedTitle}>
              {status.errorMessage ? "Generation Failed" : "Validation Failures"}
            </h3>
            <p className={styles.failedSubtitle}>
              {status.errorMessage
                ? status.errorMessage
                : `${failures.length} validation(s) failed. ${canRepair ? "Auto-repair is available." : "Manual fixes required."}`}
            </p>
          </div>
        </div>

        {failures.length > 0 && (
          <div className={styles.failureList}>
            {failures.map((f) => (
              <div key={f.id} className={styles.failureItem}>
                <XCircleIcon size={14} />
                <span>
                  <strong>{f.id}</strong>: {f.message ?? "Validation failed"}
                </span>
              </div>
            ))}
          </div>
        )}

        <div className={styles.failedActions}>
          {canRepair && (
            <button
              type="button"
              className={styles.repairButton}
              onClick={handleRepair}
              disabled={isPending}
            >
              {isPending ? <Spin size="small" /> : <RefreshCwIcon size={16} />}
              Auto-Repair ({maxRepairAttempts - repairAttempts} attempts left)
            </button>
          )}
          {canUseRefinement && (
            <button
              type="button"
              className={styles.repairButton}
              onClick={onBack}
              style={{ background: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)' }}
            >
              <RefreshCwIcon size={16} />
              Use Refinement Instead
            </button>
          )}
          <button
            type="button"
            className={styles.secondaryButton}
            onClick={onBack}
          >
            <ArrowLeftIcon size={16} />
            Back to Spec
          </button>
          {!status.errorMessage && onSaveOnly && (
            <button
              type="button"
              className={styles.secondaryButton}
              onClick={onSaveOnly}
              disabled={isSaving}
              style={{ borderColor: '#ef4444', color: '#ef4444' }}
            >
              {isSaving ? <Spin size="small" /> : <SaveIcon size={16} />}
              {isSaving ? "Saving..." : "Save to DB"}
            </button>
          )}
        </div>
      </motion.div>
    </div>
  );
}
