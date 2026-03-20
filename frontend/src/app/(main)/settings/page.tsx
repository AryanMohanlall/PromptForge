"use client";

import { useStyles } from "./styles/style";
import {
  Card,
  Switch,
  Typography,
  Space,
  Spin,
  Tag,
  Button,
  Empty,
} from "antd";
import { useEffect, useState } from "react";
import { getAxiosInstance } from "@/utils/axiosInstance";
import {
  GithubIcon,
  CheckCircleIcon,
  AlertCircleIcon,
  UserPlusIcon,
} from "lucide-react";
import InviteModal from "@/components/auth/InviteModal";

interface GitHubStatus {
  connected: boolean;
  id?: number;
  slug?: string;
  name?: string;
  message?: string;
  error?: string;
}

export default function SettingsPage() {
  const { styles } = useStyles();
  const [githubStatus, setGithubStatus] = useState<GitHubStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [inviteModalOpen, setInviteModalOpen] = useState(false);

  const API_BASE_URL =
    process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:44311";

  useEffect(() => {
    const fetchGitHubStatus = async () => {
      try {
        setLoading(true);
        const axiosInstance = getAxiosInstance();
        const response = await axiosInstance.get<GitHubStatus>(
          "/api/github-app/status",
        );
        setGithubStatus(response.data);
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (error: any) {
        // GitHub App status fails because credentials aren't configured
        // But GitHub OAuth might still be available
        if (error.response?.status === 400) {
          // GitHub App not configured, but OAuth might be
          setGithubStatus({
            connected: false,
            message:
              "Ready to connect GitHub account. Click Connect to link your GitHub profile.",
          });
        } else {
          setGithubStatus({
            connected: false,
            message:
              "GitHub integration not available. Please configure GitHub OAuth credentials.",
            error: error instanceof Error ? error.message : "Unknown error",
          });
        }
      } finally {
        setLoading(false);
      }
    };

    fetchGitHubStatus();
  }, []);

  const handleGitHubConnect = async () => {
    try {
      console.log(
        "Initiating GitHub login to:",
        `${API_BASE_URL}/api/tokenauth/GitHubLogin`,
      );
      // Direct navigation to the GitHub OAuth endpoint
      window.location.href = `${API_BASE_URL}/api/tokenauth/GitHubLogin`;
    } catch (error) {
      console.error("Failed to initiate GitHub login:", error);
      alert(
        `Error: ${error instanceof Error ? error.message : "Unknown error"}`,
      );
    }
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <Typography.Title level={2} className={styles.title}>
          Settings
        </Typography.Title>
        <Typography.Paragraph className={styles.subtitle}>
          Configure your PromptForge experience.
        </Typography.Paragraph>
      </div>

      <div className={styles.grid}>
        <Card className={styles.card} title="Theme">
          <div className={styles.settingRow}>
            <span className={styles.settingLabel}>Dark mode</span>
            <Switch checked disabled />
          </div>
          <div className={styles.settingRow}>
            <span className={styles.settingLabel}>Enable animations</span>
            <Switch defaultChecked />
          </div>
        </Card>
        <Card className={styles.card} title="Account">
          <div className={styles.settingRow}>
            <span className={styles.settingLabel}>Email notifications</span>
            <Switch defaultChecked />
          </div>
          <div className={styles.settingRow}>
            <span className={styles.settingLabel}>Auto-update</span>
            <Switch />
          </div>
        </Card>

        <Card
          className={styles.card}
          title={
            <Space>
              <GithubIcon size={20} />
              GitHub Integration
            </Space>
          }
        >
          <Spin spinning={loading}>
            {githubStatus ? (
              <Space orientation="vertical" style={{ width: "100%" }}>
                <div className={styles.settingRow}>
                  <span className={styles.settingLabel}>Status</span>
                  <Tag
                    color={githubStatus.connected ? "green" : "red"}
                    icon={
                      githubStatus.connected ? (
                        <CheckCircleIcon size={16} />
                      ) : (
                        <AlertCircleIcon size={16} />
                      )
                    }
                  >
                    {githubStatus.connected ? "Connected" : "Disconnected"}
                  </Tag>
                </div>

                {githubStatus.connected && githubStatus.name && (
                  <>
                    <div className={styles.settingRow}>
                      <span className={styles.settingLabel}>App Name</span>
                      <span>{githubStatus.name}</span>
                    </div>
                    <div className={styles.settingRow}>
                      <span className={styles.settingLabel}>App Slug</span>
                      <span>{githubStatus.slug}</span>
                    </div>
                    <div className={styles.settingRow}>
                      <span className={styles.settingLabel}>App ID</span>
                      <span>{githubStatus.id}</span>
                    </div>
                  </>
                )}

                {!githubStatus.connected && (
                  <div>
                    {githubStatus.message && (
                      <Typography.Paragraph
                        type="secondary"
                        style={{ fontSize: "12px" }}
                      >
                        {githubStatus.message}
                      </Typography.Paragraph>
                    )}
                  </div>
                )}

                <Button
                  type={githubStatus.connected ? "default" : "primary"}
                  onClick={handleGitHubConnect}
                  style={{ marginTop: "8px" }}
                >
                  {githubStatus.connected ? "Reconnect" : "Connect"} GitHub
                </Button>
              </Space>
            ) : (
              <Empty description="Unable to load GitHub status" />
            )}
          </Spin>
        </Card>

        <Card
          className={styles.card}
          title={
            <Space>
              <UserPlusIcon size={20} />
              User Invitations
            </Space>
          }
        >
          <Typography.Paragraph type="secondary" style={{ marginBottom: 16 }}>
            Invite new users to join your organization. They will receive an
            email with a link to create their account.
          </Typography.Paragraph>
          <Button
            type="primary"
            onClick={() => setInviteModalOpen(true)}
            icon={<UserPlusIcon size={16} />}
          >
            Invite User
          </Button>
        </Card>
      </div>

      <InviteModal
        open={inviteModalOpen}
        onClose={() => setInviteModalOpen(false)}
      />
    </div>
  );
}
