export function caseInsensitiveEquals(
  a: string | undefined | null,
  b: string | undefined | null
) {
  return typeof a === "string" && typeof b === "string"
    ? a.localeCompare(b, undefined, { sensitivity: "accent" }) === 0
    : a === b;
}
