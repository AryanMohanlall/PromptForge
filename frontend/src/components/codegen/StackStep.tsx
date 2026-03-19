"use client";

import { useEffect, useState } from "react";
import { Spin, Tooltip, message } from "antd";
import { CheckIcon, ArrowRightIcon, SparklesIcon } from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import type { IStackConfig } from "@/providers/codegen-provider";
import { useStyles } from "./StackStep.styles";

interface StackStepProps {
  sessionId: string;
  onNext: (stack: IStackConfig) => void;
  onBack: () => void;
}

interface StackCategory {
  key: keyof Omit<IStackConfig, "reasoning">;
  label: string;
  options: string[];
}

const STACK_CATEGORIES: StackCategory[] = [
  {
    key: "framework",
    label: "Framework",
    options: ["Next.js", "React + Vite", "Angular", "Vue", ".NET Blazor"],
  },
  {
    key: "language",
    label: "Language",
    options: ["TypeScript", "JavaScript", "C#"],
  },
  {
    key: "styling",
    label: "Styling",
    options: ["Tailwind CSS", "CSS Modules", "Ant Design", "Material UI"],
  },
  {
    key: "database",
    label: "Database",
    options: ["PostgreSQL", "MongoDB", "SQLite", "None"],
  },
  {
    key: "orm",
    label: "ORM",
    options: ["Prisma", "Drizzle", "TypeORM", "Entity Framework", "None"],
  },
  {
    key: "auth",
    label: "Authentication",
    options: ["NextAuth.js", "Clerk", "Custom JWT", "None"],
  },
];

export function StackStep({ sessionId, onNext, onBack }: StackStepProps) {
  const { styles, cx } = useStyles();
  const { isPending, recommendation } = useCodeGenState();
  const { recommendStack, saveStack } = useCodeGenAction();

  const [selections, setSelections] = useState<Record<string, string>>({
    framework: "",
    language: "",
    styling: "",
    database: "",
    orm: "",
    auth: "",
  });
  const [reasoning, setReasoning] = useState<Record<string, string>>({});
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    if (!loaded) {
      recommendStack(sessionId)
        .then((rec) => {
          setSelections({
            framework: rec.framework,
            language: rec.language,
            styling: rec.styling,
            database: rec.database,
            orm: rec.orm,
            auth: rec.auth,
          });
          setReasoning(rec.reasoning);
          setLoaded(true);
        })
        .catch(() => {
          message.error("Failed to load stack recommendations.");
          setLoaded(true);
        });
    }
  }, [loaded, recommendStack, sessionId]);

  const handleSelect = (category: string, value: string) => {
    setSelections((prev) => ({ ...prev, [category]: value }));
  };

  const handleConfirm = async () => {
    const stack: IStackConfig = {
      framework: selections.framework,
      language: selections.language,
      styling: selections.styling,
      database: selections.database,
      orm: selections.orm,
      auth: selections.auth,
      reasoning,
    };

    try {
      await saveStack(sessionId, stack);
      onNext(stack);
    } catch {
      message.error("Failed to save stack configuration.");
    }
  };

  const allSelected = STACK_CATEGORIES.every((cat) => selections[cat.key]);

  if (!loaded) {
    return (
      <div className={styles.container}>
        <div style={{ display: "flex", flexDirection: "column", alignItems: "center", padding: 80, gap: 16 }}>
          <Spin size="large" />
          <span style={{ color: "var(--ant-color-text-secondary)" }}>
            AI is analyzing your requirements and recommending a stack...
          </span>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Configure your stack</h2>
        <p className={styles.subtitle}>
          Our AI has recommended the best stack for your project. You can override any choice.
        </p>
      </div>

      <motion.div
        className={styles.selectionSection}
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        {STACK_CATEGORIES.map((category) => (
          <div key={category.key}>
            <div className={styles.sectionLabel}>{category.label}</div>
            <div className={styles.selectionGrid}>
              {category.options.map((option) => {
                const isSelected = selections[category.key] === option;
                const isRecommended =
                  recommendation?.[category.key as keyof typeof recommendation] === option;
                const reasonText = reasoning[category.key];

                return (
                  <Tooltip
                    key={option}
                    title={isRecommended && reasonText ? reasonText : undefined}
                  >
                    <button
                      type="button"
                      className={cx(
                        styles.selectionCard,
                        isSelected
                          ? styles.selectionCardSelected
                          : styles.selectionCardDefault
                      )}
                      onClick={() => handleSelect(category.key, option)}
                    >
                      {isRecommended && (
                        <span className={styles.recommendedBadge}>
                          <SparklesIcon size={10} /> AI Pick
                        </span>
                      )}
                      {isSelected && (
                        <CheckIcon className={styles.selectionCheck} size={16} />
                      )}
                      <span className={styles.selectionLabel}>{option}</span>
                      {isRecommended && reasonText && (
                        <span className={styles.reasoning}>{reasonText}</span>
                      )}
                    </button>
                  </Tooltip>
                );
              })}
            </div>
          </div>
        ))}
      </motion.div>

      <div className={styles.actionRow}>
        <button type="button" className={styles.backButton} onClick={onBack}>
          Back
        </button>
        <button
          type="button"
          className={styles.nextButton}
          onClick={handleConfirm}
          disabled={!allSelected || isPending}
        >
          {isPending ? <Spin size="small" /> : null}
          Generate Spec
          <ArrowRightIcon className={styles.iconSmall} />
        </button>
      </div>
    </div>
  );
}
