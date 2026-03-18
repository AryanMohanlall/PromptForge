"use client";

import { useState } from "react";
import Image from "next/image";
import { redirect, useSearchParams } from "next/navigation";
import { Button, Input, Divider } from "antd";
import {
  usePageStyles,
  useCardStyles,
  useInputStyles,
  useSocialBtnStyles,
  useDividerStyles,
  useAuthStyles,
} from "./styles/style";
import {
  GitHubIcon,
  MailIcon,
  LockIcon,
  UserIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
  BrandingStackIcon,
} from "./icons";
import { useAuthAction, useAuthState } from "@/providers/auth-provider";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:44311";

const handleGitHubSignIn = () => {
  window.location.href = `${API_BASE_URL}/api/TokenAuth/GitHubLogin`;
};

// ─── Types ────────────────────────────────────────────────────────────────────
type Page = "signin" | "signup" | "forgot";

interface InputProps {
  readonly icon: React.ReactNode;
  readonly type?: string;
  readonly placeholder: string;
  readonly value: string;
  readonly onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  readonly onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  readonly showToggle?: boolean;
  readonly confirmState?: "idle" | "match" | "mismatch";
}

interface SocialButtonProps {
  readonly icon: React.ReactNode;
  readonly label: string;
  readonly onClick?: () => void;
}

interface PageProps {
  readonly onSwitch: (page: Page) => void;
}

const decodeTenantId = (value: string | null) => {
  if (!value) return undefined;
  try {
    const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
    const padding = normalized.length % 4;
    const decoded = atob(
      padding
        ? normalized.padEnd(normalized.length + (4 - padding), "=")
        : normalized,
    );
    const tenantId = Number.parseInt(decoded, 10);
    return Number.isNaN(tenantId) ? undefined : tenantId;
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
}: InputProps) {
  const { styles } = useInputStyles();
  const prefix = <span className={styles.icon}>{icon}</span>;

  if (showToggle) {
    return (
      <Input.Password
        prefix={prefix}
        placeholder={placeholder}
        value={value}
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
    />
  );
}

function SocialButton({ icon, label, onClick }: SocialButtonProps) {
  const { styles } = useSocialBtnStyles();
  return (
    <Button block icon={icon} onClick={onClick} className={styles.btn}>
      {label}
    </Button>
  );
}

function AuthDivider() {
  const { styles } = useDividerStyles();
  return <Divider className={styles.divider}>or</Divider>;
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

// ─── Sign In ──────────────────────────────────────────────────────────────────
function SignInPage({ onSwitch }: PageProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [attempted, setAttempted] = useState(false);
  const [validationError, setValidationError] = useState("");
  const { login } = useAuthAction();
  const { isPending, isError } = useAuthState();
  const { styles } = useAuthStyles();

  const handleSignIn = () => {
    setAttempted(true);
    if (!email || !password) {
      setValidationError("Please enter your email and password.");
      return;
    }
    setValidationError("");
    void login(email, password);
    redirect("/dashboard");
  };

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      handleSignIn();
    }
  };

  const canSubmit = Boolean(email && password);

  return (
    <AuthCard>
      <div className={styles.header}>
        <AuthLogo />
        <h2 className={styles.heading}>Welcome back</h2>
        <p className={styles.subtitle}>Sign in to your PromptForge account</p>
      </div>

      <SocialButton icon={<GitHubIcon />} label="Continue with GitHub" onClick={handleGitHubSignIn} />

      <AuthDivider />

      <div className={styles.formGroup}>
        <AuthInput
          icon={<MailIcon />}
          type="email"
          placeholder="Email address"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          onKeyDown={handleKeyDown}
        />
        <AuthInput
          icon={<LockIcon />}
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          onKeyDown={handleKeyDown}
          showToggle
        />
      </div>

      {attempted && isError && (
        <p className={styles.subtitle}>
          Invalid credentials. Please try again.
        </p>
      )}
      {attempted && validationError && (
        <p className={styles.subtitle}>{validationError}</p>
      )}

      <div className={styles.forgotRow}>
        <button
          type="button"
          onClick={() => onSwitch("forgot")}
          className={styles.linkBtn}
        >
          Forgot password?
        </button>
      </div>

      <Button
        block
        type="primary"
        className={styles.primaryBtn}
        onClick={handleSignIn}
        loading={isPending}
        disabled={!canSubmit}
      >
        Sign in
      </Button>

      <p className={styles.switchText}>
        Don&apos;t have an account?{" "}
        <button
          type="button"
          onClick={() => onSwitch("signup")}
          className={styles.switchBtn}
        >
          Sign up
        </button>
      </p>
    </AuthCard>
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

function SignUpPage({ onSwitch }: PageProps) {
  const [name, setName] = useState("");
  const [surname, setSurname] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [attempted, setAttempted] = useState(false);
  const [validationError, setValidationError] = useState("");
  const searchParams = useSearchParams();
  const tenantId = decodeTenantId(searchParams.get("tenant"));
  const { register } = useAuthAction();
  const { isPending, isError } = useAuthState();
  const { styles } = useAuthStyles();

  const passwordsMatch =
    confirmPassword.length > 0 && password === confirmPassword;
  const finalUserName = userName || email;
  const allChecksMet = PASSWORD_CHECKS.every(({ test }) => test(password));

  const handleRegister = () => {
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
    void register({
      name,
      surname,
      userName: finalUserName,
      emailAddress: email,
      password,
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

  return (
    <AuthCard>
      <div className={styles.header}>
        <AuthLogo />
        <h2 className={styles.heading}>Create your account</h2>
        <p className={styles.subtitle}>Start building with PromptForge</p>
      </div>

      <SocialButton icon={<GitHubIcon />} label="Sign up with GitHub" onClick={handleGitHubSignIn} />

      <AuthDivider />

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
        <button
          type="button"
          onClick={() => onSwitch("signin")}
          className={styles.switchBtn}
        >
          Sign in
        </button>
      </p>
    </AuthCard>
  );
}

// ─── Forgot Password ──────────────────────────────────────────────────────────
function ForgotPasswordPage({ onSwitch }: PageProps) {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const { styles } = useAuthStyles();

  return (
    <AuthCard>
      <button
        type="button"
        onClick={() => onSwitch("signin")}
        className={styles.backBtn}
      >
        <ArrowLeftIcon /> Back to sign in
      </button>

      {sent ? (
        <div className={styles.successWrapper}>
          <div className={styles.successIconRow}>
            <CheckCircleIcon />
          </div>
          <h2 className={styles.successHeading}>Check your email</h2>
          <p className={styles.successText}>
            We&apos;ve sent a password reset link to
            <br />
            <span className={styles.emailHighlight}>{email}</span>
          </p>
          <p className={styles.resendText}>
            Didn&apos;t receive it?{" "}
            <button
              type="button"
              onClick={() => setSent(false)}
              className={styles.switchBtn}
            >
              Try again
            </button>
          </p>
        </div>
      ) : (
        <>
          <div className={styles.forgotHeaderBlock}>
            <h2 className={styles.forgotHeading}>Reset your password</h2>
            <p className={styles.forgotDescription}>
              Enter the email address linked to your account and we&apos;ll send
              you a reset link.
            </p>
          </div>

          <AuthInput
            icon={<MailIcon />}
            type="email"
            placeholder="Email address"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />

          <Button
            block
            type="primary"
            onClick={() => email && setSent(true)}
            className={
              email ? styles.forgotSubmitBtn : styles.forgotSubmitBtnDisabled
            }
          >
            Send reset link
          </Button>
        </>
      )}
    </AuthCard>
  );
}

// ─── Root ─────────────────────────────────────────────────────────────────────
export default function PromptForgeAuth() {
  const [page, setPage] = useState<Page>("signin");
  const { styles } = usePageStyles();

  return (
    <div className={styles.page}>
      <div className={styles.bgOrb1} />
      <div className={styles.bgOrb2} />
      <div className={styles.gridOverlay} />

      <div key={page} className={styles.cardWrapper}>
        {page === "signin" && <SignInPage onSwitch={setPage} />}
        {page === "signup" && <SignUpPage onSwitch={setPage} />}
        {page === "forgot" && <ForgotPasswordPage onSwitch={setPage} />}
      </div>

      <div className={styles.branding}>
        <BrandingStackIcon />
        PromptForge
      </div>
    </div>
  );
}
