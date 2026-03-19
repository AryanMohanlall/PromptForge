import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import GeneratePage from "./page";

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn(), replace: vi.fn() }),
  usePathname: () => "/generate",
}));

vi.mock("@/providers/codegen-provider", () => ({
  useCodeGenState: () => ({
    isPending: false,
    isSuccess: false,
    isError: false,
    session: null,
    recommendation: null,
    generationStatus: null,
    errorMessage: "",
  }),
  useCodeGenAction: () => ({
    createSession: vi.fn(),
    recommendStack: vi.fn(),
    saveStack: vi.fn(),
    generateSpec: vi.fn(),
    saveSpec: vi.fn(),
    confirmSpec: vi.fn(),
    startGeneration: vi.fn(),
    pollStatus: vi.fn(),
    triggerRepair: vi.fn(),
    resetSession: vi.fn(),
  }),
}));

vi.mock("@/components/StepIndicator", () => ({
  StepIndicator: ({ currentStep, steps }: { currentStep: number; steps: string[] }) => (
    <div data-testid="step-indicator">
      Step {currentStep} of {steps.length}
    </div>
  ),
}));

vi.mock("@/components/codegen/CaptureStep", () => ({
  CaptureStep: ({ onNext }: { onNext: () => void }) => (
    <div data-testid="capture-step">
      <button onClick={onNext}>Next</button>
    </div>
  ),
}));

vi.mock("@/components/codegen/StackStep", () => ({
  StackStep: () => <div data-testid="stack-step" />,
}));

vi.mock("@/components/codegen/SpecPreview", () => ({
  SpecPreview: () => <div data-testid="spec-preview" />,
}));

vi.mock("@/components/codegen/GenerationProgress", () => ({
  GenerationProgress: () => <div data-testid="generation-progress" />,
}));

vi.mock("@/components/codegen/GenerationResult", () => ({
  GenerationResult: () => <div data-testid="generation-result" />,
}));

describe("GeneratePage", () => {
  it("renders the step indicator", () => {
    render(<GeneratePage />);
    expect(screen.getByTestId("step-indicator")).toBeTruthy();
    expect(screen.getByText("Step 1 of 5")).toBeTruthy();
  });

  it("renders CaptureStep on initial load", () => {
    render(<GeneratePage />);
    expect(screen.getByTestId("capture-step")).toBeTruthy();
  });

  it("does not render StackStep on initial load", () => {
    render(<GeneratePage />);
    expect(screen.queryByTestId("stack-step")).toBeNull();
  });
});
