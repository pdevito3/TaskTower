import { JobsWorklist } from "@/domain/jobs/components/worklist/jobs-worklist";
import { createColumns } from "@/domain/jobs/components/worklist/jobs-worklist-columns";
import { Job } from "@/domain/jobs/types";
import { getEnv } from "@/utils";
import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { Helmet } from "react-helmet";

export function JobsWorklistPage() {
  // const environment = getEnv();
  return (
    <>
      <Helmet>
        <title>Jobs</title>
      </Helmet>

      <div className="w-full space-y-5">
        {/* <h1 className="text-xl font-bold text-violet-500">
          Hello Task Tower in ({environment})
        </h1> */}
        <h1 className="text-4xl font-bold tracking-tight scroll-m-20">Jobs</h1>
        <Jobs />
      </div>
    </>
  );
}

function useJobs() {
  return useQuery({
    queryKey: ["jobs"],
    queryFn: async () =>
      axios
        .get(
          getEnv() === "Standalone"
            ? "http://localhost:5130/api/v1/jobs/paginated"
            : "/api/v1/jobs/paginated"
        )
        .then((response) => response.data as Job[]),
  });
}

function Jobs() {
  const { isLoading, data: jobs } = useJobs();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  const columns = createColumns();
  return <JobsWorklist columns={columns} data={jobs ?? []} isLoading={false} />;
}
