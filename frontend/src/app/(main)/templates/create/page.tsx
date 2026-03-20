"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  Form,
  Input,
  Button,
  Select,
  Switch,
  Card,
  message,
  Space,
  Typography,
  Divider,
} from "antd";
import {
  useTemplateAction,
  useTemplateState,
} from "@/providers/templates-provider";
import { TemplateCategory } from "@/providers/templates-provider/context";
import {
  ProjectFramework,
  ProjectProgrammingLanguage,
  ProjectDatabaseOption,
} from "@/providers/projects-provider/context";
import { ArrowLeftIcon, SparklesIcon } from "lucide-react";

const { Title, Paragraph, Text } = Typography;
const { TextArea } = Input;

export default function CreateTemplatePage() {
  const router = useRouter();
  const [form] = Form.useForm();
  const { create } = useTemplateAction();
  const { isPending } = useTemplateState();
  const [isSubmitting, setIsSubmitting] = useState(false);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const onFinish = async (values: any) => {
    setIsSubmitting(true);
    try {
      await create({
        ...values,
        version: "1.0.0",
        isFeatured: false,
      });
      message.success("Template shared with the community!");
      router.push("/templates");
    } catch (error) {
      console.error(error);
      message.error("Failed to create template.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div style={{ padding: "40px 24px", maxWidth: "900px", margin: "0 auto" }}>
      <Button
        type="link"
        icon={<ArrowLeftIcon size={16} />}
        onClick={() => router.push("/templates")}
        style={{ marginBottom: 24, padding: 0, color: "#666" }}
      >
        Back to Marketplace
      </Button>

      <div style={{ marginBottom: 40, textAlign: "center" }}>
        <Title level={1}>Share your project structure</Title>
        <Paragraph style={{ fontSize: "16px", color: "#666" }}>
          Help others start their journey by providing a solid foundation.
        </Paragraph>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        requiredMark={false}
        initialValues={{
          framework: ProjectFramework.NextJS,
          language: ProjectProgrammingLanguage.TypeScript,
          database: ProjectDatabaseOption.RenderPostgres,
          category: TemplateCategory.AppsAndGames,
          includesAuth: true,
        }}
      >
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(400px, 1fr))",
            gap: "24px",
          }}
        >
          <Card
            title={
              <Space>
                <SparklesIcon size={18} />
                <span>Basic Details</span>
              </Space>
            }
          >
            <Form.Item
              name="name"
              label="What's the name of this template?"
              rules={[{ required: true, message: "A name is required" }]}
            >
              <Input size="large" placeholder="e.g. Clean SaaS Architecture" />
            </Form.Item>

            <Form.Item name="description" label="Brief description">
              <TextArea
                rows={4}
                placeholder="Explain what this template includes and who it's for."
              />
            </Form.Item>

            <Form.Item
              name="category"
              label="Best category"
              rules={[{ required: true }]}
            >
              <Select
                size="large"
                options={Object.entries(TemplateCategory)
                  .filter(([key]) => isNaN(Number(key)))
                  .map(([key, value]) => ({ label: key, value }))}
              />
            </Form.Item>
          </Card>

          <Card title="Tech Stack & Config">
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "1fr 1fr",
                gap: "16px",
              }}
            >
              <Form.Item name="framework" label="Framework">
                <Select
                  options={Object.entries(ProjectFramework)
                    .filter(([key]) => isNaN(Number(key)))
                    .map(([key, value]) => ({ label: key, value }))}
                />
              </Form.Item>

              <Form.Item name="language" label="Language">
                <Select
                  options={Object.entries(ProjectProgrammingLanguage)
                    .filter(([key]) => isNaN(Number(key)))
                    .map(([key, value]) => ({ label: key, value }))}
                />
              </Form.Item>
            </div>

            <Form.Item name="database" label="Preferred Database">
              <Select
                options={Object.entries(ProjectDatabaseOption)
                  .filter(([key]) => isNaN(Number(key)))
                  .map(([key, value]) => ({ label: key, value }))}
              />
            </Form.Item>

            <Form.Item
              name="includesAuth"
              label="Pre-configured Auth"
              valuePropName="checked"
            >
              <Switch />
            </Form.Item>

            <Divider />

            <Form.Item name="tags" label="Tags (comma separated)">
              <Input placeholder="saas, productivity, clean-code" />
            </Form.Item>
          </Card>
        </div>

        <Card title="Blueprint (Scaffold Config)" style={{ marginTop: 24 }}>
          <Text type="secondary" style={{ display: "block", marginBottom: 16 }}>
            Paste the JSON configuration that defines the folder structure and
            dependencies. This is what the AI will use to build the app.
          </Text>
          <Form.Item name="scaffoldConfig">
            <TextArea
              rows={8}
              style={{ fontFamily: "monospace" }}
              placeholder='{ "structure": ["src/app", "src/components"], "deps": { "antd": "latest" } }'
            />
          </Form.Item>
        </Card>

        <Card title="Presentation (Optional)" style={{ marginTop: 24 }}>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr",
              gap: "16px",
            }}
          >
            <Form.Item name="thumbnailUrl" label="Thumbnail Image URL">
              <Input placeholder="https://..." />
            </Form.Item>

            <Form.Item name="previewUrl" label="Live Demo URL">
              <Input placeholder="https://..." />
            </Form.Item>
          </div>
        </Card>

        <div style={{ marginTop: 40, textAlign: "center" }}>
          <Space size="large">
            <Button
              type="primary"
              size="large"
              htmlType="submit"
              loading={isSubmitting || isPending}
              style={{ minWidth: 200, height: 48 }}
            >
              Publish Template
            </Button>
            <Button
              size="large"
              style={{ minWidth: 120, height: 48 }}
              onClick={() => router.push("/templates")}
            >
              Cancel
            </Button>
          </Space>
        </div>
      </Form>
    </div>
  );
}
