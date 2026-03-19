"use client";

import React from "react";
import { CheckCircle2, ExternalLink } from "lucide-react";
import { useStyles } from "./styles";
import Image from "next/image";

interface ClaimDeploymentProps {
  url: string;
  previewImage?: string;
  onClaimClick: () => void;
}

export const ClaimDeployment = ({
  url,
  previewImage = "https://placehold.co/600x400/000/fff?text=Deployment+Preview",
  onClaimClick,
}: ClaimDeploymentProps) => {
  const { styles, cx } = useStyles();

  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <div className={styles.status}>
          <CheckCircle2 size={16} />
          <span>Deployment Ready</span>
        </div>
        <h2 className={styles.title}>Claim your project</h2>
        <p className={styles.description}>
          Transfer this deployment to your Vercel account to manage domains and
          settings.
        </p>
      </div>

      <div className={cx(styles.preview, "preview")}>
        <Image
          width={400}
          height={200}
          src={previewImage}
          alt="Preview"
          className={styles.previewImage}
        />
        <div className={cx(styles.overlay, "overlay")}>
          <a
            href={url}
            target="_blank"
            rel="noreferrer"
            className={styles.viewButton}
          >
            View Live <ExternalLink size={14} />
          </a>
        </div>
      </div>

      <div className={styles.footer}>
        <button
          type="button"
          onClick={onClaimClick}
          className={styles.claimButton}
        >
          Claim Deployment
        </button>
        <p className={styles.poweredBy}>Powered by Vercel</p>
      </div>
    </div>
  );
};
