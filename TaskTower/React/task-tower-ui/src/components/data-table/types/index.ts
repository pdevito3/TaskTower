import { SortingState } from "@tanstack/react-table";

export interface PaginatedTableStore {
  setPageNumber: (page: number) => void;
  pageNumber: number;
  pageSize: number;
  setPageSize: (size: number) => void;
  sorting: SortingState;
  setSorting: React.Dispatch<React.SetStateAction<SortingState>>;
  initialPageSize: number;
  isFiltered: { result: () => boolean };
  resetFilters: () => void;
  queryKit: {
    filterValue: () => string;
  };
}
