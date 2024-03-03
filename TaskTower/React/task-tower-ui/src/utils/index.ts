import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function getEnv() {
  const env = window.ASPNETCORE_ENVIRONMENT;
  return env === "{{ASPNETCORE_ENVIRONMENT}}" ? "Standalone" : env;
}

export function isStandaloneEnv() {
  return getEnv() === "Standalone";
}

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export { toDateOnly } from "./dates";
export { caseInsensitiveEquals } from "./strings";
