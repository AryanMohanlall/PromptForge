import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    background: #070b10;
    padding: 28px 20px;
    font-family: 'Sora', sans-serif;
  `,
  inner: css`
    max-width: 1320px;
    margin: 0 auto;
  `,
  headerIcon: css`
    width: 40px;
    height: 40px;
    border-radius: ${token.borderRadiusLG}px;
    background: linear-gradient(135deg, #4f46e5, #7c3aed);
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 0 20px rgba(99,102,241,0.25);
    flex-shrink: 0;
  `,
  headerTitle: css`
    margin: 0;
    font-size: 19px;
    font-weight: 700;
    font-family: 'JetBrains Mono', monospace;
    color: #f1f5f9;
    letter-spacing: -0.03em;
  `,
  headerSubtitle: css`
    margin: 0;
    font-size: 11px;
    color: ${token.colorTextTertiary};
  `,
  card: css`
    background: rgba(255,255,255,0.02);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: ${token.borderRadiusLG}px;
    overflow: hidden;
  `,
  filterBar: css`
    padding: 12px 16px;
    display: flex;
    gap: 10px;
    align-items: center;
    flex-wrap: wrap;
    border-bottom: 1px solid rgba(255,255,255,0.05);
  `,
  tableHeader: css`
    padding: 10px 16px;
    display: grid;
    grid-template-columns: 120px 1fr 140px 100px 130px 110px;
    gap: 12px;
    border-bottom: 1px solid rgba(255,255,255,0.06);
  `,
  tableHeaderCell: css`
    font-size: 10px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: ${token.colorTextQuaternary};
  `,
  tableRow: css`
    padding: 12px 16px;
    display: grid;
    grid-template-columns: 120px 1fr 140px 100px 130px 110px;
    gap: 12px;
    align-items: center;
    border-bottom: 1px solid rgba(255,255,255,0.03);
    transition: background 0.15s;
    &:last-child {
      border-bottom: none;
    }
    &:hover {
      background: rgba(255,255,255,0.02);
    }
  `,
  deploymentName: css`
    font-size: 12px;
    font-weight: 500;
    color: #e2e8f0;
    font-family: 'JetBrains Mono', monospace;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  deploymentUrl: css`
    font-size: 11px;
    color: #6366f1;
    text-decoration: none;
    font-family: 'JetBrains Mono', monospace;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    display: block;
    &:hover { text-decoration: underline; color: #818cf8; }
  `,
  metaText: css`
    font-size: 11px;
    color: ${token.colorTextTertiary};
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  emptyState: css`
    padding: 48px 16px;
    text-align: center;
    color: ${token.colorTextQuaternary};
    font-size: 13px;
  `,
  modalLabel: css`
    font-size: 11px;
    font-weight: 600;
    color: ${token.colorTextTertiary};
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: 4px;
  `,
}));
