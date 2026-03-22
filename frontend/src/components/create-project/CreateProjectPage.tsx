"use client";

import { useEffect, useState } from "react";
import { SparklesIcon, Edit2Icon, CheckIcon } from "lucide-react";
import { motion } from "framer-motion";
import { StepIndicator } from "../StepIndicator";
import { useStyles } from "./styles";
import { useAuthAction, useAuthState } from "@/providers/auth-provider";
import {
  ProjectDatabaseOption,
  ProjectFramework,
  ProjectProgrammingLanguage,
  ProjectStatus,
  useProjectAction,
} from "@/providers/projects-provider";
import {
  useTemplateAction,
  useTemplateState,
} from "@/providers/templates-provider";

interface CreateProjectPageProps {
  onNavigate: (page: string) => void;
}

export function CreateProjectPage({ onNavigate }: CreateProjectPageProps) {
  const { styles, cx } = useStyles();
  const { isGithubConnected } = useAuthState();
  const { connectGithub, markProjectCreated } = useAuthAction();
  const { create } = useProjectAction();
  const { items: templates, isPending: isLoadingTemplates } =
    useTemplateState();
  const { fetchAll: fetchTemplates } = useTemplateAction();
  const [currentStep, setCurrentStep] = useState(1);
  const [prompt, setPrompt] = useState("");
  const [framework, setFramework] = useState("Next.js");
  const [language, setLanguage] = useState("TypeScript");
  const [styling, setStyling] = useState("Tailwind CSS");
  const [selectedTemplateId, setSelectedTemplateId] = useState<number | null>(
    null,
  );

  useEffect(() => {
    fetchTemplates({
      isMyTemplates: true,
    });
  }, [fetchTemplates]);
  const [database, setDatabase] = useState("PostgreSQL");
  const [authEnabled, setAuthEnabled] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const suggestions = [
    "SaaS dashboard",
    "E-commerce store",
    "Blog platform",
    "Portfolio site",
    "Project tracker",
    "Social media app",
  ];
  const frameworks = [
    "Next.js",
    "React + Vite",
    "Angular",
    "Vue",
    ".NET Blazor",
  ];
  const languages = ["TypeScript", "JavaScript", "C#"];
  const stylings = ["Tailwind CSS", "CSS Modules", "Material UI", "Bootstrap"];
  const databases = ["PostgreSQL", "MongoDB", "SQLite", "None"];

  const handleNext = () => setCurrentStep((prev) => Math.min(prev + 1, 3));
  const handleBack = () => setCurrentStep((prev) => Math.max(prev - 1, 1));
  const requiresGithub = !isGithubConnected;

  const mapFramework = (value: string): ProjectFramework => {
    switch (value) {
      case "React + Vite":
        return ProjectFramework.ReactVite;
      case "Angular":
        return ProjectFramework.Angular;
      case "Vue":
        return ProjectFramework.Vue;
      case ".NET Blazor":
        return ProjectFramework.DotNetBlazor;
      case "Next.js":
      default:
        return ProjectFramework.NextJS;
    }
  };

  const mapLanguage = (value: string): ProjectProgrammingLanguage => {
    switch (value) {
      case "JavaScript":
        return ProjectProgrammingLanguage.JavaScript;
      case "C#":
        return ProjectProgrammingLanguage.CSharp;
      case "TypeScript":
      default:
        return ProjectProgrammingLanguage.TypeScript;
    }
  };

  const mapDatabase = (value: string): ProjectDatabaseOption => {
    switch (value) {
      case "MongoDB":
        return ProjectDatabaseOption.MongoCloud;
      case "PostgreSQL":
      default:
        return ProjectDatabaseOption.RenderPostgres;
    }
  };

  const deriveProjectName = (value: string): string => {
    const normalized = value
      .replace(/\s+/g, " ")
      .trim()
      .replace(/[^a-zA-Z0-9 ]/g, "");

    if (!normalized) {
      return `Project ${new Date().toISOString().slice(0, 10)}`;
    }

    return normalized.slice(0, 128);
  };

  const handleGenerate = async () => {
    if (requiresGithub) {
      return;
    }

    setIsSubmitting(true);
    setSubmitError(null);

    try {
      const project = await create({
        name: deriveProjectName(prompt),
        prompt,
        promptVersion: 1,
        promptSubmittedAt: new Date().toISOString(),
        framework: mapFramework(framework),
        language: mapLanguage(language),
        databaseOption: mapDatabase(database),
        includeAuth: authEnabled,
        status: ProjectStatus.PromptSubmitted,
        templateId: selectedTemplateId ?? undefined,
      });

      markProjectCreated();
      if (typeof window !== "undefined") {
        sessionStorage.setItem("generatingProjectId", String(project.id));
      }
      onNavigate("generation");
    } catch {
      setSubmitError("We could not create the project. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderSelectionCard = (
    options: string[],
    selected: string,
    onSelect: (val: string) => void,
  ) => (
    <div className={styles.selectionGrid}>
      {options.map((opt) => {
        const isSelected = selected === opt;
        return (
          <button
            key={opt}
            type="button"
            onClick={() => onSelect(opt)}
            className={cx(
              styles.selectionCard,
              styles.focusRing,
              isSelected
                ? styles.selectionCardSelected
                : styles.selectionCardDefault,
            )}
          >
            {isSelected && (
              <div className={styles.selectionCheck}>
                <CheckIcon className={styles.iconSmall} />
              </div>
            )}
            <span className={styles.selectionLabel}>{opt}</span>
          </button>
        );
      })}
    </div>
  );

  return (
    <div className={styles.page}>
      {requiresGithub ? (
        <div className={styles.connectWrap}>
          <div className={styles.connectCard}>
            <h2 className={styles.connectTitle}>Connect GitHub to continue</h2>
            <p className={styles.connectText}>
              Before creating your first project, connect your GitHub account to
              enable repository creation and code delivery.
            </p>
            <button
              type="button"
              onClick={connectGithub}
              className={cx(styles.connectButton, styles.focusRing)}
            >
              Connect GitHub
            </button>
          </div>
        </div>
      ) : (
        <StepIndicator
          currentStep={currentStep}
          steps={[
            "Describe your app",
            "Configure stack",
            "Review and generate",
          ]}
        />
      )}

      {!requiresGithub && (
        <motion.div
          key={currentStep}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: 0.3 }}
        >
          {currentStep === 1 && (
            <div className={styles.narrowSection}>
              <div className={styles.headerCenter}>
                <h2 className={styles.title}>What do you want to build?</h2>
                <p className={styles.subtitle}>
                  Describe your app idea in plain English. Be as detailed as you
                  like the more context you give, the better the result.
                </p>
              </div>

              <div>
                <textarea
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder="e.g. A project management app with Kanban boards, team collaboration, real-time notifications, and a dashboard showing project analytics. Users can sign up with email or Google, create workspaces, and invite team members."
                  className={cx(styles.textarea, styles.focusRing)}
                />
                <div className={styles.counter}>
                  {prompt.length.toLocaleString()} / 5,000
                </div>
              </div>

              <div className={styles.suggestionRow}>
                {suggestions.map((suggestion) => (
                  <button
                    key={suggestion}
                    type="button"
                    onClick={() =>
                      setPrompt(
                        `A modern ${suggestion.toLowerCase()} with user authentication, a dashboard, and responsive design.`,
                      )
                    }
                    className={cx(styles.suggestionButton, styles.focusRing)}
                  >
                    {suggestion}
                  </button>
                ))}
              </div>

              <div className={styles.actionRow}>
                <button type="button" disabled className={styles.textButton}>
                  Back
                </button>
                <button
                  type="button"
                  onClick={handleNext}
                  disabled={prompt.length < 10}
                  className={cx(styles.primaryButton, styles.focusRing)}
                >
                  Next: Configure stack
                </button>
              </div>
            </div>
          )}

          {currentStep === 2 && (
            <div className={styles.mediumSection}>
              <h2 className={styles.title}>Choose your tech stack</h2>

              {/* ── Template selection (always visible) ─────────────────── */}
              <div className={styles.templateSection}>
                <h3 className={styles.sectionLabel}>Start from a template</h3>
                <p className={styles.templateHint}>
                  Pick a template to guide the AI, or start from scratch and let
                  it figure out the best structure.
                </p>
                <div className={styles.templateGrid}>
                  <button
                    type="button"
                    onClick={() => setSelectedTemplateId(null)}
                    className={cx(
                      styles.templateCard,
                      styles.focusRing,
                      selectedTemplateId === null
                        ? styles.templateCardSelected
                        : styles.templateCardDefault,
                    )}
                  >
                    {selectedTemplateId === null && (
                      <div className={styles.selectionCheck}>
                        <CheckIcon className={styles.iconSmall} />
                      </div>
                    )}
                    <div className={styles.templateCardIcon}>
                      <SparklesIcon style={{ width: 20, height: 20 }} />
                    </div>
                    <span className={styles.templateCardName}>
                      Start from scratch
                    </span>
                    <span className={styles.templateCardDesc}>
                      AI decides the best structure
                    </span>
                  </button>
                  {isLoadingTemplates && templates.length === 0 && (
                    <div
                      className={styles.templateCardDesc}
                      style={{
                        padding: 16,
                        gridColumn: "1 / -1",
                        textAlign: "center",
                      }}
                    >
                      Loading templates...
                    </div>
                  )}
                  {templates.map((t) => (
                    <button
                      key={t.id}
                      type="button"
                      onClick={() => setSelectedTemplateId(t.id)}
                      className={cx(
                        styles.templateCard,
                        styles.focusRing,
                        selectedTemplateId === t.id
                          ? styles.templateCardSelected
                          : styles.templateCardDefault,
                      )}
                    >
                      {selectedTemplateId === t.id && (
                        <div className={styles.selectionCheck}>
                          <CheckIcon className={styles.iconSmall} />
                        </div>
                      )}
                      <div className={styles.templateCardIcon}>
                        <SparklesIcon style={{ width: 20, height: 20 }} />
                      </div>
                      <span className={styles.templateCardName}>{t.name}</span>
                      {t.category && (
                        <span className={styles.templateCardDesc}>
                          {t.category}
                        </span>
                      )}
                      {t.description && (
                        <span className={styles.templateCardDesc}>
                          {t.description}
                        </span>
                      )}
                    </button>
                  ))}
                </div>
              </div>

              <div className={styles.divider} />

              {/* ── Stack configuration ─────────────────────────────────── */}
              <div className={styles.selectionSection}>
                <div>
                  <h3 className={styles.sectionLabel}>Framework</h3>
                  {renderSelectionCard(frameworks, framework, setFramework)}
                </div>

                <div>
                  <h3 className={styles.sectionLabel}>Language</h3>
                  {renderSelectionCard(languages, language, setLanguage)}
                </div>

                <div>
                  <h3 className={styles.sectionLabel}>Styling</h3>
                  {renderSelectionCard(stylings, styling, setStyling)}
                </div>

                <div>
                  <h3 className={styles.sectionLabel}>Database</h3>
                  {renderSelectionCard(databases, database, setDatabase)}
                </div>
              </div>

              <div className={styles.divider} />

              <div className={styles.toggleRow}>
                <div>
                  <h3 className={styles.toggleTitle}>Include authentication</h3>
                  <p className={styles.toggleSubtitle}>
                    Adds sign-up, sign-in, password reset, and session
                    management
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => setAuthEnabled(!authEnabled)}
                  className={cx(
                    styles.toggleButton,
                    styles.focusRing,
                    authEnabled ? styles.toggleOn : styles.toggleOff,
                  )}
                >
                  <div
                    className={cx(
                      styles.toggleThumb,
                      authEnabled && styles.toggleThumbOn,
                    )}
                  />
                </button>
              </div>

              <div className={styles.actionRow}>
                <button
                  type="button"
                  onClick={handleBack}
                  className={cx(styles.textButton, styles.focusRing)}
                >
                  Back
                </button>
                <button
                  type="button"
                  onClick={handleNext}
                  className={cx(styles.primaryButton, styles.focusRing)}
                >
                  Next: Review
                </button>
              </div>
            </div>
          )}

          {currentStep === 3 && (
            <div>
              <div className={styles.reviewCard}>
                <div className={styles.reviewGrid}>
                  <div className={styles.reviewPanel}>
                    <h3 className={styles.reviewTitle}>Configuration</h3>

                    <div className={styles.reviewSection}>
                      <div className={styles.reviewLabelRow}>
                        <span className={styles.reviewLabel}>
                          Project description
                        </span>
                        <button
                          type="button"
                          onClick={() => setCurrentStep(1)}
                          className={cx(styles.editButton, styles.focusRing)}
                        >
                          <Edit2Icon className={styles.iconSmall} />
                        </button>
                      </div>
                      <p className={styles.reviewPrompt}>
                        &quot;
                        {prompt.length > 150
                          ? `${prompt.substring(0, 150)}...`
                          : prompt}
                        &quot;
                      </p>
                    </div>

                    <div className={styles.summaryGrid}>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>Framework</span>
                        <span>{framework}</span>
                      </div>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>Language</span>
                        <span>{language}</span>
                      </div>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>Styling</span>
                        <span>{styling}</span>
                      </div>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>Database</span>
                        <span>{database}</span>
                      </div>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>Template</span>
                        <span>
                          {selectedTemplateId === null
                            ? "Start from scratch"
                            : (templates.find(
                                (t) => t.id === selectedTemplateId,
                              )?.name ?? "Custom")}
                        </span>
                      </div>
                      <div className={styles.summaryItem}>
                        <span className={styles.summaryLabel}>
                          Authentication
                        </span>
                        <div className={styles.authRow}>
                          {authEnabled ? (
                            <>
                              <CheckIcon className={styles.statusIcon} />
                              Enabled
                            </>
                          ) : (
                            "Disabled"
                          )}
                        </div>
                      </div>
                    </div>
                  </div>

                  <div
                    className={cx(styles.reviewPanel, styles.reviewPanelAlt)}
                  >
                    <h3 className={styles.reviewTitle}>Estimated output</h3>
                    <ul className={styles.outputList}>
                      <li className={styles.outputItem}>
                        <div className={styles.outputBullet} />
                        <div>
                          <span className={styles.outputTitle}>
                            Estimated files
                          </span>
                          <span className={styles.outputSubtitle}>
                            ~45-60 files
                          </span>
                        </div>
                      </li>
                      <li className={styles.outputItem}>
                        <div className={styles.outputBullet} />
                        <div>
                          <span className={styles.outputTitle}>
                            Generation time
                          </span>
                          <span className={styles.outputSubtitle}>
                            30-60 seconds
                          </span>
                        </div>
                      </li>
                      <li className={styles.outputItem}>
                        <div className={styles.outputBullet} />
                        <div>
                          <span className={styles.outputTitle}>Repository</span>
                          <span className={styles.outputSubtitle}>
                            Will be created on GitHub
                          </span>
                        </div>
                      </li>
                    </ul>
                  </div>
                </div>
              </div>

              <div className={styles.generateSection}>
                <button
                  type="button"
                  onClick={handleGenerate}
                  disabled={isSubmitting}
                  className={cx(styles.generateButton, styles.focusRing)}
                >
                  <SparklesIcon className={styles.iconMedium} />
                  {isSubmitting ? "Creating project..." : "Generate my app"}
                </button>
                <p className={styles.generateNote}>
                  This will create a new GitHub repository and start code
                  generation.
                </p>
                {submitError && (
                  <p className={styles.generateNote}>{submitError}</p>
                )}

                <button
                  type="button"
                  onClick={handleBack}
                  className={cx(styles.backLink, styles.focusRing)}
                >
                  Back to configuration
                </button>
              </div>
            </div>
          )}
        </motion.div>
      )}
    </div>
  );
}
