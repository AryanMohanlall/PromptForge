import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    max-width: 1200px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 24px;
  `,
  title: css`
    font-size: 24px;
    font-weight: 600;
    color: #ffffff !important;
    margin: 0 !important;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  subtitle: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0 !important;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(1, minmax(0, 1fr));
    gap: 16px;

    @media (min-width: 768px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  card: css`
    border: 1px solid rgba(255, 255, 255, 0.06) !important;
    background: rgba(12, 18, 28, 0.7) !important;
    backdrop-filter: blur(20px);
    border-radius: 20px !important;
    box-shadow: 0 0 80px rgba(45, 212, 168, 0.03) !important;
    color: #ffffff;

    .ant-card-head {
      border-bottom: 1px solid rgba(255, 255, 255, 0.06);
      color: #ffffff;
      font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
      font-weight: 600;
      font-size: 14px;
      background: linear-gradient(135deg, rgba(45, 212, 168, 0.1), rgba(32, 196, 154, 0.1));
      border-radius: 20px 20px 0 0;
    }

    .ant-card-body {
      padding: 16px;
    }
  `,
  settingRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.04);

    &:last-child {
      border-bottom: none;
    }
  `,
  settingLabel: css`
    font-size: 14px;
    font-weight: 500;
    color: #c9d1db;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
}));