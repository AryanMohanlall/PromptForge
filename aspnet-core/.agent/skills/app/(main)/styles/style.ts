import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  layout: css`
    --sidebar-width: ${token.paddingXL * 6.5}px;
    min-height: 100vh;
    background: ${token.colorBgLayout};
  `,
  content: css`
    margin-left: var(--sidebar-width, ${token.paddingXL * 6.5}px);
    min-height: 100vh;
  `,
}));
