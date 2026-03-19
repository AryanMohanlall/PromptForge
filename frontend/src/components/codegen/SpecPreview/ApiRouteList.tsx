"use client";

import { useState } from "react";
import { Tag, Button, Input, Select, Switch, Modal } from "antd";
import { PlusIcon, Trash2Icon } from "lucide-react";
import type { IApiRouteSpec } from "@/providers/codegen-provider";

interface ApiRouteListProps {
  routes: IApiRouteSpec[];
  onChange: (routes: IApiRouteSpec[]) => void;
}

const METHOD_COLORS: Record<string, string> = {
  GET: "green",
  POST: "blue",
  PUT: "orange",
  PATCH: "gold",
  DELETE: "red",
};

const METHOD_OPTIONS = ["GET", "POST", "PUT", "PATCH", "DELETE"].map((m) => ({
  label: m,
  value: m,
}));

export function ApiRouteList({ routes, onChange }: ApiRouteListProps) {
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<IApiRouteSpec>({
    method: "GET",
    path: "",
    handler: "",
    responseShape: {},
    auth: true,
    description: "",
  });

  const handleRemove = (index: number) => {
    onChange(routes.filter((_, i) => i !== index));
  };

  const handleAdd = () => {
    if (!editing.path || !editing.handler) return;
    onChange([...routes, editing]);
    setModalOpen(false);
    setEditing({
      method: "GET",
      path: "",
      handler: "",
      responseShape: {},
      auth: true,
      description: "",
    });
  };

  const grouped = routes.reduce<Record<string, { route: IApiRouteSpec; index: number }[]>>(
    (acc, route, index) => {
      const resource = route.path.split("/").filter(Boolean)[1] ?? "other";
      if (!acc[resource]) acc[resource] = [];
      acc[resource].push({ route, index });
      return acc;
    },
    {}
  );

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: 12 }}>
        <Button
          size="small"
          icon={<PlusIcon size={14} />}
          onClick={() => setModalOpen(true)}
        >
          Add Route
        </Button>
      </div>

      {Object.entries(grouped).map(([resource, items]) => (
        <div key={resource} style={{ marginBottom: 16 }}>
          <div
            style={{
              fontSize: 12,
              fontWeight: 600,
              color: "var(--ant-color-text-secondary)",
              textTransform: "uppercase",
              letterSpacing: "0.06em",
              marginBottom: 6,
            }}
          >
            {resource}
          </div>
          <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
            {items.map(({ route, index }) => (
              <div
                key={index}
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  padding: "6px 12px",
                  borderRadius: 8,
                  background: "var(--ant-color-fill-quaternary)",
                }}
              >
                <div style={{ display: "flex", alignItems: "center", gap: 8, flex: 1, minWidth: 0 }}>
                  <Tag color={METHOD_COLORS[route.method]}>{route.method}</Tag>
                  <strong style={{ fontFamily: "var(--ant-font-family-code)", fontSize: 13 }}>
                    {route.path}
                  </strong>
                  {route.auth && <Tag color="purple">Auth</Tag>}
                  <span
                    style={{
                      color: "var(--ant-color-text-secondary)",
                      fontSize: 12,
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      whiteSpace: "nowrap",
                    }}
                  >
                    {route.description}
                  </span>
                </div>
                <Button
                  type="text"
                  danger
                  size="small"
                  icon={<Trash2Icon size={14} />}
                  onClick={() => handleRemove(index)}
                />
              </div>
            ))}
          </div>
        </div>
      ))}

      <Modal
        title="Add API Route"
        open={modalOpen}
        onOk={handleAdd}
        onCancel={() => setModalOpen(false)}
        destroyOnClose
      >
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          <Select
            value={editing.method}
            onChange={(v) => setEditing({ ...editing, method: v as IApiRouteSpec["method"] })}
            options={METHOD_OPTIONS}
          />
          <Input
            placeholder="Path (e.g. /api/boards)"
            value={editing.path}
            onChange={(e) => setEditing({ ...editing, path: e.target.value })}
          />
          <Input
            placeholder="Handler (e.g. boards.getAll)"
            value={editing.handler}
            onChange={(e) => setEditing({ ...editing, handler: e.target.value })}
          />
          <Input
            placeholder="Description"
            value={editing.description}
            onChange={(e) => setEditing({ ...editing, description: e.target.value })}
          />
          <label style={{ display: "flex", alignItems: "center", gap: 8 }}>
            Requires Auth <Switch checked={editing.auth} onChange={(v) => setEditing({ ...editing, auth: v })} />
          </label>
        </div>
      </Modal>
    </div>
  );
}
