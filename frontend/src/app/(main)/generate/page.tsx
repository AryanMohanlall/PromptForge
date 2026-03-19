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
import {
  ProjectDatabaseOption,
  ProjectFramework,
  ProjectProgrammingLanguage,
  ProjectStatus,
  useProjectAction,
} from "@/providers/projects-provider";
import { useStyles } from "./styles/style";

const STEPS = ["Describe", "Stack", "Spec", "Preview", "Generate"];

function mapFramework(fw: string | undefined): ProjectFramework {
  if (!fw) return ProjectFramework.NextJS;
  const lower = fw.toLowerCase();
  if (lower.includes("next")) return ProjectFramework.NextJS;
  if (lower.includes("react") || lower.includes("vite")) return ProjectFramework.ReactVite;
  if (lower.includes("angular")) return ProjectFramework.Angular;
  if (lower.includes("vue")) return ProjectFramework.Vue;
  if (lower.includes("blazor") || lower.includes(".net")) return ProjectFramework.DotNetBlazor;
  return ProjectFramework.NextJS;
}

function mapLanguage(lang: string | undefined): ProjectProgrammingLanguage {
  if (!lang) return ProjectProgrammingLanguage.TypeScript;
  const lower = lang.toLowerCase();
  if (lower.includes("javascript") && !lower.includes("type")) return ProjectProgrammingLanguage.JavaScript;
  if (lower.includes("c#") || lower.includes("csharp")) return ProjectProgrammingLanguage.CSharp;
  return ProjectProgrammingLanguage.TypeScript;
}

function mapDatabase(db: string | undefined): ProjectDatabaseOption {
  if (!db) return ProjectDatabaseOption.RenderPostgres;
  const lower = db.toLowerCase();
  if (lower.includes("neon")) return ProjectDatabaseOption.NeonPostgres;
  if (lower.includes("mongo")) return ProjectDatabaseOption.MongoCloud;
  return ProjectDatabaseOption.RenderPostgres;
}

export default function GeneratePage() {
  const { styles } = useStyles();
  const router = useRouter();
  const { create: createProject } = useProjectAction();

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

  const handleDeploy = async () => {
    if (!session) return;

    try {
      // Create a Project entity from the session so the GenerationPage
      // can load it. Status = CodeGenerationCompleted skips background
      // codegen (files are already generated and on disk).
      const stack = session.confirmedStack;
      const project = await createProject({
        name: session.projectName || "generated-app",
        prompt: session.normalizedRequirement || session.prompt,
        promptVersion: 1,
        framework: mapFramework(stack?.framework),
        language: mapLanguage(stack?.language),
        databaseOption: mapDatabase(stack?.database),
        includeAuth: !!stack?.auth && stack.auth.toLowerCase() !== "none",
        status: ProjectStatus.CodeGenerationCompleted,
      });

      sessionStorage.setItem("generatingProjectId", String(project.id));
      router.push("/generation");
    } catch {
      // If project creation fails, navigate anyway with session fallback
      if (session.projectId) {
        sessionStorage.setItem("generatingProjectId", String(session.projectId));
      }
      router.push("/generation");
    }
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
