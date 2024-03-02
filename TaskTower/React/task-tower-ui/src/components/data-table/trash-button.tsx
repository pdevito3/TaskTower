import { cn } from "@/utils";
import { Trash2Icon } from "lucide-react";
import { MouseEvent } from "react";

interface TrashButtonProps {
  onClick: (e: MouseEvent<HTMLButtonElement>) => void;
  hideInGroup?: boolean;
}

export function TrashButton({ onClick, hideInGroup = true }: TrashButtonProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        "inline-flex items-center px-2 py-2 text-sm font-medium leading-5 transition duration-100 ease-in bg-white border border-gray-300 rounded-full shadow-sm",
        "hover:bg-rose-200 hover:text-rose-800 hover:outline-none",
        "dark:border-slate-900 dark:bg-slate-800 dark:text-white dark:hover:bg-rose-800 dark:hover:text-rose-300 dark:hover:outline-none",
        "sm:px-3 sm:py-1 dark:hover:shadow dark:shadow-rose-400 dark:hover:shadow-rose-300",
        hideInGroup && "sm:opacity-0 sm:group-hover:opacity-100"
      )}
    >
      <Trash2Icon className="w-4 h-4" />
    </button>
  );
}
