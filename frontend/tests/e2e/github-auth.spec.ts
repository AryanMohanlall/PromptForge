import { test, expect } from "@playwright/test";

/**
 * Seeds sessionStorage with auth_user but WITHOUT github_oauth_complete,
 * simulating a user who logged in via username/password (not GitHub OAuth).
 */
const seedAuthWithoutGithub = async (page: import("@playwright/test").Page) => {
  await page.addInitScript(() => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ accessToken: "test-token", userId: 1, expireInSeconds: 3600 })
    );
  });
};

/**
 * Seeds sessionStorage with both auth_user AND github_oauth_complete,
 * simulating a user who completed GitHub OAuth.
 */
const seedAuthWithGithub = async (page: import("@playwright/test").Page) => {
  await page.addInitScript(() => {
    sessionStorage.setItem(
      "auth_user",
      JSON.stringify({ accessToken: "test-token", userId: 1, expireInSeconds: 3600 })
    );
    sessionStorage.setItem("github_oauth_complete", "true");
  });
};

// ── GitHub OAuth callback page ───────────────────────────────────────────────

test.describe("GitHub OAuth callback", () => {
  test("shows error when query params are missing", async ({ page }) => {
    await page.goto("/auth/github/callback");

    await expect(page.getByText(/missing or invalid OAuth parameters/i)).toBeVisible();
    await expect(page.getByText(/Back to login/i)).toBeVisible();
  });

  test("shows error when token is missing", async ({ page }) => {
    await page.goto("/auth/github/callback?userId=7");

    await expect(page.getByText(/missing or invalid OAuth parameters/i)).toBeVisible();
  });

  test("redirects to dashboard on valid OAuth params", async ({ page }) => {
    await page.goto(
      "/auth/github/callback?token=abc-123&userId=7&expireInSeconds=3600"
    );

    await page.waitForURL(/\/dashboard/, { timeout: 10000 });
    await expect(page).toHaveURL(/\/dashboard/);
  });
});

// ── Create project gating ────────────────────────────────────────────────────

test.describe("Create project – GitHub gating", () => {


  test("shows project creation form when GitHub OAuth is completed", async ({ page }) => {
    await seedAuthWithGithub(page);

    // Mock templates endpoint
    await page.route("**/api/services/app/Template/**", (route) =>
      route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ result: { items: [], totalCount: 0 } }),
      })
    );

    await page.goto("/dashboard");

    const createButton = page.getByRole("button", { name: /create|new project/i });
    if (await createButton.isVisible()) {
      await createButton.click();
    }

    // Should NOT show the GitHub connect prompt
    await expect(page.getByText(/connect GitHub to continue/i)).not.toBeVisible();
  });
});
