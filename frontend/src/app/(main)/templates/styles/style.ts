import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    max-width: 1200px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  toolbar: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 24px;

    @media (min-width: 768px) {
      flex-direction: row;
      align-items: flex-start;
      justify-content: space-between;
    }
  `,

  titleWrap: css`
    display: flex;
    flex-direction: column;
    gap: 4px;
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

  controls: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,

  search: css`
    flex: 1;
    min-width: 200px;
  `,

  stateCard: css`
    border: 1px solid rgba(255, 255, 255, 0.06) !important;
    background: rgba(12, 18, 28, 0.7) !important;
    backdrop-filter: blur(20px);
    border-radius: 20px !important;
  `,

  stateInner: css`
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 48px;
  `,

  grid: css`
    display: grid;
    grid-template-columns: repeat(1, minmax(0, 1fr));
    gap: 24px;

    @media (min-width: 768px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (min-width: 1024px) {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  `,

  templateCard: css`
    display: flex;
    flex-direction: column;
    border: 1px solid rgba(255, 255, 255, 0.06) !important;
    background: rgba(12, 18, 28, 0.7) !important;
    backdrop-filter: blur(20px);
    border-radius: 20px !important;
    box-shadow: 0 0 80px rgba(45, 212, 168, 0.03) !important;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;

    &:hover {
      border-color: rgba(45, 212, 168, 0.2) !important;
      box-shadow: 0 0 40px rgba(45, 212, 168, 0.08) !important;
    }

    .ant-card-body {
      display: flex;
      flex-direction: column;
      height: 100%;
      padding: 20px;
    }
  `,

  templateHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 8px;
    margin-bottom: 10px;
  `,

  templateName: css`
    font-size: 16px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.3px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  description: css`
    font-size: 13px;
    color: #8b95a2;
    margin: 0 0 16px;
    line-height: 1.6;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
  `,

  metaGrid: css`
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 10px;
    margin-bottom: 16px;
    padding: 12px;
    border-radius: 12px;
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.04);
  `,

  metaItem: css`
    display: flex;
    flex-direction: column;
    gap: 2px;
  `,

  metaLabel: css`
    font-size: 11px;
    font-weight: 500;
    color: #5a6572;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  metaValue: css`
    font-size: 13px;
    font-weight: 500;
    color: #c9d1db;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  tagsRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-bottom: 8px;
  `,
}));