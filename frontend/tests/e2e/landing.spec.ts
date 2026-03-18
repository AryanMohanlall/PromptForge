import { test, expect } from "@playwright/test";

test.describe("Landing page", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/");
  });

  test("renders PromptForge logo and brand name in nav", async ({ page }) => {
    await expect(page.getByRole("navigation").getByText("PromptForge")).toBeVisible();
  });

  test("renders Sign in and Get Started nav buttons", async ({ page }) => {
    await expect(page.getByRole("link", { name: "Sign in" })).toBeVisible();
    await expect(page.getByRole("link", { name: "Get Started" })).toBeVisible();
  });

  test("Sign in and Get Started nav links point to /auth", async ({ page }) => {
    const signInLink = page.getByRole("link", { name: "Sign in" });
    const getStartedLink = page.getByRole("link", { name: "Get Started" });
    await expect(signInLink).toHaveAttribute("href", "/auth");
    await expect(getStartedLink).toHaveAttribute("href", "/auth");
  });

  test("renders hero tag and title", async ({ page }) => {
    await expect(page.getByText("prompt → code → deploy")).toBeVisible();
    await expect(page.getByRole("heading", { name: /From idea to/i })).toBeVisible();
    await expect(page.getByText("live app")).toBeVisible();
  });

  test("renders hero subtitle", async ({ page }) => {
    await expect(
      page.getByText("Describe your application in plain English")
    ).toBeVisible();
  });

  test("renders prompt input card with Generate App button", async ({ page }) => {
    await expect(page.getByPlaceholder(/Generate a project management tool/)).toBeVisible();
    await expect(page.getByRole("button", { name: "Generate App" })).toBeVisible();
  });

  test("renders all four feature cards", async ({ page }) => {
    await expect(page.getByText("Prompt-Driven")).toBeVisible();
    await expect(page.getByText("GitHub Integration")).toBeVisible();
    await expect(page.getByText("Auto Deploy")).toBeVisible();
    await expect(page.getByText("Iterative Refinement")).toBeVisible();
  });

  test("renders intelligent pipeline section title", async ({ page }) => {
    await expect(page.getByRole("heading", { name: /Intelligent/i })).toBeVisible();
    await expect(page.getByText("promptforge — generation pipeline")).toBeVisible();
  });

  test("renders pipeline steps", async ({ page }) => {
    await expect(page.getByText("Parse requirements")).toBeVisible();
    await expect(page.getByText("Generate frontend")).toBeVisible();
    await expect(page.getByText("Generate backend & API")).toBeVisible();
    await expect(page.getByText("Create GitHub repository")).toBeVisible();
    await expect(page.getByText("Push code & commit")).toBeVisible();
    await expect(page.getByText("Deploy to production")).toBeVisible();
  });

  test("renders footer with PromptForge branding", async ({ page }) => {
    await expect(page.getByText("Built for builders.")).toBeVisible();
  });
});
