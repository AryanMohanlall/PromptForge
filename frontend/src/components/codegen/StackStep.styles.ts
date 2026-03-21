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
  selectionSection: css`
    display: flex;
    flex-direction: column;
    gap: 40px;
    background: rgba(12, 18, 28, 0.4);
    backdrop-filter: blur(20px);
    border: 1px solid rgba(255, 255, 255, 0.06);
    border-radius: 24px;
    padding: 40px;
    box-shadow: 0 16px 48px rgba(0, 0, 0, 0.3);
  `,
  templateSection: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
  `,
  sectionLabel: css`
    font-size: 14px;
    font-weight: 600;
    color: #8b95a2;
    letter-spacing: 0.1em;
    text-transform: uppercase;
  `,
  templateHint: css`
    font-size: 14px;
    color: #5a6572;
    margin: -8px 0 8px 0;
  `,
  templateGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 16px;
  `,
  templateCard: css`
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    padding: 24px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 18px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    text-align: left;
    overflow: hidden;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      border-color: rgba(255, 255, 255, 0.15);
      transform: translateY(-2px);
    }
  `,
  templateCardSelected: css`
    background: rgba(45, 212, 168, 0.08) !important;
    border-color: rgba(45, 212, 168, 0.4) !important;
    box-shadow: 0 0 0 1px rgba(45, 212, 168, 0.4);
  `,
  templateCardDefault: css`
    background: rgba(255, 255, 255, 0.03);
  `,
  templateCardIcon: css`
    width: 40px;
    height: 40px;
    background: rgba(45, 212, 168, 0.15);
    color: #2dd4a8;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 16px;
  `,
  templateCardName: css`
    font-size: 16px;
    font-weight: 600;
    color: #ffffff;
    margin-bottom: 4px;
  `,
  templateCardDesc: css`
    font-size: 13px;
    color: #8b95a2;
    line-height: 1.5;
  `,
  selectionCheck: css`
    position: absolute;
    top: 16px;
    right: 16px;
    color: #2dd4a8;
  `,
  divider: css`
    height: 1px;
    background: rgba(255, 255, 255, 0.08);
    margin: 8px 0;
  `,
  selectionGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 12px;
    margin-top: 12px;
  `,
  selectionCard: css`
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 16px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 14px;
    cursor: pointer;
    transition: all 0.2s ease;
    text-align: left;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      border-color: rgba(255, 255, 255, 0.15);
    }
  `,
  selectionCardSelected: css`
    background: rgba(45, 212, 168, 0.08) !important;
    border-color: rgba(45, 212, 168, 0.4) !important;
    
    .selectionLabel {
      color: #2dd4a8;
    }
  `,
  selectionCardDefault: css`
    background: rgba(255, 255, 255, 0.03);
  `,
  selectionLabel: css`
    font-size: 14px;
    font-weight: 500;
    color: #ffffff;
  `,
  recommendedBadge: css`
    position: absolute;
    top: -8px;
    right: 12px;
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 2px 8px;
    background: #2dd4a8;
    color: #0c121a;
    font-size: 10px;
    font-weight: 700;
    border-radius: 99px;
    text-transform: uppercase;
    box-shadow: 0 4px 12px rgba(45, 212, 168, 0.3);
  `,
  reasoning: css`
    font-size: 12px;
    color: #5a6572;
    font-style: italic;
    line-height: 1.4;
  `,
  actionRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 16px;
  `,
  backButton: css`
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
  nextButton: css`
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
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 8px 24px rgba(45, 212, 168, 0.25);

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
  iconSmall: css`
    width: 18px;
    height: 18px;
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));