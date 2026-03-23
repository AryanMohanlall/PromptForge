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
      opacity: 0.6;
      cursor: not-allowed;
      background: rgba(12, 18, 28, 0.2);
    }
  `,
  textareaAnalyzed: css`
    min-height: 120px !important;
    border-color: rgba(45, 212, 168, 0.2);
    background: rgba(45, 212, 168, 0.03);
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
    margin-top: 48px;
    padding: 36px;
    background: rgba(12, 18, 28, 0.4);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 28px;
    backdrop-filter: blur(24px);
    box-shadow: 0 24px 64px rgba(0, 0, 0, 0.5);
    position: relative;
    overflow: hidden;

    &::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 1px;
      background: linear-gradient(90deg, transparent, rgba(45, 212, 168, 0.3), transparent);
    }
  `,
  resultTitle: css`
    font-size: 24px;
    font-weight: 700;
    color: #ffffff;
    margin: 0 0 8px 0;
    display: flex;
    align-items: center;
    gap: 12px;
    letter-spacing: -0.02em;
  `,
  resultSubtitle: css`
    font-size: 15px;
    color: #8b95a2;
    margin: 0 0 32px 0;
    line-height: 1.6;
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
    transition: all 0.2s ease;

    &:hover {
      background: rgba(45, 212, 168, 0.12);
      border-color: rgba(45, 212, 168, 0.5);
    }

    strong {
      color: #ffffff;
      font-weight: 600;
    }
  `,
  projectNameInput: css`
    .ant-input {
      color: #ffffff !important;
      font-weight: 600 !important;
      font-size: 15px !important;
      background: transparent !important;
      padding: 4px 8px !important;
      border-radius: 6px !important;
      transition: all 0.2s ease !important;

      &:hover {
        background: rgba(255, 255, 255, 0.05) !important;
      }

      &:focus {
        background: rgba(255, 255, 255, 0.1) !important;
        box-shadow: 0 0 0 2px rgba(45, 212, 168, 0.2) !important;
      }
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
    gap: 10px;
    margin-bottom: 20px;

    .ant-tag {
      padding: 8px 16px;
      border-radius: 12px;
      font-weight: 600;
      font-size: 14px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      background: rgba(255, 255, 255, 0.05);
      color: #ffffff;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      margin: 0;
      display: flex;
      align-items: center;
      gap: 6px;

      &:hover {
        background: rgba(255, 255, 255, 0.1);
        border-color: rgba(255, 255, 255, 0.2);
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
      }

      .anticon {
        color: #8b95a2;
        transition: all 0.2s ease;
        &:hover {
          color: #ff4d4f;
        }
      }
    }
  `,
  tagInput: css`
    .ant-input {
      width: 120px !important;
      font-size: 14px !important;
      padding: 4px 8px !important;
      background: rgba(45, 212, 168, 0.1) !important;
      border: 1px solid rgba(45, 212, 168, 0.4) !important;
      color: #ffffff !important;
      border-radius: 10px !important;
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
  visibilityGrid: css`
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
    margin-top: 12px;
  `,
  visibilityCard: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 16px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 16px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    text-align: left;
    position: relative;
    overflow: hidden;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      border-color: rgba(255, 255, 255, 0.15);
      transform: translateY(-2px);
    }
  `,
  visibilityCardSelected: css`
    background: rgba(45, 212, 168, 0.08) !important;
    border: 1px solid rgba(45, 212, 168, 0.3) !important;
    box-shadow: 0 8px 24px rgba(45, 212, 168, 0.1);

    &::after {
      content: '';
      position: absolute;
      top: 0;
      right: 0;
      width: 0;
      height: 0;
      border-style: solid;
      border-width: 0 32px 32px 0;
      border-color: transparent rgba(45, 212, 168, 0.4) transparent transparent;
    }
  `,
  visibilityIcon: css`
    width: 24px;
    height: 24px;
    color: #8b95a2;
    margin-bottom: 4px;
    transition: all 0.3s ease;
  `,
  visibilityIconSelected: css`
    color: #2dd4a8;
  `,
  visibilityLabel: css`
    font-size: 15px;
    font-weight: 600;
    color: #ffffff;
  `,
  visibilityDesc: css`
    font-size: 13px;
    color: #8b95a2;
    line-height: 1.4;
  `,
  nextRow: css`
    display: flex;
    justify-content: flex-end;
    margin-top: 40px;
    padding-top: 32px;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
  `,
  dropZone: css`
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 28px 24px;
    border: 2px dashed rgba(255, 255, 255, 0.1);
    border-radius: 20px;
    background: rgba(12, 18, 28, 0.25);
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    &:hover {
      border-color: rgba(45, 212, 168, 0.3);
      background: rgba(45, 212, 168, 0.04);
    }
  `,
  dropZoneActive: css`
    border-color: rgba(45, 212, 168, 0.5) !important;
    background: rgba(45, 212, 168, 0.08) !important;
    box-shadow: 0 0 0 4px rgba(45, 212, 168, 0.1);
  `,
  dropZoneContent: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
  `,
  dropZoneIcon: css`
    width: 32px;
    height: 32px;
    color: #5a6572;
    transition: color 0.2s ease;
  `,
  dropZoneText: css`
    font-size: 14px;
    color: #8b95a2;
    text-align: center;

    strong {
      color: #ffffff;
      font-weight: 600;
    }
  `,
  dropZoneHint: css`
    font-size: 12px;
    color: #4b5563;
    margin-top: 2px;
  `,
  removeFileButton: css`
    display: flex;
    align-items: center;
    justify-content: center;
    width: 24px;
    height: 24px;
    border: none;
    background: rgba(255, 77, 79, 0.15);
    color: #ff4d4f;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s ease;
    margin-top: 4px;

    &:hover {
      background: rgba(255, 77, 79, 0.3);
    }
  `,
}));