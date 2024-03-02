import { format } from "date-fns";

export function toDateOnly(date: Date | undefined) {
  return date !== undefined ? format(date, "yyyy-MM-dd") : undefined;
}
