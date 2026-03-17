"use client";

import { useState } from "react";
import { SearchIcon, ChevronDownIcon } from "lucide-react";
import { motion } from "framer-motion";
import { ProjectCard, ProjectData } from "../ProjectCard";
import { useStyles } from "./styles";

const SAMPLE_PROJECTS: ProjectData[] = [
  {
    id: "1",
    name: "TaskFlow Pro",
    status: "Live",
    framework: "Next.js",
    language: "TypeScript",
    updatedAt: "Updated 3 hours ago",
    url: "taskflow-pro.vercel.app",
  },
  {
    id: "2",
    name: "ShopEasy",
    status: "Generating",
    framework: "React + Vite",
    language: "TypeScript",
    updatedAt: "Updated 12 minutes ago",
  },
  {
    id: "3",
    name: "DevPortfolio",
    status: "Generated",
    framework: "Vue",
    language: "JavaScript",
    updatedAt: "Updated 1 day ago",
  },
  {
    id: "4",
    name: "MealPlanner",
    status: "Deploying",
    framework: "Angular",
    language: "TypeScript",
    updatedAt: "Updated 28 minutes ago",
  },
  {
    id: "5",
    name: "BugTracker",
    status: "Draft",
    framework: "Next.js",
    language: "TypeScript",
    updatedAt: "Updated 5 days ago",
  },
  {
    id: "6",
    name: "FitCoach",
    status: "Failed",
    framework: "React + Vite",
    language: "JavaScript",
    updatedAt: "Updated 2 hours ago",
  },
];

interface DashboardPageProps {
  onNavigate: (page: string) => void;
}

export function DashboardPage({ onNavigate }: DashboardPageProps) {
  const { styles, cx } = useStyles();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  const statuses = [
    "All",
    "Draft",
    "Generating",
    "Generated",
    "Deploying",
    "Live",
    "Failed",
  ];

  const filteredProjects = SAMPLE_PROJECTS.filter((project) => {
    const matchesSearch = project.name
      .toLowerCase()
      .includes(searchTerm.toLowerCase());
    const matchesStatus =
      statusFilter === "All" || project.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const containerVariants = {
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: { staggerChildren: 0.1 },
    },
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    show: {
      opacity: 1,
      y: 0,
      transition: {
        type: "spring",
        stiffness: 300,
        damping: 24,
      },
    },
  };

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <h1 className={styles.title}>My projects</h1>

        <div className={styles.actions}>
          <div className={styles.searchWrap}>
            <SearchIcon className={styles.searchIcon} />
            <input
              type="text"
              placeholder="Search projects..."
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
              className={cx(styles.searchInput, styles.focusRing)}
            />
          </div>

          <div className={styles.filterWrap}>
            <button
              type="button"
              onClick={() => setIsFilterOpen(!isFilterOpen)}
              className={cx(styles.filterButton, styles.focusRing)}
            >
              {statusFilter}
              <ChevronDownIcon className={styles.filterIcon} />
            </button>

            {isFilterOpen && (
              <div className={styles.filterMenu}>
                {statuses.map((status) => (
                  <button
                    key={status}
                    type="button"
                    onClick={() => {
                      setStatusFilter(status);
                      setIsFilterOpen(false);
                    }}
                    className={cx(
                      styles.filterItem,
                      statusFilter === status && styles.filterItemActive
                    )}
                  >
                    {status}
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {filteredProjects.length > 0 ? (
        <motion.div
          variants={containerVariants}
          initial="hidden"
          animate="show"
          className={styles.grid}
        >
          {filteredProjects.map((project) => (
            <motion.div key={project.id} variants={itemVariants}>
              <ProjectCard
                project={project}
                onView={() => onNavigate("generation")}
              />
            </motion.div>
          ))}
        </motion.div>
      ) : (
        <div className={styles.emptyState}>
          No projects found matching your criteria.
        </div>
      )}
    </div>
  );
}
