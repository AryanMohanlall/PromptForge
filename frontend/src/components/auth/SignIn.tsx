"use client";

import { useState } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { Button, Input } from "antd";
import {
  usePageStyles,
  useCardStyles,
  useInputStyles,
  useAuthStyles,
} from "./styles/style";
import {
  MailIcon,
  LockIcon,
  ArrowLeftIcon,
  CheckCircleIcon,
  BrandingStackIcon,
} from "./icons";
import { useAuthAction, useAuthState } from "@/providers/auth-provider";

// ─── Types ────────────────────────────────────────────────────────────────────

type Page = "signin" | "forgot";

interface InputProps {
  icon: React.ReactNode;
  type?: string;
  placeholder?: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onKeyDown?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
  showToggle?: boolean;
  confirmState?: "match" | "mismatch" | undefined;
}

interface PageProps {
  onSwitch: (page: Page) => void;
}

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
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [attempted, setAttempted] = useState(false);
  const [validationError, setValidationError] = useState("");
  const { login } = useAuthAction();
  const { isPending, isError } = useAuthState();
  const { styles } = useAuthStyles();

  const handleSignIn = async () => {
    setAttempted(true);
    if (!email || !password) {
      setValidationError("Please enter your email and password.");
      return;
    }
    setValidationError("");
    const user = await login(email, password);
    if (user) {
      router.replace("/dashboard");
    }
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
        <a href="/register" className={styles.switchBtn}>
          Sign up
        </a>
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
export default function SignIn() {
  const [page, setPage] = useState<Page>("signin");
  const { styles } = usePageStyles();

  return (
    <div className={styles.page}>
      <div className={styles.bgOrb1} />
      <div className={styles.bgOrb2} />
      <div className={styles.gridOverlay} />

      <div key={page} className={styles.cardWrapper}>
        {page === "signin" && <SignInPage onSwitch={setPage} />}
        {page === "forgot" && <ForgotPasswordPage onSwitch={setPage} />}
      </div>

      <div className={styles.branding}>
        <BrandingStackIcon />
        PromptForge
      </div>
    </div>
  );
}