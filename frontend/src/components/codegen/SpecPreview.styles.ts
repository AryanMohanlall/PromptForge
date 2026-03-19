import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 32}px;
    margin: 0 auto;
  `,
  header: css`
    text-align: center;
    margin-bottom: ${token.marginXL}px;
  `,
  title: css`
    font-size: ${token.fontSizeXL * 1.2}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM}px;
  `,
  subtitle: css`
    color: ${token.colorTextSecondary};
    margin: 0;
    line-height: 1.6;
  `,
  loadingWrap: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: ${token.paddingXL * 3}px;
    gap: ${token.marginLG}px;
  `,
  loadingText: css`
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSize}px;
  `,
  sectionStack: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,
  sectionCard: css`
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    overflow: hidden;
  `,
  sectionHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: ${token.paddingLG}px ${token.paddingXL}px;
    border-bottom: 1px solid ${token.colorBorder};
    cursor: pointer;
    transition: background 0.2s ease;

    &:hover {
      background: ${token.colorFillQuaternary};
    }
  `,
  sectionTitle: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  sectionBadge: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
    padding: 2px 10px;
    border-radius: ${token.borderRadiusLG}px;
  `,
  sectionBody: css`
    padding: ${token.paddingLG}px ${token.paddingXL}px;
  `,
  actionRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: ${token.marginXL}px;
    padding-top: ${token.paddingLG}px;
    border-top: 1px solid ${token.colorBorder};
  `,
  backButton: css`
    padding: ${token.paddingSM}px 0;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    background: transparent;
    border: none;
    cursor: pointer;

    &:hover {
      color: ${token.colorText};
    }
  `,
  confirmButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM * 1.5}px ${token.paddingXL}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
    font-size: ${token.fontSize}px;
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease;

    &:hover:not(:disabled) {
      filter: brightness(1.05);
    }

    &:active:not(:disabled) {
      transform: scale(0.98);
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `,
  chevron: css`
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
    color: ${token.colorTextSecondary};
    transition: transform 0.2s ease;
  `,
  chevronOpen: css`
    transform: rotate(180deg);
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
}));
