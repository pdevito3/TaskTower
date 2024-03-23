import { Notification } from "@/components/notifications";
import { cn } from "@/utils";

export function CopyButton({
  textToCopy,
  className,
}: {
  textToCopy: string;
  className?: string;
}) {
  return (
    <button
      className={cn(
        "rounded-md inline-flex items-center self-center p-2 text-sm font-medium text-center text-gray-900 transition-opacity bg-white md:opacity-0 hover:bg-gray-100 focus:outline-none dark:text-white  dark:bg-gray-900 dark:hover:bg-gray-800 dark:focus:ring-gray-600 group-hover:opacity-100",
        className
      )}
      type="button"
      onClick={() => {
        navigator.clipboard.writeText(textToCopy);
        Notification.success("Copied to clipboard");
      }}
    >
      {/* https://iconbuddy.app/akar-icons/copy */}
      <svg
        className="w-4 h-4 text-gray-500 dark:text-gray-400"
        width="512"
        height="512"
        viewBox="0 0 24 24"
        xmlns="http://www.w3.org/2000/svg"
      >
        <g
          fill="none"
          stroke="currentColor"
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth="2"
        >
          <path d="M8 4v12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V7.242a2 2 0 0 0-.602-1.43L16.083 2.57A2 2 0 0 0 14.685 2H10a2 2 0 0 0-2 2" />
          <path d="M16 18v2a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h2" />
        </g>
      </svg>
    </button>
  );
}
