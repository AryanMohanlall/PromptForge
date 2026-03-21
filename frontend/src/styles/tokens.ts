// Design System Tokens
// Modern glassmorphism theme matching the auth pages

export const colors = {
  bg: "#0c121a",
  surface: "rgba(12,18,28,0.7)",
  surfaceHover: "rgba(20,30,42,0.8)",
  text: "#ffffff",
  textSecondary: "#c8d0d8",
  textTertiary: "#8b95a2",
  textQuaternary: "#5a6572",
  border: "rgba(255,255,255,0.06)",
  borderHover: "rgba(255,255,255,0.15)",
  primary: "#2dd4a8",
  primaryMed: "#20c49a",
  primaryDark: "#18b892",
  error: "#ff4d4f",
  success: "#52c41a",
  warning: "#faad14",
  white: "#ffffff",
  black: "#000000",
} as const;

export const gradients = {
  primary: "linear-gradient(135deg, #2dd4a8, #20c49a)",
  subtle: "linear-gradient(135deg, rgba(45,212,168,0.1), rgba(32,196,154,0.1))",
} as const;

export const typography = {
  fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
  scale: {
    xs: "12px",
    sm: "14px",
    base: "16px",
    lg: "18px",
    xl: "20px",
    "2xl": "24px",
    "3xl": "30px",
  },
  weights: {
    normal: 400,
    medium: 500,
    semibold: 600,
    bold: 700,
  },
} as const;

export const spacing = {
  xs: "4px",
  sm: "8px",
  md: "12px",
  lg: "16px",
  xl: "24px",
  "2xl": "32px",
  "3xl": "48px",
} as const;

export const radius = {
  sm: "8px",
  md: "12px",
  lg: "16px",
  xl: "20px",
  "2xl": "24px",
  pill: "9999px",
} as const;

export const borders = {
  subtle: "1px solid rgba(255,255,255,0.06)",
  hover: "1px solid rgba(255,255,255,0.15)",
  active: "1px solid rgba(45,212,168,0.3)",
  width: "1px",
  style: "solid",
  color: colors.border,
} as const;

export const shadows = {
  subtle: "0 0 80px rgba(45,212,168,0.03)",
  glow: "0 4px 24px rgba(45,212,168,0.35)",
  card: "0 0 80px rgba(45,212,168,0.03)",
} as const;

export const effects = {
  glass: "backdrop-filter: blur(20px)",
  glassDark: "backdrop-filter: blur(16px) bg-black/40",
  gridPattern: `
    linear-gradient(rgba(255,255,255,0.5) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,0.5) 1px, transparent 1px)
  `,
} as const;

export const animations = {
  fadeIn: `
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `,
  float: `
    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-10px); }
    }
  `,
  pulse: `
    @keyframes pulse {
      50% { opacity: .5; }
    }
  `,
} as const;

// Combined token export
export const tokens = {
  colors,
  gradients,
  typography,
  spacing,
  radius,
  borders,
  shadows,
  effects,
  animations,
} as const;

export default tokens;