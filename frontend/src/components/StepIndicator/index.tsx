"use client";

import { CheckIcon } from "lucide-react";
import { useStyles } from "./styles";

interface StepIndicatorProps {
  currentStep: number;
  steps: string[];
}

export function StepIndicator({ currentStep, steps }: StepIndicatorProps) {
  const { styles, cx } = useStyles();

  return (
    <div className={styles.container}>
      <div className={styles.stepRow}>
        {steps.map((step, index) => {
          const stepNumber = index + 1;
          const isDone = stepNumber < currentStep;
          const isActive = stepNumber === currentStep;

          return (
            <div key={step} className={styles.stepGroup}>
              <div className={styles.stepItem}>
                <div
                  className={cx(
                    styles.stepCircle,
                    isDone && styles.stepCircleDone,
                    isActive && styles.stepCircleActive
                  )}
                >
                  {isDone ? (
                    <CheckIcon className={styles.iconSmall} />
                  ) : (
                    stepNumber
                  )}
                </div>
                <span
                  className={cx(
                    styles.stepLabel,
                    isDone && styles.stepLabelDone,
                    isActive && styles.stepLabelActive
                  )}
                >
                  {step}
                </span>
              </div>
              {index < steps.length - 1 && (
                <div
                  className={cx(
                    styles.connector,
                    isDone && styles.connectorDone,
                    isActive && styles.connectorActive
                  )}
                />
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
