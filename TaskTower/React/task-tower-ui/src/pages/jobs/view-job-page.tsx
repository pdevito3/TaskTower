import { useGetJobView } from "@/domain/jobs/apis/get-job-view";
import { JobView } from "@/domain/jobs/components/job-view";
import { jobsRoute } from "@/router";
import { useParams } from "@tanstack/react-router";
import { Helmet } from "react-helmet";

export function JobViewPage() {
  const queryParams = useParams({
    from: `${jobsRoute.fullPath}/$jobId`,
  });
  const jobId = queryParams.jobId;
  const { data: jobData } = useGetJobView(jobId);
  const jobNumberTitle = jobId ? ` - ${jobId}` : "";

  return (
    <div className="">
      <Helmet>
        <title>Job {jobNumberTitle}</title>
      </Helmet>

      {jobData && <JobView jobData={jobData} />}
    </div>
  );
}
