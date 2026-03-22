"use client";

import { useEffect, useState, useCallback } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import {
  Button, Input, Tag, Tooltip, Spin, Empty, Badge, Typography, Space, Divider,
  Modal, Form, Checkbox, Alert,
} from "antd";
import {
  GithubOutlined, BranchesOutlined, PullRequestOutlined, IssuesCloseOutlined,
  TagOutlined, ThunderboltOutlined, FolderOutlined, FileOutlined,
  StarOutlined, LockOutlined, UnlockOutlined, ReloadOutlined,
  ExportOutlined, PlusOutlined, LeftOutlined, CopyOutlined,
  CheckCircleOutlined, CloseCircleOutlined, ClockCircleOutlined,
  LoadingOutlined, WarningOutlined, MessageOutlined,
} from "@ant-design/icons";
import {
  GitCommitIcon, ChevronRightIcon, ChevronDownIcon,
} from "lucide-react";
import { useStyles } from "./styles/style";

const { Text, Title, Paragraph } = Typography;

// ─── Types ────────────────────────────────────────────────────────────────────
interface Repository {
  id: number; name: string; fullName: string; htmlUrl: string; private: boolean;
  description?: string; language?: string; stargazersCount?: number;
  forksCount?: number; updatedAt?: string; defaultBranch?: string;
}
interface Commit { sha: string; message: string; author: string; date: string; url: string; }
interface Branch { name: string; sha: string; protected: boolean; }
interface PullRequest {
  id: number; number: number; title: string; state: string; author: string;
  authorAvatar: string; createdAt: string; url: string; draft: boolean;
  labels: { name: string; color: string }[];
}
interface Issue {
  id: number; number: number; title: string; state: string; author: string;
  authorAvatar: string; createdAt: string; url: string; comments: number;
  labels: { name: string; color: string }[];
}
interface Release {
  id: number; tagName: string; name: string; body: string; prerelease: boolean;
  draft: boolean; publishedAt: string; url: string; author: string;
}
interface WorkflowRun {
  id: number; name: string; status: string; conclusion: string | null;
  event_: string; branch: string; commitSha: string; createdAt: string;
  updatedAt: string; url: string; durationMs: number;
}
interface FileEntry { name: string; path: string; type: string; size: number; url: string; }
interface FileContent { name: string; path: string; size: number; content: string; url: string; }
type TabType = "commits" | "branches" | "prs" | "issues" | "releases" | "actions" | "files";

// ─── Helpers ─────────────────────────────────────────────────────────────────
function timeAgo(dateStr: string): string {
  if (!dateStr) return "";
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  return `${Math.floor(hrs / 24)}d ago`;
}
function formatDuration(ms: number): string {
  if (!ms) return "—";
  const s = Math.floor(ms / 1000);
  if (s < 60) return `${s}s`;
  return `${Math.floor(s / 60)}m ${s % 60}s`;
}
function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes}B`;
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)}KB`;
  return `${(bytes / 1048576).toFixed(1)}MB`;
}

const LANG_COLORS: Record<string, string> = {
  TypeScript: "#3178c6", JavaScript: "#f1e05a", Python: "#3572A5",
  "C#": "#178600", Go: "#00ADD8", Rust: "#dea584", Java: "#b07219",
  CSS: "#563d7c", HTML: "#e34c26", Shell: "#89e051", default: "#8b949e",
};

function conclusionIcon(conclusion: string | null, status: string) {
  if (status === "in_progress" || status === "queued") return <LoadingOutlined style={{ color: "#f59e0b" }} />;
  if (conclusion === "success") return <CheckCircleOutlined style={{ color: "#10b981" }} />;
  if (conclusion === "failure") return <CloseCircleOutlined style={{ color: "#ef4444" }} />;
  if (conclusion === "cancelled") return <CloseCircleOutlined style={{ color: "#64748b" }} />;
  return <WarningOutlined style={{ color: "#f59e0b" }} />;
}
function conclusionColor(conclusion: string | null, status: string): string {
  if (status === "in_progress" || status === "queued") return "#f59e0b";
  if (conclusion === "success") return "#10b981";
  if (conclusion === "failure") return "#ef4444";
  return "#64748b";
}

// ─── Language Bar ─────────────────────────────────────────────────────────────
function LanguageBar({ languages }: { languages: Record<string, number> }) {
  const { styles } = useStyles();
  const total = Object.values(languages).reduce((a, b) => a + b, 0);
  if (!total) return null;
  const entries = Object.entries(languages).sort((a, b) => b[1] - a[1]);
  return (
    <div>
      <div className={styles.langBar}>
        {entries.map(([lang, bytes]) => (
          <Tooltip key={lang} title={`${lang}: ${((bytes / total) * 100).toFixed(1)}%`}>
            <div style={{ width: `${(bytes / total) * 100}%`, background: LANG_COLORS[lang] ?? LANG_COLORS.default, minWidth: 2, cursor: "default" }} />
          </Tooltip>
        ))}
      </div>
      <div className={styles.langList}>
        {entries.slice(0, 6).map(([lang, bytes]) => (
          <span key={lang} style={{ display: "flex", alignItems: "center", gap: 5 }}>
            <span style={{ width: 10, height: 10, borderRadius: "50%", background: LANG_COLORS[lang] ?? LANG_COLORS.default, flexShrink: 0 }} />
            <Text style={{ fontSize: 11, color: "#94a3b8" }}>{lang}</Text>
            <Text style={{ fontSize: 11, color: "#475569" }}>{((bytes / total) * 100).toFixed(1)}%</Text>
          </span>
        ))}
      </div>
    </div>
  );
}

// ─── File Tree ────────────────────────────────────────────────────────────────
function FileTree({ owner, repo, onFileSelect }: { owner: string; repo: string; onFileSelect: (path: string, name: string) => void }) {
  const { styles } = useStyles();
  const instance = getAxiosInstance();
  const [tree, setTree] = useState<Record<string, FileEntry[]>>({ "": [] });
  const [expanded, setExpanded] = useState<Set<string>>(new Set([""]));
  const [loading, setLoading] = useState<Set<string>>(new Set());

  const fetchDir = useCallback(async (path: string) => {
    if ((tree[path]?.length ?? 0) > 0 || loading.has(path)) return;
    setLoading(prev => new Set([...prev, path]));
    try {
      const res = await instance.get(`/api/github-app/contents?owner=${owner}&repo=${repo}&path=${encodeURIComponent(path)}`);
      const entries: FileEntry[] = res.data.result?.contents ?? res.data.contents ?? [];
      setTree(prev => ({ ...prev, [path]: entries }));
    } catch { /* ignore */ } finally {
      setLoading(prev => { const n = new Set(prev); n.delete(path); return n; });
    }
  }, [instance, owner, repo, tree, loading]);

  useEffect(() => { fetchDir(""); }, []);

  function toggle(path: string) {
    setExpanded(prev => {
      const n = new Set(prev);
      if (n.has(path)) { n.delete(path); } else { n.add(path); fetchDir(path); }
      return n;
    });
  }

  function renderEntries(path: string, depth: number): React.ReactNode {
    const entries = tree[path];
    if (!entries) return null;
    return entries.map(entry => (
      <div key={entry.path}>
        <button
          type="button"
          className={styles.fileTreeEntry}
          onClick={() => entry.type === "dir" ? toggle(entry.path) : onFileSelect(entry.path, entry.name)}
          style={{ padding: `5px 8px 5px ${8 + depth * 16}px`, color: entry.type === "dir" ? "#94a3b8" : "#cbd5e1" }}
        >
          {entry.type === "dir"
            ? expanded.has(entry.path)
              ? <ChevronDownIcon style={{ width: 12, height: 12, flexShrink: 0 }} />
              : <ChevronRightIcon style={{ width: 12, height: 12, flexShrink: 0 }} />
            : <span style={{ width: 12, flexShrink: 0 }} />}
          {entry.type === "dir"
            ? <FolderOutlined style={{ color: "#6366f1", flexShrink: 0 }} />
            : <FileOutlined style={{ color: "#64748b", flexShrink: 0 }} />}
          <span>{entry.name}</span>
          {entry.type === "file" && entry.size > 0 && (
            <Text className={styles.fileSize}>{formatBytes(entry.size)}</Text>
          )}
        </button>
        {entry.type === "dir" && expanded.has(entry.path) && (
          loading.has(entry.path)
            ? <div style={{ padding: `4px 8px 4px ${8 + (depth + 1) * 16}px` }}><Spin size="small" /></div>
            : renderEntries(entry.path, depth + 1)
        )}
      </div>
    ));
  }

  return (
    <div className={styles.fileTree}>
      {loading.has("") && tree[""].length === 0
        ? <div style={{ padding: 24, textAlign: "center" }}><Spin /></div>
        : renderEntries("", 0)}
    </div>
  );
}

// ─── File Viewer ──────────────────────────────────────────────────────────────
function FileViewer({ owner, repo, path, onBack }: { owner: string; repo: string; path: string; onBack: () => void }) {
  const { styles } = useStyles();
  const instance = getAxiosInstance();
  const [file, setFile] = useState<FileContent | null>(null);
  const [loading, setLoading] = useState(true);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let active = true;
    async function fetchFile() {
      await Promise.resolve();
      if (!active) return;
      setLoading(true);
      try {
        const res = await instance.get(`/api/github-app/file?owner=${owner}&repo=${repo}&path=${encodeURIComponent(path)}`);
        if (active) setFile(res.data.result?.file ?? res.data.file);
      } catch {
        if (active) setFile(null);
      } finally {
        if (active) setLoading(false);
      }
    }
    fetchFile();
    return () => { active = false; };
  }, [instance, owner, repo, path]);

  const copy = () => {
    if (file?.content) {
      navigator.clipboard.writeText(file.content);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  return (
    <div style={{ display: "flex", flexDirection: "column" }}>
      <div className={styles.fileViewerHeader}>
        <Button type="text" size="small" icon={<LeftOutlined />} onClick={onBack} style={{ color: "#64748b" }}>
          Back
        </Button>
        <Text className={styles.fileViewerPath}>{path}</Text>
        <Space>
          {file && <Text className={styles.metaText}>{formatBytes(file.size)}</Text>}
          <Button
            size="small"
            icon={<CopyOutlined />}
            onClick={copy}
            style={{ fontSize: 11, color: copied ? "#10b981" : "#94a3b8", background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.07)" }}
          >
            {copied ? "Copied!" : "Copy"}
          </Button>
          {file?.url && (
            <Button
              size="small"
              icon={<ExportOutlined />}
              href={file.url}
              target="_blank"
              style={{ fontSize: 11, color: "#6366f1", background: "rgba(99,102,241,0.08)", border: "1px solid rgba(99,102,241,0.15)" }}
            >
              GitHub
            </Button>
          )}
        </Space>
      </div>
      <div className={styles.fileViewerBody}>
        {loading && <div style={{ textAlign: "center", padding: "40px 0" }}><Spin /></div>}
        {!loading && !file && <Alert type="error" message="Failed to load file." showIcon />}
        {!loading && file && <pre className={styles.fileViewerPre}>{file.content ?? "Binary file — cannot display."}</pre>}
      </div>
    </div>
  );
}

// ─── Repo Card ────────────────────────────────────────────────────────────────
function RepoCard({ repo, onSelect, selected }: { repo: Repository; onSelect: (r: Repository) => void; selected: boolean }) {
  const { styles, cx } = useStyles();
  const langColor = LANG_COLORS[repo.language ?? ""] ?? LANG_COLORS.default;
  return (
    <button
      type="button"
      className={cx(styles.repoCard, selected && styles.repoCardSelected)}
      onClick={() => onSelect(repo)}
    >
      <div style={{ display: "flex", justifyContent: "space-between", gap: 8 }}>
        <div style={{ display: "flex", alignItems: "center", gap: 7, minWidth: 0 }}>
          {repo.private
            ? <LockOutlined style={{ fontSize: 12, color: "#f59e0b", flexShrink: 0 }} />
            : <UnlockOutlined style={{ fontSize: 12, color: "#10b981", flexShrink: 0 }} />}
          <Text className={styles.repoCardName}>{repo.name}</Text>
        </div>
        <Tooltip title="Open on GitHub">
          <a href={repo.htmlUrl} target="_blank" rel="noopener noreferrer" onClick={e => e.stopPropagation()} className={styles.externalLink}>
            <ExportOutlined style={{ fontSize: 11 }} />
          </a>
        </Tooltip>
      </div>
      {repo.description && <p className={styles.repoCardDesc}>{repo.description}</p>}
      <div className={styles.repoCardMeta}>
        {repo.language && (
          <span style={{ display: "flex", alignItems: "center", gap: 4 }}>
            <span style={{ width: 8, height: 8, borderRadius: "50%", background: langColor }} />
            <Text style={{ fontSize: 10, color: "#94a3b8" }}>{repo.language}</Text>
          </span>
        )}
        {!!repo.stargazersCount && (
          <span style={{ display: "flex", alignItems: "center", gap: 3 }}>
            <StarOutlined style={{ fontSize: 10, color: "#f59e0b" }} />
            <Text style={{ fontSize: 10, color: "#94a3b8" }}>{repo.stargazersCount}</Text>
          </span>
        )}
        {repo.updatedAt && (
          <span style={{ display: "flex", alignItems: "center", gap: 3 }}>
            <ClockCircleOutlined style={{ fontSize: 10, color: "#475569" }} />
            <Text className={styles.metaText}>{timeAgo(repo.updatedAt)}</Text>
          </span>
        )}
      </div>
    </button>
  );
}

// ─── Create Repo Modal ────────────────────────────────────────────────────────
function CreateRepoModal({ open, onClose, onCreated }: { open: boolean; onClose: () => void; onCreated: (r: Repository) => void }) {
  const { styles } = useStyles();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreate = async () => {
    const values = await form.validateFields();
    setLoading(true); setError(null);
    try {
      const res = await getAxiosInstance().post("/api/github-app/repositories", {
        name: values.name.trim(),
        description: values.description?.trim() || undefined,
        isPrivate: values.isPrivate ?? false,
        autoInit: true,
      });
      onCreated(res.data.result?.repository ?? res.data.repository);
      form.resetFields();
      onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e.response?.data?.error ?? "Failed to create repository.");
    } finally { setLoading(false); }
  };

  return (
    <Modal
      open={open}
      onCancel={onClose}
      onOk={handleCreate}
      confirmLoading={loading}
      title={<span className={styles.modalTitle}>New repository</span>}
      okText="Create repository"
      okButtonProps={{ style: { background: "#4f46e5", borderColor: "#4f46e5" } }}
      styles={{
        body: { background: "#0d1117", border: "1px solid rgba(255,255,255,0.1)" },
        header: { background: "#0d1117", borderBottom: "1px solid rgba(255,255,255,0.07)" },
        footer: { background: "#0d1117", borderTop: "1px solid rgba(255,255,255,0.07)" },
        mask: { backdropFilter: "blur(6px)" },
      }}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item
          name="name"
          label={<Text style={{ color: "#94a3b8", fontSize: 12 }}>Repository name</Text>}
          rules={[{ required: true, message: "Repository name is required" }]}
        >
          <Input
            placeholder="my-awesome-app"
            style={{ background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.08)", color: "#e2e8f0", fontFamily: "'JetBrains Mono', monospace" }}
          />
        </Form.Item>
        <Form.Item
          name="description"
          label={<Text style={{ color: "#94a3b8", fontSize: 12 }}>Description</Text>}
        >
          <Input
            placeholder="Optional..."
            style={{ background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.08)", color: "#e2e8f0" }}
          />
        </Form.Item>
        <Form.Item name="isPrivate" valuePropName="checked">
          <Checkbox style={{ color: "#94a3b8", fontSize: 12 }}>Private repository</Checkbox>
        </Form.Item>
        {error && <Alert type="error" message={error} showIcon style={{ marginBottom: 8 }} />}
      </Form>
    </Modal>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────
export default function GitHubDashboardPage() {
  const { styles, cx } = useStyles();
  const instance = getAxiosInstance();

  const [repos, setRepos] = useState<Repository[]>([]);
  const [selectedRepo, setSelectedRepo] = useState<Repository | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>("commits");
  const [loadingRepos, setLoadingRepos] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [reposError, setReposError] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [commits, setCommits] = useState<Commit[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [prs, setPrs] = useState<PullRequest[]>([]);
  const [issues, setIssues] = useState<Issue[]>([]);
  const [releases, setReleases] = useState<Release[]>([]);
  const [runs, setRuns] = useState<WorkflowRun[]>([]);
  const [languages, setLanguages] = useState<Record<string, number>>({});
  const [viewingFile, setViewingFile] = useState<{ path: string; name: string } | null>(null);

  const fetchRepos = useCallback(async () => {
    setLoadingRepos(true); setReposError(null);
    try {
      const res = await instance.get("/api/github-app/repositories");
      setRepos(res.data.result?.repositories ?? res.data.repositories ?? []);
    } catch { setReposError("Failed to load repositories. Make sure your GitHub account is connected."); }
    finally { setLoadingRepos(false); }
  }, [instance]);

  useEffect(() => { fetchRepos(); }, [fetchRepos]);

  const ownerOf = (r: Repository) => r.fullName.split("/")[0];

  const fetchTab = useCallback(async (tab: TabType, repo: Repository) => {
    if (tab === "files") { setViewingFile(null); return; }
    setLoadingDetail(true);
    try {
      const o = ownerOf(repo), r = repo.name;
      if (tab === "commits") { const res = await instance.get(`/api/github-app/commits?owner=${o}&repo=${r}`); setCommits(res.data.result?.commits ?? res.data.commits ?? []); }
      else if (tab === "branches") { const res = await instance.get(`/api/github-app/branches?owner=${o}&repo=${r}`); setBranches(res.data.result?.branches ?? res.data.branches ?? []); }
      else if (tab === "prs") { const res = await instance.get(`/api/github-app/pull-requests?owner=${o}&repo=${r}`); setPrs(res.data.result?.pullRequests ?? res.data.pullRequests ?? []); }
      else if (tab === "issues") { const res = await instance.get(`/api/github-app/issues?owner=${o}&repo=${r}`); setIssues(res.data.result?.issues ?? res.data.issues ?? []); }
      else if (tab === "releases") { const res = await instance.get(`/api/github-app/releases?owner=${o}&repo=${r}`); setReleases(res.data.result?.releases ?? res.data.releases ?? []); }
      else if (tab === "actions") { const res = await instance.get(`/api/github-app/actions?owner=${o}&repo=${r}`); setRuns(res.data.result?.runs ?? res.data.runs ?? []); }
    } catch { /* silently fail per tab */ }
    finally { setLoadingDetail(false); }
  }, [instance]);

  useEffect(() => {
    if (!selectedRepo) return;
    instance.get(`/api/github-app/stats/languages?owner=${ownerOf(selectedRepo)}&repo=${selectedRepo.name}`)
      .then(res => setLanguages(res.data.result?.languages ?? res.data.languages ?? {}))
      .catch(() => setLanguages({}));
  }, [selectedRepo]);

  const handleSelectRepo = (repo: Repository) => {
    setSelectedRepo(repo); setActiveTab("commits");
    setCommits([]); setBranches([]); setPrs([]); setIssues([]); setReleases([]); setRuns([]);
    setViewingFile(null); fetchTab("commits", repo);
  };

  const handleTabChange = (tab: TabType) => { setActiveTab(tab); if (selectedRepo) fetchTab(tab, selectedRepo); };
  const filteredRepos = repos.filter(r => r?.name?.toLowerCase().includes(searchQuery.toLowerCase()));

  const TABS: { id: TabType; label: string; icon: React.ReactNode }[] = [
    { id: "commits", label: "Commits", icon: <GitCommitIcon style={{ width: 12, height: 12 }} /> },
    { id: "branches", label: "Branches", icon: <BranchesOutlined /> },
    { id: "prs", label: "PRs", icon: <PullRequestOutlined /> },
    { id: "issues", label: "Issues", icon: <IssuesCloseOutlined /> },
    { id: "releases", label: "Releases", icon: <TagOutlined /> },
    { id: "actions", label: "Actions", icon: <ThunderboltOutlined /> },
    { id: "files", label: "Files", icon: <FolderOutlined /> },
  ];

  return (
    <>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Sora:wght@300;400;500;600;700&display=swap');
      `}</style>

      <div className={styles.page}>
        <div className={styles.inner}>

          {/* Header */}
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 26, flexWrap: "wrap", gap: 12 }}>
            <Space size={13}>
              <div className={styles.headerIcon}>
                <GithubOutlined style={{ fontSize: 19, color: "#fff" }} />
              </div>
              <div>
                <Title level={4} className={styles.headerTitle}>GitHub Dashboard</Title>
                <Text className={styles.headerSubtitle}>{repos.length > 0 ? `${repos.length} repositories` : "Loading..."}</Text>
              </div>
            </Space>
            <Space>
              <Button
                icon={<ReloadOutlined className={loadingRepos ? styles.spinning : ""} />}
                onClick={fetchRepos}
                loading={loadingRepos}
                style={{ background: "rgba(255,255,255,0.03)", border: "1px solid rgba(255,255,255,0.07)", color: "#64748b" }}
              >
                Refresh
              </Button>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setShowCreateModal(true)}
                style={{ background: "#4f46e5", borderColor: "#4f46e5", boxShadow: "0 0 14px rgba(99,102,241,0.2)" }}
              >
                New repo
              </Button>
            </Space>
          </div>

          {/* Main grid */}
          <div className={styles.mainGrid} style={{ gridTemplateColumns: selectedRepo ? "300px 1fr" : "1fr" }}>

            {/* Repo list */}
            <div className={styles.card}>
              <div className={styles.repoListHeader}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 9 }}>
                  <Space>
                    <Text className={styles.repoListLabel}>Repositories</Text>
                    {repos.length > 0 && <Badge count={repos.length} style={{ background: "rgba(99,102,241,0.15)", color: "#6366f1", boxShadow: "none", fontSize: 9 }} />}
                  </Space>
                </div>
                <Input
                  placeholder="Search..."
                  value={searchQuery}
                  onChange={e => setSearchQuery(e.target.value)}
                  style={{ background: "rgba(255,255,255,0.03)", border: "1px solid rgba(255,255,255,0.06)", color: "#e2e8f0" }}
                />
              </div>
              <div className={styles.repoScroll}>
                {loadingRepos && <div style={{ padding: "28px 14px", textAlign: "center" }}><Spin /></div>}
                {reposError && (
                  <Alert
                    type="error"
                    message={reposError}
                    showIcon
                    style={{ margin: "5px 7px", background: "rgba(239,68,68,0.05)", border: "1px solid rgba(239,68,68,0.1)" }}
                  />
                )}
                {!loadingRepos && !reposError && filteredRepos.length === 0 && (
                  <Empty description={searchQuery ? "No matches" : "No repositories"} style={{ padding: "28px 14px", color: "#475569" }} />
                )}
                {filteredRepos.map(repo => (
                  <RepoCard key={repo.id} repo={repo} onSelect={handleSelectRepo} selected={selectedRepo?.id === repo.id} />
                ))}
              </div>
            </div>

            {/* Detail panel */}
            {selectedRepo && (
              <div className={styles.detailPanel}>

                {/* Repo header card */}
                <div className={styles.card}>
                  <div className={styles.repoHeader}>
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: 10 }}>
                      <div style={{ minWidth: 0 }}>
                        <Space wrap>
                          <FolderOutlined style={{ color: "#6366f1" }} />
                          <Text className={styles.repoFullName}>{selectedRepo.fullName}</Text>
                          <Tag
                            color={selectedRepo.private ? "warning" : "success"}
                            style={{ fontSize: 9, fontWeight: 700 }}
                          >
                            {selectedRepo.private ? "PRIVATE" : "PUBLIC"}
                          </Tag>
                          {selectedRepo.defaultBranch && (
                            <Tag color="blue" style={{ fontSize: 9, fontWeight: 600 }}>
                              {selectedRepo.defaultBranch}
                            </Tag>
                          )}
                        </Space>
                        {selectedRepo.description && <Paragraph className={styles.repoDesc}>{selectedRepo.description}</Paragraph>}
                        <div className={styles.repoStats}>
                          {selectedRepo.language && (
                            <span style={{ display: "flex", alignItems: "center", gap: 4 }}>
                              <span style={{ width: 8, height: 8, borderRadius: "50%", background: LANG_COLORS[selectedRepo.language] ?? LANG_COLORS.default }} />
                              <Text style={{ fontSize: 10, color: "#94a3b8" }}>{selectedRepo.language}</Text>
                            </span>
                          )}
                          {!!selectedRepo.stargazersCount && (
                            <Space size={3}>
                              <StarOutlined style={{ fontSize: 10, color: "#f59e0b" }} />
                              <Text style={{ fontSize: 10, color: "#94a3b8" }}>{selectedRepo.stargazersCount}</Text>
                            </Space>
                          )}
                          {!!selectedRepo.forksCount && (
                            <Space size={3}>
                              <BranchesOutlined style={{ fontSize: 10, color: "#64748b" }} />
                              <Text style={{ fontSize: 10, color: "#94a3b8" }}>{selectedRepo.forksCount} forks</Text>
                            </Space>
                          )}
                        </div>
                      </div>
                      <Space>
                        <Button
                          size="small"
                          icon={<ExportOutlined />}
                          href={selectedRepo.htmlUrl}
                          target="_blank"
                          style={{ fontSize: 11, color: "#6366f1", background: "rgba(99,102,241,0.08)", border: "1px solid rgba(99,102,241,0.15)" }}
                        >
                          Open
                        </Button>
                        <Button
                          size="small"
                          icon={<span style={{ fontSize: 11 }}>✕</span>}
                          onClick={() => setSelectedRepo(null)}
                          style={{ background: "none", border: "1px solid rgba(255,255,255,0.07)", color: "#475569" }}
                        />
                      </Space>
                    </div>
                    {Object.keys(languages).length > 0 && (
                      <div className={styles.langSection}>
                        <p className={styles.langLabel}>Languages</p>
                        <LanguageBar languages={languages} />
                      </div>
                    )}
                  </div>

                  {/* Tabs */}
                  <div style={{ padding: "0 12px 11px", display: "flex", gap: 2, flexWrap: "wrap" }}>
                    {TABS.map(tab => (
                      <Button
                        key={tab.id}
                        type="text"
                        size="small"
                        icon={tab.icon}
                        onClick={() => handleTabChange(tab.id)}
                        style={{
                          color: activeTab === tab.id ? "#6366f1" : "#64748b",
                          background: activeTab === tab.id ? "rgba(99,102,241,0.12)" : "transparent",
                          fontWeight: activeTab === tab.id ? 600 : 400,
                          fontSize: 11,
                        }}
                      >
                        {tab.label}
                      </Button>
                    ))}
                  </div>
                </div>

                {/* Tab content card */}
                <div className={cx(styles.card, styles.tabContent)}>
                  {loadingDetail && activeTab !== "files" && (
                    <div className={styles.tabLoading}><Spin /></div>
                  )}

                  {/* Commits */}
                  {!loadingDetail && activeTab === "commits" && (
                    <div className={styles.tabList}>
                      {commits.length === 0 && <Empty description="No commits found." style={{ padding: "28px 0", color: "#475569" }} />}
                      {commits.map(c => (
                        <div key={c.sha} className={styles.commitRow}>
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 8 }}>
                            <Text className={styles.commitMsg}>{c.message.split("\n")[0]}</Text>
                            <Tooltip title="View on GitHub">
                              <a href={c.url} target="_blank" rel="noopener noreferrer" className={styles.externalLink}>
                                <ExportOutlined style={{ fontSize: 10 }} />
                              </a>
                            </Tooltip>
                          </div>
                          <div className={styles.commitMeta}>
                            <Text className={styles.commitSha}>{c.sha.slice(0, 7)}</Text>
                            <Text className={styles.metaText}>{c.author}</Text>
                            <Text className={styles.metaTextFaint}>{timeAgo(c.date)}</Text>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Branches */}
                  {!loadingDetail && activeTab === "branches" && (
                    <div className={styles.tabList}>
                      {branches.length === 0 && <Empty description="No branches." style={{ padding: "28px 0" }} />}
                      {branches.map(b => (
                        <div
                          key={b.name}
                          className={styles.branchRow}
                          style={{
                            background: b.name === selectedRepo.defaultBranch ? "rgba(99,102,241,0.05)" : "rgba(255,255,255,0.02)",
                            border: b.name === selectedRepo.defaultBranch ? "1px solid rgba(99,102,241,0.14)" : "1px solid rgba(255,255,255,0.04)",
                          }}
                        >
                          <Space size={9}>
                            <div className={styles.branchIcon} style={{ background: b.name === selectedRepo.defaultBranch ? "rgba(99,102,241,0.1)" : "rgba(255,255,255,0.04)" }}>
                              <BranchesOutlined style={{ color: b.name === selectedRepo.defaultBranch ? "#6366f1" : "#64748b" }} />
                            </div>
                            <div>
                              <Text className={styles.branchName}>{b.name}</Text>
                              <Text className={styles.branchSha}>{b.sha.slice(0, 7)}</Text>
                            </div>
                          </Space>
                          <Space size={5}>
                            {b.name === selectedRepo.defaultBranch && <Tag color="blue" style={{ fontSize: 9, fontWeight: 700 }}>DEFAULT</Tag>}
                            {b.protected && (
                              <Tag icon={<CheckCircleOutlined />} color="success" style={{ fontSize: 9, fontWeight: 700 }}>
                                PROTECTED
                              </Tag>
                            )}
                          </Space>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* PRs */}
                  {!loadingDetail && activeTab === "prs" && (
                    <div className={styles.tabList}>
                      {prs.length === 0 && <Empty description="No open pull requests." style={{ padding: "28px 0" }} />}
                      {prs.map(pr => (
                        <div key={pr.id} className={styles.prRow}>
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 8 }}>
                            <Space align="start">
                              <PullRequestOutlined style={{ color: pr.draft ? "#64748b" : "#10b981", marginTop: 2 }} />
                              <div>
                                <Text className={styles.prTitle}>{pr.title}</Text>
                                <div className={styles.prMeta}>
                                  <Text className={styles.monoText}>#{pr.number}</Text>
                                  <img src={pr.authorAvatar} alt={pr.author} className={styles.prAvatar} />
                                  <Text className={styles.metaText}>{pr.author}</Text>
                                  <Text className={styles.metaTextFaint}>{timeAgo(pr.createdAt)}</Text>
                                  {pr.draft && <Tag color="default" style={{ fontSize: 9, fontWeight: 600 }}>DRAFT</Tag>}
                                  {pr.labels.map(l => (
                                    <Tag key={l.name} style={{ fontSize: 9, background: `#${l.color}22`, color: `#${l.color}`, border: `1px solid #${l.color}44` }}>
                                      {l.name}
                                    </Tag>
                                  ))}
                                </div>
                              </div>
                            </Space>
                            <Tooltip title="View on GitHub">
                              <a href={pr.url} target="_blank" rel="noopener noreferrer" className={styles.externalLink}>
                                <ExportOutlined style={{ fontSize: 10 }} />
                              </a>
                            </Tooltip>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Issues */}
                  {!loadingDetail && activeTab === "issues" && (
                    <div className={styles.tabList}>
                      {issues.length === 0 && <Empty description="No open issues." style={{ padding: "28px 0" }} />}
                      {issues.map(issue => (
                        <div key={issue.id} className={styles.prRow}>
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 8 }}>
                            <Space align="start">
                              <IssuesCloseOutlined style={{ color: "#10b981", marginTop: 2 }} />
                              <div>
                                <Text className={styles.prTitle}>{issue.title}</Text>
                                <div className={styles.prMeta}>
                                  <Text className={styles.monoText}>#{issue.number}</Text>
                                  <img src={issue.authorAvatar} alt={issue.author} className={styles.prAvatar} />
                                  <Text className={styles.metaText}>{issue.author}</Text>
                                  <Text className={styles.metaTextFaint}>{timeAgo(issue.createdAt)}</Text>
                                  {issue.comments > 0 && (
                                    <Space size={3}>
                                      <MessageOutlined style={{ fontSize: 9, color: "#475569" }} />
                                      <Text className={styles.metaText}>{issue.comments}</Text>
                                    </Space>
                                  )}
                                  {issue.labels.map(l => (
                                    <Tag key={l.name} style={{ fontSize: 9, background: `#${l.color}22`, color: `#${l.color}`, border: `1px solid #${l.color}44` }}>
                                      {l.name}
                                    </Tag>
                                  ))}
                                </div>
                              </div>
                            </Space>
                            <Tooltip title="View on GitHub">
                              <a href={issue.url} target="_blank" rel="noopener noreferrer" className={styles.externalLink}>
                                <ExportOutlined style={{ fontSize: 10 }} />
                              </a>
                            </Tooltip>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Releases */}
                  {!loadingDetail && activeTab === "releases" && (
                    <div className={styles.tabList} style={{ gap: 7 }}>
                      {releases.length === 0 && <Empty description="No releases." style={{ padding: "28px 0" }} />}
                      {releases.map(rel => (
                        <div
                          key={rel.id}
                          className={styles.releaseRow}
                          style={{
                            background: rel.prerelease ? "rgba(245,158,11,0.03)" : "rgba(255,255,255,0.02)",
                            border: rel.prerelease ? "1px solid rgba(245,158,11,0.1)" : "1px solid rgba(255,255,255,0.04)",
                          }}
                        >
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 8 }}>
                            <Space>
                              <TagOutlined style={{ color: "#6366f1" }} />
                              <Text className={styles.releaseTag}>{rel.tagName}</Text>
                              {rel.prerelease && <Tag color="warning" style={{ fontSize: 9, fontWeight: 700 }}>PRE-RELEASE</Tag>}
                              {rel.draft && <Tag color="default" style={{ fontSize: 9, fontWeight: 700 }}>DRAFT</Tag>}
                            </Space>
                            <Tooltip title="View on GitHub">
                              <a href={rel.url} target="_blank" rel="noopener noreferrer" className={styles.externalLink}>
                                <ExportOutlined style={{ fontSize: 10 }} />
                              </a>
                            </Tooltip>
                          </div>
                          {rel.name && rel.name !== rel.tagName && <Paragraph style={{ margin: "4px 0 0 19px", fontSize: 11, color: "#94a3b8" }}>{rel.name}</Paragraph>}
                          <div style={{ display: "flex", gap: 9, marginTop: 5, marginLeft: 19 }}>
                            {rel.author && <Text className={styles.metaText}>by {rel.author}</Text>}
                            {rel.publishedAt && <Text className={styles.metaTextFaint}>{timeAgo(rel.publishedAt)}</Text>}
                          </div>
                          {rel.body && <pre className={styles.releaseBody}>{rel.body.slice(0, 250)}{rel.body.length > 250 ? "..." : ""}</pre>}
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Actions */}
                  {!loadingDetail && activeTab === "actions" && (
                    <div className={styles.tabList}>
                      {runs.length === 0 && <Empty description="No workflow runs found." style={{ padding: "28px 0" }} />}
                      {runs.map(run => (
                        <div key={run.id} className={styles.runRow}>
                          <Tooltip title={run.conclusion ?? run.status}>
                            <div className={styles.runIcon} style={{ background: `${conclusionColor(run.conclusion, run.status)}15` }}>
                              {conclusionIcon(run.conclusion, run.status)}
                            </div>
                          </Tooltip>
                          <div style={{ flex: 1, minWidth: 0 }}>
                            <Text className={styles.runName}>{run.name}</Text>
                            <div className={styles.runMeta}>
                              <Text className={styles.monoText}>{run.branch}</Text>
                              <Text className={styles.monoText}>{run.commitSha}</Text>
                              <Tag style={{ fontSize: 9, background: "rgba(255,255,255,0.04)", border: "1px solid rgba(255,255,255,0.07)", color: "#64748b" }}>{run.event_}</Tag>
                              <Text className={styles.metaTextFaint}>{timeAgo(run.createdAt)}</Text>
                              {run.durationMs > 0 && <Text className={styles.metaTextFaint}>{formatDuration(run.durationMs)}</Text>}
                            </div>
                          </div>
                          <Tooltip title="View on GitHub">
                            <a href={run.url} target="_blank" rel="noopener noreferrer" className={styles.externalLink}>
                              <ExportOutlined style={{ fontSize: 10 }} />
                            </a>
                          </Tooltip>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Files */}
                  {activeTab === "files" && (
                    viewingFile
                      ? <FileViewer owner={ownerOf(selectedRepo)} repo={selectedRepo.name} path={viewingFile.path} onBack={() => setViewingFile(null)} />
                      : <FileTree owner={ownerOf(selectedRepo)} repo={selectedRepo.name} onFileSelect={(path, name) => setViewingFile({ path, name })} />
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      <CreateRepoModal
        open={showCreateModal}
        onClose={() => setShowCreateModal(false)}
        onCreated={repo => setRepos(prev => [repo, ...prev])}
      />
    </>
  );
}