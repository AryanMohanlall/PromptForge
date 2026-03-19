"use client";

import { useEffect, useMemo, useState } from "react";
import { Button, Card, Empty, Input, Select, Spin, Tag } from "antd";
import { useTemplateAction, useTemplateState } from "@/providers/templates-provider";
import { useStyles } from "./styles/style";

export default function TemplatesPage() {
  const { styles } = useStyles();
  const { fetchAll } = useTemplateAction();
  const { items, isPending, isError } = useTemplateState();
  const [searchTerm, setSearchTerm] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<string>("all");

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  const categories = useMemo(() => {
    const categoryOptions = Array.from(
      new Set(items.map((template) => template.category).filter(Boolean)),
    ).sort((first, second) => first.localeCompare(second));

    return [{ label: "All categories", value: "all" }].concat(
      categoryOptions.map((category) => ({ label: category, value: category })),
    );
  }, [items]);

  const filteredItems = useMemo(() => {
    const query = searchTerm.trim().toLowerCase();

    return items.filter((template) => {
      const matchesCategory =
        categoryFilter === "all" || template.category === categoryFilter;

      if (!matchesCategory) {
        return false;
      }

      if (!query) {
        return true;
      }

      const content = [
        template.name,
        template.slug,
        template.description,
        template.category,
        template.tags,
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
          <h1 className={styles.title}>Templates</h1>
          <p className={styles.subtitle}>
            Browse backend templates and use them as starting points for generation.
          </p>
        </div>

        <div className={styles.controls}>
          <Input.Search
            allowClear
            placeholder="Search templates"
            className={styles.search}
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
          />
          <Select
            value={categoryFilter}
            options={categories}
            onChange={(value) => setCategoryFilter(value)}
          />
          <Button onClick={() => fetchAll()} loading={isPending}>
            Refresh
          </Button>
        </div>
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
              description="No templates match your filters."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          </div>
        </Card>
      ) : (
        <div className={styles.grid}>
          {filteredItems.map((template) => {
            const tagList = (template.tags ?? "")
              .split(",")
              .map((tag) => tag.trim())
              .filter(Boolean)
              .slice(0, 4);

            return (
              <Card key={template.id} className={styles.templateCard}>
                <div className={styles.templateHeader}>
                  <h2 className={styles.templateName}>{template.name}</h2>
                  <Tag>{template.category}</Tag>
                </div>

                <p className={styles.description}>
                  {template.description?.trim() || "No description provided."}
                </p>

                <div className={styles.metaGrid}>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Author</span>
                    <span className={styles.metaValue}>{template.author || "Unknown"}</span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Slug</span>
                    <span className={styles.metaValue}>{template.slug}</span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Likes</span>
                    <span className={styles.metaValue}>{template.likeCount ?? 0}</span>
                  </div>
                  <div className={styles.metaItem}>
                    <span className={styles.metaLabel}>Views</span>
                    <span className={styles.metaValue}>{template.viewCount ?? 0}</span>
                  </div>
                </div>

                {tagList.length > 0 && (
                  <div className={styles.tagsRow}>
                    {tagList.map((tag) => (
                      <Tag key={`${template.id}-${tag}`}>{tag}</Tag>
                    ))}
                  </div>
                )}

                {template.sourceUrl && (
                  <a
                    href={template.sourceUrl}
                    target="_blank"
                    rel="noreferrer"
                    className={styles.sourceLink}
                  >
                    View source
                  </a>
                )}
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
