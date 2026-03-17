"use client";

import { Button, Card, Input, Tag } from "antd";
import {
  CheckCircleOutlined,
  LoadingOutlined,
  MinusCircleOutlined,
} from "@ant-design/icons";
import { useStyles } from "./styles/style";
import Link from "next/link";

const features = [
  {
    title: "Prompt-Driven",
    desc: "Describe your app in plain English. PromptForge interprets requirements and scaffolds the stack.",
  },
  {
    title: "GitHub Integration",
    desc: "Automatically creates repositories, commits generated code, and manages branches.",
  },
  {
    title: "Auto Deploy",
    desc: "Triggers deployment pipelines and returns a live URL from idea to production in minutes.",
  },
  {
    title: "Iterative Refinement",
    desc: "Refine your prompt, regenerate, and compare versions. Every iteration is tracked.",
  },
];

const pipelineSteps = [
  { label: "Parse requirements", status: "done" },
  { label: "Generate frontend", status: "done" },
  { label: "Generate backend & API", status: "running" },
  { label: "Create GitHub repository", status: "pending" },
  { label: "Push code & commit", status: "pending" },
  { label: "Deploy to production", status: "pending" },
 ] as const;

export default function PromptForgeLanding() {
  const { styles } = useStyles();
  const featureCardClasses = [
    styles.featureCard0,
    styles.featureCard1,
    styles.featureCard2,
    styles.featureCard3,
  ];
  const pipelineStepClasses = {
    done: styles.stepdone,
    running: styles.steprunning,
    pending: styles.steppending,
  };

  return (
    <div className={`${styles.page} ${styles.pageMounted}`}>
      <div className={styles.bgOrbPrimary} />
      <div className={styles.bgOrbSecondary} />

      <nav className={styles.nav}>
        <div className={styles.logo}>
          <img
            src="/logo.svg"
            alt="PromptForge logo"
            className={styles.logoImage}
          />
          PromptForge
        </div>
        <div className={styles.navActions}>
          <Button type="text" className={styles.signInBtn}>
            <Link href="/auth">Sign in</Link>
          </Button>
          <Button type="primary" className={styles.ctaBtn}>
            <Link href="/auth">Get Started</Link>
          </Button>
        </div>
      </nav>

      <section className={styles.hero}>
        <Tag className={styles.heroPill}>prompt → code → deploy</Tag>
        <h1 className={styles.heroTitle}>
          From idea to <span className={styles.heroHighlight}>live app</span> in
          minutes
        </h1>
        <p className={styles.heroSubtitle}>
          Describe your application in plain English. PromptForge generates the
          full stack, pushes to GitHub, and deploys with a live URL.
        </p>

        <Card className={styles.promptCard}>
          <Input.TextArea
            className={styles.promptInput}
            placeholder="Generate a project management tool with kanban boards and team collaboration..."
          />
          <div className={styles.promptFooter}>
            <span className={styles.promptLabel}>Describe your app</span>
            <Button type="primary" className={styles.generateBtn}>
              Generate App
            </Button>
          </div>
        </Card>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>
          From prompt to{" "}
          <span className={styles.sectionHighlight}>production</span>
        </h2>
        <p className={styles.sectionSubtitle}>
          PromptForge orchestrates every step, from generation to deployment, so
          you can focus on your idea.
        </p>
        <div className={styles.featureGrid}>
          {features.map((feature, index) => (
            <Card
              key={feature.title}
              className={`${styles.featureCard} ${featureCardClasses[index] ?? ""}`}
            >
              <div className={styles.featureIcon}>
                <span className={styles.featureIconDot} />
              </div>
              <div className={styles.featureTitle}>{feature.title}</div>
              <p className={styles.featureDesc}>{feature.desc}</p>
            </Card>
          ))}
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>
          Intelligent <span className={styles.sectionHighlight}>pipeline</span>
        </h2>
        <p className={styles.sectionSubtitle}>
          Watch your app come to life step by step, from parsing your prompt to
          deploying a live URL.
        </p>
        <div className={styles.pipelineWrap}>
          <Card className={styles.pipelineCard}>
            <div className={styles.pipelineHeader}>
              <div className={styles.pipelineDots}>
                <span className={styles.pipelineDot} />
                <span className={styles.pipelineDot} />
                <span className={styles.pipelineDot} />
              </div>
              <span className={styles.pipelineTitle}>
                promptforge — generation pipeline
              </span>
            </div>
            <div className={styles.pipelineBody}>
              {pipelineSteps.map((step) => (
                <div
                  key={step.label}
                  className={`${styles.pipelineStep} ${pipelineStepClasses[step.status]}`}
                >
                  {step.status === "done" && (
                    <CheckCircleOutlined className={styles.stepIconDone} />
                  )}
                  {step.status === "running" && (
                    <LoadingOutlined spin className={styles.stepIconRunning} />
                  )}
                  {step.status === "pending" && (
                    <MinusCircleOutlined className={styles.stepIconPending} />
                  )}
                  <span className={styles.stepLabel}>{step.label}</span>
                  <span className={styles.stepRight}>
                    {step.status === "done" && "✓"}
                    {step.status === "running" && "running"}
                  </span>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </section>

      <footer className={styles.footer}>
        <div className={styles.footerLogo}>
          <img
            src="/logo.svg"
            alt="PromptForge logo"
            className={styles.logoImageSmall}
          />
          PromptForge
        </div>
        <span className={styles.footerText}>Built for builders.</span>
      </footer>
    </div>
  );
}
