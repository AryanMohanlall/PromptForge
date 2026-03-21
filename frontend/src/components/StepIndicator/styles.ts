import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    max-width: 900px;
    margin: 0 auto 48px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  stepRow: css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0;
  `,
  stepGroup: css`
    display: flex;
    align-items: center;
    flex: 1;

    &:last-child {
      flex: 0;
    }
  `,
  stepItem: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
    position: relative;
    z-index: 2;
  `,
  stepCircle: css`
    width: 44px;
    height: 44px;
    border-radius: 50%;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-size: 15px;
    border: 2px solid rgba(255, 255, 255, 0.06);
    background: rgba(12, 18, 28, 0.6);
    color: #5a6572;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    backdrop-filter: blur(12px);
  `,
  stepCircleActive: css`
    border-color: #2dd4a8;
    background: rgba(45, 212, 168, 0.1);
    color: #2dd4a8;
    box-shadow: 0 0 20px rgba(45, 212, 168, 0.3);
    transform: scale(1.1);
  `,
  stepCircleDone: css`
    border-color: rgba(45, 212, 168, 0.4);
    background: #2dd4a8;
    color: #0c121a;
    box-shadow: 0 4px 12px rgba(45, 212, 168, 0.2);
  `,
  stepLabel: css`
    font-size: 13px;
    font-weight: 500;
    color: #5a6572;
    text-align: center;
    transition: all 0.3s ease;
    white-space: nowrap;
  `,
  stepLabelActive: css`
    color: #ffffff;
    font-weight: 600;
    transform: translateY(-2px);
  `,
  stepLabelDone: css`
    color: #8b95a2;
  `,
  connector: css`
    flex: 1;
    height: 2px;
    background: rgba(255, 255, 255, 0.06);
    margin: 0 -10px;
    margin-top: -24px;
    position: relative;
    z-index: 1;
    border-radius: 1px;
    transition: all 0.4s ease;
  `,
  connectorActive: css`
    background: linear-gradient(90deg, #2dd4a8, rgba(255, 255, 255, 0.06));
  `,
  connectorDone: css`
    background: #2dd4a8;
    opacity: 0.6;
  `,
  iconSmall: css`
    width: 20px;
    height: 20px;
  `,
}));