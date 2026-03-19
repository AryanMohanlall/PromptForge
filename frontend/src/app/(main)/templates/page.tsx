"use client";

import { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Empty,
  Input,
  Select,
  Spin,
  Tag,
  Tabs,
  message,
} from "antd";
import { PlusIcon, HeartIcon } from "lucide-react";
import {
  useTemplateAction,
  useTemplateState,
} from "@/providers/templates-provider";
import { useStyles } from "./styles/style";
import { TemplateCategory } from "@/providers/templates-provider/context";
import { useRouter } from "next/navigation";

export default function TemplatesPage() {
  const { styles } = useStyles();
  const router = useRouter();
  const { fetchAll, toggleFavorite } = useTemplateAction();
  const { items, isPending, isError } = useTemplateState();
  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");
  const [activeTab, setActiveTab] = useState("marketplace");

  useEffect(() => {
    fetchAll({
      isMyTemplates: activeTab === "my-templates",
    });
  }, [fetchAll, activeTab]);

  const categories = useMemo(() => {
    const categoryOptions = Object.entries(TemplateCategory)
      .filter(([key]) => isNaN(Number(key)))
      .map(([key, value]) => ({ label: key, value: String(value) }));

    return [{ label: "All categories", value: "all" }].concat(categoryOptions);
  }, []);

  const handleToggleFavorite = async (id: number) => {
    try {
      await toggleFavorite(id);
      message.success("Collection updated");
    } catch (error) {
      message.error(
        "Failed to update collection" +
          (error instanceof Error ? `: ${error.message}` : ""),
      );
    }
  };

  const filteredItems = useMemo(() => {
    const query = searchTerm.trim().toLowerCase();

    return items.filter((template) => {
      const matchesCategory =
        categoryFilter === "all" ||
        String(template.category) === categoryFilter;

      if (!matchesCategory) {
        return false;
      }

      if (!query) {
        return true;
      }

      const content = [
        template.name,
        template.description,
        template.categoryName,
        template.tags?.join(" "),
        template.author,
      ]
        .filter(Boolean)
        .join(" ")
        .toLowerCase();

      return content.includes(query);
    });
  }, [categoryFilter, items, searchTerm]);

  return (
    <div className={styles.page}>
      <div className={styles.toolbar}>
        <div className={styles.titleWrap}>
          <h1 className={styles.title}>Templates Marketplace</h1>
          <p className={styles.subtitle}>
            Discover and share project structures with the community.
          </p>
        </div>

        <div className={styles.controls}>
          <Button
            type="primary"
            icon={<PlusIcon size={16} />}
            onClick={() => router.push("/templates/create")}
          >
            Add Template
          </Button>
        </div>
      </div>

      <div style={{ marginBottom: 24 }}>
        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={[
            { key: "marketplace", label: "Marketplace" },
            { key: "my-templates", label: "My Templates" },
          ]}
        />
      </div>

      <div className={styles.controls} style={{ marginBottom: 24 }}>
        <Input.Search
          allowClear
          placeholder="Search templates..."
          className={styles.search}
          value={searchTerm}
          onChange={(event) => setSearchTerm(event.target.value)}
        />
        <Select
          style={{ minWidth: 160 }}
          value={categoryFilter}
          options={categories}
          onChange={(value) => setCategoryFilter(value)}
        />
        <Button
          onClick={() =>
            fetchAll({ isMyTemplates: activeTab === "my-templates" })
          }
          loading={isPending}
        >
          Refresh
        </Button>
      </div>

      {isPending && items.length === 0 ? (
        <Card className={styles.stateCard}>
          <div className={styles.stateInner}>
            <Spin size="large" />
          </div>
        </Card>
      ) : isError && items.length === 0 ? (
        <Card className={styles.stateCard}>
          <div className={styles.stateInner}>
            <Empty
              description="Could not load templates."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          </div>
        </Card>
      ) : filteredItems.length === 0 ? (
        <Card className={styles.stateCard}>
          <div className={styles.stateInner}>
            <Empty
              description={
                activeTab === "my-templates"
                  ? "You haven't created any templates yet."
                  : "No templates match your filters."
              }
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          </div>
        </Card>
      ) : (
        <div className={styles.grid}>
          {filteredItems.map((template) => {
            const tagList = template.tags.slice(0, 4);

            return (
              <Card key={template.id} className={styles.templateCard}>
                <div className={styles.templateHeader}>
                  <h2 className={styles.templateName}>{template.name}</h2>
                  <div
                    style={{ display: "flex", alignItems: "center", gap: 8 }}
                  >
                    <Tag color="blue">{template.categoryName}</Tag>
                    <Button
                      type="text"
                      icon={
                        <HeartIcon
                          size={18}
                          fill={template.isFavorite ? "#ff4d4f" : "none"}
                          color={
                            template.isFavorite ? "#ff4d4f" : "currentColor"
                          }
                        />
                      }
                      onClick={() => handleToggleFavorite(template.id)}
                    />
                  </div>
                </div>

                <p className={styles.description}>
                  {template.description?.trim() || "No description provided."}
                </p>

                <div className={styles.metaGrid}>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Author</span>
                    <span className={styles.metaValue}>
                      {template.author || "Unknown"}
                    </span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Version</span>
                    <span className={styles.metaValue}>{template.version}</span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Forks</span>
                    <span className={styles.metaValue}>
                      {template.forkCount}
                    </span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Auth</span>
                    <span className={styles.metaValue}>
                      {template.includesAuth ? "Included" : "None"}
                    </span>
                  </div>
                </div>

                {tagList.length > 0 && (
                  <div className={styles.tagsRow}>
                    {tagList.map((tag) => (
                      <Tag key={`${template.id}-${tag}`}>{tag}</Tag>
                    ))}
                  </div>
                )}

                <div
                  style={{
                    marginTop: "auto",
                    display: "flex",
                    gap: 8,
                    paddingTop: 16,
                  }}
                >
                  {template.previewUrl && (
                    <Button
                      type="link"
                      href={template.previewUrl}
                      target="_blank"
                      style={{ padding: 0 }}
                    >
                      Live Preview
                    </Button>
                  )}
                  <Button
                    type="primary"
                    size="small"
                    style={{ marginLeft: "auto" }}
                    onClick={() => {
                      message.info("Template selected for next project");
                      // Implementation for setting preference can be added here
                    }}
                  >
                    Use Template
                  </Button>
                </div>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
