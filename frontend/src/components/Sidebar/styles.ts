import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  sidebar: css`
    width: 240px;
    height: 100vh;
    background: rgba(12, 18, 28, 0.85);
    backdrop-filter: blur(40px) saturate(180%);
    color: #ffffff;
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
    position: fixed;
    left: 0;
    top: 0;
    border-right: 1px solid rgba(255,255,255,0.06);
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    z-index: 50;
  `,
  content: css`
    padding: 16px;
    display: flex;
    flex-direction: column;
    height: 100%;
  `,
  brand: css`
    font-size: 22px;
    font-weight: 700;
    margin: 0 0 24px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    letter-spacing: -0.5px;
  `,
  newButton: css`
    width: 100%;
    border: none;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    padding: 12px 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.2s ease;
    margin-bottom: 24px;
    border-radius: 12px;
    letter-spacing: -0.2px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }

    &:active {
      transform: translateY(0);
    }
  `,
  newIcon: css`
    width: 20px;
    height: 20px;
    margin-right: 8px;
  `,
  nav: css`
    display: flex;
    flex-direction: column;
    gap: 4px;
  `,
  navButton: css`
    width: 100%;
    display: flex;
    align-items: center;
    padding: 10px 12px;
    border: 1px solid transparent;
    background: transparent;
    color: #8b95a2;
    cursor: pointer;
    transition: all 0.2s ease;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    font-size: 14px;
    border-radius: 12px;

    &:hover {
      background: rgba(20,30,42,0.8);
      color: #ffffff;
      border-color: rgba(255,255,255,0.15);
    }
  `,
  navButtonActive: css`
    background: rgba(45,212,168,0.1);
    color: #2dd4a8;
    border-color: rgba(45,212,168,0.3);

    &:hover {
      background: rgba(45,212,168,0.15);
      color: #2dd4a8;
      border-color: rgba(45,212,168,0.4);
    }
  `,
  navIcon: css`
    width: 20px;
    height: 20px;
    margin-right: 12px;
  `,
  navIconActive: css`
    color: #2dd4a8;
  `,
  adminLabel: css`
    margin: 24px 8px 8px;
    font-size: 12px;
    font-weight: 600;
    color: #5a6572;
    text-transform: uppercase;
    letter-spacing: 1px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  divider: css`
    height: 1px;
    background: rgba(255,255,255,0.06);
    margin: 0 8px 8px;
  `,
  footer: css`
    margin-top: auto;
    padding-top: 16px;
    border-top: 1px solid rgba(255,255,255,0.06);
  `,
  profileCard: css`
    width: 100%;
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px;
    border: 1px solid rgba(255, 255, 255, 0.08);
    background: rgba(255, 255, 255, 0.03);
    backdrop-filter: blur(20px);
    border-radius: 18px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    position: relative;
    overflow: hidden;

    &:hover {
      background: rgba(255, 255, 255, 0.06);
      border-color: rgba(45, 212, 168, 0.4);
      box-shadow: 
        0 8px 32px rgba(0, 0, 0, 0.4),
        0 0 16px rgba(45, 212, 168, 0.1);
      transform: translateY(-1px);
    }

    &::after {
      content: '';
      position: absolute;
      inset: 0;
      background: radial-gradient(circle at top right, rgba(45, 212, 168, 0.1), transparent 70%);
      opacity: 0;
      transition: opacity 0.3s ease;
    }

    &:hover::after {
      opacity: 1;
    }
  `,
  logoutButton: css`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    margin-top: 12px;
    padding: 12px;
    border: 1px solid rgba(255, 77, 79, 0.15);
    background: rgba(255, 77, 79, 0.05);
    color: #ff6b6b;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    font-size: 13px;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 14px;

    &:hover {
      background: rgba(255, 77, 79, 0.12);
      border-color: rgba(255, 77, 79, 0.3);
      color: #ff4d4f;
      box-shadow: 0 4px 20px rgba(255, 77, 79, 0.15);
    }

    &:active {
      transform: scale(0.98);
    }
  `,
  logoutIcon: css`
    width: 16px;
    height: 16px;
    color: currentColor;
  `,
  profileInfo: css`
    display: flex;
    align-items: center;
    gap: 12px;
    min-width: 0;
    width: 100%;
  `,
  avatar: css`
    width: 42px;
    height: 42px;
    border: 1px solid rgba(45, 212, 168, 0.4);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 15px;
    font-weight: 700;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background: linear-gradient(135deg, rgba(45, 212, 168, 0.2), rgba(32, 196, 154, 0.1));
    border-radius: 12px;
    box-shadow: inset 0 0 12px rgba(45, 212, 168, 0.1);
    position: relative;
    z-index: 2;
  `,
  profileName: css`
    font-size: 14px;
    font-weight: 600;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  `,
  profileTextBlock: css`
    display: flex;
    flex-direction: column;
    min-width: 0;
    gap: 2px;
    flex: 1;
    z-index: 2;
  `,
  profileMeta: css`
    font-size: 14px;
    color: #8b95a2;
    line-height: 1.25;
    max-width: 100%;
    white-space: normal;
    word-break: break-word;
  `,
  roleBadge: css`
    display: inline-flex;
    width: fit-content;
    max-width: 100%;
    padding: 2px 6px;
    border: 1px solid rgba(255, 255, 255, 0.08);
    color: #8b95a2;
    font-size: 11px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    background: rgba(255, 255, 255, 0.04);
    border-radius: 6px;
  `,
  profileActionIcon: css`
    width: 16px;
    height: 16px;
    color: #5a6572;
    opacity: 0;
    transition: all 0.3s ease;

    .ant-btn:hover &, 
    button:hover & {
      opacity: 1;
      transform: rotate(45deg);
      color: #2dd4a8;
    }
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));