import { createStyles } from "antd-style";

export const useStyles = createStyles(({ css }) => ({
  page: css`
    max-width: 1200px;
    margin: 0 auto;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  header: css`
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-bottom: 24px;

    @media (min-width: 768px) {
      flex-direction: row;
      align-items: center;
      justify-content: space-between;
    }
  `,
  title: css`
    font-size: 24px;
    font-weight: 600;
    color: #ffffff;
    margin: 0;
    letter-spacing: -0.5px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  `,
  actions: css`
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;
  `,
  searchWrap: css`
    position: relative;
  `,
  searchIcon: css`
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    width: 20px;
    height: 20px;
    color: #8b95a2;
  `,
  searchInput: css`
    padding: 10px 16px;
    padding-left: 40px;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    font-size: 14px;
    color: #ffffff;
    width: 200px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    border-radius: 12px;
    backdrop-filter: blur(20px);
    transition: all 0.2s ease;

    &:focus {
      outline: none;
      border-color: rgba(45,212,168,0.3);
      box-shadow: 0 0 0 2px rgba(45,212,168,0.2);
    }

    &::placeholder {
      color: #5a6572;
    }
  `,
  filterWrap: css`
    position: relative;
  `,
  filterButton: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    padding: 10px 16px;
    font-size: 14px;
    font-weight: 500;
    color: #ffffff;
    cursor: pointer;
    min-width: 150px;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    transition: all 0.2s ease;
    border-radius: 12px;
    backdrop-filter: blur(20px);

    &:hover {
      background: rgba(20,30,42,0.8);
      border-color: rgba(255,255,255,0.15);
    }
  `,
  filterIcon: css`
    width: 16px;
    height: 16px;
    color: #8b95a2;
  `,
  filterMenu: css`
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 8px;
    width: 180px;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: 12px;
    overflow: hidden;
    z-index: 20;
    backdrop-filter: blur(20px);
    box-shadow: 0 4px 24px rgba(45,212,168,0.35);
  `,
  filterItem: css`
    width: 100%;
    text-align: left;
    padding: 12px 16px;
    font-size: 14px;
    background: transparent;
    border: none;
    border-bottom: 1px solid rgba(255,255,255,0.06);
    color: #ffffff;
    cursor: pointer;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    transition: all 0.1s ease;

    &:hover {
      background: rgba(20,30,42,0.8);
      color: #2dd4a8;
    }

    &:last-child {
      border-bottom: none;
    }
  `,
  filterItemActive: css`
    background: rgba(45,212,168,0.1);
    color: #2dd4a8;

    &:hover {
      background: rgba(45,212,168,0.15);
      color: #2dd4a8;
    }
  `,
  grid: css`
    display: grid;
    grid-template-columns: repeat(1, minmax(0, 1fr));
    gap: 24px;

    @media (min-width: 768px) {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    @media (min-width: 1024px) {
      grid-template-columns: repeat(3, minmax(0, 1fr));
    }
  `,
  emptyState: css`
    text-align: center;
    padding: 48px;
    background: rgba(12,18,28,0.7);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: 20px;
    color: #8b95a2;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    backdrop-filter: blur(20px);
    box-shadow: 0 0 80px rgba(45,212,168,0.03);
  `,
  focusRing: css`
    &:focus-visible {
      outline: 2px solid #2dd4a8;
      outline-offset: 2px;
    }
  `,
}));