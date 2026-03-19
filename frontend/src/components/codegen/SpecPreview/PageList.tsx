"use client";

import { useState } from "react";
import { Tag, Button, Input, Select, Modal } from "antd";
import { PlusIcon, Trash2Icon } from "lucide-react";
import type { IPageSpec } from "@/providers/codegen-provider";

interface PageListProps {
  pages: IPageSpec[];
  onChange: (pages: IPageSpec[]) => void;
}

const LAYOUT_OPTIONS = [
  { label: "Authenticated", value: "authenticated" },
  { label: "Public", value: "public" },
  { label: "Admin", value: "admin" },
];

const LAYOUT_COLORS: Record<string, string> = {
  authenticated: "blue",
  public: "green",
  admin: "red",
};

export function PageList({ pages, onChange }: PageListProps) {
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<IPageSpec>({
    route: "",
    name: "",
    layout: "authenticated",
    components: [],
    dataRequirements: [],
    description: "",
  });

  const handleRemove = (route: string) => {
    onChange(pages.filter((p) => p.route !== route));
  };

  const handleAdd = () => {
    if (!editing.route || !editing.name) return;
    onChange([...pages, editing]);
    setModalOpen(false);
    setEditing({
      route: "",
      name: "",
      layout: "authenticated",
      components: [],
      dataRequirements: [],
      description: "",
    });
  };

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "flex-end", marginBottom: 12 }}>
        <Button
          size="small"
          icon={<PlusIcon size={14} />}
          onClick={() => setModalOpen(true)}
        >
          Add Page
        </Button>
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
        {pages.map((page) => (
          <div
            key={page.route}
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              padding: "8px 12px",
              borderRadius: 8,
              background: "var(--ant-color-fill-quaternary)",
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: 8, flex: 1, minWidth: 0 }}>
              <Tag color={LAYOUT_COLORS[page.layout]}>{page.layout}</Tag>
              <strong style={{ fontFamily: "var(--ant-font-family-code)", fontSize: 13 }}>
                {page.route}
              </strong>
              <span style={{ color: "var(--ant-color-text-secondary)", fontSize: 13 }}>
                — {page.name}
              </span>
            </div>
            <div style={{ display: "flex", alignItems: "center", gap: 4, flexShrink: 0 }}>
              {page.components.map((c) => (
                <Tag key={c} color="default">
                  {c}
                </Tag>
              ))}
              <Button
                type="text"
                danger
                size="small"
                icon={<Trash2Icon size={14} />}
                onClick={() => handleRemove(page.route)}
              />
            </div>
          </div>
        ))}
      </div>

      <Modal
        title="Add Page"
        open={modalOpen}
        onOk={handleAdd}
        onCancel={() => setModalOpen(false)}
        destroyOnClose
      >
        <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
          <Input
            placeholder="Route (e.g. /boards/[boardId])"
            value={editing.route}
            onChange={(e) => setEditing({ ...editing, route: e.target.value })}
          />
          <Input
            placeholder="Page name (e.g. BoardDetail)"
            value={editing.name}
            onChange={(e) => setEditing({ ...editing, name: e.target.value })}
          />
          <Select
            value={editing.layout}
            onChange={(v) => setEditing({ ...editing, layout: v as IPageSpec["layout"] })}
            options={LAYOUT_OPTIONS}
          />
          <Input
            placeholder="Description"
            value={editing.description}
            onChange={(e) => setEditing({ ...editing, description: e.target.value })}
          />
          <Input
            placeholder="Components (comma-separated)"
            value={editing.components.join(", ")}
            onChange={(e) =>
              setEditing({
                ...editing,
                components: e.target.value.split(",").map((s) => s.trim()).filter(Boolean),
              })
            }
          />
          <Input
            placeholder="Data requirements (comma-separated)"
            value={editing.dataRequirements.join(", ")}
            onChange={(e) =>
              setEditing({
                ...editing,
                dataRequirements: e.target.value.split(",").map((s) => s.trim()).filter(Boolean),
              })
            }
          />
        </div>
      </Modal>
    </div>
  );
}
