import { Notification } from "@/components/notifications";
import AuthLayout from "@/layouts/auth-layout";
import { ReactQueryDevtools, TanStackRouterDevtools } from "@/lib/dev-tools";
import { siteConfig } from "@/lib/site-config";
import { IndexPage } from "@/pages/index";
import { JobsWorklistPage } from "@/pages/jobs";
import {
  Outlet,
  RoutePaths,
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { Helmet } from "react-helmet";
import { z } from "zod";
import { EditJobPage } from "./pages/jobs/edit-job-page";

const appRoute = createRootRoute({
  component: () => {
    return (
      <>
        <Helmet
          titleTemplate={`%s | ${siteConfig.name}`}
          defaultTitle={siteConfig.name}
        >
          {/* <meta name="description" content={siteConfig.description} />
          <meta name="authhor" content="bachiitter" />
          <link rel="author" href="https://bachitter.dev" />

          <meta property="og:type" content="website" />
          <meta property="og:site_name" content="Shoubhit Dash" />
          <meta property="og:url" content={siteConfig.url} />
          <meta property="og:title" content={siteConfig.name} />
          <meta property="og:description" content={siteConfig.name} />
          <meta property="og:image" content={siteConfig.ogImage} />

          <meta property="twitter:card" content="summary_large_image" />
          <meta property="twitter:url" content={siteConfig.url} />
          <meta property="twitter:title" content={siteConfig.name} />
          <meta
            property="twitter:description"
            content={siteConfig.description}
          />
          <meta property="twitter:image" content={siteConfig.ogImage} /> */}
        </Helmet>

        <div className="h-full min-h-screen font-sans antialiased scroll-smooth debug-screens [font-feature-settings:'ss01'] ">
          <Outlet />
          <Notification />
          <div className="hidden md:block">
            <TanStackRouterDevtools
              position="top-right"
              toggleButtonProps={{
                style: {
                  marginRight: "5rem",
                  marginTop: "1.25rem",
                },
              }}
            />
            <ReactQueryDevtools buttonPosition="top-right" />
          </div>
          <div className="block md:hidden">
            <TanStackRouterDevtools
              position="bottom-left"
              toggleButtonProps={{
                style: {
                  // marginLeft: "5rem",
                  marginBottom: "2rem",
                },
              }}
            />
            <div className="mb-6 ml-24">
              <ReactQueryDevtools buttonPosition="bottom-left" />
            </div>
          </div>
        </div>
      </>
    );
  },
});

const authLayout = createRoute({
  getParentRoute: () => appRoute,
  id: "auth-layout",
  component: AuthLayout,
  // component: () => {
  //   return (
  //     <div className="p-8">
  //       <Outlet />
  //     </div>
  //   );
  // },
});

const dashboardRoute = createRoute({
  getParentRoute: () => authLayout,
  path: "/",
  component: IndexPage,
});

const jobsRoute = createRoute({
  getParentRoute: () => authLayout,
  path: "jobs",
  component: () => {
    return <Outlet />;
  },
});

const jobWorklistRoute = createRoute({
  getParentRoute: () => jobsRoute,
  path: "/",
  component: JobsWorklistPage,
});

const jobRoute = createRoute({
  getParentRoute: () => jobsRoute,
  path: "$jobId",
  parseParams: (params) => ({
    jobId: z.string().uuid().parse(params.jobId),
  }),
  component: EditJobPage,
});

const routeTree = appRoute.addChildren([
  authLayout.addChildren([
    dashboardRoute,
    jobsRoute.addChildren([jobWorklistRoute, jobRoute]),
  ]),
]);

// Create the router using your route tree
export const router = createRouter({ routeTree });

// Register your router for maximum type safety
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

// export type AllRoutesPaths = RegisteredRouter["routePaths"];
export type AllRoutesPaths = RoutePaths<typeof routeTree>;
