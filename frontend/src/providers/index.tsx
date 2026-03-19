"use client";

import { AuthProvider } from "./auth-provider";
import { ProjectProvider } from "./projects-provider";
import { TemplateProvider } from "./templates-provider";
import { CodeGenProvider } from "./codegen-provider";

interface AppProvidersProps {
  children: React.ReactNode;
}

export const AppProviders: React.FC<AppProvidersProps> = ({ children }) => {
  return (
    <AuthProvider>
      <ProjectProvider>
        <TemplateProvider>
          <CodeGenProvider>{children}</CodeGenProvider>
        </TemplateProvider>
      </ProjectProvider>
    </AuthProvider>
  );
};
