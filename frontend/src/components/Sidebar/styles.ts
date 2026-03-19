import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  sidebar: css`
    width: var(--sidebar-width, ${token.paddingXL * 7.5}px);
    height: 100vh;
    background: rgba(18, 30, 35, 0.88);
    color: ${token.colorText};
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
    position: fixed;
    left: 0;
    top: 0;
    box-shadow: 4px 0 16px rgba(0, 0, 0, 0.35);
    border-right: 1px solid rgba(255, 255, 255, 0.08);
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

    /* Dashboard: 90deg left-to-right (green → purple → yellow → white → black) */
    & button:nth-child(1):hover {
      background: linear-gradient(
        90deg,
        rgba(34, 197, 94, 0.18),
        rgba(109, 40, 217, 0.16),
        rgba(250, 204, 21, 0.18),
        rgba(255, 255, 255, 0.08),
        rgba(0, 0, 0, 0.34)
      );
      color: ${token.colorText};
    }

    /* My Projects: 135deg diagonal (yellow → purple → green → black → white) */
    & button:nth-child(2):hover {
      background: linear-gradient(
        135deg,
        rgba(250, 204, 21, 0.18),
        rgba(109, 40, 217, 0.16),
        rgba(34, 197, 94, 0.18),
        rgba(0, 0, 0, 0.34),
        rgba(255, 255, 255, 0.08)
      );
      color: ${token.colorText};
    }

    /* Templates: 45deg diagonal (purple → yellow → white → green → black) */
    & button:nth-child(3):hover {
      background: linear-gradient(
        45deg,
        rgba(109, 40, 217, 0.16),
        rgba(250, 204, 21, 0.18),
        rgba(255, 255, 255, 0.08),
        rgba(34, 197, 94, 0.18),
        rgba(0, 0, 0, 0.34)
      );
      color: ${token.colorText};
    }

    /* Settings: 75deg diagonal (green → yellow → purple → black → white) */
    & button:nth-child(4):hover {
      background: linear-gradient(
        75deg,
        rgba(34, 197, 94, 0.18),
        rgba(250, 204, 21, 0.18),
        rgba(109, 40, 217, 0.16),
        rgba(0, 0, 0, 0.34),
        rgba(255, 255, 255, 0.08)
      );
      color: ${token.colorText};
    }

    /* Admin buttons with varying gradients */
    /* Admin 1 (Overview): 225deg diagonal (green → purple → yellow) */
    & button:nth-child(6):hover {
      background: linear-gradient(
        225deg,
        rgba(34, 197, 94, 0.18),
        rgba(109, 40, 217, 0.16),
        rgba(250, 204, 21, 0.18)
      );
      color: ${token.colorText};
    }

    /* Admin 2 (Users): -45deg (white → yellow → purple) */
    & button:nth-child(7):hover {
      background: linear-gradient(
        -45deg,
        rgba(255, 255, 255, 0.08),
        rgba(250, 204, 21, 0.18),
        rgba(109, 40, 217, 0.16)
      );
      color: ${token.colorText};
    }

    /* Admin 3 (Projects): 0deg (top-to-bottom, purple → black → green) */
    & button:nth-child(8):hover {
      background: linear-gradient(
        0deg,
        rgba(109, 40, 217, 0.16),
        rgba(0, 0, 0, 0.34),
        rgba(34, 197, 94, 0.18)
      );
      color: ${token.colorText};
    }

    /* Admin 4 (Deployments): 270deg (bottom-to-top, yellow → green → white) */
    & button:nth-child(9):hover {
      background: linear-gradient(
        270deg,
        rgba(250, 204, 21, 0.18),
        rgba(34, 197, 94, 0.18),
        rgba(255, 255, 255, 0.08)
      );
      color: ${token.colorText};
    }

    /* Admin 5 (System Health): 315deg (purple → white → black → yellow) */
    & button:nth-child(10):hover {
      background: linear-gradient(
        315deg,
        rgba(109, 40, 217, 0.16),
        rgba(255, 255, 255, 0.08),
        rgba(0, 0, 0, 0.34),
        rgba(250, 204, 21, 0.18)
      );
      color: ${token.colorText};
    }
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
  `,
  navButtonActive: css`
    background: linear-gradient(
      90deg,
      rgba(34, 197, 94, 0.28),
      rgba(109, 40, 217, 0.18),
      rgba(250, 204, 21, 0.22),
      rgba(255, 255, 255, 0.1),
      rgba(0, 0, 0, 0.5)
    );
    border-left: 4px solid rgba(34, 197, 94, 0.85);
    color: ${token.colorText};
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
    background: rgba(34, 197, 94, 0.2);
    margin: 0 ${token.paddingSM}px ${token.marginSM}px;
  `,
  footer: css`
    margin-top: auto;
    padding-top: ${token.padding}px;
    border-top: 1px solid rgba(34, 197, 94, 0.25);
  `,
  profileCard: css`
    width: 100%;
    display: flex;
    align-items: flex-start;
    gap: ${token.margin}px;
    padding: ${token.padding}px;
    border-radius: ${token.borderRadiusLG * 1.5}px;
    border: 1px solid rgba(255, 255, 255, 0.18);
    background: linear-gradient(
      135deg,
      rgba(255, 255, 255, 0.14),
      rgba(255, 255, 255, 0.04)
    );
    box-shadow: 0 8px 18px rgba(0, 0, 0, 0.2);
    backdrop-filter: blur(7px);
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
    border: 1px solid rgba(255, 120, 120, 0.35);
    background: rgba(255, 95, 95, 0.06);
    color: rgba(255, 191, 191, 0.96);
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    transition: transform 0.2s ease, background 0.2s ease, color 0.2s ease,
      border-color 0.2s ease;

    &:hover {
      background: rgba(255, 95, 95, 0.18);
      border-color: rgba(255, 140, 140, 0.55);
      color: rgba(255, 235, 235, 0.98);
      transform: translateY(-1px);
    }

    &:active {
      transform: translateY(0);
    }
  `,
  logoutIcon: css`
    width: ${token.fontSize}px;
    height: ${token.fontSize}px;
    color: currentColor;
  `,
  profileInfo: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    min-width: 0;
    width: 100%;
  `,
  avatar: css`
    width: ${token.paddingLG + token.paddingSM}px;
    height: ${token.paddingLG + token.paddingSM}px;
    border-radius: 50%;
    background: linear-gradient(
      135deg,
      rgba(99, 102, 241, 0.95),
      rgba(14, 165, 233, 0.9)
    );
    border: 1px solid rgba(255, 255, 255, 0.28);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
    color: #ffffff;
  `,
  profileName: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    color: #ffffff;
  `,
  profileTextBlock: css`
    display: flex;
    flex-direction: column;
    min-width: 0;
    gap: 4px;
    flex: 1;
  `,
  profileMeta: css`
    font-size: ${token.fontSizeSM}px;
    color: rgba(255, 255, 255, 0.78);
    line-height: 1.25;
    max-width: 100%;
    white-space: normal;
    word-break: break-word;
  `,
  roleBadge: css`
    display: inline-flex;
    width: fit-content;
    max-width: 100%;
    margin-top: 2px;
    padding: 3px 8px;
    border-radius: ${token.borderRadius}px;
    background: rgba(56, 189, 248, 0.2);
    color: rgba(191, 236, 255, 0.98);
    border: 1px solid rgba(56, 189, 248, 0.35);
    font-size: ${token.fontSizeSM - 1}px;
    white-space: normal;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid ${token.colorPrimary};
      outline-offset: 2px;
    }
  `,
}));
