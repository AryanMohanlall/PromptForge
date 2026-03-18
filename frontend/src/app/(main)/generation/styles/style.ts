import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    max-width: ${token.paddingXL * 32}px;
    margin: 0 auto;
    padding-bottom: ${token.paddingXL * 2}px;
  `,
}));
