import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 28}px;
    margin: 0 auto;
  `,
  header: css`
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingXL}px;
    margin-bottom: ${token.marginXL}px;
  `,
  titleRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: ${token.marginLG}px;
  `,
  title: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0;
  `,
  statusBadge: css`
    display: inline-flex;
    align-items: center;
    padding: ${token.paddingSM / 2}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorWarning};
    background: ${token.colorWarningBg};
    border: 1px solid ${token.colorWarningBorder};
  `,
  statusComplete: css`
    color: ${token.colorSuccess};
    background: ${token.colorSuccessBg};
    border-color: ${token.colorSuccessBorder};
  `,
  statusFailed: css`
    color: ${token.colorError};
    background: ${token.colorErrorBg};
    border-color: ${token.colorErrorBorder};
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
    transition: width 0.5s ease;
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
      0% { transform: translateX(-100%); }
      100% { transform: translateX(100%); }
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
      grid-template-columns: 1fr 1fr;
    }
  `,
  card: css`
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid ${token.colorBorder};
    padding: ${token.paddingXL}px;
  `,
  cardTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginLG}px;
  `,
  validationStack: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,
  validationItem: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px;
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorFillQuaternary};
  `,
  validationPending: css`
    color: ${token.colorTextTertiary};
  `,
  validationRunning: css`
    color: ${token.colorPrimary};
  `,
  validationPassed: css`
    color: ${token.colorSuccess};
  `,
  validationFailed: css`
    color: ${token.colorError};
  `,
  validationText: css`
    flex: 1;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  activityFeed: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
    max-height: 400px;
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
    font-size: ${token.fontSizeSM}px;
  `,
  activityDot: css`
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: ${token.colorSuccess};
    flex-shrink: 0;
  `,
  activityDotActive: css`
    background: ${token.colorPrimary};
  `,
  activityText: css`
    color: ${token.colorTextSecondary};
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
}));
