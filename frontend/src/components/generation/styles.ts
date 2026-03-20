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
    display: inline-flex;
    align-items: center;
    padding: ${token.paddingSM / 2}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    border: 1px solid transparent;
    white-space: nowrap;
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
    border-radius: ${token.borderRadiusLG}px;
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
    white-space: nowrap;
  `,

  // ─── Pipeline steps ──────────────────────────────────────────────────

  stepStack: css`
    display: flex;
    flex-direction: column;
    gap: 0;
  `,
  stepRow: css`
    display: flex;
    gap: ${token.marginSM}px;
    min-height: 56px;
  `,
  stepRail: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 28px;
    flex-shrink: 0;
  `,
  stepDot: css`
    width: 28px;
    height: 28px;
    border-radius: 50%;
    border: 2px solid ${token.colorBorder};
    display: flex;
    align-items: center;
    justify-content: center;
    color: ${token.colorTextQuaternary};
    background: ${token.colorBgContainer};
    transition: all 0.3s ease;
    flex-shrink: 0;
  `,
  stepDotActive: css`
    border-color: ${token.colorPrimary};
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
    box-shadow: 0 0 0 4px ${token.colorPrimaryBg};
  `,
  stepDotCompleted: css`
    border-color: ${token.colorSuccess};
    background: ${token.colorSuccess};
    color: ${token.colorBgContainer};
  `,
  stepCheck: css`
    width: 14px;
    height: 14px;
  `,
  stepIcon: css`
    width: 14px;
    height: 14px;
  `,
  stepLine: css`
    width: 2px;
    flex: 1;
    min-height: 16px;
    background: ${token.colorBorder};
    transition: background 0.3s ease;
  `,
  stepLineCompleted: css`
    background: ${token.colorSuccess};
  `,
  stepContent: css`
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: 4px 0 ${token.marginSM}px;
    min-width: 0;
  `,
  stepTitleRow: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,
  stepTitle: css`
    font-size: ${token.fontSize}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  stepTitleActive: css`
    color: ${token.colorPrimary};
  `,
  stepTitlePending: css`
    color: ${token.colorTextTertiary};
  `,
  stepActiveBadge: css`
    font-size: 11px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorPrimary};
    background: ${token.colorPrimaryBg};
    padding: 1px 8px;
    border-radius: ${token.borderRadiusLG}px;
    border: 1px solid ${token.colorPrimaryBorder};
  `,
  stepDescription: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    line-height: 1.4;
  `,
  stepDetail: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorPrimary};
    font-weight: 500;
    margin-top: 2px;
    overflow: hidden;
  `,
  stepDuration: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  // ─── Activity feed ────────────────────────────────────────────────────

  activityFeed: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
    max-height: 320px;
    overflow-y: auto;
    padding: ${token.paddingSM}px;
    background: ${token.colorFillQuaternary};
    border-radius: ${token.borderRadiusLG}px;

    &::-webkit-scrollbar {
      width: 4px;
    }
    &::-webkit-scrollbar-thumb {
      background: ${token.colorBorder};
      border-radius: 2px;
    }
  `,
  activityItem: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: 4px 8px;
    border-radius: ${token.borderRadius}px;
    font-size: ${token.fontSizeSM}px;
  `,
  activityDot: css`
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: ${token.colorPrimary};
    flex-shrink: 0;
  `,
  activityDotActive: css`
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: ${token.colorPrimary};
    flex-shrink: 0;
  `,
  activityDotDone: css`
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: ${token.colorSuccess};
    flex-shrink: 0;
  `,
  activityText: css`
    color: ${token.colorTextSecondary};
    line-height: 1.4;
  `,
  activityTextActive: css`
    color: ${token.colorPrimary};
    font-weight: 500;
    line-height: 1.4;
  `,

  // ─── Summary grid ─────────────────────────────────────────────────────

  summaryGrid: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  summaryItem: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px;
    background: ${token.colorFillQuaternary};
    border-radius: ${token.borderRadiusLG}px;
  `,
  summaryIcon: css`
    width: 20px;
    height: 20px;
    color: ${token.colorPrimary};
    flex-shrink: 0;
  `,
  summaryLabel: css`
    display: block;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    font-size: ${token.fontSize}px;
  `,
  summaryValue: css`
    display: block;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,

  // ─── CTA / Deploy section ─────────────────────────────────────────────

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
    flex-shrink: 0;
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
