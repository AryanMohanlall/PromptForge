import { test, expect } from "@playwright/test";

test.describe("Auth pages", () => {
  test("signs in with mocked auth response", async ({ page }) => {
    await page.route("**/api/TokenAuth/Authenticate", route =>
      route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          result: { accessToken: "token-123", expireInSeconds: 3600, userId: 7 },
        }),
      })
    );

    await page.goto("/login");
    await page.getByPlaceholder("Email address").fill("user@example.com");
    await page.getByPlaceholder("Password").fill("Password1!");
    await Promise.all([
      page.waitForRequest("**/api/TokenAuth/Authenticate"),
      page.getByRole("button", { name: "Sign in" }).click(),
    ]);
    await expect(page.getByRole("button", { name: "Sign in" })).toBeVisible();
  });

  test("registers with tenant header when tenant param exists", async ({ page }) => {
    let tenantHeader: string | undefined;

    await page.route("**/api/services/app/Account/Register", route => {
      tenantHeader = route.request().headers()["abp.tenantid"];
      route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({ result: {} }),
      });
    });

    await page.route("**/api/TokenAuth/Authenticate", route =>
      route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          result: { accessToken: "token-123", expireInSeconds: 3600, userId: 7 },
        }),
      })
    );

    await page.goto("/register?tenant=Nw==");

    await page.getByPlaceholder("First name").fill("Jane");
    await page.getByPlaceholder("Surname").fill("Doe");
    await page.getByPlaceholder("Username").fill("jane.doe");
    await page.getByPlaceholder("Email address").fill("jane@example.com");
    await page.getByPlaceholder("Create a password").fill("Password1!");
    await page.getByPlaceholder("Confirm password").fill("Password1!");
    await Promise.all([
      page.waitForRequest("**/api/services/app/Account/Register"),
      page.getByRole("button", { name: "Create account" }).click(),
    ]);
    expect(tenantHeader).toBe("7");
  });
});
