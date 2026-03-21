import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  layout: css`
    --sidebar-width: 240px;
    min-height: 100vh;
    background: #0c121a;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  content: css`
    margin-left: var(--sidebar-width, 240px);
    min-height: 100vh;
    padding: 24px;
    background: #0c121a;
    color: #ffffff;
  `,
}));