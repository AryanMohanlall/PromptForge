"use client";

import { useEffect, useState, useCallback } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import {
  GithubIcon,
  GitBranchIcon,
  GitCommitIcon,
  StarIcon,
  LockIcon,
  UnlockIcon,
  RefreshCwIcon,
  ExternalLinkIcon,
  FolderIcon,
  ClockIcon,
  PlusIcon,
  XIcon,
  AlertCircleIcon,
  CheckCircleIcon,
} from "lucide-react";

// ─── Types ────────────────────────────────────────────────────────────────────

interface Repository {
  id: number;
  name: string;
  fullName: string;
  htmlUrl: string;
  private: boolean;
  description?: string;
  language?: string;
  stargazersCount?: number;
  forksCount?: number;
  updatedAt?: string;
  defaultBranch?: string;
}

interface Commit {
  sha: string;
  message: string;
  author: string;
  date: string;
  url: string;
}

interface Branch {
  name: string;
  sha: string;
  protected: boolean;
}

type TabType = "repositories" | "commits" | "branches";

// ─── Helpers ─────────────────────────────────────────────────────────────────

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  return `${days}d ago`;
}

const LANG_COLORS: Record<string, string> = {
  TypeScript: "#3178c6",
  JavaScript: "#f1e05a",
  Python: "#3572A5",
  "C#": "#178600",
  Go: "#00ADD8",
  Rust: "#dea584",
  Java: "#b07219",
  default: "#8b949e",
};

// ─── Sub-components ──────────────────────────────────────────────────────────

function RepoCard({
  repo,
  onSelect,
  selected,
}: {
  repo: Repository;
  onSelect: (repo: Repository) => void;
  selected: boolean;
}) {
  const langColor = LANG_COLORS[repo.language ?? ""] ?? LANG_COLORS.default;

  return (
    <button
      type="button"
      onClick={() => onSelect(repo)}
      style={{
        width: "100%",
        textAlign: "left",
        background: selected
          ? "rgba(99,102,241,0.08)"
          : "rgba(255,255,255,0.02)",
        border: selected
          ? "1px solid rgba(99,102,241,0.4)"
          : "1px solid rgba(255,255,255,0.06)",
        borderRadius: 12,
        padding: "16px 20px",
        cursor: "pointer",
        transition: "all 0.15s ease",
      }}
      onMouseEnter={(e) => {
        if (!selected)
          (e.currentTarget as HTMLElement).style.background =
            "rgba(255,255,255,0.04)";
      }}
      onMouseLeave={(e) => {
        if (!selected)
          (e.currentTarget as HTMLElement).style.background =
            "rgba(255,255,255,0.02)";
      }}
    >
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-start",
          gap: 12,
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
          {repo.private ? (
            <LockIcon
              style={{ width: 14, height: 14, color: "#f59e0b", flexShrink: 0 }}
            />
          ) : (
            <UnlockIcon
              style={{ width: 14, height: 14, color: "#10b981", flexShrink: 0 }}
            />
          )}
          <span
            style={{
              fontFamily: "'JetBrains Mono', monospace",
              fontSize: 13,
              fontWeight: 600,
              color: "#e2e8f0",
            }}
          >
            {repo.name}
          </span>
        </div>
        <a
          href={repo.htmlUrl}
          target="_blank"
          rel="noopener noreferrer"
          onClick={(e) => e.stopPropagation()}
          style={{ color: "#6366f1", flexShrink: 0 }}
        >
          <ExternalLinkIcon style={{ width: 13, height: 13 }} />
        </a>
      </div>

      {repo.description && (
        <p
          style={{
            margin: "8px 0 0 22px",
            fontSize: 12,
            color: "#94a3b8",
            lineHeight: 1.5,
          }}
        >
          {repo.description}
        </p>
      )}

      <div
        style={{
          display: "flex",
          gap: 16,
          marginTop: 12,
          marginLeft: 22,
          flexWrap: "wrap",
        }}
      >
        {repo.language && (
          <span style={{ display: "flex", alignItems: "center", gap: 5 }}>
            <span
              style={{
                width: 10,
                height: 10,
                borderRadius: "50%",
                background: langColor,
                flexShrink: 0,
              }}
            />
            <span style={{ fontSize: 11, color: "#94a3b8" }}>
              {repo.language}
            </span>
          </span>
        )}
        {repo.stargazersCount !== undefined && repo.stargazersCount > 0 && (
          <span
            style={{ display: "flex", alignItems: "center", gap: 4 }}
          >
            <StarIcon
              style={{ width: 11, height: 11, color: "#f59e0b" }}
            />
            <span style={{ fontSize: 11, color: "#94a3b8" }}>
              {repo.stargazersCount}
            </span>
          </span>
        )}
        {repo.updatedAt && (
          <span
            style={{ display: "flex", alignItems: "center", gap: 4 }}
          >
            <ClockIcon style={{ width: 11, height: 11, color: "#64748b" }} />
            <span style={{ fontSize: 11, color: "#64748b" }}>
              {timeAgo(repo.updatedAt)}
            </span>
          </span>
        )}
      </div>
    </button>
  );
}

function CreateRepoModal({
  onClose,
  onCreated,
}: {
  onClose: () => void;
  onCreated: (repo: Repository) => void;
}) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isPrivate, setIsPrivate] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreate = async () => {
    if (!name.trim()) return;
    setLoading(true);
    setError(null);
    try {
      const instance = getAxiosInstance();
      const res = await instance.post<{ repository: Repository }>(
        "/api/github-app/repositories",
        {
          name: name.trim(),
          description: description.trim() || undefined,
          isPrivate,
          autoInit: true,
        }
      );
      onCreated(res.data.repository);
      onClose();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e.response?.data?.error ?? "Failed to create repository.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        position: "fixed",
        inset: 0,
        background: "rgba(0,0,0,0.7)",
        backdropFilter: "blur(4px)",
        zIndex: 50,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: 24,
      }}
      onClick={onClose}
    >
      <div
        style={{
          background: "#0f1117",
          border: "1px solid rgba(255,255,255,0.1)",
          borderRadius: 16,
          padding: 32,
          width: "100%",
          maxWidth: 480,
        }}
        onClick={(e) => e.stopPropagation()}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: 24,
          }}
        >
          <h3
            style={{
              margin: 0,
              fontFamily: "'JetBrains Mono', monospace",
              fontSize: 16,
              color: "#e2e8f0",
            }}
          >
            New repository
          </h3>
          <button
            type="button"
            onClick={onClose}
            style={{
              background: "none",
              border: "none",
              color: "#64748b",
              cursor: "pointer",
              padding: 4,
            }}
          >
            <XIcon style={{ width: 16, height: 16 }} />
          </button>
        </div>

        <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div>
            <label
              style={{
                display: "block",
                fontSize: 12,
                color: "#94a3b8",
                marginBottom: 6,
                fontFamily: "'JetBrains Mono', monospace",
              }}
            >
              Repository name *
            </label>
            <input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="my-awesome-app"
              style={{
                width: "100%",
                background: "rgba(255,255,255,0.04)",
                border: "1px solid rgba(255,255,255,0.1)",
                borderRadius: 8,
                padding: "10px 14px",
                color: "#e2e8f0",
                fontSize: 13,
                fontFamily: "'JetBrains Mono', monospace",
                outline: "none",
                boxSizing: "border-box",
              }}
            />
          </div>

          <div>
            <label
              style={{
                display: "block",
                fontSize: 12,
                color: "#94a3b8",
                marginBottom: 6,
                fontFamily: "'JetBrains Mono', monospace",
              }}
            >
              Description
            </label>
            <input
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Optional description..."
              style={{
                width: "100%",
                background: "rgba(255,255,255,0.04)",
                border: "1px solid rgba(255,255,255,0.1)",
                borderRadius: 8,
                padding: "10px 14px",
                color: "#e2e8f0",
                fontSize: 13,
                outline: "none",
                boxSizing: "border-box",
              }}
            />
          </div>

          <label
            style={{
              display: "flex",
              alignItems: "center",
              gap: 10,
              cursor: "pointer",
            }}
          >
            <input
              type="checkbox"
              checked={isPrivate}
              onChange={(e) => setIsPrivate(e.target.checked)}
              style={{ width: 14, height: 14, accentColor: "#6366f1" }}
            />
            <span style={{ fontSize: 13, color: "#94a3b8" }}>
              Private repository
            </span>
          </label>

          {error && (
            <div
              style={{
                display: "flex",
                gap: 8,
                padding: "10px 14px",
                background: "rgba(239,68,68,0.1)",
                border: "1px solid rgba(239,68,68,0.2)",
                borderRadius: 8,
              }}
            >
              <AlertCircleIcon
                style={{ width: 14, height: 14, color: "#ef4444", flexShrink: 0, marginTop: 1 }}
              />
              <span style={{ fontSize: 12, color: "#ef4444" }}>{error}</span>
            </div>
          )}

          <button
            type="button"
            onClick={handleCreate}
            disabled={!name.trim() || loading}
            style={{
              background: name.trim() && !loading ? "#6366f1" : "#374151",
              border: "none",
              borderRadius: 8,
              padding: "11px 20px",
              color: name.trim() && !loading ? "#fff" : "#6b7280",
              fontSize: 13,
              fontWeight: 600,
              cursor: name.trim() && !loading ? "pointer" : "not-allowed",
              fontFamily: "'JetBrains Mono', monospace",
              transition: "all 0.15s",
            }}
          >
            {loading ? "Creating..." : "Create repository"}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export default function GitHubDashboardPage() {
  const [repos, setRepos] = useState<Repository[]>([]);
  const [selectedRepo, setSelectedRepo] = useState<Repository | null>(null);
  const [commits, setCommits] = useState<Commit[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [activeTab, setActiveTab] = useState<TabType>("repositories");
  const [loadingRepos, setLoadingRepos] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [reposError, setReposError] = useState<string | null>(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");

  const instance = getAxiosInstance();

  const fetchRepos = useCallback(async () => {
    setLoadingRepos(true);
    setReposError(null);
    try {
      const res = await instance.get<{ repositories: Repository[] }>(
        "/api/github-app/repositories"
      );
      setRepos(res.data.repositories ?? []);
    } catch {
      setReposError("Failed to load repositories. Make sure your GitHub account is connected.");
    } finally {
      setLoadingRepos(false);
    }
  }, [instance]);

  const fetchCommits = useCallback(
    async (repo: Repository) => {
      setLoadingDetail(true);
      try {
        const res = await instance.get<{ commits: Commit[] }>(
          `/api/github-app/commits?owner=${repo.fullName.split("/")[0]}&repo=${repo.name}`
        );
        setCommits(res.data.commits ?? []);
      } catch {
        setCommits([]);
      } finally {
        setLoadingDetail(false);
      }
    },
    [instance]
  );

  const fetchBranches = useCallback(
    async (repo: Repository) => {
      setLoadingDetail(true);
      try {
        const res = await instance.get<{ branches: Branch[] }>(
          `/api/github-app/branches?owner=${repo.fullName.split("/")[0]}&repo=${repo.name}`
        );
        setBranches(res.data.branches ?? []);
      } catch {
        setBranches([]);
      } finally {
        setLoadingDetail(false);
      }
    },
    [instance]
  );

  useEffect(() => {
    fetchRepos();
  }, [fetchRepos]);

  const handleSelectRepo = (repo: Repository) => {
    setSelectedRepo(repo);
    setActiveTab("commits");
    fetchCommits(repo);
  };

  const handleTabChange = (tab: TabType) => {
    setActiveTab(tab);
    if (!selectedRepo) return;
    if (tab === "commits") fetchCommits(selectedRepo);
    if (tab === "branches") fetchBranches(selectedRepo);
  };

  const filteredRepos = repos.filter((r) =>
    r.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const inputStyle: React.CSSProperties = {
    background: "rgba(255,255,255,0.04)",
    border: "1px solid rgba(255,255,255,0.08)",
    borderRadius: 8,
    padding: "9px 14px",
    color: "#e2e8f0",
    fontSize: 13,
    outline: "none",
    width: "100%",
    boxSizing: "border-box",
  };

  return (
    <>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Sora:wght@400;500;600;700&display=swap');
        * { box-sizing: border-box; }
        ::-webkit-scrollbar { width: 4px; }
        ::-webkit-scrollbar-track { background: transparent; }
        ::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.08); border-radius: 4px; }
      `}</style>

      <div
        style={{
          minHeight: "100vh",
          background: "#080a0f",
          color: "#e2e8f0",
          fontFamily: "'Sora', sans-serif",
          padding: "32px 24px",
        }}
      >
        <div style={{ maxWidth: 1200, margin: "0 auto" }}>

          {/* ── Header ── */}
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "flex-start",
              marginBottom: 32,
              flexWrap: "wrap",
              gap: 16,
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: 14 }}>
              <div
                style={{
                  width: 44,
                  height: 44,
                  borderRadius: 12,
                  background: "linear-gradient(135deg, #6366f1, #8b5cf6)",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                }}
              >
                <GithubIcon style={{ width: 22, height: 22, color: "#fff" }} />
              </div>
              <div>
                <h1
                  style={{
                    margin: 0,
                    fontSize: 22,
                    fontWeight: 700,
                    fontFamily: "'JetBrains Mono', monospace",
                    background: "linear-gradient(135deg, #e2e8f0, #94a3b8)",
                    WebkitBackgroundClip: "text",
                    WebkitTextFillColor: "transparent",
                  }}
                >
                  GitHub Dashboard
                </h1>
                <p style={{ margin: 0, fontSize: 13, color: "#64748b" }}>
                  Manage repositories, commits, and branches
                </p>
              </div>
            </div>

            <div style={{ display: "flex", gap: 10 }}>
              <button
                type="button"
                onClick={fetchRepos}
                disabled={loadingRepos}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 6,
                  background: "rgba(255,255,255,0.04)",
                  border: "1px solid rgba(255,255,255,0.08)",
                  borderRadius: 8,
                  padding: "9px 16px",
                  color: "#94a3b8",
                  fontSize: 13,
                  cursor: loadingRepos ? "not-allowed" : "pointer",
                  fontFamily: "'Sora', sans-serif",
                }}
              >
                <RefreshCwIcon
                  style={{
                    width: 13,
                    height: 13,
                    animation: loadingRepos ? "spin 1s linear infinite" : "none",
                  }}
                />
                Refresh
              </button>
              <button
                type="button"
                onClick={() => setShowCreateModal(true)}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 6,
                  background: "#6366f1",
                  border: "none",
                  borderRadius: 8,
                  padding: "9px 16px",
                  color: "#fff",
                  fontSize: 13,
                  fontWeight: 600,
                  cursor: "pointer",
                  fontFamily: "'Sora', sans-serif",
                }}
              >
                <PlusIcon style={{ width: 14, height: 14 }} />
                New repo
              </button>
            </div>
          </div>

          {/* ── Main grid ── */}
          <div
            style={{
              display: "grid",
              gridTemplateColumns: selectedRepo ? "380px 1fr" : "1fr",
              gap: 20,
              alignItems: "start",
            }}
          >
            {/* ── Repo list ── */}
            <div
              style={{
                background: "rgba(255,255,255,0.02)",
                border: "1px solid rgba(255,255,255,0.06)",
                borderRadius: 16,
                overflow: "hidden",
              }}
            >
              <div style={{ padding: "16px 20px 12px" }}>
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: 12,
                  }}
                >
                  <span
                    style={{
                      fontSize: 12,
                      fontWeight: 600,
                      color: "#64748b",
                      fontFamily: "'JetBrains Mono', monospace",
                      textTransform: "uppercase",
                      letterSpacing: "0.06em",
                    }}
                  >
                    Repositories
                    {repos.length > 0 && (
                      <span
                        style={{
                          marginLeft: 8,
                          background: "rgba(99,102,241,0.15)",
                          color: "#6366f1",
                          borderRadius: 6,
                          padding: "1px 7px",
                          fontSize: 11,
                        }}
                      >
                        {repos.length}
                      </span>
                    )}
                  </span>
                </div>
                <input
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search repositories..."
                  style={inputStyle}
                />
              </div>

              <div
                style={{
                  maxHeight: 560,
                  overflowY: "auto",
                  padding: "0 12px 12px",
                  display: "flex",
                  flexDirection: "column",
                  gap: 8,
                }}
              >
                {loadingRepos && (
                  <div
                    style={{
                      padding: "40px 20px",
                      textAlign: "center",
                      color: "#64748b",
                      fontSize: 13,
                    }}
                  >
                    Loading repositories...
                  </div>
                )}

                {reposError && (
                  <div
                    style={{
                      margin: 8,
                      padding: "14px 16px",
                      background: "rgba(239,68,68,0.08)",
                      border: "1px solid rgba(239,68,68,0.15)",
                      borderRadius: 10,
                      display: "flex",
                      gap: 10,
                    }}
                  >
                    <AlertCircleIcon
                      style={{ width: 15, height: 15, color: "#ef4444", flexShrink: 0, marginTop: 1 }}
                    />
                    <span style={{ fontSize: 12, color: "#fca5a5", lineHeight: 1.5 }}>
                      {reposError}
                    </span>
                  </div>
                )}

                {!loadingRepos &&
                  !reposError &&
                  filteredRepos.length === 0 && (
                    <div
                      style={{
                        padding: "40px 20px",
                        textAlign: "center",
                        color: "#64748b",
                        fontSize: 13,
                      }}
                    >
                      {searchQuery ? "No matching repositories" : "No repositories found"}
                    </div>
                  )}

                {filteredRepos.map((repo) => (
                  <RepoCard
                    key={repo.id}
                    repo={repo}
                    onSelect={handleSelectRepo}
                    selected={selectedRepo?.id === repo.id}
                  />
                ))}
              </div>
            </div>

            {/* ── Detail panel ── */}
            {selectedRepo && (
              <div
                style={{
                  background: "rgba(255,255,255,0.02)",
                  border: "1px solid rgba(255,255,255,0.06)",
                  borderRadius: 16,
                  overflow: "hidden",
                }}
              >
                {/* Repo header */}
                <div
                  style={{
                    padding: "20px 24px 16px",
                    borderBottom: "1px solid rgba(255,255,255,0.06)",
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "flex-start",
                  }}
                >
                  <div>
                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                      <FolderIcon style={{ width: 15, height: 15, color: "#6366f1" }} />
                      <span
                        style={{
                          fontFamily: "'JetBrains Mono', monospace",
                          fontSize: 15,
                          fontWeight: 700,
                          color: "#e2e8f0",
                        }}
                      >
                        {selectedRepo.fullName}
                      </span>
                      {selectedRepo.private ? (
                        <span
                          style={{
                            fontSize: 10,
                            padding: "2px 7px",
                            background: "rgba(245,158,11,0.1)",
                            color: "#f59e0b",
                            borderRadius: 5,
                            fontWeight: 600,
                          }}
                        >
                          PRIVATE
                        </span>
                      ) : (
                        <span
                          style={{
                            fontSize: 10,
                            padding: "2px 7px",
                            background: "rgba(16,185,129,0.1)",
                            color: "#10b981",
                            borderRadius: 5,
                            fontWeight: 600,
                          }}
                        >
                          PUBLIC
                        </span>
                      )}
                    </div>
                    {selectedRepo.description && (
                      <p style={{ margin: "6px 0 0 23px", fontSize: 12, color: "#94a3b8" }}>
                        {selectedRepo.description}
                      </p>
                    )}
                  </div>
                  <div style={{ display: "flex", gap: 8 }}>
                    <a
                      href={selectedRepo.htmlUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 5,
                        fontSize: 12,
                        color: "#6366f1",
                        textDecoration: "none",
                        padding: "6px 12px",
                        background: "rgba(99,102,241,0.08)",
                        borderRadius: 7,
                        border: "1px solid rgba(99,102,241,0.15)",
                      }}
                    >
                      <ExternalLinkIcon style={{ width: 12, height: 12 }} />
                      Open on GitHub
                    </a>
                    <button
                      type="button"
                      onClick={() => setSelectedRepo(null)}
                      style={{
                        background: "none",
                        border: "1px solid rgba(255,255,255,0.08)",
                        borderRadius: 7,
                        padding: "6px 10px",
                        color: "#64748b",
                        cursor: "pointer",
                      }}
                    >
                      <XIcon style={{ width: 13, height: 13 }} />
                    </button>
                  </div>
                </div>

                {/* Tabs */}
                <div
                  style={{
                    display: "flex",
                    gap: 2,
                    padding: "12px 16px",
                    borderBottom: "1px solid rgba(255,255,255,0.06)",
                  }}
                >
                  {(["commits", "branches"] as TabType[]).map((tab) => (
                    <button
                      key={tab}
                      type="button"
                      onClick={() => handleTabChange(tab)}
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 6,
                        padding: "7px 14px",
                        borderRadius: 7,
                        border: "none",
                        background:
                          activeTab === tab
                            ? "rgba(99,102,241,0.12)"
                            : "transparent",
                        color:
                          activeTab === tab ? "#6366f1" : "#64748b",
                        fontSize: 12,
                        fontWeight: activeTab === tab ? 600 : 400,
                        cursor: "pointer",
                        fontFamily: "'Sora', sans-serif",
                        transition: "all 0.12s",
                      }}
                    >
                      {tab === "commits" ? (
                        <GitCommitIcon style={{ width: 13, height: 13 }} />
                      ) : (
                        <GitBranchIcon style={{ width: 13, height: 13 }} />
                      )}
                      {tab.charAt(0).toUpperCase() + tab.slice(1)}
                    </button>
                  ))}
                </div>

                {/* Tab content */}
                <div style={{ padding: 16, minHeight: 300 }}>
                  {loadingDetail && (
                    <div
                      style={{
                        padding: "40px 20px",
                        textAlign: "center",
                        color: "#64748b",
                        fontSize: 13,
                      }}
                    >
                      Loading...
                    </div>
                  )}

                  {/* Commits */}
                  {!loadingDetail && activeTab === "commits" && (
                    <div style={{ display: "flex", flexDirection: "column", gap: 2 }}>
                      {commits.length === 0 && (
                        <p style={{ color: "#64748b", fontSize: 13, textAlign: "center", padding: "40px 0" }}>
                          No commits found or endpoint not configured.
                        </p>
                      )}
                      {commits.map((commit) => (
                        <div
                          key={commit.sha}
                          style={{
                            padding: "12px 16px",
                            borderRadius: 10,
                            background: "rgba(255,255,255,0.02)",
                            border: "1px solid rgba(255,255,255,0.04)",
                            marginBottom: 6,
                          }}
                        >
                          <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
                            <span
                              style={{
                                fontSize: 13,
                                color: "#e2e8f0",
                                lineHeight: 1.5,
                                flex: 1,
                              }}
                            >
                              {commit.message.split("\n")[0]}
                            </span>
                            <a
                              href={commit.url}
                              target="_blank"
                              rel="noopener noreferrer"
                              style={{ color: "#6366f1", flexShrink: 0 }}
                            >
                              <ExternalLinkIcon style={{ width: 12, height: 12 }} />
                            </a>
                          </div>
                          <div
                            style={{
                              display: "flex",
                              gap: 12,
                              marginTop: 6,
                              flexWrap: "wrap",
                            }}
                          >
                            <span
                              style={{
                                fontFamily: "'JetBrains Mono', monospace",
                                fontSize: 11,
                                color: "#6366f1",
                                background: "rgba(99,102,241,0.08)",
                                padding: "1px 6px",
                                borderRadius: 4,
                              }}
                            >
                              {commit.sha.slice(0, 7)}
                            </span>
                            <span style={{ fontSize: 11, color: "#64748b" }}>
                              {commit.author}
                            </span>
                            <span style={{ fontSize: 11, color: "#64748b" }}>
                              {timeAgo(commit.date)}
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Branches */}
                  {!loadingDetail && activeTab === "branches" && (
                    <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
                      {branches.length === 0 && (
                        <p style={{ color: "#64748b", fontSize: 13, textAlign: "center", padding: "40px 0" }}>
                          No branches found or endpoint not configured.
                        </p>
                      )}
                      {branches.map((branch) => (
                        <div
                          key={branch.name}
                          style={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            padding: "12px 16px",
                            borderRadius: 10,
                            background: "rgba(255,255,255,0.02)",
                            border: "1px solid rgba(255,255,255,0.04)",
                          }}
                        >
                          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                            <GitBranchIcon
                              style={{ width: 13, height: 13, color: "#6366f1" }}
                            />
                            <span
                              style={{
                                fontFamily: "'JetBrains Mono', monospace",
                                fontSize: 13,
                                color: "#e2e8f0",
                              }}
                            >
                              {branch.name}
                            </span>
                            {branch.name === selectedRepo.defaultBranch && (
                              <span
                                style={{
                                  fontSize: 10,
                                  padding: "1px 7px",
                                  background: "rgba(16,185,129,0.1)",
                                  color: "#10b981",
                                  borderRadius: 5,
                                  fontWeight: 600,
                                }}
                              >
                                DEFAULT
                              </span>
                            )}
                          </div>
                          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                            {branch.protected && (
                              <span style={{ display: "flex", alignItems: "center", gap: 4 }}>
                                <CheckCircleIcon
                                  style={{ width: 12, height: 12, color: "#10b981" }}
                                />
                                <span style={{ fontSize: 11, color: "#10b981" }}>
                                  Protected
                                </span>
                              </span>
                            )}
                            <span
                              style={{
                                fontFamily: "'JetBrains Mono', monospace",
                                fontSize: 11,
                                color: "#64748b",
                              }}
                            >
                              {branch.sha.slice(0, 7)}
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {showCreateModal && (
        <CreateRepoModal
          onClose={() => setShowCreateModal(false)}
          onCreated={(repo) => setRepos((prev) => [repo, ...prev])}
        />
      )}

      <style>{`
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
      `}</style>
    </>
  );
}