declare module "js-cookie" {
  export interface CookieAttributes {
    domain?: string;
    expires?: number | Date;
    path?: string;
    sameSite?: "strict" | "lax" | "none";
    secure?: boolean;
  }

  export interface CookiesStatic {
    get(name: string): string | undefined;
    set(name: string, value: string, attributes?: CookieAttributes): string | undefined;
    remove(name: string, attributes?: CookieAttributes): void;
  }

  const Cookies: CookiesStatic;
  export default Cookies;
}