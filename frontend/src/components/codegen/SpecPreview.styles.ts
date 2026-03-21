import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  section: css`
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    overflow: hidden;
    box-shadow: 0 0 80px rgba(45,212,168,0.03);
  `,
  sectionHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 16px;
    background: linear-gradient(135deg, rgba(45,212,168,0.1), rgba(32,196,154,0.1));
    color: #ffffff;
    font-weight: 600;
    font-size: 14px;
  `,
  sectionTitle: css`
    margin: 0;
    font-size: 14px;
    font-weight: 600;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  sectionBody: css`
    padding: 16px;
  `,
  previewContainer: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,
  previewItem: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
  `,
  previewLabel: css`
    font-size: 12px;
    font-weight: 500;
    color: #8b95a2;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  previewValue: css`
    font-size: 14px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    padding: 12px;
    border-radius: 12px;
    backdrop-filter: blur(20px);
    white-space: pre-wrap;
    overflow-x: auto;
  `,
  actions: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,
  actionButton: css`
    padding: 10px 20px;
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    color: #ffffff;
    font-size: 14px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    backdrop-filter: blur(20px);

    &:hover {
      background: rgba(20,30,42,0.8);
      border-color: rgba(255,255,255,0.15);
    }
  `,
  primaryAction: css`
    padding: 10px 20px;
    border: none;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-size: 14px;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    letter-spacing: -0.2px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));