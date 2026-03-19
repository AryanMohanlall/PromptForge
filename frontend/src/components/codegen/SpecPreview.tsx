"use client";

import { useEffect, useState } from "react";
import { Spin, message } from "antd";
import {
  ChevronDownIcon,
  DatabaseIcon,
  LayoutIcon,
  RouteIcon,
  ShieldCheckIcon,
  FolderTreeIcon,
  RocketIcon,
} from "lucide-react";
import { motion } from "framer-motion";
import { useCodeGenAction, useCodeGenState } from "@/providers/codegen-provider";
import type { IAppSpec } from "@/providers/codegen-provider";
import { EntityTable } from "./SpecPreview/EntityTable";
import { PageList } from "./SpecPreview/PageList";
import { ApiRouteList } from "./SpecPreview/ApiRouteList";
import { ValidationChecklist } from "./SpecPreview/ValidationChecklist";
import { FileTree } from "./SpecPreview/FileTree";
import { useStyles } from "./SpecPreview.styles";

interface SpecPreviewProps {
  sessionId: string;
  onConfirm: () => void;
  onBack: () => void;
}

export function SpecPreview({ sessionId, onConfirm, onBack }: SpecPreviewProps) {
  const { styles, cx } = useStyles();
  const { isPending, session } = useCodeGenState();
  const { generateSpec, saveSpec, confirmSpec } = useCodeGenAction();

  const [spec, setSpec] = useState<IAppSpec | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [openSections, setOpenSections] = useState<Record<string, boolean>>({
    entities: true,
    pages: true,
    apiRoutes: true,
    validations: false,
    fileManifest: false,
  });

  useEffect(() => {
    if (session?.spec) {
      setSpec(session.spec);
      setLoading(false);
      setLoadError("");
    } else {
      setLoadError("");
      generateSpec(sessionId)
        .then((result) => {
          setSpec(result);
          setLoading(false);
        })
        .catch(() => {
          setLoadError("Failed to generate specification. Please retry.");
          message.error("Failed to generate specification.");
          setLoading(false);
        });
    }
  }, [generateSpec, session?.id, session?.spec, sessionId]);

  const toggleSection = (key: string) => {
    setOpenSections((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  const handleConfirm = async () => {
    if (!spec) return;

    try {
      await saveSpec(sessionId, spec);
      await confirmSpec(sessionId);
      onConfirm();
    } catch {
      message.error("Failed to confirm specification.");
    }
  };

  const handleRetryGenerateSpec = async () => {
    setLoading(true);
    setLoadError("");

    try {
      const result = await generateSpec(sessionId);
      setSpec(result);
    } catch {
      setLoadError("Failed to generate specification. Please retry.");
      message.error("Failed to generate specification.");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingWrap}>
          <Spin size="large" />
          <span className={styles.loadingText}>
            AI is generating a detailed application specification...
          </span>
          <span className={styles.loadingText}>This may take a minute.</span>
        </div>
      </div>
    );
  }

  if (!spec) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingWrap}>
          <span className={styles.loadingText}>{loadError || "Specification is unavailable."}</span>
          <div className={styles.actionRow}>
            <button type="button" className={styles.backButton} onClick={onBack}>
              Back
            </button>
            <button type="button" className={styles.confirmButton} onClick={handleRetryGenerateSpec}>
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  const sections = [
    {
      key: "entities",
      label: "Entities",
      icon: <DatabaseIcon size={18} />,
      count: spec.entities.length,
      content: (
        <EntityTable
          entities={spec.entities}
          onChange={(entities) => setSpec({ ...spec, entities })}
        />
      ),
    },
    {
      key: "pages",
      label: "Pages",
      icon: <LayoutIcon size={18} />,
      count: spec.pages.length,
      content: (
        <PageList
          pages={spec.pages}
          onChange={(pages) => setSpec({ ...spec, pages })}
        />
      ),
    },
    {
      key: "apiRoutes",
      label: "API Routes",
      icon: <RouteIcon size={18} />,
      count: spec.apiRoutes.length,
      content: (
        <ApiRouteList
          routes={spec.apiRoutes}
          onChange={(apiRoutes) => setSpec({ ...spec, apiRoutes })}
        />
      ),
    },
    {
      key: "validations",
      label: "Validations",
      icon: <ShieldCheckIcon size={18} />,
      count: spec.validations.length,
      content: (
        <ValidationChecklist
          validations={spec.validations}
          onChange={(validations) => setSpec({ ...spec, validations })}
        />
      ),
    },
    {
      key: "fileManifest",
      label: "File Manifest",
      icon: <FolderTreeIcon size={18} />,
      count: spec.fileManifest.length,
      content: <FileTree files={spec.fileManifest} />,
    },
  ];

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.title}>Review Application Spec</h2>
        <p className={styles.subtitle}>
          The AI has produced a complete specification. Review and edit before generating.
        </p>
      </div>

      <motion.div
        className={styles.sectionStack}
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        {sections.map((section) => (
          <div key={section.key} className={styles.sectionCard}>
            <div
              className={styles.sectionHeader}
              onClick={() => toggleSection(section.key)}
            >
              <div className={styles.sectionTitle}>
                {section.icon}
                {section.label}
                <span className={styles.sectionBadge}>{section.count}</span>
              </div>
              <ChevronDownIcon
                className={cx(
                  styles.chevron,
                  openSections[section.key] && styles.chevronOpen
                )}
              />
            </div>
            {openSections[section.key] && (
              <div className={styles.sectionBody}>{section.content}</div>
            )}
          </div>
        ))}
      </motion.div>

      <div className={styles.actionRow}>
        <button type="button" className={styles.backButton} onClick={onBack}>
          Back
        </button>
        <button
          type="button"
          className={styles.confirmButton}
          onClick={handleConfirm}
          disabled={isPending}
        >
          {isPending ? <Spin size="small" /> : <RocketIcon className={styles.iconSmall} />}
          Confirm &amp; Generate
        </button>
      </div>
    </div>
  );
}
