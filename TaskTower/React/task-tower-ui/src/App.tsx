import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { JobStatusBadge } from "./domain/jobs/components/job-status";
import { Job } from "./domain/jobs/types";
import { getEnv } from "./utils";

export default function App() {
  const environment = getEnv();
  return (
    <div className="p-8">
      <h1 className="text-xl font-bold text-violet-500">
        Hello Task Tower in ({environment})
      </h1>
      <Jobs />
    </div>
  );
}

function useJobs() {
  return useQuery({
    queryKey: ["jobs"],
    queryFn: async () =>
      axios
        .get(
          getEnv() === "Standalone"
            ? "http://localhost:5130/api/v1/jobs"
            : "/api/v1/jobs"
        )
        .then((response) => response.data as Job[]),
  });
}

function Jobs() {
  const { isLoading, data } = useJobs();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {data &&
        data?.map((job) => (
          <div key={job.id} className="p-4 bg-white rounded-lg shadow-lg">
            <h2 className="text-lg font-bold text-slate-800">{job.jobName}</h2>
            {/* <pre>{JSON.stringify(job, null, 2)}</pre> */}

            <JobStatusBadge status={job.status.value} />
          </div>
        ))}
    </div>
  );
}
