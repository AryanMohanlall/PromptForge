"use client";

import { useRouter } from "next/navigation";
import { GenerationPage } from "@/components/generation/GenerationPage";
import { useStyles } from "./styles/style";

export default function Generation() {
  const { styles } = useStyles();
  const router = useRouter();

  const handleNavigate = (page: string) => {
    if (page === "generation") {
      router.push("/generation");
      return;
    }

    router.push(`/${page}`);
  };

  return (
    <div className={styles.page}>
      <GenerationPage onNavigate={handleNavigate} />
    </div>
  );
}
