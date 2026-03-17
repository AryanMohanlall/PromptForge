import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    background: linear-gradient(
      160deg,
      ${token.colorBgLayout} 0%,
      ${token.colorBgContainer} 100%
    );
    color: ${token.colorText};
    font-family: ${token.fontFamily};
    position: relative;
    overflow: hidden;
    opacity: 0;
    transition: opacity 0.5s ease;
  `,
  pageMounted: css`
    opacity: 1;
  `,
  bgOrbPrimary: css`
    position: absolute;
    width: ${token.paddingXL * 12}px;
    height: ${token.paddingXL * 12}px;
    border-radius: 50%;
    background: radial-gradient(
      circle,
      ${token.colorPrimary} 0%,
      transparent 70%
    );
    opacity: 0.12;
    top: -${token.paddingXL * 6}px;
    right: -${token.paddingXL * 4}px;
    pointer-events: none;
  `,
  bgOrbSecondary: css`
    position: absolute;
    width: ${token.paddingXL * 14}px;
    height: ${token.paddingXL * 14}px;
    border-radius: 50%;
    background: radial-gradient(
      circle,
      ${token.colorSuccess} 0%,
      transparent 70%
    );
    opacity: 0.1;
    bottom: -${token.paddingXL * 7}px;
    left: -${token.paddingXL * 6}px;
    pointer-events: none;
  `,
  nav: css`
    position: sticky;
    top: 0;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: ${token.paddingLG}px ${token.paddingXL}px;
    background: ${token.colorBgElevated};
    border-bottom: 1px solid ${token.colorBorder};
    backdrop-filter: blur(${token.padding}px);
  `,
  logo: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    font-weight: ${token.fontWeightStrong};
    font-size: ${token.fontSizeLG}px;
    color: ${token.colorText};
  `,
  logoImage: css`
    width: ${token.paddingXL}px;
    height: ${token.paddingXL}px;
    object-fit: contain;
  `,
  logoImageSmall: css`
    width: ${token.paddingLG}px;
    height: ${token.paddingLG}px;
    object-fit: contain;
  `,
  navActions: css`
    display: flex;
    align-items: center;
    gap: ${token.margin}px;
  `,
  signInBtn: css`
    color: ${token.colorTextSecondary};
    font-weight: ${token.fontWeightStrong};
  `,
  ctaBtn: css`
    border-radius: ${token.borderRadiusLG}px;
    font-weight: ${token.fontWeightStrong};
    box-shadow: ${token.boxShadowSecondary};
  `,
  hero: css`
    padding: ${token.paddingXL * 3}px ${token.paddingXL}px ${token.paddingXL * 2}px;
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: ${token.marginLG}px;
  `,
  heroPill: css`
    padding: ${token.paddingSM}px ${token.paddingLG}px;
    border-radius: ${token.borderRadiusLG}px;
    background: ${token.colorBgContainer};
    border: 1px solid ${token.colorBorder};
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  heroTitle: css`
    margin: 0;
    font-size: ${token.fontSizeXL * 2}px;
    font-weight: ${token.fontWeightStrong};
    color: ${token.colorText};
  `,
  heroHighlight: css`
    background: linear-gradient(
      120deg,
      ${token.colorPrimary},
      ${token.colorSuccess}
    );
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  `,
  heroSubtitle: css`
    max-width: ${token.paddingXL * 16}px;
    color: ${token.colorTextSecondary};
    font-size: ${token.fontSizeLG}px;
    line-height: 1.7;
    margin: 0;
  `,
  promptCard: css`
    width: 100%;
    max-width: ${token.paddingXL * 18}px;
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadow};
    border: 1px solid ${token.colorBorder};
  `,
  promptInput: css`
    font-family: ${token.fontFamily};
  `,
  promptFooter: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: ${token.margin}px;
    gap: ${token.margin}px;
  `,
  promptLabel: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  generateBtn: css`
    border-radius: ${token.borderRadiusLG}px;
    font-weight: ${token.fontWeightStrong};
  `,
  section: css`
    padding: ${token.paddingXL * 2}px ${token.paddingXL}px;
  `,
  sectionTitle: css`
    text-align: center;
    font-size: ${token.fontSizeXL * 1.5}px;
    font-weight: ${token.fontWeightStrong};
    margin-bottom: ${token.margin}px;
  `,
  sectionHighlight: css`
    color: ${token.colorPrimary};
  `,
  sectionSubtitle: css`
    text-align: center;
    color: ${token.colorTextSecondary};
    max-width: ${token.paddingXL * 15}px;
    margin: 0 auto ${token.marginXL}px;
    font-size: ${token.fontSizeLG}px;
    line-height: 1.7;
  `,
  featureGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(${token.paddingXL * 6}px, 1fr));
    gap: ${token.marginLG}px;
  `,
  featureCard: css`
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadowSecondary};
    border: 1px solid ${token.colorBorder};
    animation: fadeUp 0.6s ease both;

    @keyframes fadeUp {
      from {
        opacity: 0;
        transform: translateY(${token.paddingLG}px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
  `,
  featureCard0: css`
    animation-delay: 0.05s;
  `,
  featureCard1: css`
    animation-delay: 0.15s;
  `,
  featureCard2: css`
    animation-delay: 0.25s;
  `,
  featureCard3: css`
    animation-delay: 0.35s;
  `,
  featureIcon: css`
    width: ${token.paddingXL}px;
    height: ${token.paddingXL}px;
    border-radius: ${token.borderRadius}px;
    background: ${token.colorBgLayout};
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: ${token.margin}px;
  `,
  featureIconDot: css`
    width: ${token.padding}px;
    height: ${token.padding}px;
    border-radius: 50%;
    background: ${token.colorPrimary};
  `,
  featureTitle: css`
    font-size: ${token.fontSizeLG}px;
    font-weight: ${token.fontWeightStrong};
    margin-bottom: ${token.marginSM}px;
  `,
  featureDesc: css`
    margin: 0;
    color: ${token.colorTextSecondary};
    line-height: 1.6;
  `,
  pipelineWrap: css`
    display: flex;
    justify-content: center;
  `,
  pipelineCard: css`
    width: 100%;
    max-width: ${token.paddingXL * 16}px;
    border-radius: ${token.borderRadiusLG}px;
    box-shadow: ${token.boxShadow};
    border: 1px solid ${token.colorBorder};
  `,
  pipelineHeader: css`
    display: flex;
    align-items: center;
    gap: ${token.margin}px;
    padding: ${token.padding}px ${token.paddingLG}px;
    border-bottom: 1px solid ${token.colorBorder};
  `,
  pipelineDots: css`
    display: flex;
    gap: ${token.marginSM}px;
  `,
  pipelineDot: css`
    width: ${token.paddingSM}px;
    height: ${token.paddingSM}px;
    border-radius: 50%;
    background: ${token.colorBorder};
  `,
  pipelineTitle: css`
    font-size: ${token.fontSizeSM}px;
    color: ${token.colorTextSecondary};
  `,
  pipelineBody: css`
    padding: ${token.paddingLG}px;
    display: grid;
    gap: ${token.marginSM}px;
  `,
  pipelineStep: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    padding: ${token.paddingSM}px 0;
    font-size: ${token.fontSize}px;
  `,
  stepdone: css`
    color: ${token.colorText};
  `,
  steprunning: css`
    color: ${token.colorPrimary};
  `,
  steppending: css`
    color: ${token.colorTextSecondary};
  `,
  stepLabel: css`
    flex: 1;
  `,
  stepRight: css`
    font-size: ${token.fontSizeSM}px;
  `,
  stepIconDone: css`
    color: ${token.colorSuccess};
  `,
  stepIconRunning: css`
    color: ${token.colorPrimary};
    animation: spin 1s linear infinite;

    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }
  `,
  stepIconPending: css`
    color: ${token.colorBorder};
  `,
  footer: css`
    padding: ${token.paddingLG}px ${token.paddingXL}px ${token.paddingXL}px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid ${token.colorBorder};
  `,
  footerLogo: css`
    display: flex;
    align-items: center;
    gap: ${token.marginSM}px;
    font-weight: ${token.fontWeightStrong};
  `,
  footerText: css`
    color: ${token.colorTextSecondary};
  `,
}));
