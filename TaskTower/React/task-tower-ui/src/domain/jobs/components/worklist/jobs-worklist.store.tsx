import { PaginatedTableStore } from "@/components/data-table/types";
import { create } from "zustand";

// Assuming the types for your jobs' sorting and pagination logic are defined elsewhere
// You might need to adapt these types according to your actual data structure and requirements
interface JobsTableStore extends PaginatedTableStore {
  status: string[];
  addStatus: (status: string) => void;
  removeStatus: (status: string) => void;
  clearStatus: () => void;
  queue: string[];
  addQueue: (queue: string) => void;
  removeQueue: (queue: string) => void;
  clearQueue: () => void;
  filterInput: string | null;
  setFilterInput: (f: string | null) => void;
}

export const useJobsTableStore = create<JobsTableStore>((set, get) => ({
  initialPageSize: 10,
  pageNumber: 1,
  setPageNumber: (page) => set({ pageNumber: page }),
  pageSize: 10,
  setPageSize: (size) => set({ pageSize: size }),
  sorting: [],
  setSorting: (sortOrUpdater) => {
    if (typeof sortOrUpdater === "function") {
      set((prevState) => ({ sorting: sortOrUpdater(prevState.sorting) }));
    } else {
      set({ sorting: sortOrUpdater });
    }
  },
  status: [],
  addStatus: (status) =>
    set((prevState) => ({ status: [...prevState.status, status] })),
  removeStatus: (status) =>
    set((prevState) => ({
      status: prevState.status.filter((s) => s !== status),
    })),
  clearStatus: () => set({ status: [] }),
  queue: [],
  addQueue: (queue) =>
    set((prevState) => ({ queue: [...prevState.queue, queue] })),
  removeQueue: (queue) =>
    set((prevState) => ({
      queue: prevState.queue.filter((s) => s !== queue),
    })),
  clearQueue: () => set({ queue: [] }),
  filterInput: null,
  setFilterInput: (f) => set({ filterInput: f }),
  isFiltered: {
    result: () =>
      get().status.length > 0 ||
      (get().filterInput?.length ?? 0) > 0 ||
      get().queue.length > 0,
  },
  resetFilters: () => set({ status: [], filterInput: null, queue: [] }),
  queryKit: {
    // filterValue: () => {
    //   const statusFilter = get()
    //     .status.map((status) => `status == "${status}"`)
    //     .join(" || ");
    //   const jobIdFilter = get().filterInput
    //     ? `jobId @=* "${get().filterInput}"`
    //     : "";
    //   if (statusFilter && jobIdFilter) {
    //     return `${statusFilter} && ${jobIdFilter}`;
    //   }
    //   if (statusFilter.length > 0) return statusFilter;
    //   if (jobIdFilter.length > 0) return jobIdFilter;
    //   return "";
    // },
    filterValue: () => {
      return "";
    },
    filterText: () => {
      const jobIdFilter = get().filterInput ? `${get().filterInput}` : "";
      if (jobIdFilter.length > 0) return jobIdFilter;
      return "";
    },
  },
}));
