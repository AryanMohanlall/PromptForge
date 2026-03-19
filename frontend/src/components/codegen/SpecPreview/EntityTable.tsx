"use client";

import { useState } from "react";
import { Table, Tag, Button, Input, Select, Switch, Modal, message } from "antd";
import { PlusIcon, Trash2Icon, EditIcon } from "lucide-react";
import type { IEntitySpec, IFieldSpec, IRelationSpec } from "@/providers/codegen-provider";

interface EntityTableProps {
  entities: IEntitySpec[];
  onChange: (entities: IEntitySpec[]) => void;
}

const FIELD_TYPES: IFieldSpec["type"][] = [
  "string", "int", "float", "boolean", "datetime", "enum", "json",
];

export function EntityTable({ entities, onChange }: EntityTableProps) {
  const [editingEntity, setEditingEntity] = useState<IEntitySpec | null>(null);
  const [editingField, setEditingField] = useState<IFieldSpec | null>(null);
  const [fieldModalOpen, setFieldModalOpen] = useState(false);
  const [targetEntityName, setTargetEntityName] = useState("");

  const handleRemoveEntity = (name: string) => {
    onChange(entities.filter((e) => e.name !== name));
  };

  const handleAddField = (entityName: string) => {
    setTargetEntityName(entityName);
    setEditingField({
      name: "",
      type: "string",
      required: true,
      description: "",
    });
    setFieldModalOpen(true);
  };

  const handleRemoveField = (entityName: string, fieldName: string) => {
    onChange(
      entities.map((e) =>
        e.name === entityName
          ? { ...e, fields: e.fields.filter((f) => f.name !== fieldName) }
          : e
      )
    );
  };

  const handleSaveField = () => {
    if (!editingField?.name) {
      message.warning("Field name is required.");
      return;
    }

    onChange(
      entities.map((e) => {
        if (e.name !== targetEntityName) return e;
        const existing = e.fields.findIndex((f) => f.name === editingField.name);
        if (existing >= 0) {
          const updated = [...e.fields];
          updated[existing] = editingField;
          return { ...e, fields: updated };
        }
        return { ...e, fields: [...e.fields, editingField] };
      })
    );
    setFieldModalOpen(false);
    setEditingField(null);
  };

  const fieldColumns = [
    { title: "Name", dataIndex: "name", key: "name", width: 140 },
    { title: "Type", dataIndex: "type", key: "type", width: 100 },
    {
      title: "Required",
      dataIndex: "required",
      key: "required",
      width: 80,
      render: (v: boolean) => (v ? <Tag color="red">Yes</Tag> : <Tag>No</Tag>),
    },
    {
      title: "Unique",
      dataIndex: "unique",
      key: "unique",
      width: 80,
      render: (v: boolean | undefined) =>
        v ? <Tag color="blue">Yes</Tag> : <Tag>No</Tag>,
    },
    { title: "Description", dataIndex: "description", key: "description", ellipsis: true },
    {
      title: "",
      key: "actions",
      width: 40,
      render: (_: unknown, record: IFieldSpec, _idx: number) => (
        <Button
          type="text"
          danger
          size="small"
          icon={<Trash2Icon size={14} />}
          onClick={() => handleRemoveField(targetEntityName || "", record.name)}
        />
      ),
    },
  ];

  return (
    <div>
      {entities.map((entity) => (
        <div key={entity.name} style={{ marginBottom: 24 }}>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              marginBottom: 8,
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
              <strong>{entity.name}</strong>
              <Tag color="default">{entity.tableName}</Tag>
            </div>
            <div style={{ display: "flex", gap: 4 }}>
              <Button
                size="small"
                icon={<PlusIcon size={14} />}
                onClick={() => handleAddField(entity.name)}
              >
                Field
              </Button>
              <Button
                size="small"
                danger
                icon={<Trash2Icon size={14} />}
                onClick={() => handleRemoveEntity(entity.name)}
              />
            </div>
          </div>

          <Table
            dataSource={entity.fields}
            columns={fieldColumns.map((col) =>
              col.key === "actions"
                ? {
                    ...col,
                    render: (_: unknown, record: IFieldSpec) => (
                      <Button
                        type="text"
                        danger
                        size="small"
                        icon={<Trash2Icon size={14} />}
                        onClick={() => handleRemoveField(entity.name, record.name)}
                      />
                    ),
                  }
                : col
            )}
            rowKey="name"
            size="small"
            pagination={false}
          />

          {entity.relations.length > 0 && (
            <div style={{ marginTop: 8 }}>
              {entity.relations.map((rel, idx) => (
                <Tag key={idx} color="purple">
                  {rel.type} → {rel.target}
                  {rel.foreignKey ? ` (${rel.foreignKey})` : ""}
                </Tag>
              ))}
            </div>
          )}
        </div>
      ))}

      <Modal
        title="Add / Edit Field"
        open={fieldModalOpen}
        onOk={handleSaveField}
        onCancel={() => setFieldModalOpen(false)}
        destroyOnClose
      >
        {editingField && (
          <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
            <Input
              placeholder="Field name"
              value={editingField.name}
              onChange={(e) =>
                setEditingField({ ...editingField, name: e.target.value })
              }
            />
            <Select
              value={editingField.type}
              onChange={(v) =>
                setEditingField({ ...editingField, type: v as IFieldSpec["type"] })
              }
              options={FIELD_TYPES.map((t) => ({ label: t, value: t }))}
            />
            <div style={{ display: "flex", gap: 16 }}>
              <label style={{ display: "flex", alignItems: "center", gap: 4 }}>
                Required <Switch checked={editingField.required} onChange={(v) => setEditingField({ ...editingField, required: v })} />
              </label>
              <label style={{ display: "flex", alignItems: "center", gap: 4 }}>
                Unique <Switch checked={editingField.unique ?? false} onChange={(v) => setEditingField({ ...editingField, unique: v })} />
              </label>
            </div>
            <Input
              placeholder="Description"
              value={editingField.description}
              onChange={(e) =>
                setEditingField({ ...editingField, description: e.target.value })
              }
            />
            {editingField.type === "string" && (
              <Input
                placeholder="Max length"
                type="number"
                value={editingField.maxLength ?? ""}
                onChange={(e) =>
                  setEditingField({
                    ...editingField,
                    maxLength: e.target.value ? Number(e.target.value) : undefined,
                  })
                }
              />
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
