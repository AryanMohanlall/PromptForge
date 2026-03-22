"use client";

import { useEffect, useState, useCallback } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import {
  Button, Select, Input, Modal, Form, Spin, Alert, Tooltip, Space,
} from "antd";
import {
  ReloadOutlined, PlusOutlined, SendOutlined, ExportOutlined,
  GithubOutlined, BranchesOutlined, CodeOutlined, TagOutlined,
} from "@ant-design/icons";
import { RocketIcon } from "lucide-react";
import { useStyles } from "./styles/style";

const { Option } = Select;

// ─── Types ────────────────────────────────────────────────────────────────────

interface VercelProject {
  id: string;
  name: string;
  framework: string | null;
  updatedAt: number;
}

interface VercelDeployment {
  uid: string;
  name: string;
  projectId: string;
  url: string | null;
  state: string;
  target: string | null;
  created: number;
  inspectorUrl: string | null;
  errorMessage: string | null;
  meta: Record<string, string> | null;
  creator: { uid: string; username: string; email: string } | null;
}

interface GithubRepo {
  id: number;
  name: string;
  fullName: string;
  defaultBranch: string;
  private: boolean;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

const STATE_COLOR: Record<string, string> = {
  READY: "#22c55e",
  BUILDING: "#6366f1",
  INITIALIZING: "#6366f1",
  QUEUED: "#94a3b8",
  ERROR: "#ef4444",
  CANCELED: "#64748b",
  DELETED: "#475569",
};

const STATE_BG: Record<string, string> = {
  READY: "rgba(34,197,94,0.12)",
  BUILDING: "rgba(99,102,241,0.12)",
  INITIALIZING: "rgba(99,102,241,0.12)",
  QUEUED: "rgba(148,163,184,0.12)",
  ERROR: "rgba(239,68,68,0.12)",
  CANCELED: "rgba(100,116,139,0.12)",
  DELETED: "rgba(71,85,105,0.12)",
};

function StateBadge({ state }: { state: string }) {
  const color = STATE_COLOR[state] ?? "#94a3b8";
  const bg = STATE_BG[state] ?? "rgba(148,163,184,0.12)";
  return (
    <span style={{
      display: "inline-flex", alignItems: "center", gap: 5,
      padding: "2px 8px", borderRadius: 20,
      background: bg, border: `1px solid ${color}22`,
      fontSize: 10, fontWeight: 600, color, letterSpacing: "0.05em",
      textTransform: "uppercase",
    }}>
      <span style={{
        width: 5, height: 5, borderRadius: "50%", background: color,
        boxShadow: state === "BUILDING" || state === "INITIALIZING" ? `0 0 6px ${color}` : "none",
      }} />
      {state}
    </span>
  );
}

function formatTs(ts: number): string {
  if (!ts) return "—";
  return new Date(ts).toLocaleString(undefined, {
    month: "short", day: "numeric", hour: "2-digit", minute: "2-digit",
  });
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function DeploymentsPage() {
  const { styles } = useStyles();

  // Data
  const [projects, setProjects] = useState<VercelProject[]>([]);
  const [deployments, setDeployments] = useState<VercelDeployment[]>([]);
  const [githubRepos, setGithubRepos] = useState<GithubRepo[]>([]);

  // Filters
  const [selectedProjectId, setSelectedProjectId] = useState<string>("");
  const [stateFilter, setStateFilter] = useState<string>("");
  const [branchFilter, setBranchFilter] = useState<string>("");

  // UI state
  const [loadingDeployments, setLoadingDeployments] = useState(false);
  const [loadingProjects, setLoadingProjects] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [redeployingId, setRedeployingId] = useState<string | null>(null);

  // Create modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm] = Form.useForm();
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [loadingRepos, setLoadingRepos] = useState(false);
  const [repoLoadError, setRepoLoadError] = useState<string | null>(null);

  const instance = getAxiosInstance();

  // ── Load Vercel projects ──────────────────────────────────────────────────

  const loadProjects = useCallback(async () => {
    await Promise.resolve(); // yield before any setState — keeps effect body free of sync setState
    setLoadingProjects(true);
    try {
      const res = await instance.get("/api/vercel/projects?limit=50");
      const data = res.data.result ?? res.data;
      setProjects(data.projects ?? []);
    } catch {
      // projects are optional — silently ignore
    } finally {
      setLoadingProjects(false);
    }
  }, [instance]);

  // ── Load deployments ──────────────────────────────────────────────────────

  const loadDeployments = useCallback(async () => {
    await Promise.resolve(); // yield before any setState — keeps effect body free of sync setState
    setLoadingDeployments(true);
    setError(null);
    try {
      const params = new URLSearchParams({ limit: "30" });
      if (selectedProjectId) params.set("projectId", selectedProjectId);
      if (stateFilter) params.set("state", stateFilter);
      if (branchFilter.trim()) params.set("branch", branchFilter.trim());

      const res = await instance.get(`/api/vercel/deployments?${params.toString()}`);
      const data = res.data.result ?? res.data;
      setDeployments(data.deployments ?? []);
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { result?: { error?: string }; error?: string } } })?.response?.data?.result?.error ??
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        "Failed to load deployments.";
      setError(msg);
    } finally {
      setLoadingDeployments(false);
    }
  }, [instance, selectedProjectId, stateFilter, branchFilter]);

  // ── Load GitHub repos for create modal ───────────────────────────────────

  const loadGithubRepos = useCallback(async () => {
    setLoadingRepos(true);
    setRepoLoadError(null);
    try {
      const res = await instance.get("/api/github-app/repositories");
      const data = res.data.result ?? res.data;
      setGithubRepos(data.repositories ?? []);
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string; error?: string } } })?.response?.data?.message ??
        (err as { response?: { data?: { message?: string; error?: string } } })?.response?.data?.error ??
        "Failed to load repositories. Connect GitHub in Settings first.";
      setRepoLoadError(msg);
    } finally {
      setLoadingRepos(false);
    }
  }, [instance]);

  useEffect(() => {
    loadProjects();
  }, [loadProjects]);

  useEffect(() => {
    loadDeployments();
  }, [loadDeployments]);

  useEffect(() => {
    if (showCreateModal && githubRepos.length === 0) {
      loadGithubRepos();
    }
  }, [showCreateModal]); // eslint-disable-line react-hooks/exhaustive-deps

  // ── Redeploy ─────────────────────────────────────────────────────────────

  const handleRedeploy = async (deployment: VercelDeployment) => {
    setRedeployingId(deployment.uid);
    try {
      await instance.post("/api/vercel/deployments/redeploy", {
        deploymentId: deployment.uid,
        projectName: deployment.name,
      });
      await loadDeployments();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { result?: { error?: string }; error?: string } } })?.response?.data?.result?.error ??
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        "Redeploy failed.";
      setError(msg);
    } finally {
      setRedeployingId(null);
    }
  };

  // ── Create deployment ─────────────────────────────────────────────────────

  const openCreateModal = () => {
    setCreateError(null);
    setRepoLoadError(null);
    createForm.resetFields();
    setShowCreateModal(true);
  };

  const handleRepoSelect = (fullName: string) => {
    const repo = githubRepos.find((r) => r.fullName === fullName);
    if (repo) {
      createForm.setFieldsValue({ branch: repo.defaultBranch ?? "main" });
    }
  };

  const handleCreate = async () => {
    let values: {
      repoFullName: string;
      branch: string;
      projectName?: string;
      commitSha?: string;
    };
    try {
      values = await createForm.validateFields();
    } catch {
      return;
    }

    const repo = githubRepos.find((r) => r.fullName === values.repoFullName);
    if (!repo) {
      setCreateError("Selected repository not found.");
      return;
    }

    setCreating(true);
    setCreateError(null);
    try {
      await instance.post("/api/vercel/deployments", {
        repositoryFullName: repo.fullName,
        repoId: repo.id,
        branch: values.branch || "main",
        projectName: values.projectName || repo.name,
        commitSha: values.commitSha || null,
      });
      setShowCreateModal(false);
      await loadDeployments();
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { result?: { error?: string }; error?: string } } })?.response?.data?.result?.error ??
        (err as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        "Failed to create deployment.";
      setCreateError(msg);
    } finally {
      setCreating(false);
    }
  };

  // ─── Render ───────────────────────────────────────────────────────────────

  return (
    <div className={styles.page}>
      <div className={styles.inner}>

        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", marginBottom: 20 }}>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            <div className={styles.headerIcon}>
              <RocketIcon size={18} color="#fff" />
            </div>
            <div>
              <p className={styles.headerTitle}>Deployments</p>
              <p className={styles.headerSubtitle}>View and manage your Vercel deployments</p>
            </div>
          </div>
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={loadDeployments}
              loading={loadingDeployments}
              style={{ background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.1)", color: "#94a3b8" }}
            >
              Refresh
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={openCreateModal}
              style={{ background: "linear-gradient(135deg,#4f46e5,#7c3aed)", border: "none" }}
            >
              New Deployment
            </Button>
          </Space>
        </div>

        {/* Error banner */}
        {error && (
          <Alert
            type="error"
            message={error}
            closable
            onClose={() => setError(null)}
            style={{ marginBottom: 16, background: "rgba(239,68,68,0.08)", border: "1px solid rgba(239,68,68,0.2)" }}
          />
        )}

        {/* Filters + table */}
        <div className={styles.card}>

          {/* Filter bar */}
          <div className={styles.filterBar}>
            <Select
              placeholder="All projects"
              allowClear
              loading={loadingProjects}
              value={selectedProjectId || undefined}
              onChange={(v) => setSelectedProjectId(v ?? "")}
              style={{ width: 200, background: "transparent" }}
              popupMatchSelectWidth={false}
            >
              {projects.map((p) => (
                <Option key={p.id} value={p.id}>{p.name}</Option>
              ))}
            </Select>

            <Select
              placeholder="All states"
              allowClear
              value={stateFilter || undefined}
              onChange={(v) => setStateFilter(v ?? "")}
              style={{ width: 150 }}
            >
              {["READY", "BUILDING", "INITIALIZING", "QUEUED", "ERROR", "CANCELED"].map((s) => (
                <Option key={s} value={s}>
                  <StateBadge state={s} />
                </Option>
              ))}
            </Select>

            <Input
              placeholder="Filter by branch…"
              allowClear
              value={branchFilter}
              onChange={(e) => setBranchFilter(e.target.value)}
              onPressEnter={loadDeployments}
              style={{ width: 180, background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.08)", color: "#e2e8f0" }}
            />
          </div>

          {/* Table header */}
          <div className={styles.tableHeader}>
            <span className={styles.tableHeaderCell}>Status</span>
            <span className={styles.tableHeaderCell}>Deployment</span>
            <span className={styles.tableHeaderCell}>Branch / Target</span>
            <span className={styles.tableHeaderCell}>Environment</span>
            <span className={styles.tableHeaderCell}>Created</span>
            <span className={styles.tableHeaderCell}>Actions</span>
          </div>

          {/* Rows */}
          {loadingDeployments ? (
            <div className={styles.emptyState}>
              <Spin size="small" /> <span style={{ marginLeft: 8 }}>Loading deployments…</span>
            </div>
          ) : deployments.length === 0 ? (
            <div className={styles.emptyState}>No deployments found.</div>
          ) : (
            deployments.map((d) => {
              const branch = d.meta?.gitBranch ?? "—";
              const target = d.target ?? "preview";
              return (
                <div key={d.uid} className={styles.tableRow}>
                  {/* Status */}
                  <div><StateBadge state={d.state} /></div>

                  {/* Name + URL */}
                  <div style={{ minWidth: 0 }}>
                    <div className={styles.deploymentName}>{d.name}</div>
                    {d.url ? (
                      <a
                        href={d.url}
                        target="_blank"
                        rel="noreferrer"
                        className={styles.deploymentUrl}
                      >
                        {d.url.replace(/^https?:\/\//, "")}
                      </a>
                    ) : (
                      <span className={styles.metaText}>—</span>
                    )}
                  </div>

                  {/* Branch */}
                  <div className={styles.metaText}>{branch}</div>

                  {/* Target */}
                  <div>
                    <span style={{
                      fontSize: 10, fontWeight: 600, padding: "2px 7px", borderRadius: 20,
                      background: target === "production" ? "rgba(99,102,241,0.15)" : "rgba(148,163,184,0.1)",
                      color: target === "production" ? "#818cf8" : "#94a3b8",
                      border: `1px solid ${target === "production" ? "#6366f133" : "#64748b33"}`,
                      textTransform: "capitalize",
                    }}>
                      {target}
                    </span>
                  </div>

                  {/* Created */}
                  <div className={styles.metaText}>{formatTs(d.created)}</div>

                  {/* Actions */}
                  <div style={{ display: "flex", gap: 6 }}>
                    <Tooltip title="Redeploy">
                      <Button
                        size="small"
                        icon={<SendOutlined />}
                        loading={redeployingId === d.uid}
                        onClick={() => handleRedeploy(d)}
                        style={{
                          background: "rgba(99,102,241,0.1)", border: "1px solid rgba(99,102,241,0.25)",
                          color: "#818cf8",
                        }}
                      />
                    </Tooltip>
                    {d.inspectorUrl && (
                      <Tooltip title="Open in Vercel">
                        <Button
                          size="small"
                          icon={<ExportOutlined />}
                          href={d.inspectorUrl}
                          target="_blank"
                          style={{
                            background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.08)",
                            color: "#64748b",
                          }}
                        />
                      </Tooltip>
                    )}
                  </div>
                </div>
              );
            })
          )}
        </div>

      </div>

      {/* ── Create Deployment Modal ───────────────────────────────────────── */}
      <Modal
        open={showCreateModal}
        onCancel={() => setShowCreateModal(false)}
        footer={null}
        width={480}
        styles={{
          body: {
            background: "#0d1117",
            border: "1px solid rgba(255,255,255,0.08)",
            borderRadius: 12,
            padding: 0,
            overflow: "hidden",
          },
          mask: { backdropFilter: "blur(6px)", background: "rgba(0,0,0,0.6)" },
        }}
      >
        {/* Modal header */}
        <div style={{
          padding: "20px 24px 16px",
          borderBottom: "1px solid rgba(255,255,255,0.06)",
          display: "flex", alignItems: "center", gap: 12,
        }}>
          <div style={{
            width: 36, height: 36, borderRadius: 8, flexShrink: 0,
            background: "linear-gradient(135deg,#4f46e5,#7c3aed)",
            display: "flex", alignItems: "center", justifyContent: "center",
            boxShadow: "0 0 16px rgba(99,102,241,0.3)",
          }}>
            <RocketIcon size={16} color="#fff" />
          </div>
          <div>
            <div style={{ color: "#f1f5f9", fontWeight: 700, fontSize: 14, fontFamily: "'JetBrains Mono', monospace" }}>
              New Deployment
            </div>
            <div style={{ color: "#475569", fontSize: 11, marginTop: 1 }}>
              Deploy from a GitHub repository to Vercel
            </div>
          </div>
        </div>

        {/* Modal body */}
        <div style={{ padding: "20px 24px" }}>
          {repoLoadError && (
            <Alert
              type="warning"
              message={repoLoadError}
              style={{
                marginBottom: 16,
                background: "rgba(234,179,8,0.08)",
                border: "1px solid rgba(234,179,8,0.2)",
                borderRadius: 8,
                color: "#fde68a",
              }}
            />
          )}
          {createError && (
            <Alert
              type="error"
              message={createError}
              style={{
                marginBottom: 16,
                background: "rgba(239,68,68,0.08)",
                border: "1px solid rgba(239,68,68,0.2)",
                borderRadius: 8,
                color: "#fca5a5",
              }}
            />
          )}

          <Form form={createForm} layout="vertical" requiredMark={false}>

            {/* Repository */}
            <Form.Item
              name="repoFullName"
              style={{ marginBottom: 14 }}
              label={
                <span style={{
                  fontSize: 10, fontWeight: 600, letterSpacing: "0.07em",
                  textTransform: "uppercase", color: "#64748b",
                }}>
                  GitHub Repository
                </span>
              }
              rules={[{ required: true, message: "Select a repository" }]}
            >
              <Select
                showSearch
                placeholder="owner/repository"
                loading={loadingRepos}
                onChange={handleRepoSelect}
                suffixIcon={<GithubOutlined style={{ color: "#475569" }} />}
                filterOption={(input, option) =>
                  String(option?.value ?? "").toLowerCase().includes(input.toLowerCase())
                }
                style={{ width: "100%" }}
                styles={{
                  popup: { root: { background: "#0d1117", border: "1px solid rgba(255,255,255,0.1)" } },
                }}
              >
                {githubRepos.map((r) => (
                  <Option key={r.fullName} value={r.fullName}>
                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                      <GithubOutlined style={{ color: "#475569", fontSize: 12 }} />
                      <span style={{ color: "#e2e8f0", fontSize: 12, fontFamily: "'JetBrains Mono', monospace" }}>
                        {r.fullName}
                      </span>
                      {r.private && (
                        <span style={{
                          fontSize: 9, fontWeight: 600, padding: "1px 5px", borderRadius: 4,
                          background: "rgba(148,163,184,0.1)", color: "#64748b",
                          border: "1px solid rgba(100,116,139,0.2)", textTransform: "uppercase",
                        }}>
                          private
                        </span>
                      )}
                    </div>
                  </Option>
                ))}
              </Select>
            </Form.Item>

            {/* Branch + Project name side by side */}
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12, marginBottom: 0 }}>
              <Form.Item
                name="branch"
                style={{ marginBottom: 14 }}
                label={
                  <span style={{
                    fontSize: 10, fontWeight: 600, letterSpacing: "0.07em",
                    textTransform: "uppercase", color: "#64748b",
                  }}>
                    Branch
                  </span>
                }
                initialValue="main"
                rules={[{ required: true, message: "Required" }]}
              >
                <Input
                  placeholder="main"
                  prefix={<BranchesOutlined style={{ color: "#475569", fontSize: 12 }} />}
                  style={{ fontFamily: "'JetBrains Mono', monospace", fontSize: 12 }}
                />
              </Form.Item>

              <Form.Item
                name="projectName"
                style={{ marginBottom: 14 }}
                label={
                  <span style={{
                    fontSize: 10, fontWeight: 600, letterSpacing: "0.07em",
                    textTransform: "uppercase", color: "#64748b",
                  }}>
                    Project Name <span style={{ color: "#334155", fontWeight: 400 }}>(optional)</span>
                  </span>
                }
              >
                <Input
                  placeholder="From repo name"
                  prefix={<TagOutlined style={{ color: "#475569", fontSize: 12 }} />}
                />
              </Form.Item>
            </div>

            {/* Commit SHA */}
            <Form.Item
              name="commitSha"
              style={{ marginBottom: 0 }}
              label={
                <span style={{
                  fontSize: 10, fontWeight: 600, letterSpacing: "0.07em",
                  textTransform: "uppercase", color: "#64748b",
                }}>
                  Commit SHA <span style={{ color: "#334155", fontWeight: 400 }}>(optional)</span>
                </span>
              }
            >
              <Input
                placeholder="Latest commit if omitted"
                prefix={<CodeOutlined style={{ color: "#475569", fontSize: 12 }} />}
                style={{ fontFamily: "'JetBrains Mono', monospace", fontSize: 12 }}
              />
            </Form.Item>
          </Form>
        </div>

        {/* Modal footer */}
        <div style={{
          padding: "14px 24px",
          borderTop: "1px solid rgba(255,255,255,0.06)",
          display: "flex", justifyContent: "flex-end", gap: 8,
        }}>
          <Button
            onClick={() => setShowCreateModal(false)}
            style={{
              background: "rgba(255,255,255,0.04)",
              border: "1px solid rgba(255,255,255,0.08)",
              color: "#64748b",
            }}
          >
            Cancel
          </Button>
          <Button
            type="primary"
            icon={<RocketIcon size={13} />}
            loading={creating}
            onClick={handleCreate}
            style={{
              background: "linear-gradient(135deg,#4f46e5,#7c3aed)",
              border: "none",
              display: "flex", alignItems: "center", gap: 6,
              boxShadow: "0 0 16px rgba(99,102,241,0.3)",
            }}
          >
            Deploy
          </Button>
        </div>
      </Modal>
    </div>
  );
}
