import { NextUIProvider } from "@nextui-org/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { RouterProvider } from "@tanstack/react-router";
import { StrictMode, Suspense } from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import { router } from "./router";

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Suspense fallback={null}>
      <QueryClientProvider client={queryClient}>
        <NextUIProvider>
          <RouterProvider router={router} />{" "}
        </NextUIProvider>
      </QueryClientProvider>
    </Suspense>
  </StrictMode>
);
