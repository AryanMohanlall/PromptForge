import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    padding: ${token.paddingXL}px;
    background: ${token.colorBgLayout};
  `,
  toolbar: css`
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: ${token.margin}px;
    margin-bottom: ${token.marginLG}px;
  `,
  titleWrap: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
  `,
  title: css`
    margin: 0;
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  subtitle: css`
    margin: 0;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSize}px;
  `,
  controls: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    flex-wrap: wrap;
  `,
  search: css`
    width: ${token.paddingXL * 10}px;
    max-width: 100%;
  `,
  stateCard: css`
    border-radius: ${token.borderRadiusLG}px;
    border-color: ${token.colorBorder};
    background: ${token.colorBgContainer};
  `,
  stateInner: css`
    min-height: ${token.paddingXL * 8}px;
    display: flex;
    align-items: center;
    justify-content: center;
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(${token.paddingXL * 10}px, 1fr));
    gap: ${token.marginLG}px;
  `,
  templateCard: css`
    height: 100%;
    border-radius: ${token.borderRadiusLG}px;
    border-color: ${token.colorBorder};
    background: ${token.colorBgContainer};
  `,
  templateHeader: css`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: ${token.marginSM}px;
  `,
  templateName: css`
    margin: 0;
    color: ${token.colorText};
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
  `,
  description: css`
    margin: ${token.marginSM}px 0 ${token.margin}px;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSize}px;
    line-height: 1.5;
    min-height: ${token.paddingXL * 2}px;
  `,
  metaGrid: css`
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.margin}px;
  `,
  metaItem: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 3}px;
  `,
  metaLabel: css`
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
  `,
  metaValue: css`
    color: ${token.colorText};
    font-size: ${token.fontSize}px;
    font-weight: ${token.fontWeightStrong};
  `,
  tagsRow: css`
    display: flex;
    flex-wrap: wrap;
    gap: ${token.marginSM / 2}px;
    margin-bottom: ${token.margin}px;
  `,
  sourceLink: css`
    color: ${token.colorPrimary};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  `,
}));
