import { Slot } from "@radix-ui/react-slot";
import { cva, type VariantProps } from "class-variance-authority";
import * as React from "react";

import { cn } from "@/utils";

const buttonVariants = cva(
  "inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium ring-offset-white transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50",
  {
    variants: {
      variant: {
        default: "bg-emerald-500 text-white hover:bg-emerald-500/90",
        destructive:
          "hover:bg-rose-200 hover:text-rose-800 dark:border-slate-900 dark:bg-slate-800 dark:text-white dark:hover:bg-rose-800 dark:hover:text-rose-300 dark:shadow-rose-400 dark:hover:shadow-rose-300",
        outline:
          "border border-input bg-background hover:bg-zinc-100 hover:text-zinc-900",
        secondary:
          "hover:bg-slate-200 hover:text-slate-800 dark:border-slate-900 dark:bg-slate-800 dark:text-white dark:hover:bg-slate-800 dark:hover:text-slate-300 dark:shadow-slate-400 dark:hover:shadow-slate-300",
        ghost: "hover:bg-gray-100 hover:text-gray-900",
        link: "text-emerald-500 underline-offset-4 hover:underline",
      },
      size: {
        default: "h-10 px-4 py-2",
        sm: "h-9 rounded-md px-3",
        lg: "h-11 rounded-md px-8",
        icon: "h-10 w-10",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
);

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean;
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : "button";
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    );
  }
);
Button.displayName = "Button";

export { Button, buttonVariants };
