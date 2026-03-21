import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    display: flex;
    flex-direction: column;
    gap: 32px;
    max-width: 800px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  successCard: css`
    position: relative;
    background: rgba(12, 18, 28, 0.6);
    backdrop-filter: blur(24px);
    border: 1px solid rgba(45, 212, 168, 0.2);
    border-radius: 32px;
    padding: 60px 40px;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    overflow: hidden;
    box-shadow: 0 24px 60px rgba(0, 0, 0, 0.5), 0 0 100px rgba(45, 212, 168, 0.05);
  `,
  successOverlay: css`
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle at center, rgba(45, 212, 168, 0.1) 0%, transparent 70%);
    pointer-events: none;
  `,
  successContent: css`
    position: relative;
    z-index: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
    width: 100%;
  `,
  successIcon: css`
    width: 80px;
    height: 80px;
    background: #2dd4a8;
    color: #0c121a;
    border-radius: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 12px 32px rgba(45, 212, 168, 0.4);
    margin-bottom: 8px;
  `,
  iconLarge: css`
    width: 40px;
    height: 40px;
  `,
  successTitle: css`
    font-size: 32px;
    font-weight: 800;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.02em;
  `,
  successSubtitle: css`
    font-size: 16px;
    color: #8b95a2;
    line-height: 1.6;
    max-width: 500px;
    margin: 0;
  `,
  successActions: css`
    display: flex;
    gap: 16px;
    margin-top: 16px;
    width: 100%;
    justify-content: center;
  `,
  primaryButton: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 16px 32px;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    border: none;
    color: #0c121a;
    font-size: 16px;
    font-weight: 700;
    border-radius: 16px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 8px 24px rgba(45, 212, 168, 0.25);

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 12px 32px rgba(45, 212, 168, 0.4);
      filter: brightness(1.1);
    }
  `,
  ghostButton: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 16px 32px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.08);
    color: #ffffff;
    font-size: 16px;
    font-weight: 600;
    border-radius: 16px;
    cursor: pointer;
    transition: all 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.1);
      border-color: rgba(255, 255, 255, 0.2);
    }
  `,
  failedCard: css`
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 77, 79, 0.2);
    border-radius: 24px;
    padding: 40px;
    display: flex;
    flex-direction: column;
    gap: 32px;
  `,
  failedHeader: css`
    display: flex;
    align-items: center;
    gap: 24px;
  `,
  failedIcon: css`
    width: 64px;
    height: 64px;
    background: rgba(255, 77, 79, 0.1);
    color: #ff4d4f;
    border-radius: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid rgba(255, 77, 79, 0.2);
  `,
  failedTitle: css`
    font-size: 24px;
    font-weight: 700;
    color: #ffffff;
    margin: 0 0 4px 0;
  `,
  failedSubtitle: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0;
  `,
  failureList: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
    background: rgba(0, 0, 0, 0.2);
    border-radius: 16px;
    padding: 20px;
    border: 1px solid rgba(255, 255, 255, 0.04);
  `,
  failureItem: css`
    display: flex;
    align-items: flex-start;
    gap: 12px;
    font-size: 14px;
    color: #ff4d4f;
    line-height: 1.5;

    strong {
      color: #ffffff;
      margin-right: 4px;
    }
  `,
  failedActions: css`
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
    padding-top: 16px;
    border-top: 1px solid rgba(255, 255, 255, 0.06);
  `,
  repairButton: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 24px;
    background: linear-gradient(135deg, #3b82f6, #2563eb);
    border: none;
    color: #ffffff;
    font-size: 14px;
    font-weight: 600;
    border-radius: 12px;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);

    &:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 8px 24px rgba(37, 99, 235, 0.3);
      filter: brightness(1.1);
    }
  `,
  secondaryButton: css`
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 24px;
    background: transparent;
    border: 1px solid rgba(255, 255, 255, 0.08);
    color: #8b95a2;
    font-size: 14px;
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
}));