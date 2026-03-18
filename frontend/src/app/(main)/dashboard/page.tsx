"use client";

import { useRouter } from "next/navigation";
import { DashboardPage } from "@/components/dashboard/DashboardPage";
import { useStyles } from "./styles/style";

export default function Dashboard() {
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
      <DashboardPage onNavigate={handleNavigate} />
    </div>
  );
}
