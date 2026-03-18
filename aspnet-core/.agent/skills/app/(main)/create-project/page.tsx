"use client";

import { useRouter } from "next/navigation";
import { CreateProjectPage } from "@/components/create-project/CreateProjectPage";
import { useStyles } from "./styles/style";

export default function CreateProject() {
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
      <CreateProjectPage onNavigate={handleNavigate} />
    </div>
  );
}
