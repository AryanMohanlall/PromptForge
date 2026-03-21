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
  header: css`
    text-align: center;
    margin-bottom: 8px;
  `,
  title: css`
    font-size: 32px;
    font-weight: 700;
    color: #ffffff;
    margin: 0 0 12px 0;
    letter-spacing: -0.02em;
  `,
  subtitle: css`
    font-size: 16px;
    color: #8b95a2;
    margin: 0;
    line-height: 1.6;
  `,
  textarea: css`
    width: 100%;
    min-height: 240px;
    padding: 24px;
    border: 1px solid rgba(255, 255, 255, 0.08);
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(12px);
    border-radius: 20px;
    color: #ffffff;
    font-size: 16px;
    font-family: inherit;
    line-height: 1.6;
    resize: none;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 32px rgba(0, 0, 0, 0.2);

    &:focus {
      outline: none;
      border-color: rgba(45, 212, 168, 0.4);
      background: rgba(12, 18, 28, 0.6);
      box-shadow: 0 0 0 4px rgba(45, 212, 168, 0.1), 0 8px 40px rgba(0, 0, 0, 0.3);
    }

    &::placeholder {
      color: #4b5563;
    }

    &:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }
  `,
  counter: css`
    font-size: 13px;
    color: #5a6572;
    text-align: right;
    margin-top: -12px;
    font-weight: 500;
  `,
  counterWarning: css`
    color: #ff4d4f;
  `,
  actionRow: css`
    display: flex;
    justify-content: center;
    margin-top: 8px;
  `,
  analyzeButton: css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    padding: 14px 28px;
    border: none;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    border-radius: 14px;
    box-shadow: 0 8px 24px rgba(45, 212, 168, 0.25);

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 12px 32px rgba(45, 212, 168, 0.4);
      filter: brightness(1.1);
    }

    &:active:not(:disabled) {
      transform: translateY(0);
    }

    &:disabled {
      background: rgba(45, 212, 168, 0.3);
      color: rgba(12, 18, 28, 0.5);
      cursor: not-allowed;
      box-shadow: none;
    }
  `,
  iconSmall: css`
    width: 18px;
    height: 18px;
  `,
  resultSection: css`
    margin-top: 40px;
    padding: 32px;
    background: rgba(12, 18, 28, 0.6);
    border: 1px solid rgba(45, 212, 168, 0.15);
    border-radius: 24px;
    backdrop-filter: blur(20px);
    box-shadow: 0 16px 48px rgba(0, 0, 0, 0.4);
  `,
  resultTitle: css`
    font-size: 20px;
    font-weight: 600;
    color: #ffffff;
    margin: 0 0 8px 0;
    display: flex;
    align-items: center;
    gap: 10px;
  `,
  resultSubtitle: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0 0 24px 0;
  `,
  projectName: css`
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 16px 20px;
    background: rgba(45, 212, 168, 0.08);
    border: 1px dashed rgba(45, 212, 168, 0.3);
    border-radius: 14px;
    color: #2dd4a8;
    font-size: 15px;
    margin-bottom: 32px;

    strong {
      color: #ffffff;
      font-weight: 600;
    }
  `,
  sectionLabel: css`
    font-size: 14px;
    font-weight: 600;
    color: #ffffff;
    margin-bottom: 12px;
    letter-spacing: 0.02em;
    text-transform: uppercase;
    color: #8b95a2;
  `,
  tagRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-bottom: 16px;

    .ant-tag {
      padding: 6px 14px;
      border-radius: 10px;
      font-weight: 500;
      border: 1px solid rgba(45, 212, 168, 0.2);
      background: rgba(45, 212, 168, 0.05);
      color: #2dd4a8;
      transition: all 0.2s ease;

      &:hover {
        background: rgba(45, 212, 168, 0.1);
        border-color: rgba(45, 212, 168, 0.4);
      }

      .anticon {
        color: #2dd4a8;
      }
    }
  `,
  addInput: css`
    display: flex;
    gap: 8px;
    margin-bottom: 24px;

    .ant-input {
      background: rgba(255, 255, 255, 0.03);
      border: 1px solid rgba(255, 255, 255, 0.08);
      color: #ffffff;
      border-radius: 10px;
      padding: 8px 12px;

      &:focus {
        border-color: rgba(45, 212, 168, 0.4);
        box-shadow: 0 0 0 2px rgba(45, 212, 168, 0.1);
      }
    }

    .ant-btn {
      border-radius: 10px;
      height: 38px;
      font-weight: 500;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #ffffff;

      &:hover {
        background: rgba(255, 255, 255, 0.1);
        border-color: rgba(255, 255, 255, 0.2);
        color: #ffffff;
      }
    }
  `,
  entityList: css`
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    margin-bottom: 16px;

    .ant-tag {
      padding: 6px 14px;
      border-radius: 10px;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #ffffff;
    }
  `,
  nextRow: css`
    display: flex;
    justify-content: flex-end;
    margin-top: 40px;
    padding-top: 32px;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
  `,
}));