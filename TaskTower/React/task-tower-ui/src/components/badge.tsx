import { cn } from "../utils";

export type BadgeVariant =
  | "amber"
  | "red"
  | "gray"
  | "yellow"
  | "blue"
  | "indigo"
  | "sky"
  | "rose"
  | "pink"
  | "purple"
  | "violet"
  | "orange"
  | "green"
  | "teal"
  | "cyan"
  | "emerald";

const badgeVariants: Record<BadgeVariant, string> = {
  amber: "text-amber-600 bg-amber-50 ring-amber-600/20",
  red: "text-red-700 bg-red-50 ring-red-600/10",
  gray: "text-gray-600 bg-gray-50 ring-gray-500/10",
  yellow: "text-yellow-800 bg-yellow-50 ring-yellow-600/20",
  blue: "text-blue-700 bg-blue-50 ring-blue-700/10",
  indigo: "text-indigo-600 bg-indigo-50 ring-indigo-600/10",
  sky: "text-sky-700 bg-sky-50 ring-sky-700/10",
  rose: "text-rose-700 bg-rose-50 ring-rose-700/10",
  pink: "text-pink-700 bg-pink-50 ring-pink-700/10",
  purple: "text-purple-700 bg-purple-50 ring-purple-700/10",
  violet: "text-violet-700 bg-violet-50 ring-violet-700/10",
  orange: "text-orange-700 bg-orange-50 ring-orange-700/10",
  green: "text-green-700 bg-green-50 ring-green-700/10",
  teal: "text-teal-700 bg-teal-50 ring-teal-700/10",
  cyan: "text-cyan-700 bg-cyan-50 ring-cyan-700/10",
  emerald: "text-emerald-700 bg-emerald-50 ring-emerald-700/10",
};

interface BadgeProps {
  text: string;
  variant: BadgeVariant;
  className?: string;
  props?: React.HTMLProps<HTMLSpanElement>;
}

export const Badge: React.FC<BadgeProps> = ({
  text,
  variant,
  className,
  props,
}) => {
  const variantClasses = badgeVariants[variant] || badgeVariants["gray"];

  return (
    <span
      className={cn(
        `inline-flex ring-inset ring-1 items-center px-2 py-1 text-xs font-medium rounded-md ${variantClasses}`,
        className
      )}
      {...props}
    >
      {text}
    </span>
  );
};
