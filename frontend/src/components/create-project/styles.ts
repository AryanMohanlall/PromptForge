import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    max-width: ${token.paddingXL * 28}px;
    margin: 0 auto;
    padding-bottom: ${token.paddingXL * 2}px;
  `,
  connectWrap: css`
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: calc(100vh - ${token.paddingXL * 6}px);
  `,
  connectCard: css`
    max-width: ${token.paddingXL * 18}px;
    width: 100%;
    padding: ${token.paddingXL}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    box-shadow: ${token.boxShadowSecondary};
    text-align: center;
  `,
  connectTitle: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM}px;
  `,
  connectText: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    margin: 0 0 ${token.marginLG}px;
    line-height: 1.6;
  `,
  connectButton: css`
    width: 100%;
    padding: ${token.paddingSM * 1.5}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
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
  narrowSection: css`
    max-width: ${token.paddingXL * 21}px;
    margin: 0 auto;
  `,
  mediumSection: css`
    max-width: ${token.paddingXL * 24}px;
    margin: 0 auto;
  `,
  headerCenter: css`
    text-align: center;
    margin-bottom: ${token.marginXL}px;
  `,
  title: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM}px;
  `,
  subtitle: css`
    color: ${token.colorTextSecondary};
    margin: 0;
    line-height: 1.6;
  `,
  textarea: css`
    width: 100%;
    height: ${token.paddingXL * 6}px;
    padding: ${token.paddingLG}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    resize: none;
    font-size: ${token.fontSize}px;
    color: ${token.colorText};
    font-family: ${token.fontFamily};
  `,
  counter: css`
    margin-top: ${token.marginSM}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  suggestionRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginXL * 1.5}px;
  `,
  suggestionButton: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.paddingXL}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
    transition: color 0.2s ease, border-color 0.2s ease;

    &:hover {
      color: ${token.colorPrimary};
      border-color: ${token.colorPrimary};
    }
  `,
  actionRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
  `,
  textButton: css`
    padding: ${token.paddingSM}px 0;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    background: transparent;
    border: none;
    cursor: pointer;

    &:hover {
      color: ${token.colorText};
    }

    &:disabled {
      color: ${token.colorTextSecondary};
      cursor: not-allowed;
      opacity: 0.6;
    }
  `,
  primaryButton: css`
    padding: ${token.paddingSM * 1.5}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease, opacity 0.2s ease;

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
    margin-bottom: ${token.marginSM}px;
  `,
  selectionGrid: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
  `,
  selectionCard: css`
    position: relative;
    width: ${token.paddingXL * 4}px;
    height: ${token.paddingXL * 2.5}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingSM}px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: ${token.marginSM}px;
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
  selectionLabel: css`
    text-align: center;
  `,
  divider: css`
    height: 1px;
    width: 100%;
    background: ${token.colorBorder};
    margin: ${token.marginXL}px 0;
  `,
  toggleRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorderSecondary};
    background: ${token.colorBgContainer};
    margin-bottom: ${token.marginXL * 1.5}px;
  `,
  toggleTitle: css`
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin-bottom: ${token.marginSM / 2}px;
  `,
  toggleSubtitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  toggleButton: css`
    width: ${token.paddingXL * 1.5}px;
    height: ${token.paddingXL * 0.75}px;
    border-radius: ${token.paddingXL}px;
    border: none;
    position: relative;
    cursor: pointer;
    transition: background 0.2s ease;
  `,
  toggleOn: css`
    background: ${token.colorPrimary};
  `,
  toggleOff: css`
    background: ${token.colorBorder};
  `,
  toggleThumb: css`
    width: ${token.paddingSM}px;
    height: ${token.paddingSM}px;
    background: ${token.colorBgContainer};
    border-radius: 50%;
    position: absolute;
    top: 50%;
    left: ${token.paddingSM / 2}px;
    transform: translateY(-50%);
    transition: transform 0.2s ease;
  `,
  toggleThumbOn: css`
    transform: translate(${token.paddingXL * 0.75}px, -50%);
  `,
  reviewCard: css`
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    box-shadow: ${token.boxShadowSecondary};
    background: ${token.colorBgContainer};
    overflow: hidden;
    margin-bottom: ${token.marginXL}px;
  `,
  reviewGrid: css`
    display: grid;
    grid-template-columns: 1fr;
    border-color: ${token.colorBorder};

    @media (min-width: ${token.screenMD}px) {
      grid-template-columns: 1fr 1fr;
    }
  `,
  reviewPanel: css`
    padding: ${token.paddingXL}px;
  `,
  reviewPanelAlt: css`
    background: ${token.colorBgLayout};
  `,
  reviewTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin-bottom: ${token.marginLG}px;
  `,
  reviewSection: css`
    margin-bottom: ${token.marginLG}px;
  `,
  reviewLabelRow: css`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: ${token.marginSM}px;
  `,
  reviewLabel: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
  `,
  editButton: css`
    background: transparent;
    border: none;
    color: ${token.colorPrimary};
    cursor: pointer;
    padding: ${token.paddingSM / 2}px;

    &:hover {
      color: ${token.colorPrimaryTextActive};
    }
  `,
  reviewPrompt: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorText};
    background: ${token.colorBgLayout};
    padding: ${token.padding}px;
    border-radius: ${token.borderRadiusLG}px;
    border: 1px solid ${token.colorBorderSecondary};
    margin: 0;
  `,
  summaryGrid: css`
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: ${token.margin}px;
  `,
  summaryItem: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorText};
  `,
  summaryLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  authRow: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  statusIcon: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
    color: ${token.colorSuccess};
  `,
  outputList: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
    margin: 0;
    padding: 0;
    list-style: none;
  `,
  outputItem: css`
    display: flex;
    align-items: flex-start;
    gap: ${token.marginSM}px;
  `,
  outputBullet: css`
    width: ${token.borderRadiusSM}px;
    height: ${token.borderRadiusSM}px;
    border-radius: 50%;
    background: ${token.colorPrimary};
    margin-top: ${token.marginSM}px;
  `,
  outputTitle: css`
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  outputSubtitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  generateSection: css`
    display: flex;
    flex-direction: column;
    align-items: center;
  `,
  generateButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.padding}px ${token.paddingXL}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    box-shadow: ${token.boxShadow};
    transition: transform 0.2s ease, filter 0.2s ease;

    &:hover {
      filter: brightness(1.05);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  generateNote: css`
    margin-top: ${token.marginSM}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  backLink: css`
    margin-top: ${token.marginXL}px;
    background: transparent;
    border: none;
    color: ${token.colorTextSecondary};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;

    &:hover {
      color: ${token.colorText};
    }
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
  iconMedium: css`
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
  `,
  templateSection: css`
    margin-bottom: ${token.marginLG}px;
  `,
  templateHint: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0 0 ${token.marginSM}px;
  `,
  templateGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: ${token.marginSM}px;
  `,
  templateCard: css`
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingLG}px ${token.paddingSM}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease;
    text-align: center;
    min-height: ${token.paddingXL * 4}px;
  `,
  templateCardSelected: css`
    border-color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
  `,
  templateCardDefault: css`
    &:hover {
      border-color: ${token.colorBorderSecondary};
    }
  `,
  templateCardIcon: css`
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorBgLayout};
    color: ${token.colorPrimary};
  `,
  templateCardName: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  templateCardDesc: css`
    font-size: ${token.fontSizeSM * 0.9}px;
    color: ${token.colorTextSecondary};
    line-height: 1.4;
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
