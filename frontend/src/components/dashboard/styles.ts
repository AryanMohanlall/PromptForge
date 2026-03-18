import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    max-width: ${token.paddingXL * 32}px;
    margin: 0 auto;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginLG}px;
    margin-bottom: ${token.marginXL}px;

    @media (min-width: ${token.screenMD}px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  `,
  title: css`
    font-size: ${token.fontSizeXL * 1.2}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0;
  `,
  actions: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    flex-wrap: wrap;
  `,
  searchWrap: css`
    position: relative;
  `,
  searchIcon: css`
    position: absolute;
    left: ${token.paddingSM}px;
    top: 50%;
    transform: translateY(-50%);
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
    color: ${token.colorTextSecondary};
  `,
  searchInput: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    padding-left: ${token.paddingXL}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorText};
    width: ${token.paddingXL * 6}px;
  `,
  filterWrap: css`
    position: relative;
  `,
  filterButton: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${token.marginSM}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    cursor: pointer;
    min-width: ${token.paddingXL * 4.5}px;
    transition: background 0.2s ease;

    &:hover {
      background: ${token.colorFillSecondary};
    }
  `,
  filterIcon: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
  `,
  filterMenu: css`
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: ${token.marginSM}px;
    width: ${token.paddingXL * 5}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    box-shadow: ${token.boxShadow};
    overflow: hidden;
    z-index: 20;
  `,
  filterItem: css`
    width: 100%;
    text-align: left;
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    font-size: ${token.fontSizeSM}px;
    background: transparent;
    border: none;
    color: ${token.colorText};
    cursor: pointer;
    transition: background 0.2s ease, color 0.2s ease;

    &:hover {
      background: ${token.colorFillSecondary};
    }
  `,
  filterItemActive: css`
    background: ${token.colorPrimaryBg};
    color: ${token.colorPrimary};
    font-weight: ${token.fontWeightStrong};
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(1, minmax(0, 1fr));
    gap: ${token.marginLG}px;

    @media (min-width: ${token.screenMD}px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (min-width: ${token.screenLG}px) {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  `,
  emptyState: css`
    text-align: center;
    padding: ${token.paddingXL * 2}px;
    background: ${token.colorBgContainer};
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px dashed ${token.colorBorder};
    color: ${token.colorTextSecondary};
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
