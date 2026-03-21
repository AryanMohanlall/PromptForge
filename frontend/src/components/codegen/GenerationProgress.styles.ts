import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    display: flex;
    flex-direction: column;
    gap: 32px;
    max-width: 900px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  header: css`
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 24px;
    padding: 32px;
    display: flex;
    flex-direction: column;
    gap: 24px;
    box-shadow: 0 16px 48px rgba(0, 0, 0, 0.3);
  `,
  titleRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
  `,
  title: css`
    font-size: 24px;
    font-weight: 700;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.01em;
  `,
  statusBadge: css`
    padding: 6px 14px;
    background: rgba(45, 212, 168, 0.1);
    border: 1px solid rgba(45, 212, 168, 0.2);
    border-radius: 99px;
    color: #2dd4a8;
    font-size: 13px;
    font-weight: 600;
  `,
  statusComplete: css`
    background: rgba(82, 196, 26, 0.1);
    border-color: rgba(82, 196, 26, 0.2);
    color: #52c41a;
  `,
  statusFailed: css`
    background: rgba(255, 77, 79, 0.1);
    border-color: rgba(255, 77, 79, 0.2);
    color: #ff4d4f;
  `,
  progressWrap: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,
  progressTrack: css`
    position: relative;
    height: 10px;
    background: rgba(255, 255, 255, 0.05);
    border-radius: 5px;
    overflow: hidden;
  `,
  progressFill: css`
    height: 100%;
    background: linear-gradient(90deg, #2dd4a8, #20c49a);
    border-radius: 5px;
    transition: width 0.5s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 0 15px rgba(45, 212, 168, 0.4);
  `,
  progressShimmer: css`
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: linear-gradient(
      90deg,
      transparent,
      rgba(255, 255, 255, 0.1),
      transparent
    );
    animation: shimmer 2s infinite linear;

    @keyframes shimmer {
      0% { transform: translateX(-100%); }
      100% { transform: translateX(100%); }
    }
  `,
  progressMeta: css`
    display: flex;
    justify-content: space-between;
    font-size: 14px;
    font-weight: 500;
    color: #8b95a2;
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 24px;
  `,
  card: css`
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 24px;
    padding: 24px;
    height: 400px;
    display: flex;
    flex-direction: column;
  `,
  cardTitle: css`
    font-size: 16px;
    font-weight: 600;
    color: #ffffff;
    margin-bottom: 20px;
    letter-spacing: 0.02em;
    text-transform: uppercase;
    color: #5a6572;
  `,
  validationStack: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
    overflow-y: auto;
    padding-right: 8px;

    &::-webkit-scrollbar {
      width: 4px;
    }
    &::-webkit-scrollbar-thumb {
      background: rgba(255, 255, 255, 0.05);
      border-radius: 2px;
    }
  `,
  validationItem: css`
    display: flex;
    align-items: flex-start;
    gap: 12px;
    padding: 12px;
    background: rgba(255, 255, 255, 0.02);
    border: 1px solid rgba(255, 255, 255, 0.05);
    border-radius: 12px;
  `,
  validationText: css`
    font-size: 13px;
    color: #8b95a2;
    line-height: 1.5;
  `,
  iconSmall: css`
    width: 16px;
    height: 16px;
    flex-shrink: 0;
    margin-top: 2px;
  `,
  validationPassed: css`
    color: #52c41a;
  `,
  validationFailed: css`
    color: #ff4d4f;
  `,
  validationRunning: css`
    color: #2dd4a8;
    animation: spin 2s infinite linear;
    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }
  `,
  validationPending: css`
    color: #5a6572;
  `,
  activityFeed: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
    overflow-y: auto;
    padding-right: 8px;

    &::-webkit-scrollbar {
      width: 4px;
    }
    &::-webkit-scrollbar-thumb {
      background: rgba(255, 255, 255, 0.05);
      border-radius: 2px;
    }
  `,
  activityItem: css`
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 8px 12px;
    position: relative;
  `,
  activityDot: css`
    width: 6px;
    height: 6px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 50%;
    flex-shrink: 0;
  `,
  activityDotActive: css`
    background: #2dd4a8;
    box-shadow: 0 0 8px #2dd4a8;
  `,
  activityText: css`
    font-size: 13px;
    color: #8b95a2;
  `,
}));