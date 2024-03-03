const JobKeys = {
  all: ["Jobs"] as const,
  lists: () => [...JobKeys.all, "list"] as const,
  list: (queryParams: string) => [...JobKeys.lists(), { queryParams }] as const,
  details: () => [...JobKeys.all, "detail"] as const,
  detail: (accessionId: string) => [...JobKeys.details(), accessionId] as const,
};

export { JobKeys };
