import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function getEnv() {
  const env = window.ASPNETCORE_ENVIRONMENT;
  return env === "{{ASPNETCORE_ENVIRONMENT}}" ? "Standalone" : env;
}

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}