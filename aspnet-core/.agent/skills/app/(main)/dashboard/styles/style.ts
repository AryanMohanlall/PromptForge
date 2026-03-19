import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    padding: ${token.paddingXL}px;
    background: ${token.colorBgLayout};
  `,
}));
