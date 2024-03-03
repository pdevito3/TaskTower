import { JobsWorklist } from "@/domain/jobs/components/worklist/jobs-worklist";
import { createColumns } from "@/domain/jobs/components/worklist/jobs-worklist-columns";
import { Job } from "@/domain/jobs/types";
import { getEnv, isStandaloneEnv } from "@/utils";
import { useQuery } from "@tanstack/react-query";
import axios from "axios";

export default function App() {
  const environment = getEnv();
  return (
    <div className="p-8 w-full space-y-5">
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
          isStandaloneEnv()
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
