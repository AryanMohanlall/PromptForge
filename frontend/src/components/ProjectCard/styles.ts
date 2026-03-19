import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
    padding: ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    box-shadow: ${token.boxShadowSecondary};
    height: 100%;
  `,
  header: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: ${token.marginSM}px;
  `,
  title: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM}px;
  `,
  meta: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  statusBadge: css`
    padding: ${token.paddingSM / 2}px ${token.paddingSM}px;
    border-radius: ${token.borderRadiusLG}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    text-transform: uppercase;
    letter-spacing: 0.04em;
  `,
  statusDraft: css`
    color: ${token.colorTextSecondary};
    background: ${token.colorFillSecondary};
  `,
  statusGenerating: css`
    color: ${token.colorWarning};
    background: ${token.colorWarningBg};
  `,
  statusGenerated: css`
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
  `,
  statusDeploying: css`
    color: ${token.colorWarning};
    background: ${token.colorWarningBg};
  `,
  statusLive: css`
    color: ${token.colorSuccess};
    background: ${token.colorSuccessBg};
  `,
  statusFailed: css`
    color: ${token.colorError};
    background: ${token.colorErrorBg};
  `,
  body: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  updatedAt: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  url: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorPrimary};
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  `,
  urlMuted: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  footer: css`
    margin-top: auto;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${token.marginSM}px;
  `,
  footerActions: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,
  deleteButton: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorErrorBorder};
    background: ${token.colorErrorBg};
    color: ${token.colorError};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: filter 0.2s ease;

    &:hover:not(:disabled) {
      filter: brightness(0.98);
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.7;
    }
  `,
  viewButton: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease;

    &:hover {
      filter: brightness(1.05);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  claimButton: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid #000;
    background: #000;
    color: #fff;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease;

    &:hover {
      filter: brightness(1.2);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  details: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
