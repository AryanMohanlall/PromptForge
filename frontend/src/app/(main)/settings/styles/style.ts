import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    padding: ${token.paddingXL * 2}px ${token.paddingXL}px;
    display: flex;
    flex-direction: column;
    gap: ${token.paddingXL * 1.5}px;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    gap: ${token.margin}px;
  `,
  title: css`
    margin: 0;
    color: ${token.colorText};
  `,
  subtitle: css`
    margin: 0;
    color: ${token.colorTextSecondary};
    max-width: 720px;
  `,
  grid: css`
    display: grid;
    gap: ${token.paddingXL}px;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  `,
  card: css`
    border-radius: ${token.borderRadiusLG}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    box-shadow: ${token.boxShadowSecondary};
  `,
  settingRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: ${token.paddingLG}px 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.08);

    &:last-child {
      border-bottom: none;
    }
  `,
  settingLabel: css`
    color: ${token.colorTextSecondary};
    font-weight: ${token.fontWeightStrong};
  `,
}));
