import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    display: flex;
    flex-direction: column;
    gap: 32px;
    max-width: 1000px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: 16px;
    margin-bottom: 8px;
  `,
  headerIcon: css`
    width: 56px;
    height: 56px;
    background: rgba(45, 212, 168, 0.1);
    color: #2dd4a8;
    border-radius: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid rgba(45, 212, 168, 0.2);
  `,
  title: css`
    font-size: 32px;
    font-weight: 700;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.02em;
  `,
  subtitle: css`
    font-size: 16px;
    color: #8b95a2;
    margin: 0;
    line-height: 1.6;
    max-width: 700px;
  `,

  // ── Loading (generating) state ─────────────────────────────────────────────
  loadingCard: css`
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 24px;
    padding: 48px;
    display: flex;
    flex-direction: column;
    gap: 40px;
    box-shadow: 0 16px 48px rgba(0, 0, 0, 0.4);
  `,
  loadingStageRow: css`
    display: flex;
    align-items: center;
    gap: 32px;
  `,
  loadingStageCopy: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,
  loadingEyebrow: css`
    font-size: 12px;
    font-weight: 600;
    color: #2dd4a8;
    text-transform: uppercase;
    letter-spacing: 0.1em;
  `,
  loadingTitle: css`
    font-size: 24px;
    font-weight: 600;
    color: #ffffff;
  `,
  loadingText: css`
    font-size: 14px;
    color: #8b95a2;
  `,
  loadingTimeline: css`
    display: flex;
    gap: 12px;
  `,
  loadingStagePill: css`
    flex: 1;
    height: 6px;
    border-radius: 3px;
    transition: all 0.5s ease;
  `,
  loadingStageActive: css`
    background: #2dd4a8;
    box-shadow: 0 0 12px rgba(45, 212, 168, 0.5);
  `,
  loadingStageComplete: css`
    background: rgba(45, 212, 168, 0.3);
  `,
  loadingStagePending: css`
    background: rgba(255, 255, 255, 0.06);
  `,
  loadingPreviewGrid: css`
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 20px;
  `,
  loadingPreviewCard: css`
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 16px;
    padding: 24px;
  `,

  // ── Error / unavailable state ──────────────────────────────────────────────
  loadingWrap: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 16px;
    padding: 80px 24px;
    border: 1px solid rgba(255, 255, 255, 0.06);
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border-radius: 24px;
    text-align: center;
  `,

  // ── Summary card ───────────────────────────────────────────────────────────
  summaryCard: css`
    background: rgba(45, 212, 168, 0.05);
    border: 1px solid rgba(45, 212, 168, 0.15);
    border-radius: 20px;
    padding: 24px;
  `,
  summaryTitle: css`
    font-size: 18px;
    font-weight: 600;
    color: #ffffff;
    margin-bottom: 12px;
  `,
  summaryText: css`
    font-size: 15px;
    color: #8b95a2;
    line-height: 1.6;
    margin: 0;
  `,

  // ── Plan card ──────────────────────────────────────────────────────────────
  planCard: css`
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 24px;
    padding: 32px;
    display: flex;
    flex-direction: column;
    gap: 32px;
  `,
  planHeader: css`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  `,
  planHeaderCopy: css`
    display: flex;
    flex-direction: column;
    gap: 4px;
  `,
  planStatusBadge: css`
    padding: 6px 14px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 12px;
    color: #ffffff;
    font-size: 12px;
    font-weight: 500;
    white-space: nowrap;
  `,

  // ── Metrics ────────────────────────────────────────────────────────────────
  metricGrid: css`
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
  `,
  metricCard: css`
    display: flex;
    flex-direction: column;
    gap: 4px;
    padding: 20px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 16px;
  `,
  metricLabel: css`
    font-size: 12px;
    font-weight: 600;
    color: #5a6572;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  `,
  metricValue: css`
    font-size: 28px;
    font-weight: 700;
    color: #ffffff;
  `,
  metricDetail: css`
    font-size: 11px;
    color: #5a6572;
    line-height: 1.4;
    margin-top: 4px;
  `,

  // ── Preview grid (entities / pages / routes / packages) ───────────────────
  previewGrid: css`
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 24px;
  `,
  previewSection: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,
  previewSectionHeader: css`
    display: flex;
    align-items: center;
    gap: 12px;
  `,
  previewSectionTitle: css`
    font-size: 15px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
  `,
  previewSectionHint: css`
    font-size: 12px;
    color: #5a6572;
  `,
  previewList: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,
  previewListItem: css`
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 12px;
    padding: 12px 16px;
    display: flex;
    flex-direction: column;
    gap: 4px;
  `,
  previewItemRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
  `,
  previewItemPrimary: css`
    font-size: 14px;
    font-weight: 500;
    color: #ffffff;
  `,
  previewItemSecondary: css`
    font-size: 12px;
    color: #5a6572;
  `,
  previewBadge: css`
    font-size: 10px;
    font-weight: 600;
    color: #2dd4a8;
    background: rgba(45, 212, 168, 0.1);
    padding: 2px 8px;
    border-radius: 6px;
    white-space: nowrap;
  `,
  emptyState: css`
    font-size: 13px;
    color: #5a6572;
    font-style: italic;
    margin: 0;
  `,

  // ── README viewer ──────────────────────────────────────────────────────────
  readmeCard: css`
    background: #0d1117;
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  `,
  readmeHeader: css`
    background: #161b22;
    padding: 12px 20px;
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 13px;
    font-weight: 500;
    color: #8b95a2;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
  `,
  readmeContent: css`
    padding: 24px;
    max-height: 400px;
    overflow-y: auto;
  `,
  readmePre: css`
    margin: 0;
    font-family: 'JetBrains Mono', 'Fira Code', monospace;
    font-size: 13px;
    line-height: 1.7;
    color: #c9d1d9;
    white-space: pre-wrap;
  `,

  // ── Footer actions ─────────────────────────────────────────────────────────
  actionRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 16px;
    padding-top: 24px;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
  `,
  backButton: css`
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 24px;
    background: transparent;
    border: 1px solid rgba(255, 255, 255, 0.08);
    color: #8b95a2;
    font-size: 15px;
    font-weight: 500;
    border-radius: 12px;
    cursor: pointer;
    transition: all 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.05);
      border-color: rgba(255, 255, 255, 0.15);
      color: #ffffff;
    }
  `,
  confirmButton: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 14px 32px;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    border: none;
    color: #0c121a;
    font-size: 16px;
    font-weight: 600;
    border-radius: 14px;
    cursor: pointer;
    box-shadow: 0 8px 24px rgba(45, 212, 168, 0.25);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 12px 32px rgba(45, 212, 168, 0.4);
      filter: brightness(1.1);
    }

    &:disabled {
      background: rgba(45, 212, 168, 0.3);
      color: rgba(12, 18, 28, 0.5);
      cursor: not-allowed;
      box-shadow: none;
    }
  `,
}));