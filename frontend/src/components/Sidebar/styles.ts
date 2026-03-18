import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  sidebar: css`
    width: var(--sidebar-width, ${token.paddingXL * 6.5}px);
    height: 100vh;
    background: ${token.colorText};
    color: ${token.colorBgContainer};
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
    position: fixed;
    left: 0;
    top: 0;
  `,
  content: css`
    padding: ${token.paddingLG}px;
    display: flex;
    flex-direction: column;
    height: 100%;
  `,
  brand: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    margin: 0 0 ${token.marginXL}px;
    color: ${token.colorBgContainer};
  `,
  newButton: css`
    width: 100%;
    border: none;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    background: ${token.colorPrimary};
    color: ${token.colorBgContainer};
    padding: ${token.paddingSM * 1.5}px ${token.paddingLG}px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: ${token.fontWeightStrong};
    box-shadow: ${token.boxShadowSecondary};
    cursor: pointer;
    transition: transform 0.2s ease, filter 0.2s ease;
    margin-bottom: ${token.marginXL}px;

    &:hover {
      filter: brightness(1.05);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  newIcon: css`
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
    margin-right: ${token.marginSM}px;
  `,
  nav: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM / 2}px;
  `,
  navButton: css`
    width: 100%;
    display: flex;
    align-items: center;
    padding: ${token.paddingSM}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: transparent;
    color: ${token.colorTextSecondary};
    cursor: pointer;
    transition: background 0.2s ease, color 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      color: ${token.colorBgContainer};
    }
  `,
  navButtonActive: css`
    background: rgba(255, 255, 255, 0.12);
    color: ${token.colorBgContainer};
    font-weight: ${token.fontWeightStrong};
  `,
  navIcon: css`
    width: ${token.fontSizeLG}px;
    height: ${token.fontSizeLG}px;
    margin-right: ${token.marginSM}px;
  `,
  navIconActive: css`
    color: ${token.colorPrimary};
  `,
  adminLabel: css`
    margin: ${token.marginXL}px ${token.paddingSM}px ${token.marginSM}px;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorTextSecondary};
    text-transform: uppercase;
    letter-spacing: 0.08em;
  `,
  divider: css`
    height: 1px;
    background: rgba(255, 255, 255, 0.12);
    margin: 0 ${token.paddingSM}px ${token.marginSM}px;
  `,
  footer: css`
    margin-top: auto;
    padding-top: ${token.padding}px;
    border-top: 1px solid rgba(255, 255, 255, 0.12);
  `,
  profileButton: css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: none;
    background: transparent;
    color: ${token.colorBgContainer};
    cursor: pointer;
    transition: background 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
    }
  `,
  logoutButton: css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: ${token.marginSM}px;
    margin-top: ${token.marginSM}px;
    padding: ${token.paddingSM}px ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid rgba(255, 255, 255, 0.2);
    background: transparent;
    color: ${token.colorTextSecondary};
    cursor: pointer;
    transition: background 0.2s ease, color 0.2s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      color: ${token.colorBgContainer};
    }
  `,
  logoutIcon: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
  `,
  profileInfo: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
  `,
  avatar: css`
    width: ${token.paddingXL}px;
    height: ${token.paddingXL}px;
    border-radius: 50%;
    background: rgba(255, 255, 255, 0.2);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  profileName: css`
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  chevron: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
