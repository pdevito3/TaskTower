import { Column } from "@tanstack/react-table";
// import { ArrowDownIcon, ArrowUpIcon, SortAsc } from "lucide-react";

// import { Button } from "@/components/ui/button";
import { cn } from "@/utils";

interface DataTableColumnHeaderProps<TData, TValue>
  extends React.HTMLAttributes<HTMLDivElement> {
  column: Column<TData, TValue>;
  title?: string | undefined;
  canSort?: boolean;
}

export function DataTableColumnHeader<TData, TValue>({
  column,
  title,
  className,
}: // canSort = true,
DataTableColumnHeaderProps<TData, TValue>) {
  if (!column.getCanSort()) {
    return <div className={cn(className)}>{title}</div>;
  }

  return (
    <div className={cn("flex items-center space-x-1", className)}>
      {(title?.length ?? 0) > 0 ? <span>{title}</span> : null}
      {/* {canSort && (
        <Button
          variant="ghost"
          size="sm"
          className="h-8"
          onClick={() => column.toggleSorting()}
        >
          {column.getIsSorted() === "desc" ? (
            <ArrowDownIcon className="w-4 h-4 text-primary" />
          ) : column.getIsSorted() === "asc" ? (
            <ArrowUpIcon className="w-4 h-4 text-primary" />
          ) : (
            <SortAsc className="w-4 h-4" />
          )}
        </Button>
      )} */}
    </div>
  );
}
