import { createStyles } from "antd-style";

export const useStyles = createStyles(({ token, css }) => ({
  page: css`
    min-height: 100vh;
    background: #070b10;
    padding: 28px 20px;
    font-family: 'Sora', sans-serif;
  `,
  inner: css`
    max-width: 1320px;
    margin: 0 auto;
  `,
  headerTitle: css`
    margin: 0;
    font-size: 19px;
    font-weight: 700;
    font-family: 'JetBrains Mono', monospace;
    color: #f1f5f9;
    letter-spacing: -0.03em;
  `,
  headerSubtitle: css`
    margin: 0;
    font-size: 11px;
    color: ${token.colorTextTertiary};
  `,
  headerIcon: css`
    width: 40px;
    height: 40px;
    border-radius: ${token.borderRadiusLG}px;
    background: linear-gradient(135deg, #4f46e5, #7c3aed);
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 0 20px rgba(99,102,241,0.25);
    flex-shrink: 0;
  `,
  card: css`
    background: rgba(255,255,255,0.02);
    border: 1px solid rgba(255,255,255,0.06);
    border-radius: ${token.borderRadiusLG}px;
    overflow: hidden;
  `,
  mainGrid: css`
    display: grid;
    gap: 14px;
    align-items: start;
  `,
  repoListHeader: css`
    padding: 13px 12px 9px;
  `,
  repoListLabel: css`
    font-size: 10px;
    font-weight: 600;
    color: ${token.colorTextQuaternary};
    font-family: 'JetBrains Mono', monospace;
    text-transform: uppercase;
    letter-spacing: 0.07em;
  `,
  repoScroll: css`
    max-height: 640px;
    overflow-y: auto;
    padding: 0 7px 7px;
    display: flex;
    flex-direction: column;
    gap: 4px;
  `,
  repoCard: css`
    width: 100%;
    text-align: left;
    border-radius: ${token.borderRadius}px;
    padding: 13px 14px;
    cursor: pointer;
    transition: all 0.15s;
    border: 1px solid rgba(255,255,255,0.06);
    background: rgba(255,255,255,0.02);
    &:hover { background: rgba(255,255,255,0.04); }
  `,
  repoCardSelected: css`
    background: rgba(99,102,241,0.08) !important;
    border-color: rgba(99,102,241,0.4) !important;
  `,
  repoCardName: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    font-weight: 600;
    color: #e2e8f0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  `,
  repoCardDesc: css`
    margin: 5px 0 0 19px;
    font-size: 11px;
    color: ${token.colorTextTertiary};
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  `,
  repoCardMeta: css`
    display: flex;
    gap: 10px;
    margin-top: 7px;
    margin-left: 19px;
    flex-wrap: wrap;
  `,
  detailPanel: css`
    display: flex;
    flex-direction: column;
    gap: 12px;
  `,
  repoHeader: css`
    padding: 16px 18px 14px;
  `,
  repoFullName: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 13px;
    font-weight: 700;
    color: #f1f5f9;
  `,
  repoDesc: css`
    margin: 5px 0 0 20px;
    font-size: 11px;
    color: ${token.colorTextTertiary};
  `,
  repoStats: css`
    display: flex;
    gap: 12px;
    margin-top: 8px;
    margin-left: 20px;
    flex-wrap: wrap;
  `,
  langSection: css`
    margin-top: 14px;
    padding-top: 12px;
    border-top: 1px solid rgba(255,255,255,0.05);
  `,
  langLabel: css`
    margin: 0 0 7px;
    font-size: 10px;
    font-weight: 600;
    color: ${token.colorTextQuaternary};
    font-family: 'JetBrains Mono', monospace;
    text-transform: uppercase;
    letter-spacing: 0.07em;
  `,
  langBar: css`
    display: flex;
    height: 8px;
    border-radius: 4px;
    overflow: hidden;
    gap: 1px;
  `,
  langList: css`
    display: flex;
    flex-wrap: wrap;
    gap: 6px 14px;
    margin-top: 10px;
  `,
  tabContent: css`
    min-height: 280px;
  `,
  tabLoading: css`
    padding: 36px 18px;
    text-align: center;
    color: ${token.colorTextTertiary};
    font-size: 12px;
  `,
  tabEmpty: css`
    padding: 28px 0;
    text-align: center;
    color: ${token.colorTextTertiary};
    font-size: 12px;
  `,
  tabList: css`
    padding: 12px;
    display: flex;
    flex-direction: column;
    gap: 5px;
  `,
  commitRow: css`
    padding: 10px 12px;
    border-radius: ${token.borderRadius}px;
    background: rgba(255,255,255,0.02);
    border: 1px solid rgba(255,255,255,0.04);
    transition: background 0.1s;
    &:hover { background: rgba(255,255,255,0.04); }
  `,
  commitMsg: css`
    font-size: 12px;
    color: #cbd5e1;
    line-height: 1.5;
    flex: 1;
  `,
  commitMeta: css`
    display: flex;
    gap: 9px;
    margin-top: 4px;
    flex-wrap: wrap;
  `,
  commitSha: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    color: #6366f1;
    background: rgba(99,102,241,0.08);
    padding: 1px 5px;
    border-radius: 3px;
  `,
  branchRow: css`
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 10px 12px;
    border-radius: ${token.borderRadius}px;
    transition: background 0.1s;
  `,
  branchIcon: css`
    width: 26px;
    height: 26px;
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
  `,
  branchName: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    color: #e2e8f0;
    display: block;
  `,
  branchSha: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    color: #374151;
  `,
  prRow: css`
    padding: 10px 12px;
    border-radius: ${token.borderRadius}px;
    background: rgba(255,255,255,0.02);
    border: 1px solid rgba(255,255,255,0.04);
    transition: background 0.1s;
    &:hover { background: rgba(255,255,255,0.04); }
  `,
  prTitle: css`
    font-size: 12px;
    color: #cbd5e1;
  `,
  prMeta: css`
    display: flex;
    gap: 7px;
    margin-top: 4px;
    flex-wrap: wrap;
    align-items: center;
  `,
  prAvatar: css`
    width: 13px;
    height: 13px;
    border-radius: 50%;
  `,
  releaseRow: css`
    padding: 12px 14px;
    border-radius: ${token.borderRadius}px;
    transition: background 0.1s;
  `,
  releaseTag: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    font-weight: 600;
    color: #e2e8f0;
  `,
  releaseBody: css`
    margin: 8px 0 0 19px;
    font-size: 11px;
    color: ${token.colorTextTertiary};
    font-family: 'JetBrains Mono', monospace;
    white-space: pre-wrap;
    word-break: break-word;
    line-height: 1.6;
    max-height: 70px;
    overflow: hidden;
  `,
  runRow: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 12px;
    border-radius: ${token.borderRadius}px;
    background: rgba(255,255,255,0.02);
    border: 1px solid rgba(255,255,255,0.04);
    transition: background 0.1s;
    &:hover { background: rgba(255,255,255,0.04); }
  `,
  runIcon: css`
    width: 26px;
    height: 26px;
    border-radius: 6px;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
  `,
  runName: css`
    font-size: 12px;
    color: #cbd5e1;
    display: block;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  `,
  runMeta: css`
    display: flex;
    gap: 7px;
    margin-top: 3px;
    flex-wrap: wrap;
  `,
  fileTree: css`
    overflow-y: auto;
    max-height: 520px;
  `,
  fileTreeEntry: css`
    width: 100%;
    text-align: left;
    display: flex;
    align-items: center;
    gap: 6px;
    background: none;
    border: none;
    cursor: pointer;
    border-radius: 6px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    transition: background 0.1s;
    &:hover { background: rgba(255,255,255,0.04); }
  `,
  fileSize: css`
    margin-left: auto;
    font-size: 10px;
    color: ${token.colorTextQuaternary};
  `,
  fileViewerHeader: css`
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 16px;
    border-bottom: 1px solid rgba(255,255,255,0.06);
  `,
  fileViewerPath: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 11px;
    color: ${token.colorTextTertiary};
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  `,
  fileViewerBody: css`
    padding: 16px;
    overflow: auto;
    max-height: 500px;
  `,
  fileViewerPre: css`
    margin: 0;
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    line-height: 1.7;
    color: #e2e8f0;
    white-space: pre-wrap;
    word-break: break-all;
  `,
  modalTitle: css`
    font-family: 'JetBrains Mono', monospace;
    color: #e2e8f0;
  `,
  metaText: css`
    font-size: 10px;
    color: ${token.colorTextTertiary};
  `,
  metaTextFaint: css`
    font-size: 10px;
    color: #374151;
  `,
  monoText: css`
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    color: ${token.colorTextTertiary};
  `,
  externalLink: css`
    color: ${token.colorTextQuaternary};
    flex-shrink: 0;
    transition: color 0.1s;
    &:hover { color: ${token.colorTextTertiary}; }
  `,
  badge: css`
    font-size: 9px;
    padding: 2px 6px;
    border-radius: 3px;
    font-weight: 700;
  `,
  spinning: css`
    animation: spin 1s linear infinite;
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
  `,
  repoBadge: css`
    font-size: 9px;
    padding: 2px 6px;
    border-radius: 4px;
    font-weight: 700;
  `,
  repoBranchBadge: css`
    font-size: 9px;
    padding: 2px 6px;
    border-radius: 4px;
    background: rgba(99,102,241,0.1);
    color: #6366f1;
    font-weight: 600;
  `,
}));