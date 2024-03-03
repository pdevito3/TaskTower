import { Button } from "@/components/ui/button";
import { Pagination } from "@/types/apis";
import { cn } from "@/utils";
import {
  Dropdown,
  DropdownItem,
  DropdownMenu,
  DropdownTrigger,
} from "@nextui-org/react";
import {
  ArrowLeftToLine,
  ArrowRightFromLine,
  ChevronLeftIcon,
  ChevronRightIcon,
} from "lucide-react";
import { useEffect, useState } from "react";

interface PaginationControlsProps {
  entityPlural: string;
  pageNumber: number;
  apiPagination: Pagination | undefined;
  pageSize: number;
  setPageSize: (size: number) => void;
  setPageNumber: (page: number) => void;
  className?: string;
  orientation?: "between" | "left" | "right";
}

const PageSizeOptions = [10, 20, 50, 100, 1000, 2000, 5000] as const;
// type PageSize = (typeof PageSizeOptions)[number];
export function PaginationControls({
  entityPlural,
  pageNumber,
  apiPagination,
  pageSize,
  setPageSize,
  setPageNumber,
  className,
  orientation = "between",
}: PaginationControlsProps) {
  const [totalPages, setTotalPages] = useState(apiPagination?.totalPages);
  const pageInfo = `${pageNumber} ${
    totalPages ? `of ${totalPages}` : "of ..."
  }`;

  useEffect(() => {
    if (apiPagination?.totalPages != null)
      setTotalPages(apiPagination?.totalPages);
  }, [apiPagination?.totalPages, totalPages]);

  return (
    <div
      className={cn(
        "flex items-center px-3 py-2 bg-white dark:bg-slate-700 sm:rounded-b-lg",
        orientation === "left" && "justify-start",
        orientation === "right" && "justify-end",
        orientation === "between" && "justify-between",
        className
      )}
      aria-label={`Table navigation for ${entityPlural} table`}
    >
      <div
        className={cn(
          "flex items-center space-x-5",
          orientation === "between" && "flex-1"
        )}
      >
        <span className="flex text-sm font-normal text-slate-500 dark:text-slate-400 min-w-[4rem]">
          <div>Page</div>
          <span className="pl-1 font-semibold text-slate-900 dark:text-white">
            {pageInfo}
          </span>
        </span>

        {pageSize !== undefined && (
          <div className="hidden w-32 sm:block">
            <PaginationCombobox
              value={pageSize.toString()}
              onValueChange={(value) => {
                setPageSize(Number(value));
                setTotalPages(undefined);
                setPageNumber(1);
              }}
            />
          </div>
        )}
      </div>

      <div className="inline-flex items-center -space-x-[2px]">
        <Button
          aria-label="First page"
          variant="outline"
          className={cn("rounded-r-none")}
          onClick={() => setPageNumber(1)}
          disabled={!apiPagination?.hasPrevious}
        >
          {<ArrowLeftToLine className="w-5 h-5" />}
        </Button>
        <Button
          aria-label="Previous page"
          variant="outline"
          className={cn("rounded-none")}
          onClick={() =>
            setPageNumber(
              apiPagination?.pageNumber ? apiPagination?.pageNumber - 1 : 1
            )
          }
          disabled={!apiPagination?.hasPrevious}
        >
          {<ChevronLeftIcon className="w-5 h-5" />}
        </Button>
        <Button
          aria-label="Next page"
          variant="outline"
          className={cn("rounded-none")}
          onClick={() =>
            setPageNumber(
              apiPagination?.pageNumber ? apiPagination?.pageNumber + 1 : 1
            )
          }
          disabled={!apiPagination?.hasNext}
        >
          {<ChevronRightIcon className="w-5 h-5" />}
        </Button>
        <Button
          aria-label="Last page"
          variant="outline"
          className={cn("rounded-l-none")}
          onClick={() =>
            setPageNumber(
              apiPagination?.totalPages ? apiPagination?.totalPages : 1
            )
          }
          disabled={!apiPagination?.hasNext}
        >
          {<ArrowRightFromLine className="w-5 h-5" />}
        </Button>
      </div>
    </div>
  );
}

function PaginationCombobox({
  value,
  onValueChange,
}: {
  value: string;
  onValueChange: (value: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const pageSizes = PageSizeOptions.map((selectedPageSize) => ({
    value: selectedPageSize.toString(),
    label: `Show ${selectedPageSize}`,
  }));

  return (
    <Dropdown isOpen={open} onOpenChange={setOpen}>
      <DropdownTrigger>
        <Button variant="outline">
          {value
            ? pageSizes.find((pageSize) => pageSize.value === value)?.label
            : "Select page size..."}
        </Button>
      </DropdownTrigger>
      <DropdownMenu aria-label="Page Size">
        {pageSizes.map((pageSize) => (
          <DropdownItem
            key={pageSize.value}
            onClick={() => {
              onValueChange(pageSize.value === value ? "" : pageSize.value);
              setOpen(false);
            }}
          >
            {pageSize.label}
          </DropdownItem>
        ))}
      </DropdownMenu>
    </Dropdown>
  );
}
