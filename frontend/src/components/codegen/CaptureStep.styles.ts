import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  container: css`
    max-width: ${token.paddingXL * 24}px;
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
    transition: border-color 0.2s ease;

    &:focus {
      outline: none;
      border-color: ${token.colorPrimary};
    }

    &::placeholder {
      color: ${token.colorTextQuaternary};
    }
  `,
  counter: css`
    margin-top: ${token.marginSM}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    text-align: right;
  `,
  counterWarning: css`
    color: ${token.colorWarning};
  `,
  actionRow: css`
    display: flex;
    justify-content: flex-end;
    margin-top: ${token.marginLG}px;
  `,
  analyzeButton: css`
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
  resultSection: css`
    margin-top: ${token.marginXL * 1.5}px;
    padding: ${token.paddingXL}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
  `,
  resultTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0 0 ${token.marginSM}px;
  `,
  resultSubtitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
    margin: 0 0 ${token.marginLG}px;
  `,
  sectionLabel: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: ${token.marginSM}px;
  `,
  tagRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginLG}px;
  `,
  entityList: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginLG}px;
  `,
  projectName: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    background: ${token.colorFillQuaternary};
    border-radius: ${token.borderRadiusLG}px;
    font-family: ${token.fontFamilyCode};
    font-size: ${token.fontSize}px;
    color: ${token.colorText};
    margin-bottom: ${token.marginLG}px;
  `,
  addInput: css`
    display: flex;
    gap: ${token.marginSM}px;
    margin-top: ${token.marginSM}px;
  `,
  nextRow: css`
    display: flex;
    justify-content: flex-end;
    margin-top: ${token.marginXL}px;
    padding-top: ${token.paddingLG}px;
    border-top: 1px solid ${token.colorBorder};
  `,
  iconSmall: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
}));
