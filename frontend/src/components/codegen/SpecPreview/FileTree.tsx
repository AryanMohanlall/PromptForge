"use client";

import { Tag } from "antd";
import { FileIcon, FolderIcon } from "lucide-react";
import type { IFileEntry } from "@/providers/codegen-provider";

interface FileTreeProps {
  files: IFileEntry[];
}

interface TreeNode {
  name: string;
  path: string;
  isDir: boolean;
  entry?: IFileEntry;
  children: TreeNode[];
}

function buildTree(files: IFileEntry[]): TreeNode[] {
  const root: TreeNode = { name: "", path: "", isDir: true, children: [] };

  for (const file of files) {
    const parts = file.path.split("/");
    let current = root;

    for (let i = 0; i < parts.length; i++) {
      const part = parts[i];
      const isLast = i === parts.length - 1;
      let child = current.children.find((c) => c.name === part);

      if (!child) {
        child = {
          name: part,
          path: parts.slice(0, i + 1).join("/"),
          isDir: !isLast,
          entry: isLast ? file : undefined,
          children: [],
        };
        current.children.push(child);
      }

      current = child;
    }
  }

  return root.children;
}

const TYPE_COLORS: Record<string, string> = {
  scaffold: "default",
  generated: "processing",
  static: "warning",
};

function TreeNodeRow({ node, depth }: { node: TreeNode; depth: number }) {
  return (
    <>
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: 6,
          paddingLeft: depth * 20,
          padding: `3px 8px 3px ${depth * 20 + 8}px`,
          fontSize: 13,
          opacity: node.entry?.type === "scaffold" ? 0.55 : 1,
          fontWeight: node.entry?.type === "generated" ? 500 : 400,
        }}
      >
        {node.isDir ? (
          <FolderIcon size={14} style={{ color: "var(--ant-color-primary)", flexShrink: 0 }} />
        ) : (
          <FileIcon size={14} style={{ color: "var(--ant-color-text-secondary)", flexShrink: 0 }} />
        )}
        <span style={{ flex: 1 }}>{node.name}</span>
        {node.entry && <Tag color={TYPE_COLORS[node.entry.type]}>{node.entry.type}</Tag>}
      </div>
      {node.children
        .sort((a, b) => (a.isDir === b.isDir ? a.name.localeCompare(b.name) : a.isDir ? -1 : 1))
        .map((child) => (
          <TreeNodeRow key={child.path} node={child} depth={depth + 1} />
        ))}
    </>
  );
}

export function FileTree({ files }: FileTreeProps) {
  const tree = buildTree(files);

  return (
    <div
      style={{
        maxHeight: 400,
        overflowY: "auto",
        background: "var(--ant-color-fill-quaternary)",
        borderRadius: 8,
        padding: "8px 0",
      }}
    >
      {tree
        .sort((a, b) => (a.isDir === b.isDir ? a.name.localeCompare(b.name) : a.isDir ? -1 : 1))
        .map((node) => (
          <TreeNodeRow key={node.path} node={node} depth={0} />
        ))}
    </div>
  );
}
