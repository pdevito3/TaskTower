// import { JobStatusBadge } from "@/domain/jobs/components/job-status";
// import { JobStatus } from "@/domain/jobs/types";
import { useParams } from "@tanstack/react-router";
import { Helmet } from "react-helmet";

export function EditJobPage() {
  const queryParams = useParams({
    from: "/auth-layout/jobs/$jobId",
  });
  const jobId = queryParams.jobId;
  // const { data: job } = useGetJobForEdit(jobId);

  // const jobNumber = job?.jobNumber ?? "";
  // useGetJobComment(jobId);
  // const jobNumberTitle = jobNumber ? ` - ${jobNumber}` : "";
  const jobNumberTitle = jobId ? ` - ${jobId}` : "";

  return (
    <div className="">
      <Helmet>
        <title>Edit Job {jobNumberTitle}</title>
      </Helmet>

      <div className="flex items-center justify-start w-full space-x-4">
        <h1 className="flex items-center justify-start text-4xl font-bold tracking-tight scroll-m-20">
          Edit Job
          {/* <span className="pl-3 text-2xl">({job?.jobNumber})</span> */}
        </h1>
        {/* {job && <JobStatusBadge status={job?.status as JobStatus} />} */}
      </div>
    </div>
  );
}
