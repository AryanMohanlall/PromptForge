import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    max-width: 1200px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  stepSection: css`
    margin-bottom: 32px;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 24px;

    @media (min-width: 768px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  `,
  title: css`
    font-size: 24px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  subtitle: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
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
  content: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
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
  formGroup: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 16px;

    &:last-child {
      margin-bottom: 0;
    }
  `,
  label: css`
    font-size: 12px;
    font-weight: 500;
    color: #8b95a2;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  input: css`
    padding: 10px 16px;
    border: none;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    background: transparent;
    color: #ffffff;
    font-size: 14px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    transition: all 0.2s ease;

    &:focus {
      outline: none;
      border-bottom-color: rgba(45,212,168,0.3);
      box-shadow: 0 2px 0 0 rgba(45,212,168,0.2);
    }

    &::placeholder {
      color: #5a6572;
    }
  `,
  textarea: css`
    padding: 10px 16px;
    border: none;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    background: transparent;
    color: #ffffff;
    font-size: 14px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    min-height: 100px;
    resize: vertical;
    transition: all 0.2s ease;

    &:focus {
      outline: none;
      border-bottom-color: rgba(45,212,168,0.3);
      box-shadow: 0 2px 0 0 rgba(45,212,168,0.2);
    }

    &::placeholder {
      color: #5a6572;
    }
  `,
  select: css`
    padding: 10px 16px;
    border: none;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    background: transparent;
    color: #ffffff;
    font-size: 14px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;

    &:focus {
      outline: none;
      border-bottom-color: rgba(45,212,168,0.3);
      box-shadow: 0 2px 0 0 rgba(45,212,168,0.2);
    }
  `,
  checkbox: css`
    display: flex;
    align-items: center;
    gap: 12px;
    cursor: pointer;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    font-size: 14px;
    color: #ffffff;
  `,
  checkboxInput: css`
    width: 20px;
    height: 20px;
    border: 1px solid rgba(255,255,255,0.06);
    background: transparent;
    cursor: pointer;
    accent-color: #2dd4a8;
    border-radius: 4px;
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(1, minmax(0, 1fr));
    gap: 16px;

    @media (min-width: 768px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));