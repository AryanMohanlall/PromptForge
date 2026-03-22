"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useSearchParams } from "next/navigation";
import { Button, Input } from "antd";
import {
  usePageStyles,
  useCardStyles,
  useInputStyles,
  useAuthStyles,
} from "./styles/style";
import { MailIcon, LockIcon, UserIcon, BrandingStackIcon } from "./icons";
import { useAuthAction, useAuthState } from "@/providers/auth-provider";

// ─── Types ────────────────────────────────────────────────────────────────────
interface InputProps {
  readonly icon: React.ReactNode;
  readonly type?: string;
  readonly placeholder: string;
  readonly value: string;
  readonly onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  readonly onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  readonly showToggle?: boolean;
  readonly confirmState?: "idle" | "match" | "mismatch";
  readonly disabled?: boolean;
}

interface InvitePayload {
  tenantId: number;
  role: string;
  email: string;
  expires: string;
}

const decodeInviteToken = (value: string | null): InvitePayload | undefined => {
  if (!value) return undefined;
  try {
    const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
    const padding = normalized.length % 4;
    const decoded = atob(
      padding
        ? normalized.padEnd(normalized.length + (4 - padding), "=")
        : normalized,
    );
    const payload = JSON.parse(decoded) as InvitePayload;
    if (
      typeof payload.tenantId === "number" &&
      typeof payload.role === "string" &&
      typeof payload.email === "string" &&
      typeof payload.expires === "string"
    ) {
      return payload;
    }
    return undefined;
  } catch {
    return undefined;
  }
};

// ─── Primitives ───────────────────────────────────────────────────────────────
function AuthInput({
  icon,
  type = "text",
  placeholder,
  value,
  onChange,
  onKeyDown,
  showToggle,
  confirmState,
  disabled,
}: InputProps) {
  const { styles } = useInputStyles();
  const prefix = <span className={styles.icon}>{icon}</span>;

  if (showToggle) {
    return (
      <Input.Password
        prefix={prefix}
        placeholder={placeholder}
        value={value}
        disabled={disabled}
        onChange={onChange}
        onKeyDown={onKeyDown}
        className={styles.input}
        status={confirmState === "mismatch" ? "error" : ""}
      />
    );
  }

  return (
    <Input
      prefix={prefix}
      type={type}
      placeholder={placeholder}
      value={value}
      onChange={onChange}
      onKeyDown={onKeyDown}
      className={styles.input}
      disabled={disabled}
    />
  );
}

function AuthCard({ children }: { readonly children: React.ReactNode }) {
  const { styles } = useCardStyles();
  return <div className={styles.card}>{children}</div>;
}

function AuthLogo() {
  const { styles } = useAuthStyles();
  return (
    <div className={styles.logoContainer}>
      <Image
        src="/logo.svg"
        alt="PromptForge"
        width={40}
        height={40}
        className={styles.logo}
      />
      <span className={styles.logoName}>PromptForge</span>
    </div>
  );
}

// ─── Sign Up ──────────────────────────────────────────────────────────────────
const PASSWORD_CHECKS = [
  { label: "At least 8 characters", test: (pw: string) => pw.length >= 8 },
  { label: "One uppercase letter", test: (pw: string) => /[A-Z]/.test(pw) },
  { label: "One number or symbol", test: (pw: string) => /[\d\W]/.test(pw) },
];

function CheckItem({
  label,
  met,
}: {
  readonly label: string;
  readonly met: boolean;
}) {
  const { styles } = useAuthStyles();
  return (
    <div className={met ? styles.checkItemMet : styles.checkItemUnmet}>
      <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
        <circle
          cx="7"
          cy="7"
          r="6"
          stroke={met ? "#2dd4a8" : "#2a3a4a"}
          strokeWidth="1.2"
          fill={met ? "rgba(45,212,168,0.1)" : "none"}
        />
        {met && (
          <path
            d="M4.5 7l1.8 1.8 3.2-3.6"
            stroke="#2dd4a8"
            strokeWidth="1.2"
            strokeLinecap="round"
            strokeLinejoin="round"
            fill="none"
          />
        )}
      </svg>
      {label}
    </div>
  );
}

function SignUpPage() {
  const [name, setName] = useState("");
  const [hasToken, setHasToken] = useState(false);
  const [surname, setSurname] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [attempted, setAttempted] = useState(false);
  const [validationError, setValidationError] = useState("");
  const searchParams = useSearchParams();
  const inviteToken = searchParams.get("token");
  const invitePayload = decodeInviteToken(inviteToken);
  const tenantId = invitePayload?.tenantId;
  const { register } = useAuthAction();
  const { isPending, isError } = useAuthState();
  const { styles } = useAuthStyles();

  const passwordsMatch =
    confirmPassword.length > 0 && password === confirmPassword;
  const finalUserName = userName || email;
  const allChecksMet = PASSWORD_CHECKS.every(({ test }) => test(password));

  const handleRegister = async () => {
    setAttempted(true);
    if (!name || !surname || !finalUserName || !email || !password) {
      setValidationError("Please complete all required fields.");
      return;
    }
    if (!allChecksMet || !passwordsMatch) {
      setValidationError("Please meet all password requirements.");
      return;
    }
    setValidationError("");
    await register({
      name,
      surname,
      userName: finalUserName,
      emailAddress: email,
      password,
      roleName: invitePayload?.role,
      ...(tenantId !== undefined ? { tenantId } : {}),
    });
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleRegister();
    }
  };

  const canSubmit = Boolean(
    name &&
    surname &&
    finalUserName &&
    email &&
    password &&
    passwordsMatch &&
    allChecksMet,
  );

  useEffect(() => {
    if (invitePayload) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setHasToken(true);
      setEmail(invitePayload.email);
    }
  }, [invitePayload]);

  return (
    <AuthCard>
      <div className={styles.header}>
        <AuthLogo />
        <h2 className={styles.heading}>Create your account</h2>
        <p className={styles.subtitle}>Start building with PromptForge</p>
      </div>

      <div className={styles.formGroup}>
        <AuthInput
          icon={<UserIcon />}
          type="text"
          placeholder="First name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <AuthInput
          icon={<UserIcon />}
          type="text"
          placeholder="Surname"
          value={surname}
          onChange={(e) => setSurname(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <AuthInput
          icon={<UserIcon />}
          type="text"
          placeholder="Username"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <AuthInput
          icon={<MailIcon />}
          type="email"
          placeholder="Email address"
          value={email}
          disabled={hasToken}
          onChange={(e) => setEmail(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <AuthInput
          icon={<LockIcon />}
          placeholder="Create a password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          onKeyDown={handleKeyDown}
          showToggle
        />
        <AuthInput
          icon={<LockIcon />}
          placeholder="Confirm password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          onKeyDown={handleKeyDown}
          showToggle
          confirmState={
            confirmPassword.length > 0
              ? passwordsMatch
                ? "match"
                : "mismatch"
              : "idle"
          }
        />
      </div>

      {password.length > 0 && (
        <div className={styles.passwordChecks}>
          {PASSWORD_CHECKS.map(({ label, test }) => (
            <CheckItem key={label} label={label} met={test(password)} />
          ))}
          {confirmPassword.length > 0 && (
            <CheckItem label="Passwords match" met={passwordsMatch} />
          )}
        </div>
      )}

      {attempted && isError && (
        <p className={styles.subtitle}>
          Registration failed. Please check your details.
        </p>
      )}
      {attempted && validationError && (
        <p className={styles.subtitle}>{validationError}</p>
      )}

      <Button
        block
        type="primary"
        className={styles.primaryBtnMt}
        onClick={handleRegister}
        loading={isPending}
        disabled={!canSubmit}
      >
        Create account
      </Button>

      <p className={styles.termsText}>
        By signing up, you agree to our{" "}
        <span className={styles.termsLink}>Terms of Service</span> and{" "}
        <span className={styles.termsLink}>Privacy Policy</span>.
      </p>

      <p className={styles.switchText}>
        Already have an account?{" "}
        <a href="/login" className={styles.switchBtn}>
          Sign in
        </a>
      </p>
    </AuthCard>
  );
}

// ─── Root ─────────────────────────────────────────────────────────────────────
export default function SignUp() {
  const { styles } = usePageStyles();

  return (
    <div className={styles.page}>
      <div className={styles.bgOrb1} />
      <div className={styles.bgOrb2} />
      <div className={styles.gridOverlay} />

      <div className={styles.cardWrapper}>
        <SignUpPage />
      </div>

      <div className={styles.branding}>
        <BrandingStackIcon />
        PromptForge
      </div>
    </div>
  );
}
