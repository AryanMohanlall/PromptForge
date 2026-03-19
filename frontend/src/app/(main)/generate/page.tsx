"use client";

import { useCallback, useState } from "react";
import { useRouter } from "next/navigation";
import { StepIndicator } from "@/components/StepIndicator";
import { CaptureStep } from "@/components/codegen/CaptureStep";
import { StackStep } from "@/components/codegen/StackStep";
import { SpecPreview } from "@/components/codegen/SpecPreview";
import { GenerationProgress } from "@/components/codegen/GenerationProgress";
import { GenerationResult } from "@/components/codegen/GenerationResult";
import type { ICodeGenSession, IGenerationStatus, IStackConfig } from "@/providers/codegen-provider";
import { useStyles } from "./styles/style";

const STEPS = ["Describe", "Stack", "Spec", "Preview", "Generate"];

export default function GeneratePage() {
  const { styles } = useStyles();
  const router = useRouter();

  const [currentStep, setCurrentStep] = useState(1);
  const [session, setSession] = useState<ICodeGenSession | null>(null);
  const [generationStatus, setGenerationStatus] = useState<IGenerationStatus | null>(null);

  const handleCaptureNext = (result: ICodeGenSession) => {
    setSession(result);
    setCurrentStep(2);
  };

  const handleStackNext = (_stack: IStackConfig) => {
    setCurrentStep(3);
  };

  const handleSpecConfirm = () => {
    setCurrentStep(4);
  };

  const handleGenerationComplete = useCallback((status: IGenerationStatus) => {
    setGenerationStatus(status);
    setCurrentStep(5);
  }, []);

  const handleDeploy = () => {
    if (session?.projectId) {
      sessionStorage.setItem("generatingProjectId", String(session.projectId));
    }
    router.push("/generation");
  };

  const handleRetry = () => {
    setGenerationStatus(null);
    setCurrentStep(4);
  };

  const handleBackToSpec = () => {
    setCurrentStep(3);
  };

  return (
    <div className={styles.page}>
      <div className={styles.stepSection}>
        <StepIndicator currentStep={currentStep} steps={STEPS} />
      </div>

      {currentStep === 1 && <CaptureStep onNext={handleCaptureNext} />}

      {currentStep === 2 && session && (
        <StackStep
          sessionId={session.id}
          onNext={handleStackNext}
          onBack={() => setCurrentStep(1)}
        />
      )}

      {currentStep === 3 && session && (
        <SpecPreview
          sessionId={session.id}
          onConfirm={handleSpecConfirm}
          onBack={() => setCurrentStep(2)}
        />
      )}

      {currentStep === 4 && session && (
        <GenerationProgress
          sessionId={session.id}
          onComplete={handleGenerationComplete}
        />
      )}

      {currentStep === 5 && session && generationStatus && (
        <GenerationResult
          sessionId={session.id}
          status={generationStatus}
          onDeploy={handleDeploy}
          onRetry={handleRetry}
          onBack={handleBackToSpec}
        />
      )}
    </div>
  );
}
