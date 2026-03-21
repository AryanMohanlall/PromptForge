import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  container: css`
    display: flex;
    flex-direction: column;
    gap: 24px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  // ── Loading / error state ──────────────────────────────────────────────────
  loadingWrap: css`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 16px;
    padding: 80px 24px;
    border: 1px solid rgba(255, 255, 255, 0.06);
    background: rgba(12, 18, 28, 0.7);
    backdrop-filter: blur(20px);
    border-radius: 20px;
  `,

  loadingText: css`
    font-size: 14px;
    color: #8b95a2;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    text-align: center;
  `,

  // ── Page header ────────────────────────────────────────────────────────────
  header: css`
    display: flex;
    flex-direction: column;
    gap: 6px;
  `,

  title: css`
    font-size: 22px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  subtitle: css`
    font-size: 14px;
    color: #8b95a2;
    margin: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  // ── Accordion sections ─────────────────────────────────────────────────────
  sectionStack: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,

  sectionCard: css`
    border: 1px solid rgba(255, 255, 255, 0.06);
    background: rgba(12, 18, 28, 0.7);
    backdrop-filter: blur(20px);
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 0 80px rgba(45, 212, 168, 0.03);
    transition: border-color 0.2s ease;

    &:hover {
      border-color: rgba(255, 255, 255, 0.1);
    }
  `,

  sectionHeader: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 18px;
    background: linear-gradient(135deg, rgba(45, 212, 168, 0.08), rgba(32, 196, 154, 0.08));
    cursor: pointer;
    user-select: none;

    &:hover {
      background: linear-gradient(135deg, rgba(45, 212, 168, 0.13), rgba(32, 196, 154, 0.13));
    }
  `,

  sectionTitle: css`
    display: flex;
    align-items: center;
    gap: 10px;
    font-size: 14px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  sectionBadge: css`
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 22px;
    height: 22px;
    padding: 0 6px;
    border-radius: 99px;
    background: rgba(45, 212, 168, 0.15);
    color: #2dd4a8;
    font-size: 12px;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,

  chevron: css`
    color: #8b95a2;
    transition: transform 0.25s ease;
    flex-shrink: 0;
  `,

  chevronOpen: css`
    transform: rotate(180deg);
  `,

  sectionBody: css`
    padding: 16px 18px;
    border-top: 1px solid rgba(255, 255, 255, 0.05);
  `,

  // ── Footer action row ──────────────────────────────────────────────────────
  actionRow: css`
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 12px;
    flex-wrap: wrap;
  `,

  backButton: css`
    padding: 10px 20px;
    border: 1px solid rgba(255, 255, 255, 0.06);
    background: rgba(12, 18, 28, 0.7);
    color: #ffffff;
    font-size: 14px;
    font-weight: 500;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    backdrop-filter: blur(20px);

    &:hover {
      background: rgba(20, 30, 42, 0.8);
      border-color: rgba(255, 255, 255, 0.15);
    }
  `,

  confirmButton: css`
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 22px;
    border: none;
    background: linear-gradient(135deg, #2dd4a8, #20c49a);
    color: #0c121a;
    font-size: 14px;
    font-weight: 600;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    cursor: pointer;
    transition: all 0.2s ease;
    border-radius: 12px;
    letter-spacing: -0.2px;

    &:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 24px rgba(45, 212, 168, 0.35);
    }

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  `,

  iconSmall: css`
    width: 16px;
    height: 16px;
    flex-shrink: 0;
  `,
}));