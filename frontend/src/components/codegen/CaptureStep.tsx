"use client";

import { useState, useRef } from "react";
import { Button, Input, Tag, Spin, message } from "antd";
import { SparklesIcon, PlusIcon, ArrowRightIcon, PencilIcon, LockIcon, GlobeIcon, UploadCloudIcon, FileTextIcon, XIcon } from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import { ICodeGenSession } from "@/providers/codegen-provider";
import { useStyles } from "./CaptureStep.styles";
import { extractDocumentText } from "@/utils/extractDocumentText";

interface CaptureStepProps {
  onNext: (session: ICodeGenSession) => void;
}

export function CaptureStep({ onNext }: CaptureStepProps) {
  const { styles, cx } = useStyles();
  const { isPending, session } = useCodeGenState();
  const { createSession } = useCodeGenAction();

  const [prompt, setPrompt] = useState("");
  const [analyzed, setAnalyzed] = useState(false);
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [isExtracting, setIsExtracting] = useState(false);
  const [isDragOver, setIsDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [features, setFeatures] = useState<string[]>([]);
  const [entities, setEntities] = useState<string[]>([]);
  const [projectName, setProjectName] = useState("");
  const [newFeature, setNewFeature] = useState("");
  const [newEntity, setNewEntity] = useState("");
  const [isPublic, setIsPublic] = useState(false);

  // Edit states
  const [editingFeatureIndex, setEditingFeatureIndex] = useState<number | null>(null);
  const [editingFeatureValue, setEditingFeatureValue] = useState("");
  const [editingEntityIndex, setEditingEntityIndex] = useState<number | null>(null);
  const [editingEntityValue, setEditingEntityValue] = useState("");

  const maxChars = 100000;
  const remaining = maxChars - prompt.length;

  const acceptedTypes = [".pdf", ".md", ".txt"];

  const handleFileSelect = async (file: File) => {
    const ext = file.name.split(".").pop()?.toLowerCase();
    if (!ext || !acceptedTypes.some((t) => t.endsWith(ext))) {
      message.warning("Only .pdf, .md, and .txt files are supported.");
      return;
    }

    setIsExtracting(true);
    setUploadedFile(file);
    try {
      const text = await extractDocumentText(file);
      if (!text.trim()) {
        message.warning("Could not extract any text from the document.");
        setUploadedFile(null);
      } else {
        setPrompt((prev) => (prev ? `${prev}\n\n${text}` : text).slice(0, maxChars));
        message.success(`Extracted requirements from ${file.name}`);
      }
    } catch {
      message.error("Failed to extract text from the document.");
      setUploadedFile(null);
    } finally {
      setIsExtracting(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    const file = e.dataTransfer.files?.[0];
    if (file) handleFileSelect(file);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(true);
  };

  const handleDragLeave = () => setIsDragOver(false);

  const removeUploadedFile = () => {
    setUploadedFile(null);
    if (fileInputRef.current) fileInputRef.current.value = "";
  };

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

  const startEditFeature = (index: number, value: string) => {
    setEditingFeatureIndex(index);
    setEditingFeatureValue(value);
  };

  const saveEditFeature = () => {
    if (editingFeatureIndex !== null) {
      const newFeatures = [...features];
      newFeatures[editingFeatureIndex] = editingFeatureValue.trim() || features[editingFeatureIndex];
      setFeatures(newFeatures);
      setEditingFeatureIndex(null);
      setEditingFeatureValue("");
    }
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

  const startEditEntity = (index: number, value: string) => {
    setEditingEntityIndex(index);
    setEditingEntityValue(value);
  };

  const saveEditEntity = () => {
    if (editingEntityIndex !== null) {
      const newEntities = [...entities];
      newEntities[editingEntityIndex] = editingEntityValue.trim() || entities[editingEntityIndex];
      setEntities(newEntities);
      setEditingEntityIndex(null);
      setEditingEntityValue("");
    }
  };

  const handleNext = () => {
    if (session) {
      onNext({
        ...session,
        detectedFeatures: features,
        detectedEntities: entities,
        projectName,
        isPublic,
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

      {!analyzed && (
        <div
          className={cx(styles.dropZone, isDragOver && styles.dropZoneActive)}
          onDrop={handleDrop}
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onClick={() => fileInputRef.current?.click()}
        >
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.md,.txt"
            style={{ display: "none" }}
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) handleFileSelect(file);
            }}
          />
          {isExtracting ? (
            <div className={styles.dropZoneContent}>
              <Spin size="default" />
              <span className={styles.dropZoneText}>Extracting requirements...</span>
            </div>
          ) : uploadedFile ? (
            <div className={styles.dropZoneContent}>
              <FileTextIcon className={styles.dropZoneIcon} style={{ color: "#2dd4a8" }} />
              <span className={styles.dropZoneText} style={{ color: "#2dd4a8" }}>
                {uploadedFile.name}
              </span>
              <button
                type="button"
                className={styles.removeFileButton}
                onClick={(e) => {
                  e.stopPropagation();
                  removeUploadedFile();
                }}
              >
                <XIcon size={14} />
              </button>
            </div>
          ) : (
            <div className={styles.dropZoneContent}>
              <UploadCloudIcon className={styles.dropZoneIcon} />
              <span className={styles.dropZoneText}>
                Drop a <strong>.pdf</strong>, <strong>.md</strong>, or <strong>.txt</strong> file here, or click to browse
              </span>
              <span className={styles.dropZoneHint}>Optional — extract requirements from an existing document</span>
            </div>
          )}
        </div>
      )}

      <textarea
        className={cx(styles.textarea, analyzed && styles.textareaAnalyzed)}
        value={prompt}
        onChange={(e) => setPrompt(e.target.value.slice(0, maxChars))}
        placeholder="I want to build a project management app with kanban boards, user authentication, team workspaces, and real-time notifications..."
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
          <h3 className={styles.resultTitle}>Analyses Complete</h3>
          <p className={styles.resultSubtitle}>
            Review the detected features and entities below. You can refine the project name, description, or manage items.
          </p>

          <div className={styles.projectName}>
            <SparklesIcon className={styles.iconSmall} />
            <span style={{ whiteSpace: "nowrap" }}>Suggested project name:</span>
            <div className={cx(styles.projectNameInput, "flex-1")} style={{ display: "flex", alignItems: "center", gap: 8, flex: 1 }}>
              <Input
                variant="borderless"
                value={projectName}
                onChange={(e) => setProjectName(e.target.value)}
                style={{ padding: "4px 8px" }}
              />
              <PencilIcon size={14} style={{ color: "rgba(45, 212, 168, 0.6)", flexShrink: 0 }} />
            </div>
          </div>

          <div className={styles.sectionLabel}>Detected Features</div>
          <div className={styles.tagRow}>
            {features.map((feature, idx) => {
              if (editingFeatureIndex === idx) {
                return (
                  <div key={`edit-feature-${idx}`} className={styles.tagInput}>
                    <Input
                      size="small"
                      autoFocus
                      value={editingFeatureValue}
                      onChange={(e) => setEditingFeatureValue(e.target.value)}
                      onBlur={saveEditFeature}
                      onPressEnter={saveEditFeature}
                    />
                  </div>
                );
              }
              return (
                <Tag
                  key={`${feature}-${idx}`}
                  closable
                  onClose={() => removeFeature(feature)}
                  style={{ cursor: "pointer" }}
                  onClick={() => startEditFeature(idx, feature)}
                >
                  {feature}
                </Tag>
              );
            })}
          </div>
          <div className={styles.addInput}>
            <Input
              size="small"
              placeholder="Add a feature (e.g. User Auth, Notifications)..."
              value={newFeature}
              onChange={(e) => setNewFeature(e.target.value)}
              onPressEnter={addFeature}
            />
            <Button
              size="small"
              icon={<PlusIcon size={14} />}
              onClick={addFeature}
              type="text"
              style={{ color: "#8b95a2" }}
            >
              Add
            </Button>
          </div>

          <div style={{ marginTop: 24 }}>
            <div className={styles.sectionLabel}>Detected Entities</div>
            <div className={styles.tagRow}>
              {entities.map((entity, idx) => {
                if (editingEntityIndex === idx) {
                  return (
                    <div key={`edit-entity-${idx}`} className={styles.tagInput}>
                      <Input
                        size="small"
                        autoFocus
                        value={editingEntityValue}
                        onChange={(e) => setEditingEntityValue(e.target.value)}
                        onBlur={saveEditEntity}
                        onPressEnter={saveEditEntity}
                      />
                    </div>
                  );
                }
                return (
                  <Tag
                    key={`${entity}-${idx}`}
                    closable
                    onClose={() => removeEntity(entity)}
                    style={{ cursor: "pointer" }}
                    onClick={() => startEditEntity(idx, entity)}
                  >
                    {entity}
                  </Tag>
                );
              })}
            </div>
            <div className={styles.addInput}>
              <Input
                size="small"
                placeholder="Add an entity (e.g. Project, Task, User)..."
                value={newEntity}
                onChange={(e) => setNewEntity(e.target.value)}
                onPressEnter={addEntity}
              />
              <Button
                size="small"
                icon={<PlusIcon size={14} />}
                onClick={addEntity}
                type="text"
                style={{ color: "#8b95a2" }}
              >
                Add
              </Button>
            </div>
          </div>

          <div style={{ marginTop: 24 }}>
            <div className={styles.sectionLabel}>Project Visibility</div>
            <div className={styles.visibilityGrid}>
              <button
                type="button"
                className={cx(
                  styles.visibilityCard,
                  !isPublic && styles.visibilityCardSelected
                )}
                onClick={() => setIsPublic(false)}
              >
                <LockIcon
                  className={cx(
                    styles.visibilityIcon,
                    !isPublic && styles.visibilityIconSelected
                  )}
                />
                <span className={styles.visibilityLabel}>Private</span>
                <span className={styles.visibilityDesc}>
                  Only you and your team can view this project.
                </span>
              </button>
              <button
                type="button"
                className={cx(
                  styles.visibilityCard,
                  isPublic && styles.visibilityCardSelected
                )}
                onClick={() => setIsPublic(true)}
              >
                <GlobeIcon
                  className={cx(
                    styles.visibilityIcon,
                    isPublic && styles.visibilityIconSelected
                  )}
                />
                <span className={styles.visibilityLabel}>Public</span>
                <span className={styles.visibilityDesc}>
                  Anyone with the link can view this project.
                </span>
              </button>
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
