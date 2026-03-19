import { test, expect } from "@playwright/test";

const seedAuth = async (page: import("@playwright/test").Page) => {
  await page.addInitScript(() => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ accessToken: "test-token", userId: 1, expireInSeconds: 3600 })
    );
  });
};

const mockCreateSession = (page: import("@playwright/test").Page) =>
  page.route("**/api/services/app/CodeGen/CreateSession", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        result: {
          id: "session-1",
          userId: 1,
          projectId: null,
          projectName: "my-todo-app",
          prompt: "Build a todo app with authentication and drag-and-drop kanban boards",
          normalizedRequirement: "A todo application with auth and kanban",
          detectedFeatures: ["authentication", "kanban-board", "drag-and-drop"],
          detectedEntities: ["User", "Board", "Card"],
          confirmedStack: null,
          spec: null,
          specConfirmedAt: null,
          generationStartedAt: null,
          generationCompletedAt: null,
          status: 1,
          validationResults: [],
          scaffoldTemplate: "next-ts-antd-prisma",
          generatedFiles: [],
          repairAttempts: 0,
          createdAt: "2026-01-01T00:00:00Z",
          updatedAt: "2026-01-01T00:00:00Z",
        },
      }),
    })
  );

const mockRecommendStack = (page: import("@playwright/test").Page) =>
  page.route("**/api/services/app/CodeGen/RecommendStack**", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        result: {
          framework: "Next.js",
          language: "TypeScript",
          styling: "Ant Design",
          database: "PostgreSQL",
          orm: "Prisma",
          auth: "NextAuth.js",
          reasoning: {
            framework: "Best for SSR and full-stack",
            language: "Type safety for large apps",
            styling: "Rich component library",
            database: "Relational data fits well",
            orm: "Best TypeScript ORM",
            auth: "Seamless Next.js integration",
          },
        },
      }),
    })
  );

const mockSaveStack = (page: import("@playwright/test").Page) =>
  page.route("**/api/services/app/CodeGen/SaveStack", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        result: {
          id: "session-1",
          status: 2,
          confirmedStack: {
            framework: "Next.js",
            language: "TypeScript",
            styling: "Ant Design",
            database: "PostgreSQL",
            orm: "Prisma",
            auth: "NextAuth.js",
            reasoning: {},
          },
        },
      }),
    })
  );

test.describe("Generate page - unauthenticated", () => {
  test("redirects to /auth when not authenticated", async ({ page }) => {
    await page.goto("/generate");
    await page.waitForURL(/\/auth/, { timeout: 10000 });
    await expect(page).toHaveURL(/\/auth/);
  });
});

test.describe("Generate page - Capture step", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuth(page);
    await mockCreateSession(page);
  });

  test("renders the describe step with textarea", async ({ page }) => {
    await page.goto("/generate");
    await expect(page.getByText("Describe your application")).toBeVisible();
    await expect(
      page.getByPlaceholder(/tell us what you want to build/i)
    ).toBeVisible();
  });

  test("analyze button is disabled with short input", async ({ page }) => {
    await page.goto("/generate");
    await page.getByPlaceholder(/tell us what you want to build/i).fill("short");
    const analyzeBtn = page.getByRole("button", { name: /analyze/i });
    await expect(analyzeBtn).toBeDisabled();
  });

  test("analyze shows detected features and entities", async ({ page }) => {
    await page.goto("/generate");
    await page
      .getByPlaceholder(/tell us what you want to build/i)
      .fill("Build a todo app with authentication and drag-and-drop kanban boards");
    await page.getByRole("button", { name: /analyze/i }).click();

    await expect(page.getByText("Analysis Complete")).toBeVisible();
    await expect(page.getByText("authentication")).toBeVisible();
    await expect(page.getByText("kanban-board")).toBeVisible();
    await expect(page.getByText("User")).toBeVisible();
    await expect(page.getByText("Board")).toBeVisible();
    await expect(page.getByText("my-todo-app")).toBeVisible();
  });
});

test.describe("Generate page - Stack step", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuth(page);
    await mockCreateSession(page);
    await mockRecommendStack(page);
    await mockSaveStack(page);
  });

  test("navigates to stack step after capture", async ({ page }) => {
    await page.goto("/generate");

    await page
      .getByPlaceholder(/tell us what you want to build/i)
      .fill("Build a todo app with authentication and drag-and-drop kanban boards");
    await page.getByRole("button", { name: /analyze/i }).click();
    await expect(page.getByText("Analysis Complete")).toBeVisible();

    await page.getByRole("button", { name: /continue to stack/i }).click();

    await expect(page.getByText("Configure your stack")).toBeVisible();
    await expect(page.getByText("Next.js")).toBeVisible();
    await expect(page.getByText("TypeScript")).toBeVisible();
  });
});
