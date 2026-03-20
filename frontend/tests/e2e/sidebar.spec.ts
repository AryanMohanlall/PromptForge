import { test, expect } from "@playwright/test";

// Seed sessionStorage with a valid auth token so the sidebar renders
const seedAuth = async (page: import("@playwright/test").Page) => {
  await page.addInitScript(() => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ accessToken: "test-token", userId: 1, expireInSeconds: 3600 })
    );
  });
};

test.describe("Sidebar navigation", () => {
  test.beforeEach(async ({ page }) => {
    // Seed authentication before navigating to protected pages
    await seedAuth(page);
    // Navigate to dashboard where the sidebar is rendered
    await page.goto("/dashboard");
  });

  test("has a Settings nav item", async ({ page }) => {
    const settingsButton = page.getByRole("button", { name: "Settings" });
    await expect(settingsButton).toBeVisible();
  });

  test("clicking Settings navigates to /settings", async ({ page }) => {
    const settingsButton = page.getByRole("button", { name: "Settings" });
    await settingsButton.click();
    await expect(page).toHaveURL(/\/settings$/);
    await expect(page.getByRole("heading", { name: "Settings" })).toBeVisible();
  });

  test("sidebar nav button shows gradient on hover and active state", async ({ page }) => {
    const settingsButton = page.getByRole("button", { name: "Settings" });

    await settingsButton.hover();
    await expect(settingsButton).toHaveCSS("background-image", /linear-gradient/);

    await settingsButton.click();
    await expect(settingsButton).toHaveCSS("background-image", /linear-gradient/);
  });
});
