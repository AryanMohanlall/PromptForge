import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 28}px;
    margin: 0 auto ${token.marginXL}px;
  `,
  stepRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${token.marginSM}px;
  `,
  stepGroup: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    flex: 1;
  `,
  stepItem: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: ${token.marginSM}px;
    min-width: ${token.paddingXL * 4}px;
  `,
  stepCircle: css`
    width: ${token.paddingXL}px;
    height: ${token.paddingXL}px;
    border-radius: 50%;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-weight: ${token.fontWeightStrong};
    font-size: ${token.fontSize}px;
    border: 1px solid ${token.colorBorder};
    background: ${token.colorBgContainer};
    color: ${token.colorTextSecondary};
  `,
  stepCircleActive: css`
    border-color: ${token.colorPrimary};
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
  `,
  stepCircleDone: css`
    border-color: ${token.colorSuccess};
    background: ${token.colorSuccess};
    color: ${token.colorBgContainer};
  `,
  stepLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    text-align: center;
  `,
  stepLabelActive: css`
    color: ${token.colorText};
    font-weight: ${token.fontWeightStrong};
  `,
  stepLabelDone: css`
    color: ${token.colorText};
  `,
  connector: css`
    flex: 1;
    height: ${token.borderRadiusSM}px;
    border-radius: ${token.borderRadiusSM}px;
    background: ${token.colorBorder};
  `,
  connectorActive: css`
    background: ${token.colorPrimary};
  `,
  connectorDone: css`
    background: ${token.colorSuccess};
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
}));
