"use client";

import { useState } from "react";
import { Modal, Input, Select, Button, message } from "antd";
import { getAxiosInstance } from "@/utils/axiosInstance";

interface InviteModalProps {
  open: boolean;
  onClose: () => void;
}

const ROLE_OPTIONS = [
  { label: "Developer", value: "Developer" },
  { label: "Product Builder", value: "ProductBuilder" },
  { label: "Admin", value: "Admin" },
];

export default function InviteModal({ open, onClose }: InviteModalProps) {
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<string>("Developer");
  const [loading, setLoading] = useState(false);
  const instance = getAxiosInstance();

  const handleInvite = async () => {
    if (!email) {
      message.error("Please enter an email address");
      return;
    }

    setLoading(true);
    try {
      await instance.post("/api/services/app/Invite/InviteUser", {
        emailAddress: email,
        role: role,
      });
      message.success("Invitation sent successfully!");
      setEmail("");
      setRole("Developer");
      onClose();
    } catch (error: unknown) {
      const axiosError = error as {
        response?: {
          data?: { error?: { message?: string }; message?: string };
        };
      };
      const errorMessage =
        axiosError?.response?.data?.error?.message ||
        axiosError?.response?.data?.message ||
        "Failed to send invitation";
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    setEmail("");
    setRole("Developer");
    onClose();
  };

  return (
    <Modal
      title="Invite User"
      open={open}
      onCancel={handleCancel}
      footer={[
        <Button key="cancel" onClick={handleCancel}>
          Cancel
        </Button>,
        <Button
          key="invite"
          type="primary"
          onClick={handleInvite}
          loading={loading}
        >
          Send Invitation
        </Button>,
      ]}
    >
      <div style={{ marginBottom: 16 }}>
        <label style={{ display: "block", marginBottom: 8, fontWeight: 500 }}>
          Email Address
        </label>
        <Input
          type="email"
          placeholder="Enter email address"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          onPressEnter={handleInvite}
        />
      </div>
      <div>
        <label style={{ display: "block", marginBottom: 8, fontWeight: 500 }}>
          Role
        </label>
        <Select
          style={{ width: "100%" }}
          value={role}
          onChange={setRole}
          options={ROLE_OPTIONS}
        />
      </div>
    </Modal>
  );
}
