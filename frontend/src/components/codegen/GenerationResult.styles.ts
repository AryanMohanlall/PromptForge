import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 28}px;
    margin: 0 auto;
  `,
  successCard: css`
    position: relative;
    overflow: hidden;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    padding: ${token.paddingXL * 1.5}px;
    background: linear-gradient(135deg, ${token.colorSuccess} 0%, ${token.colorPrimary} 100%);
    color: ${token.colorBgContainer};
    box-shadow: ${token.boxShadow};
  `,
  successOverlay: css`
    position: absolute;
    inset: 0;
    background: radial-gradient(
      circle at top right,
      ${token.colorFillSecondary} 0%,
      transparent 55%
    );
    opacity: 0.4;
  `,
  successContent: css`
    position: relative;
    z-index: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
    gap: ${token.marginLG}px;
  `,
  successIcon: css`
    width: ${token.controlHeightLG * 1.4}px;
    height: ${token.controlHeightLG * 1.4}px;
    border-radius: 50%;
    background: ${token.colorBgContainer};
    color: ${token.colorSuccess};
    display: inline-flex;
    align-items: center;
    justify-content: center;
  `,
  successTitle: css`
    margin: 0;
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
  `,
  successSubtitle: css`
    margin: 0;
    font-size: ${token.fontSize}px;
    opacity: 0.9;
  `,
  successActions: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
    justify-content: center;
  `,
  primaryButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorBgContainer};
    color: ${token.colorSuccess};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: transform 0.2s ease;

    &:hover {
      transform: scale(1.02);
    }
  `,
  ghostButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBgContainer};
    background: transparent;
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: background 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.1);
    }
  `,
  failedCard: css`
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorErrorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    padding: ${token.paddingXL}px;
  `,
  failedHeader: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginLG}px;
  `,
  failedIcon: css`
    width: ${token.controlHeightLG}px;
    height: ${token.controlHeightLG}px;
    border-radius: 50%;
    background: ${token.colorErrorBg};
    color: ${token.colorError};
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  `,
  failedTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0;
  `,
  failedSubtitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  failureList: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginLG}px;
  `,
  failureItem: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px;
    background: ${token.colorErrorBg};
    border-radius: ${token.borderRadiusLG}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorError};
  `,
  failedActions: css`
    display: flex;
    gap: ${token.marginSM}px;
  `,
  repairButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM * 1.5}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `,
  secondaryButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM * 1.5}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    color: ${token.colorText};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
  iconLarge: css`
    width: ${token.fontSizeXL}px;
    height: ${token.fontSizeXL}px;
  `,
}));
