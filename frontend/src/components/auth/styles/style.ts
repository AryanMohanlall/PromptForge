import { createStyles } from 'antd-style';

// Values that have no antd token equivalent (glassmorphism / brand-specific)
const glass = {
  bgCard:        'rgba(12,18,28,0.7)',
  bgInput:       'rgba(16,24,34,0.6)',
  bgInputHover:  'rgba(20,30,42,0.8)',
  bgAutofill:    '#0c121a',
  borderFaint:   'rgba(255,255,255,0.06)',
  borderSocial:  'rgba(255,255,255,0.08)',
  borderHover:   'rgba(255,255,255,0.15)',
  textSocial:    '#c8d0d8',
  textBranding:  '#2a3a4a',
  primaryDark:   '#18b892',
  primaryMed:    '#20c49a',
} as const;

// ─── Page layout ──────────────────────────────────────────────────────────────
export const usePageStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: ${token.colorBgLayout};
    font-family: ${token.fontFamily};
    position: relative;
    overflow: hidden;
    padding: 40px ${token.paddingLG}px;
  `,

  bgOrb1: css`
    position: absolute;
    width: 400px;
    height: 400px;
    border-radius: 50%;
    background: radial-gradient(circle, rgba(45,212,168,0.04) 0%, transparent 70%);
    top: 10%;
    left: -5%;
    animation: orb1 20s ease-in-out infinite;
    pointer-events: none;
  `,

  bgOrb2: css`
    position: absolute;
    width: 500px;
    height: 500px;
    border-radius: 50%;
    background: radial-gradient(circle, rgba(0,229,255,0.03) 0%, transparent 70%);
    bottom: 5%;
    right: -8%;
    animation: orb2 25s ease-in-out infinite;
    pointer-events: none;
  `,

  gridOverlay: css`
    position: absolute;
    inset: 0;
    pointer-events: none;
    opacity: 0.015;
    background-image:
      linear-gradient(rgba(255,255,255,0.5) 1px, transparent 1px),
      linear-gradient(90deg, rgba(255,255,255,0.5) 1px, transparent 1px);
    background-size: 60px 60px;
  `,

  cardWrapper: css`
    animation: fadeIn 0.35s ease both;
    position: relative;
    z-index: 1;
    width: min(680px, 100%);
  `,

  branding: css`
    position: relative;
    z-index: 1;
    margin-top: ${token.marginLG}px;
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    font-size: ${token.fontSizeSM}px;
    color: ${glass.textBranding};
    font-weight: 500;
  `,
}));

// ─── AuthCard ─────────────────────────────────────────────────────────────────
export const useCardStyles = createStyles(({ token, css }) => ({
  card: css`
    width: 100%;
    max-width: 680px;
    background: ${glass.bgCard};
    border: 1px solid rgba(45,212,168,0.06);
    border-radius: 20px;
    padding: ${token.paddingXL}px ${token.paddingLG}px;
    backdrop-filter: blur(20px);
    box-shadow: 0 0 80px rgba(45,212,168,0.03);
  `,
}));

// ─── Input overrides ──────────────────────────────────────────────────────────
export const useInputStyles = createStyles(({ token, css }) => ({
  icon: css`
    color: ${token.colorTextQuaternary};
    display: flex;
    flex-shrink: 0;
  `,

  // Applied via className on antd Input / Input.Password
  input: css`
    font-family: ${token.fontFamily};

    &.ant-input-affix-wrapper {
      background: ${glass.bgInput};
      border-color: ${glass.borderFaint};

      .ant-input {
        background: transparent;
        font-family: ${token.fontFamily};
      }
    }
  `,
}));

// ─── Social button overrides ──────────────────────────────────────────────────
export const useSocialBtnStyles = createStyles(({ token, css }) => ({
  btn: css`
    && {
      background: ${glass.bgInput};
      border: 1px solid ${glass.borderSocial};
      color: ${glass.textSocial};
      font-size: ${token.fontSize}px;
      font-weight: 500;

      &:hover {
        border-color: ${glass.borderHover};
        background: ${glass.bgInputHover};
        color: ${glass.textSocial};
        transform: translateY(-1px);
        box-shadow: 0 4px 24px rgba(45,212,168,0.35);
      }
    }
  `,
}));

// ─── Divider override ─────────────────────────────────────────────────────────
export const useDividerStyles = createStyles(({ token, css }) => ({
  divider: css`
    && {
      font-size: ${token.fontSizeSM}px;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 1px;
      margin: ${token.marginMD}px 0;
    }
  `,
}));

// ─── Shared auth form styles ───────────────────────────────────────────────────
export const useAuthStyles = createStyles(({ token, css }) => ({
  header: css`
    text-align: center;
    margin-bottom: ${token.marginXL}px;
  `,

  logoContainer: css`
    display: flex;
    align-items: center;
    justify-content: center;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginMD}px;
  `,

  logoName: css`
    font-size: 22px;
    font-weight: 700;
    color: ${token.colorText};
    letter-spacing: -0.5px;
    margin: 0;
  `,

  logo: css`
    width: 40px;
    height: 40px;
    border-radius: ${token.borderRadiusLG}px;
    overflow: hidden;
  `,

  heading: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    letter-spacing: -0.5px;
    margin-bottom: ${token.marginXS}px;
    margin-top: 0;
  `,

  subtitle: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    margin: 0;
  `,

  socialGroup: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
    margin-bottom: ${token.marginXS}px;
  `,

  formGroup: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginSM}px;
  `,

  forgotRow: css`
    display: flex;
    justify-content: flex-end;
    margin: ${token.marginSM}px 0 ${token.marginMD}px;
  `,

  linkBtn: css`
    background: none;
    border: none;
    color: ${token.colorPrimary};
    font-size: ${token.fontSizeSM}px;
    font-weight: 500;
    cursor: pointer;
    font-family: ${token.fontFamily};
    padding: 0;
    transition: opacity 0.2s;

    &:hover {
      opacity: 0.8;
    }
  `,

  // Gradient override on top of antd primary Button
  primaryBtn: css`
    && {
      background: linear-gradient(135deg, ${token.colorPrimary}, ${glass.primaryMed});
      border: none;
      color: ${token.colorBgLayout};
      letter-spacing: -0.2px;
      font-family: ${token.fontFamily};

      &:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 24px rgba(45,212,168,0.35);
        background: linear-gradient(135deg, ${token.colorPrimary}, ${glass.primaryMed});
        color: ${token.colorBgLayout};
      }
    }
  `,

  primaryBtnMt: css`
    && {
      background: linear-gradient(135deg, ${token.colorPrimary}, ${glass.primaryMed});
      border: none;
      color: ${token.colorBgLayout};
      letter-spacing: -0.2px;
      font-family: ${token.fontFamily};
      margin-top: ${token.marginLG}px;

      &:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 24px rgba(45,212,168,0.35);
        background: linear-gradient(135deg, ${token.colorPrimary}, ${glass.primaryMed});
        color: ${token.colorBgLayout};
      }
    }
  `,

  switchText: css`
    text-align: center;
    margin-top: ${token.marginLG}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextTertiary};
    margin-bottom: 0;
  `,

  switchBtn: css`
    background: none;
    border: none;
    color: ${token.colorPrimary};
    font-weight: ${token.fontWeightStrong};
    cursor: pointer;
    font-family: ${token.fontFamily};
    font-size: ${token.fontSizeSM}px;
    padding: 0;
  `,

  // Password strength checks
  passwordChecks: css`
    display: flex;
    flex-direction: column;
    gap: ${token.marginXS}px;
    margin-top: ${token.marginSM}px;
    padding-left: ${token.paddingXS}px;
  `,

  checkItemMet: css`
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorPrimary};
    transition: color 0.2s;
  `,

  checkItemUnmet: css`
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextQuaternary};
    transition: color 0.2s;
  `,

  termsText: css`
    text-align: center;
    margin-top: ${token.marginMD}px;
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextQuaternary};
    line-height: 1.6;
    margin-bottom: 0;
  `,

  termsLink: css`
    color: ${token.colorTextTertiary};
    cursor: pointer;
  `,

  // Forgot password
  backBtn: css`
    display: flex;
    align-items: center;
    gap: ${token.marginXS}px;
    background: none;
    border: none;
    color: ${token.colorTextTertiary};
    font-size: ${token.fontSizeSM}px;
    font-weight: 500;
    cursor: pointer;
    font-family: ${token.fontFamily};
    margin-bottom: ${token.marginLG}px;
    padding: 0;
    transition: color 0.2s;

    &:hover {
      color: ${token.colorTextSecondary};
    }
  `,

  forgotHeaderBlock: css`
    margin-bottom: ${token.marginXL}px;
  `,

  forgotHeading: css`
    font-size: ${token.fontSizeXL}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    letter-spacing: -0.5px;
    margin-bottom: ${token.marginXS}px;
    margin-top: 0;
  `,

  forgotDescription: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    line-height: 1.6;
    margin: 0;
  `,

  forgotSubmitBtn: css`
    && {
      background: linear-gradient(135deg, ${token.colorPrimary}, ${glass.primaryMed});
      border: none;
      color: ${token.colorBgLayout};
      letter-spacing: -0.2px;
      font-family: ${token.fontFamily};
      margin-top: ${token.marginMD}px;

      &:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 24px rgba(45,212,168,0.35);
      }
    }
  `,

  forgotSubmitBtnDisabled: css`
    && {
      background: rgba(45,212,168,0.15);
      border: none;
      color: ${token.colorPrimary};
      cursor: default;
      opacity: 0.6;
      margin-top: ${token.marginMD}px;

      &:hover {
        background: rgba(45,212,168,0.15);
        color: ${token.colorPrimary};
        transform: none;
        box-shadow: none;
      }
    }
  `,

  successWrapper: css`
    text-align: center;
    padding: ${token.paddingMD}px 0;
  `,

  successIconRow: css`
    display: flex;
    justify-content: center;
    margin-bottom: ${token.marginMD}px;
  `,

  successHeading: css`
    font-size: 22px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
    letter-spacing: -0.5px;
    margin-bottom: ${token.marginXS}px;
    margin-top: 0;
  `,

  successText: css`
    font-size: ${token.fontSize}px;
    color: ${token.colorTextSecondary};
    line-height: 1.7;
    margin-bottom: ${token.marginLG}px;
  `,

  emailHighlight: css`
    color: ${token.colorPrimary};
    font-weight: 500;
  `,

  resendText: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextQuaternary};
    margin: 0;
  `,
}));
