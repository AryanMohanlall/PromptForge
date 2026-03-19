import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  card: css`
    max-width: 440px;
    margin: 0 auto;
    overflow: hidden;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorderSecondary};
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    display: flex;
    flex-direction: column;
  `,
  header: css`
    padding: ${token.paddingLG}px;
    display: flex;
    flex-direction: column;
    gap: ${token.marginXS}px;
  `,
  status: css`
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    color: ${token.colorSuccess};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    margin-bottom: ${token.marginXS}px;
  `,
  title: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    margin: 0;
  `,
  description: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,
  preview: css`
    position: relative;
    aspect-ratio: 16 / 9;
    background: ${token.colorFillQuaternary};
    border-top: 1px solid ${token.colorBorderSecondary};
    border-bottom: 1px solid ${token.colorBorderSecondary};
    overflow: hidden;
    
    &:hover .overlay {
      opacity: 1;
    }
  `,
  previewImage: css`
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.3s ease;
    
    .preview:hover & {
      transform: scale(1.05);
    }
  `,
  overlay: css`
    position: absolute;
    inset: 0;
    background: rgba(0, 0, 0, 0.05);
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.2s ease;
  `,
  viewButton: css`
    background: ${token.colorBgElevated};
    color: ${token.colorText};
    padding: ${token.paddingXS}px ${token.paddingMD}px;
    border-radius: ${token.borderRadiusLG}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    text-decoration: none;
    box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
    
    &:hover {
      background: ${token.colorBgContainer};
    }
  `,
  footer: css`
    padding: ${token.paddingLG}px;
    display: flex;
    flex-direction: column;
    gap: ${token.marginMD}px;
  `,
  claimButton: css`
    width: 100%;
    height: 44px;
    background: #000;
    color: #fff;
    border: none;
    border-radius: ${token.borderRadius}px;
    font-size: ${token.fontSize}px;
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: background 0.2s ease;
    
    &:hover {
      background: #333;
    }
    
    &:active {
      transform: scale(0.98);
    }
  `,
  poweredBy: css`
    text-align: center;
    font-size: 10px;
    color: ${token.colorTextQuaternary};
    text-transform: uppercase;
    letter-spacing: 0.1em;
    font-weight: ${token.fontWeightStrong};
    margin: 0;
  `,
}));
