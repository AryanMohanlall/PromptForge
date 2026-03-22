import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: 1200px;
    margin: 0 auto;
    padding: ${token.paddingLG}px;
    font-family: "Inter", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  `,

  header: css`
    margin-bottom: ${token.marginLG}px;
  `,

  backLink: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginXS}px;
    padding: ${token.paddingXS}px ${token.paddingSM}px;
    margin-bottom: ${token.marginMD}px;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
    font-weight: 500;
    border-radius: ${token.borderRadius}px;
    transition: all 0.2s ease;

    &:hover {
      color: ${token.colorText};
      background: ${token.colorBgTextHover};
    }
  `,

  titleRow: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: ${token.marginMD}px;
    flex-wrap: wrap;
  `,

  projectName: css`
    font-size: ${token.fontSizeHeading2}px;
    font-weight: 700;
    color: ${token.colorText};
    margin: 0;
    letter-spacing: -0.5px;
  `,

  statusTag: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: 600;
    text-transform: capitalize;
    border-radius: ${token.borderRadiusLG}px;
    padding: ${token.paddingXXS}px ${token.paddingSM}px;
  `,

  statusMessage: css`
    margin-top: ${token.marginSM}px;
    padding: ${token.paddingSM}px;
    background: ${token.colorWarningBg};
    border: 1px solid ${token.colorWarningBorder};
    border-radius: ${token.borderRadius}px;
    color: ${token.colorWarningText};
    font-size: ${token.fontSizeSM}px;
  `,

  content: css`
    display: grid;
    grid-template-columns: 1fr;
    gap: ${token.marginLG}px;

    @media (min-width: 992px) {
      grid-template-columns: 2fr 1fr;
    }
  `,

  mainColumn: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,

  sideColumn: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,

  card: css`
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorderSecondary};
    border-radius: ${token.borderRadiusLG}px;
    overflow: hidden;
    box-shadow: ${token.boxShadowTertiary};
  `,

  cardHeader: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    background: linear-gradient(
      135deg,
      rgba(45, 212, 168, 0.08),
      rgba(32, 196, 154, 0.04)
    );
    border-bottom: 1px solid ${token.colorBorderSecondary};
  `,

  cardIcon: css`
    font-size: ${token.fontSizeLG}px;
    color: ${token.colorPrimary};
  `,

  cardTitle: css`
    font-size: ${token.fontSize}px;
    font-weight: 600;
    color: ${token.colorText};
  `,

  cardBody: css`
    padding: ${token.paddingLG}px;
  `,

  detailGrid: css`
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: ${token.marginMD}px;

    @media (min-width: 768px) {
      grid-template-columns: repeat(3, 1fr);
    }
  `,

  detailItem: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXXS}px;
  `,

  detailLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    font-weight: 500;
  `,

  detailValue: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorText};
    font-weight: 600;
  `,

  promptText: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorText};
    line-height: 1.7;
    margin: 0;
    white-space: pre-wrap;
  `,

  deploymentItem: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXS}px;
    padding-bottom: ${token.paddingSM}px;
  `,

  deploymentHeader: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    flex-wrap: wrap;
  `,

  deploymentEnv: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: 600;
    color: ${token.colorText};
  `,

  deploymentTarget: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  deploymentUrl: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginXXS}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorPrimary};
    text-decoration: none;
    transition: color 0.2s ease;

    &:hover {
      color: ${token.colorPrimaryHover};
      text-decoration: underline;
    }
  `,

  deploymentTime: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  deploymentError: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorError};
    padding: ${token.paddingXS}px ${token.paddingSM}px;
    background: ${token.colorErrorBg};
    border-radius: ${token.borderRadius}px;
    border: 1px solid ${token.colorErrorBorder};
  `,

  repoInfo: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginMD}px;
  `,

  repoName: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,

  repoIcon: css`
    font-size: ${token.fontSizeLG}px;
    color: ${token.colorTextSecondary};
  `,

  repoLink: css`
    font-size: ${token.fontSize}px;
    font-weight: 600;
    color: ${token.colorPrimary};
    text-decoration: none;
    transition: color 0.2s ease;

    &:hover {
      color: ${token.colorPrimaryHover};
      text-decoration: underline;
    }
  `,

  repoDetails: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,

  repoDetailRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: ${token.paddingXS}px 0;
    border-bottom: 1px solid ${token.colorBorderSecondary};

    &:last-child {
      border-bottom: none;
    }
  `,

  repoLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  repoValue: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: 600;
    color: ${token.colorText};
  `,

  quickInfo: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,

  quickInfoItem: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: ${token.paddingXS}px 0;
    border-bottom: 1px solid ${token.colorBorderSecondary};

    &:last-child {
      border-bottom: none;
    }
  `,

  quickInfoLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  quickInfoValue: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: 600;
    color: ${token.colorText};
  `,

  loadingState: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 400px;
    gap: ${token.marginMD}px;
  `,

  loadingText: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
  `,

  loadingInline: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingMD}px;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
  `,

  errorState: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 400px;
    text-align: center;
    gap: ${token.marginMD}px;
  `,

  errorTitle: css`
    font-size: ${token.fontSizeHeading4}px;
    font-weight: 600;
    color: ${token.colorText};
    margin: 0;
  `,

  errorDescription: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    max-width: 400px;
    margin: 0;
  `,

  backButton: css`
    margin-top: ${token.marginMD}px;
  `,

  emptyState: css`
    padding: ${token.paddingXL}px !important;
  `,
}));