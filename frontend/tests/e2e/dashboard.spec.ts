import { test, expect } from "@playwright/test";

// Seed sessionStorage with a valid auth token so the protected layout renders.
const seedAuth = async (page: import("@playwright/test").Page) => {
  await page.addInitScript(() => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ accessToken: "test-token", userId: 1, expireInSeconds: 3600 })
    );
  });
};

const mockProjectsEndpoint = (
  page: import("@playwright/test").Page,
  items: object[],
  totalCount = items.length
) =>
  page.route("**/api/services/app/Project/GetAll", route =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ result: { items, totalCount } }),
    })
  );

const sampleProjects = [
  {
    id: 1,
    workspaceId: 1,
    name: "My Next App",
    prompt: "A demo app",
    promptVersion: 1,
    framework: 1, // NextJS
    language: 1, // TypeScript
    databaseOption: 1,
    includeAuth: false,
    status: 5, // Deployed -> "Live"
    createdAt: "2024-01-01T00:00:00Z",
    updatedAt: "2024-06-01T12:00:00Z",
  },
  {
    id: 2,
    workspaceId: 1,
    name: "React Dashboard",
    prompt: "A dashboard",
    promptVersion: 1,
    framework: 2, // ReactVite
    language: 2, // JavaScript
    databaseOption: 1,
    includeAuth: false,
    status: 1, // Draft
    createdAt: "2024-02-01T00:00:00Z",
    updatedAt: "2024-06-02T08:00:00Z",
  },
  {
    id: 3,
    workspaceId: 1,
    name: "Angular Portal",
    prompt: "A portal",
    promptVersion: 1,
    framework: 3, // Angular
    language: 1, // TypeScript
    databaseOption: 1,
    includeAuth: true,
    status: 6, // Failed
    createdAt: "2024-03-01T00:00:00Z",
    updatedAt: "2024-06-03T10:00:00Z",
  },
];

test.describe("Dashboard page - unauthenticated", () => {
  test("redirects to /auth when not authenticated", async ({ page }) => {
    await page.goto("/dashboard");
    await page.waitForURL(/\/auth/, { timeout: 10000 });
    await expect(page).toHaveURL(/\/auth/);
  });
});

test.describe("Dashboard page", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuth(page);
  });

  test("renders My projects heading", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByRole("heading", { name: "My projects" })).toBeVisible();
  });

  test("shows loading state while projects are being fetched", async ({ page }) => {
    // Delay the API response so we can catch the loading state
    await page.route("**/api/services/app/Project/GetAll", async route => {
      await new Promise(resolve => setTimeout(resolve, 500));
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ result: { items: [], totalCount: 0 } }),
      });
    });
    await page.goto("/dashboard");
    await expect(page.getByText("Loading projects...")).toBeVisible();
  });

  test("renders project cards from API response", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByText("My Next App")).toBeVisible();
    await expect(page.getByText("React Dashboard")).toBeVisible();
    await expect(page.getByText("Angular Portal")).toBeVisible();
  });

  test("renders project card with framework and language info", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByText("Next.js · TypeScript")).toBeVisible();
    await expect(page.getByText("React + Vite · JavaScript")).toBeVisible();
  });

  test("renders correct status badges", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByText("Live", { exact: true }).first()).toBeVisible();
    await expect(page.getByText("Draft", { exact: true })).toBeVisible();
    await expect(page.getByText("Failed", { exact: true })).toBeVisible();
  });

  test("shows empty state when no projects are returned", async ({ page }) => {
    await mockProjectsEndpoint(page, []);
    await page.goto("/dashboard");
    await expect(page.getByText("No projects found matching your criteria.")).toBeVisible();
  });

  test("shows error state when API fails", async ({ page }) => {
    await page.route("**/api/services/app/Project/GetAll", route =>
      route.fulfill({ status: 500, body: "Internal Server Error" })
    );
    await page.goto("/dashboard");
    await expect(
      page.getByText("Failed to load projects. Please try again.")
    ).toBeVisible();
  });

  test("renders search input", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByPlaceholder("Search projects...")).toBeVisible();
  });

  test("search filters projects by name", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByPlaceholder("Search projects...").fill("React");

    await expect(page.getByText("React Dashboard")).toBeVisible();
    await expect(page.getByText("My Next App")).not.toBeVisible();
    await expect(page.getByText("Angular Portal")).not.toBeVisible();
  });

  test("search is case insensitive", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByPlaceholder("Search projects...").fill("next");
    await expect(page.getByText("My Next App")).toBeVisible();
  });

  test("search with no match shows empty state", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByPlaceholder("Search projects...").fill("zzznomatch");
    await expect(page.getByText("No projects found matching your criteria.")).toBeVisible();
  });

  test("status filter dropdown opens when clicked", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByRole("button", { name: /All/i }).click();
    await expect(page.getByRole("button", { name: "Draft" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Live" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Failed" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Generating" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Generated" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Deploying" })).toBeVisible();
  });

  test("status filter filters projects by status", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByRole("button", { name: /All/i }).click();
    await page.getByRole("button", { name: "Draft" }).click();

    await expect(page.getByText("React Dashboard")).toBeVisible();
    await expect(page.getByText("My Next App")).not.toBeVisible();
    await expect(page.getByText("Angular Portal")).not.toBeVisible();
  });

  test("status filter Live shows only live projects", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByRole("button", { name: /All/i }).click();
    await page.getByRole("button", { name: "Live" }).click();

    await expect(page.getByText("My Next App")).toBeVisible();
    await expect(page.getByText("React Dashboard")).not.toBeVisible();
    await expect(page.getByText("Angular Portal")).not.toBeVisible();
  });

  test("selecting a filter closes the dropdown", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");

    await page.getByRole("button", { name: /All/i }).click();
    await page.getByRole("button", { name: "Failed" }).click();

    // Dropdown items should no longer be visible
    await expect(page.getByRole("button", { name: "Draft" })).not.toBeVisible();
  });

  test("project card shows Open button", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    const openButtons = page.getByRole("button", { name: "Open" });
    await expect(openButtons.first()).toBeVisible();
  });

  test("project card shows No live URL yet when url is absent", async ({ page }) => {
    await mockProjectsEndpoint(page, sampleProjects);
    await page.goto("/dashboard");
    await expect(page.getByText("No live URL yet").first()).toBeVisible();
  });
});
