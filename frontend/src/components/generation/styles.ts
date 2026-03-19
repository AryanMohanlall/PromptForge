import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXL * 1.5}px;
  `,
  header: css`
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingXL}px;
    box-shadow: ${token.boxShadowSecondary};
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,
  eyebrow: css`
    text-transform: uppercase;
    letter-spacing: 0.08em;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0 0 ${token.marginSM / 2}px;
  `,
  titleRow: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;

    @media (min-width: ${token.screenMD}px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  `,
  title: css`
    font-size: ${token.fontSizeXL * 1.4}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM / 2}px;
  `,
  subtitle: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    max-width: ${token.paddingXL * 16}px;
    margin: 0;
  `,
  statusBadge: css`
    padding: ${token.paddingSM / 2}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    border: 1px solid transparent;
  `,
  statusGenerating: css`
    color: ${token.colorWarning};
    background: ${token.colorWarningBg};
    border-color: ${token.colorWarningBorder};
  `,
  statusGenerated: css`
    color: ${token.colorInfo};
    background: ${token.colorInfoBg};
    border-color: ${token.colorInfoBorder};
  `,
  statusDeploying: css`
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
    border-color: ${token.colorPrimaryBorder};
  `,
  statusLive: css`
    color: ${token.colorSuccess};
    background: ${token.colorSuccessBg};
    border-color: ${token.colorSuccessBorder};
  `,
  progressWrap: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  progressTrack: css`
    position: relative;
    height: ${token.controlHeightSM / 2}px;
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorFillSecondary};
    overflow: hidden;
  `,
  progressFill: css`
    height: 100%;
    background: linear-gradient(90deg, ${token.colorPrimary} 0%, ${token.colorSuccess} 100%);
  `,
  progressShimmer: css`
    position: absolute;
    inset: 0;
    background: linear-gradient(
      120deg,
      transparent 0%,
      ${token.colorFillQuaternary} 50%,
      transparent 100%
    );
    animation: shimmer 2s infinite;

    @keyframes shimmer {
      0% {
        transform: translateX(-100%);
      }
      100% {
        transform: translateX(100%);
      }
    }
  `,
  progressMeta: css`
    display: flex;
    justify-content: space-between;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  grid: css`
    display: grid;
    grid-template-columns: 1fr;
    gap: ${token.marginXL}px;

    @media (min-width: ${token.screenLG}px) {
      grid-template-columns: 2fr 3fr;
    }
  `,
  panelStack: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXL}px;
  `,
  card: css`
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingXL}px;
    box-shadow: ${token.boxShadowSecondary};
  `,
  cardHeader: css`
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: ${token.marginLG}px;
    margin-bottom: ${token.marginLG}px;
  `,
  cardTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM / 2}px;
  `,
  cardSubtitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  cardBadge: css`
    padding: ${token.paddingSM / 2}px ${token.paddingSM}px;
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorFillTertiary};
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  stepStack: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  stepRow: css`
    display: flex;
    gap: ${token.marginSM}px;
  `,
  stepRail: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: ${token.marginSM / 2}px;
  `,
  stepDot: css`
    width: ${token.controlHeightSM}px;
    height: ${token.controlHeightSM}px;
    border-radius: 50%;
    border: 1px solid ${token.colorBorder};
    display: flex;
    align-items: center;
    justify-content: center;
    color: ${token.colorTextSecondary};
    background: ${token.colorBgContainer};
    transition: background 0.2s ease, border-color 0.2s ease;
  `,
  stepDotActive: css`
    border-color: ${token.colorPrimary};
    box-shadow: 0 0 0 ${token.controlOutlineWidth}px ${token.colorPrimaryBg};
  `,
  stepDotCompleted: css`
    border-color: ${token.colorSuccess};
    background: ${token.colorSuccess};
    color: ${token.colorBgContainer};
  `,
  stepCheck: css`
    width: ${token.fontSizeSM}px;
    height: ${token.fontSizeSM}px;
  `,
  stepLine: css`
    width: 1px;
    flex: 1;
    background: ${token.colorBorder};
  `,
  stepLineCompleted: css`
    background: ${token.colorSuccess};
  `,
  stepContent: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
    padding-top: ${token.marginSM / 2}px;
  `,
  stepTitle: css`
    font-size: ${token.fontSize}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  stepDuration: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  codePanel: css`
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorderSecondary};
    background: ${token.colorBgContainer};
    box-shadow: ${token.boxShadowSecondary};
    overflow: hidden;
  `,
  codeHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    background: ${token.colorFillTertiary};
    border-bottom: 1px solid ${token.colorBorderSecondary};
  `,
  windowDots: css`
    display: flex;
    gap: ${token.marginSM / 2}px;
  `,
  dot: css`
    width: ${token.controlHeightSM / 2}px;
    height: ${token.controlHeightSM / 2}px;
    border-radius: 50%;
    background: ${token.colorTextSecondary};
    opacity: 0.6;
  `,
  codeTitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    font-family: ${token.fontFamilyCode};
  `,
  codeActions: css`
    display: flex;
    gap: ${token.marginSM / 2}px;
  `,
  iconButton: css`
    border: none;
    background: ${token.colorFillSecondary};
    width: ${token.controlHeightSM}px;
    height: ${token.controlHeightSM}px;
    border-radius: ${token.borderRadiusLG}px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    color: ${token.colorTextSecondary};
    cursor: pointer;
    transition: background 0.2s ease;

    &:hover {
      background: ${token.colorFillTertiary};
    }
  `,
  codeBody: css`
    display: grid;
    grid-template-columns: 1fr;
    min-height: ${token.paddingXL * 10}px;

    @media (min-width: ${token.screenMD}px) {
      grid-template-columns: ${token.paddingXL * 6}px 1fr;
    }
  `,
  fileTree: css`
    padding: ${token.paddingLG}px;
    border-right: 1px solid ${token.colorBorderSecondary};
    background: ${token.colorFillQuaternary};
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  treeItem: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  treeItemIndented: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    margin-left: ${token.marginLG}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  treeItemActive: css`
    color: ${token.colorPrimary};
  `,
  treeChevron: css`
    width: ${token.fontSizeSM}px;
    height: ${token.fontSizeSM}px;
  `,
  treeFolder: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
    color: ${token.colorInfo};
  `,
  treeFile: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
  codeContent: css`
    padding: ${token.paddingLG}px;
    overflow: auto;
    background: ${token.colorBgContainer};
  `,
  codeBlock: css`
    margin: 0;
    font-size: ${token.fontSizeSM}px;
    font-family: ${token.fontFamilyCode};
    color: ${token.colorText};
    white-space: pre-wrap;
  `,
  codeKeyword: css`
    color: ${token.colorPrimary};
  `,
  codeString: css`
    color: ${token.colorSuccess};
  `,
  codeFunction: css`
    color: ${token.colorInfo};
  `,
  codeTag: css`
    color: ${token.colorWarning};
  `,
  codeAttr: css`
    color: ${token.colorPrimaryTextActive};
  `,
  logDetails: css`
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorFillTertiary};
    overflow: hidden;
  `,
  logSummary: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    list-style: none;
  `,
  logBody: css`
    padding: ${token.paddingLG}px;
    background: ${token.colorBgContainer};
    font-family: ${token.fontFamilyCode};
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorSuccess};
    max-height: ${token.paddingXL * 6}px;
    overflow-y: auto;
    line-height: 1.6;
  `,
  reviewGrid: css`
    display: grid;
    gap: ${token.marginLG}px;

    @media (min-width: ${token.screenMD}px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  reviewLabel: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    margin-bottom: ${token.marginSM}px;
  `,
  reviewList: css`
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  reviewItem: css`
    display: flex;
    gap: ${token.marginSM}px;
    align-items: flex-start;
  `,
  reviewIcon: css`
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
    color: ${token.colorPrimary};
    margin-top: ${token.marginSM / 2}px;
  `,
  reviewTitle: css`
    display: block;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  reviewText: css`
    display: block;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  reviewFooter: css`
    margin-top: ${token.marginLG}px;
    padding-top: ${token.marginLG}px;
    border-top: 1px solid ${token.colorBorderSecondary};
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${token.marginLG}px;
    flex-wrap: wrap;
  `,
  liveUrlMuted: css`
    margin: 0;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
  `,
  ctaCard: css`
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingXL}px;
    box-shadow: ${token.boxShadow};
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,
  ctaHeader: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,
  ctaIcon: css`
    width: ${token.controlHeightLG}px;
    height: ${token.controlHeightLG}px;
    border-radius: 50%;
    background: ${token.colorSuccessBg};
    display: inline-flex;
    align-items: center;
    justify-content: center;
    color: ${token.colorSuccess};
  `,
  ctaTitle: css`
    margin: 0 0 ${token.marginSM / 2}px;
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
  `,
  ctaSubtitle: css`
    margin: 0;
    color: ${token.colorTextSecondary};
  `,
  formGrid: css`
    display: grid;
    gap: ${token.marginLG}px;

    @media (min-width: ${token.screenMD}px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  `,
  inputLabel: css`
    display: block;
    margin-bottom: ${token.marginSM / 2}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
  `,
  inputWrap: css`
    display: flex;
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    overflow: hidden;
    background: ${token.colorBgContainer};

    &:focus-within {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
  inputPrefix: css`
    padding: ${token.paddingSM}px ${token.padding}px;
    background: ${token.colorFillTertiary};
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
    border-right: 1px solid ${token.colorBorder};
  `,
  input: css`
    flex: 1;
    border: none;
    padding: ${token.paddingSM}px ${token.padding}px;
    font-size: ${token.fontSizeSM}px;
    outline: none;
    color: ${token.colorText};
    background: transparent;
  `,
  select: css`
    width: 100%;
    padding: ${token.paddingSM}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorText};
  `,
  ctaActions: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
  `,
  primaryButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    box-shadow: ${token.boxShadowSecondary};
    transition: filter 0.2s ease, transform 0.2s ease;

    &:hover {
      filter: brightness(1.05);
    }

    &:active {
      transform: scale(0.98);
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.6;
      filter: none;
    }
  `,
  secondaryButton: css`
    display: inline-flex;
    align-items: center;
    gap: ${token.marginSM / 2}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    color: ${token.colorText};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: background 0.2s ease, border-color 0.2s ease;

    &:hover {
      background: ${token.colorFillSecondary};
    }
  `,
  checkboxRow: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,
  checkbox: css`
    width: ${token.controlHeightSM}px;
    height: ${token.controlHeightSM}px;
  `,
  checkboxLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  errorText: css`
    margin: 0;
    color: ${token.colorError};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  infoText: css`
    margin: 0;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
  `,
  deployWrap: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
  `,
  sectionDivider: css`
    display: flex;
    align-items: center;
    justify-content: center;
    text-transform: uppercase;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    letter-spacing: 0.08em;

    &::before,
    &::after {
      content: "";
      height: 1px;
      background: ${token.colorBorder};
      flex: 1;
      margin: 0 ${token.marginSM}px;
    }
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
  successUrl: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    background: ${token.colorBgContainer};
    color: ${token.colorText};
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    width: 100%;
    max-width: ${token.paddingXL * 18}px;
    box-shadow: ${token.boxShadowSecondary};
  `,
  successUrlText: css`
    flex: 1;
    font-family: ${token.fontFamilyCode};
    font-size: ${token.fontSizeSM}px;
    text-align: left;
    word-break: break-all;
  `,
  successActions: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
    justify-content: center;
  `,
  successPrimary: css`
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

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  `,
  successGhost: css`
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

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
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
  iconLarge: css`
    width: ${token.fontSizeXL}px;
    height: ${token.fontSizeXL}px;
  `,
  claimWrap: css`
    display: flex;
    justify-content: center;
    margin-top: ${token.marginXL}px;
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
