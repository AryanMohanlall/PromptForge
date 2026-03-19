import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 28}px;
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
  selectionSection: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXL}px;
    margin-bottom: ${token.marginXL}px;
  `,
  sectionLabel: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: ${token.marginSM}px;
  `,
  selectionGrid: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
  `,
  selectionCard: css`
    position: relative;
    width: ${token.paddingXL * 4.5}px;
    min-height: ${token.paddingXL * 2.5}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingSM}px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: ${token.marginSM / 2}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    background: ${token.colorBgContainer};
    transition: border-color 0.2s ease, color 0.2s ease, background 0.2s ease;
    cursor: pointer;
  `,
  selectionCardSelected: css`
    border-color: ${token.colorPrimary};
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
  `,
  selectionCardDefault: css`
    color: ${token.colorText};

    &:hover {
      border-color: ${token.colorBorderSecondary};
    }
  `,
  selectionCheck: css`
    position: absolute;
    top: ${token.paddingSM / 2}px;
    right: ${token.paddingSM / 2}px;
    color: ${token.colorPrimary};
  `,
  recommendedBadge: css`
    position: absolute;
    top: -${token.marginSM}px;
    left: 50%;
    transform: translateX(-50%);
    font-size: 10px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
    border: 1px solid ${token.colorPrimaryBorder};
    padding: 1px 8px;
    border-radius: ${token.borderRadiusLG}px;
    white-space: nowrap;
  `,
  reasoning: css`
    font-size: ${token.fontSizeSM * 0.9}px;
    color: ${token.colorTextTertiary};
    font-weight: normal;
    text-align: center;
    margin-top: 2px;
  `,
  selectionLabel: css`
    text-align: center;
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
  nextButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM * 1.5}px ${token.paddingXL}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
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
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
}));
