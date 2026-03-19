"use client";

import { Checkbox, Tag } from "antd";
import { ShieldCheckIcon, WrenchIcon } from "lucide-react";
import type { IValidationRule } from "@/providers/codegen-provider";

interface ValidationChecklistProps {
  validations: IValidationRule[];
  onChange: (validations: IValidationRule[]) => void;
}

const CATEGORY_COLORS: Record<string, string> = {
  "file-exists": "blue",
  "entity-schema": "purple",
  "route-exists": "cyan",
  "build-passes": "green",
  "lint-passes": "lime",
  "env-vars": "orange",
  "test-passes": "geekblue",
  "auth-guard": "red",
  "type-check": "gold",
  "api-returns": "magenta",
};

export function ValidationChecklist({ validations, onChange }: ValidationChecklistProps) {
  const handleToggle = (id: string) => {
    onChange(validations.filter((v) => v.id !== id));
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
      {validations.map((val) => (
        <div
          key={val.id}
          style={{
            display: "flex",
            alignItems: "center",
            gap: 8,
            padding: "6px 12px",
            borderRadius: 8,
            background: "var(--ant-color-fill-quaternary)",
          }}
        >
          <Checkbox checked onChange={() => handleToggle(val.id)} />
          <Tag color={CATEGORY_COLORS[val.category] ?? "default"}>{val.category}</Tag>
          <span
            style={{
              flex: 1,
              fontSize: 13,
              color: "var(--ant-color-text)",
            }}
          >
            <strong>{val.id}</strong> — {val.description}
          </span>
          {val.automatable ? (
            <Tag icon={<ShieldCheckIcon size={12} />} color="success">
              Auto
            </Tag>
          ) : (
            <Tag icon={<WrenchIcon size={12} />} color="warning">
              Manual
            </Tag>
          )}
        </div>
      ))}
    </div>
  );
}
