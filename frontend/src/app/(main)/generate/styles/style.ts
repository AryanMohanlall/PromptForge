import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    max-width: ${token.paddingXL * 36}px;
    margin: 0 auto;
    padding-bottom: ${token.paddingXL * 2}px;
  `,
  stepSection: css`
    margin-bottom: ${token.marginXL * 1.5}px;
  `,
}));
