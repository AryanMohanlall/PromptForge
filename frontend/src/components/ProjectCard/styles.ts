import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  card: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 0;
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    height: 100%;
    border-radius: 20px;
    overflow: hidden;
    transition: all 0.2s ease;
    box-shadow: 0 0 80px rgba(45,212,168,0.03);

    &:hover {
      transform: translateY(-2px);
      border-color: rgba(255,255,255,0.15);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }
  `,
  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 12px;
    padding: 16px 20px;
    background: linear-gradient(135deg, rgba(45,212,168,0.1), rgba(32,196,154,0.1));
    color: #ffffff;
    font-weight: 600;
    font-size: 14px;
  `,
  title: css`
    font-size: 16px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  meta: css`
    font-size: 12px;
    color: #8b95a2;
    margin: 4px 0 0;
  `,
  statusBadge: css`
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 500;
    border: 1px solid rgba(255,255,255,0.1);
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    border-radius: 8px;
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
  `,
  statusDraft: css`
    color: #8b95a2;
    border-color: rgba(255,255,255,0.1);
    background: rgba(12,18,28,0.7);
  `,
  statusGenerating: css`
    color: #faad14;
    border-color: rgba(250,173,20,0.3);
    background: rgba(250,173,20,0.1);
  `,
  statusGenerated: css`
    color: #2dd4a8;
    border-color: rgba(45,212,168,0.3);
    background: rgba(45,212,168,0.1);
  `,
  statusDeploying: css`
    color: #faad14;
    border-color: rgba(250,173,20,0.3);
    background: rgba(250,173,20,0.1);
  `,
  statusLive: css`
    color: #52c41a;
    border-color: rgba(82,196,26,0.3);
    background: rgba(82,196,26,0.1);
  `,
  statusFailed: css`
    color: #ff4d4f;
    border-color: rgba(255,77,79,0.3);
    background: rgba(255,77,79,0.1);
  `,
  body: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 20px;
  `,
  updatedAt: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0;
  `,
  url: css`
    font-size: 14px;
    color: #2dd4a8;
    text-decoration: none;
    transition: opacity 0.2s ease;

    &:hover {
      opacity: 0.8;
    }
  `,
  urlMuted: css`
    font-size: 14px;
    color: #8b95a2;
  `,
  footer: css`
    margin-top: auto;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 16px 20px;
    border-top: 1px solid rgba(255,255,255,0.06);
  `,
  footerActions: css`
    display: flex;
    align-items: center;
    gap: 12px;
  `,
  deleteButton: css`
    padding: 8px 16px;
    border: 1px solid rgba(255,77,79,0.3);
    background: rgba(255,77,79,0.1);
    color: #ff4d4f;
    font-size: 12px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 8px;

    &:hover:not(:disabled) {
      background: rgba(255,77,79,0.2);
      border-color: rgba(255,77,79,0.5);
      box-shadow: 0 4px 24px rgba(255,77,79,0.35);
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.5;
    }
  `,
  viewButton: css`
    padding: 8px 16px;
    border: none;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-size: 12px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 8px;
    letter-spacing: -0.2px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }

    &:active {
      transform: translateY(0);
    }
  `,
  claimButton: css`
    padding: 8px 16px;
    border: 1px solid rgba(45,212,168,0.3);
    background: rgba(45,212,168,0.1);
    color: #2dd4a8;
    font-size: 12px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 8px;

    &:hover {
      background: rgba(45,212,168,0.2);
      border-color: rgba(45,212,168,0.5);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  details: css`
    font-size: 14px;
    color: #8b95a2;
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));