"use client";

import { useState } from "react";
import { Button, Input, Tag, Spin, message } from "antd";
import { SparklesIcon, PlusIcon, XIcon, ArrowRightIcon } from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import { ICodeGenSession } from "@/providers/codegen-provider";
import { useStyles } from "./CaptureStep.styles";

interface CaptureStepProps {
  onNext: (session: ICodeGenSession) => void;
}

export function CaptureStep({ onNext }: CaptureStepProps) {
  const { styles, cx } = useStyles();
  const { isPending, session } = useCodeGenState();
  const { createSession } = useCodeGenAction();

  const [prompt, setPrompt] = useState("");
  const [analyzed, setAnalyzed] = useState(false);
  const [features, setFeatures] = useState<string[]>([]);
  const [entities, setEntities] = useState<string[]>([]);
  const [projectName, setProjectName] = useState("");
  const [newFeature, setNewFeature] = useState("");
  const [newEntity, setNewEntity] = useState("");

  const maxChars = 5000;
  const remaining = maxChars - prompt.length;

  const handleAnalyze = async () => {
    if (!prompt.trim() || prompt.length < 20) {
      message.warning("Please enter a more detailed description (at least 20 characters).");
      return;
    }

    try {
      const result = await createSession(prompt);
      setFeatures(result.detectedFeatures);
      setEntities(result.detectedEntities);
      setProjectName(result.projectName);
      setAnalyzed(true);
    } catch {
      message.error("Analysis failed. Please try again.");
    }
  };

  const addFeature = () => {
    const trimmed = newFeature.trim();
    if (trimmed && !features.includes(trimmed)) {
      setFeatures((prev) => [...prev, trimmed]);
      setNewFeature("");
    }
  };

  const removeFeature = (feature: string) => {
    setFeatures((prev) => prev.filter((f) => f !== feature));
  };

  const addEntity = () => {
    const trimmed = newEntity.trim();
    if (trimmed && !entities.includes(trimmed)) {
      setEntities((prev) => [...prev, trimmed]);
      setNewEntity("");
    }
  };

  const removeEntity = (entity: string) => {
    setEntities((prev) => prev.filter((e) => e !== entity));
  };

  const handleNext = () => {
    if (session) {
      onNext({
        ...session,
        detectedFeatures: features,
        detectedEntities: entities,
        projectName,
      });
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Describe your application</h2>
        <p className={styles.subtitle}>
          Tell us what you want to build in plain English. Be as detailed as possible.
        </p>
      </div>

      <textarea
        className={styles.textarea}
        value={prompt}
        onChange={(e) => setPrompt(e.target.value.slice(0, maxChars))}
        placeholder="I want to build a project management app with kanban boards, user authentication, team workspaces, and real-time notifications..."
        disabled={analyzed}
      />
      <div className={cx(styles.counter, remaining < 200 && styles.counterWarning)}>
        {remaining.toLocaleString()} characters remaining
      </div>

      {!analyzed && (
        <div className={styles.actionRow}>
          <button
            type="button"
            className={styles.analyzeButton}
            onClick={handleAnalyze}
            disabled={isPending || prompt.trim().length < 20}
          >
            {isPending ? (
              <Spin size="small" />
            ) : (
              <SparklesIcon className={styles.iconSmall} />
            )}
            {isPending ? "Analyzing..." : "Analyze"}
          </button>
        </div>
      )}

      {analyzed && (
        <motion.div
          className={styles.resultSection}
          initial={{ opacity: 0, y: 16 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <h3 className={styles.resultTitle}>Analysis Complete</h3>
          <p className={styles.resultSubtitle}>
            Review the detected features and entities below. You can add or remove items.
          </p>

          <div className={styles.projectName}>
            <SparklesIcon className={styles.iconSmall} />
            Suggested project name: <strong>{projectName}</strong>
          </div>

          <div className={styles.sectionLabel}>Detected Features</div>
          <div className={styles.tagRow}>
            {features.map((feature) => (
              <Tag
                key={feature}
                closable
                onClose={() => removeFeature(feature)}
                color="processing"
              >
                {feature}
              </Tag>
            ))}
          </div>
          <div className={styles.addInput}>
            <Input
              size="small"
              placeholder="Add a feature..."
              value={newFeature}
              onChange={(e) => setNewFeature(e.target.value)}
              onPressEnter={addFeature}
            />
            <Button size="small" icon={<PlusIcon size={14} />} onClick={addFeature}>
              Add
            </Button>
          </div>

          <div style={{ marginTop: 24 }}>
            <div className={styles.sectionLabel}>Detected Entities</div>
            <div className={styles.entityList}>
              {entities.map((entity) => (
                <Tag key={entity} closable onClose={() => removeEntity(entity)}>
                  {entity}
                </Tag>
              ))}
            </div>
            <div className={styles.addInput}>
              <Input
                size="small"
                placeholder="Add an entity..."
                value={newEntity}
                onChange={(e) => setNewEntity(e.target.value)}
                onPressEnter={addEntity}
              />
              <Button size="small" icon={<PlusIcon size={14} />} onClick={addEntity}>
                Add
              </Button>
            </div>
          </div>

          <div className={styles.nextRow}>
            <button
              type="button"
              className={styles.analyzeButton}
              onClick={handleNext}
              disabled={features.length === 0 || entities.length === 0}
            >
              Continue to Stack
              <ArrowRightIcon className={styles.iconSmall} />
            </button>
          </div>
        </motion.div>
      )}
    </div>
  );
}
