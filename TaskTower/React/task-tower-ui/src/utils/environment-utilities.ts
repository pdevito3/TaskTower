export function getEnv() {
  const env = window.ASPNETCORE_ENVIRONMENT;
  return env === "{{ASPNETCORE_ENVIRONMENT}}" ? "Standalone" : env;
}
