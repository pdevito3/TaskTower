import { useJobs } from "@/domain/jobs/apis/get-jobs-worklist";
import { JobsWorklist } from "@/domain/jobs/components/worklist/jobs-worklist";
import { createColumns } from "@/domain/jobs/components/worklist/jobs-worklist-columns";
import { useJobsTableStore } from "@/domain/jobs/components/worklist/jobs-worklist.store";

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

function Jobs() {
  const { sorting, pageSize, pageNumber, queryKit } = useJobsTableStore();
  const { data: jobs, isLoading } = useJobs({
    sortOrder: sorting,
    pageSize,
    pageNumber,
    filters: queryKit.filterValue(),
    delayInMs: 450,
  });

  const columns = createColumns();
  return (
    <>
      <JobsWorklist
        columns={columns}
        data={jobs?.data ?? []}
        isLoading={isLoading}
        pagination={jobs?.pagination}
      />
    </>
  );
}
