import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    min-height: 100vh;
    background: #0c121a;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    position: relative;
    overflow: hidden;
  `,
  pageMounted: css`
    opacity: 1;
  `,
  bgOrbPrimary: css`
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
  bgOrbSecondary: css`
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
  nav: css`
    position: sticky;
    top: 0;
    z-index: 10;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 24px;
    background: rgba(12,18,28,0.7);
    border-bottom: 1px solid rgba(255,255,255,0.06);
    backdrop-filter: blur(20px);
  `,
  logo: css`
    display: flex;
    align-items: center;
    gap: 12px;
    font-weight: 700;
    font-size: 20px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    letter-spacing: -0.5px;
  `,
  logoImage: css`
    width: 32px;
    height: 32px;
    object-fit: contain;
  `,
  logoImageSmall: css`
    width: 24px;
    height: 24px;
    object-fit: contain;
  `,
  navActions: css`
    display: flex;
    align-items: center;
    gap: 12px;
  `,
  signInBtn: css`
    color: #8b95a2;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background: transparent;
    border: 1px solid rgba(255,255,255,0.06);
    padding: 10px 20px;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    backdrop-filter: blur(20px);

    &:hover {
      color: #ffffff;
      border-color: rgba(255,255,255,0.15);
      background: rgba(20,30,42,0.8);
    }
  `,
  ctaBtn: css`
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    border: none;
    padding: 10px 20px;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    letter-spacing: -0.2px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }
  `,
  hero: css`
    padding: 64px 24px 48px;
    text-align: center;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
  `,
  heroPill: css`
    padding: 8px 16px;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    color: #8b95a2;
    font-size: 14px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    border-radius: 9999px;
    backdrop-filter: blur(20px);
  `,
  heroTitle: css`
    margin: 0;
    font-size: clamp(2rem, 4vw, 3rem);
    font-weight: 700;
    color: #ffffff;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  heroHighlight: css`
    color: #2dd4a8;
  `,
  heroSubtitle: css`
    max-width: 600px;
    color: #8b95a2;
    font-size: 18px;
    line-height: 1.7;
    margin: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  promptCard: css`
    width: 100%;
    max-width: 700px;
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    box-shadow: 0 0 80px rgba(45,212,168,0.03);
  `,
  promptInput: css`
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    background: transparent;
    border: none;
    color: #ffffff;
    font-size: 16px;

    &:focus {
      outline: none;
      box-shadow: none;
    }

    &::placeholder {
      color: #5a6572;
    }
  `,
  promptFooter: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-top: 12px;
    gap: 12px;
    padding: 12px 16px;
    border-top: 1px solid rgba(255,255,255,0.06);
  `,
  promptLabel: css`
    font-size: 12px;
    color: #5a6572;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  generateBtn: css`
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    border: none;
    padding: 10px 20px;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    letter-spacing: -0.2px;

    &:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }
  `,
  section: css`
    padding: 48px 24px;
  `,
  sectionTitle: css`
    text-align: center;
    font-size: clamp(1.5rem, 3vw, 2rem);
    font-weight: 700;
    margin-bottom: 12px;
    letter-spacing: -0.5px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  sectionHighlight: css`
    color: #2dd4a8;
  `,
  sectionSubtitle: css`
    text-align: center;
    color: #8b95a2;
    max-width: 600px;
    margin: 0 auto 32px;
    font-size: 16px;
    line-height: 1.7;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  featureGrid: css`
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;
  `,
  featureCard: css`
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    padding: 24px;
    transition: all 0.2s ease;
    box-shadow: 0 0 80px rgba(45,212,168,0.03);

    &:hover {
      transform: translateY(-2px);
      border-color: rgba(255,255,255,0.15);
      box-shadow: 0 4px 24px rgba(45,212,168,0.35);
    }
  `,
  featureCard0: css`
    animation-delay: 0s;
  `,
  featureCard1: css`
    animation-delay: 0.1s;
  `,
  featureCard2: css`
    animation-delay: 0.2s;
  `,
  featureCard3: css`
    animation-delay: 0.3s;
  `,
  featureIcon: css`
    width: 48px;
    height: 48px;
    border: 1px solid rgba(45,212,168,0.3);
    background: rgba(45,212,168,0.1);
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 16px;
    border-radius: 12px;
  `,
  featureIconDot: css`
    width: 16px;
    height: 16px;
    background: #2dd4a8;
    border-radius: 50%;
  `,
  featureTitle: css`
    font-size: 18px;
    font-weight: 600;
    margin-bottom: 12px;
    letter-spacing: -0.5px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  featureDesc: css`
    margin: 0;
    color: #8b95a2;
    line-height: 1.6;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  pipelineWrap: css`
    display: flex;
    justify-content: center;
  `,
  pipelineCard: css`
    width: 100%;
    max-width: 600px;
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(12,18,28,0.7);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    overflow: hidden;
    box-shadow: 0 0 80px rgba(45,212,168,0.03);
  `,
  pipelineHeader: css`
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 16px;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    background: linear-gradient(135deg, rgba(45,212,168,0.1), rgba(32,196,154,0.1));
    color: #ffffff;
    font-weight: 600;
    font-size: 14px;
  `,
  pipelineDots: css`
    display: flex;
    gap: 8px;
  `,
  pipelineDot: css`
    width: 12px;
    height: 12px;
    background: rgba(255,255,255,0.2);
    border-radius: 50%;
  `,
  pipelineTitle: css`
    font-size: 14px;
    color: #ffffff;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  pipelineBody: css`
    padding: 16px;
    display: grid;
    gap: 12px;
  `,
  pipelineStep: css`
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 0;
    font-size: 14px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  stepdone: css`
    color: #52c41a;
  `,
  steprunning: css`
    color: #faad14;
  `,
  steppending: css`
    color: #5a6572;
  `,
  stepLabel: css`
    flex: 1;
  `,
  stepRight: css`
    font-size: 12px;
    color: #5a6572;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  stepIconDone: css`
    color: #52c41a;
  `,
  stepIconRunning: css`
    color: #faad14;
    animation: pulse 2s ease-in-out infinite;
  `,
  stepIconPending: css`
    color: rgba(255,255,255,0.1);
  `,
  footer: css`
    padding: 24px 24px 32px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid rgba(255,255,255,0.06);
  `,
  footerLogo: css`
    display: flex;
    align-items: center;
    gap: 12px;
    font-weight: 600;
    color: #2dd4a8;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  footerText: css`
    color: #5a6572;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    font-size: 14px;
  `,
}));