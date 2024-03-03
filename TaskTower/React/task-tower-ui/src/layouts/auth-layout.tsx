import logo from "@/assets/logo.svg";
import { LoadingSpinner } from "@/components/loading-spinner";
import { AllRoutesPaths } from "@/router";

import { cn } from "@/utils";
import { Link, Outlet } from "@tanstack/react-router";
import { LayoutDashboard, PackageOpen } from "lucide-react";

type NavType = {
  name: string;
  href: AllRoutesPaths;
  icon: React.FC<any>;
};

const navigation = [
  { name: "Dashboard", href: "/tasktower", icon: LayoutDashboard },
  { name: "Jobs", href: "/tasktower/jobs", icon: PackageOpen },
] as NavType[];

export default function AuthLayout() {
  // const { user, logoutUrl, isLoading } = useAuthUser();
  const isLoading = false;
  if (isLoading) return <Loading />;

  return (
    <>
      <div>
        <DesktopMenu />

        <div className="sticky top-0 z-40 flex items-center justify-center px-4 py-4 shadow-sm bg-background gap-x-2 sm:px-6 lg:hidden">
          <div className="flex items-center flex-1 px-1 space-x-2 text-sm font-semibold leading-6 text-gray-900">
            <div className="hidden sm:block">{/* <MobileMenu /> */}</div>
            <h1 className="text-sm font-semibold leading-6 text-primary">
              Task Tower
            </h1>
          </div>
        </div>

        <main className="pt-4 pb-6 lg:pt-6 lg:pb-10 lg:pl-52">
          <div className="px-4 sm:px-6 lg:px-8">
            <Outlet />
          </div>
        </main>
        <div className="sm:hidden">{/* <MobileMenu /> */}</div>
      </div>
    </>
  );
}

const sideNavWidth = "lg:w-52";

function DesktopMenu() {
  return (
    <div
      className={cn(
        "hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:flex-col",
        sideNavWidth
      )}
    >
      {/* Sidebar component, swap this element with another sidebar if you like */}
      <div className="flex flex-col px-6 overflow-y-auto border-r bg-card grow gap-y-5">
        <div className="flex items-center h-16 shrink-0">
          <Link to="/tasktower">
            <div className="flex items-center space-x-2">
              <img
                className="w-8 h-8"
                // src="https://tailwindui.com/img/logos/mark.svg?color=emerald&shade=500"
                src={logo}
                alt="Task Tower"
              />
              <p className="text-xl font-semibold tracking-tight">Task Tower</p>
            </div>
          </Link>
        </div>
        <nav className="flex flex-col flex-1">
          <ul role="list" className="flex flex-col flex-1 gap-y-7">
            <li>
              <ul role="list" className="-mx-2 space-y-1">
                {navigation.map((item) => (
                  <li key={item.name}>
                    <Link
                      to={item.href}
                      className={cn(
                        "border-transparent border-2 hover:text-emerald-400",
                        "group flex gap-x-3 rounded-md p-2 text-sm leading-6 font-semibold",
                        " data-[status=active]:bg-gray-200/80 data-[status=active]:text-emerald-500 data-[status=active]:hover:bg-zinc-100/50"
                      )}
                      // className={cn(
                      //   "text-secondary-foreground hover:text-primary/80 hover:bg-gray-50 hover:border-2 hover:border-gray-100",
                      //   "group flex gap-x-3 rounded-md p-2 text-sm leading-6 font-semibold"
                      // )}
                      activeOptions={{ exact: true }}
                      // activeProps={{
                      //   className:
                      //     "data-[status=active]:border-2 data-[status=active]:border-emerald-500 data-[status=active]:text-emerald-500 data-[status=active]:hover:bg-zinc-100/50",
                      // }}
                    >
                      <item.icon
                        className={cn("h-6 w-6 shrink-0")}
                        aria-hidden="true"
                      />
                      {item.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </li>
            {/* <li>
              <div className="text-xs font-semibold leading-6 text-gray-400">
                Your teams
              </div>
              <ul role="list" className="mt-2 -mx-2 space-y-1">
                {teams.map((team) => (
                  <li key={team.name}>
                    <Link
                      to={team.href}
                      className={cn(
                        team.current
                          ? "bg-gray-50 text-foreground"
                          : "text-gray-700 hover:text-foreground hover:bg-gray-50",
                        "group flex gap-x-3 rounded-md p-2 text-sm leading-6 font-semibold"
                      )}
                    >
                      <span
                        className={cn(
                          team.current
                            ? "text-foreground border-foreground"
                            : "text-gray-400 border-gray-200 group-hover:border-foreground group-hover:text-foreground",
                          "flex h-6 w-6 shrink-0 items-center justify-center rounded-lg border text-[0.625rem] font-medium bg-white"
                        )}
                      >
                        {team.initial}
                      </span>
                      <span className="truncate">{team.name}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </li> */}
            <li className="mt-auto">
              {/* <ProfileManagement user={user} logoutUrl={logoutUrl} /> */}
            </li>
          </ul>
        </nav>
      </div>
    </div>
  );
}

function Loading() {
  return (
    <div className="flex items-center justify-center w-screen h-screen transition-all bg-slate-100">
      <LoadingSpinner />
    </div>
  );
}
