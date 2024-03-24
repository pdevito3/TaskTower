// import { JobStatusBadge } from "@/domain/jobs/components/job-status";
// import { JobStatus } from "@/domain/jobs/types";
import { Badge } from "@/components/badge";
import { JsonSyntaxHighlighter } from "@/components/json-syntax-highlighter";
import { CopyButton } from "@/components/ui/copy-button";
import { useGetJobView } from "@/domain/jobs/apis/get-job-view";
import { JobStatusBadge } from "@/domain/jobs/components/job-status";
import { JobStatus } from "@/domain/jobs/types";
import { jobsRoute } from "@/router";
import { cn } from "@/utils";
import { useParams } from "@tanstack/react-router";
import { format, formatDistanceToNow } from "date-fns";
import { Helmet } from "react-helmet";

export function JobViewPage() {
  const queryParams = useParams({
    from: `${jobsRoute.fullPath}/$jobId`,
  });
  const jobId = queryParams.jobId;
  const { data: jobData } = useGetJobView(jobId);
  const job = jobData?.job;
  const jobNumberTitle = jobId ? ` - ${jobId}` : "";

  interface StackFrame {
    methodName: string;
    methodPath: string[];
    filePath: string;
    line: string;
    fileName?: string;
  }

  const parseExceptionString = (exceptionString: string): StackFrame[] => {
    const stackFrameRegex = /at (.+) in (\/[^:]+):line (\d+)/g;
    let match: RegExpExecArray | null;
    const frames: StackFrame[] = [];

    while ((match = stackFrameRegex.exec(exceptionString)) !== null) {
      const [, methodName, filePath, line] = match;
      const fileName = filePath.split("/").pop();

      // Splitting the methodName by '.' to separate namespaces, class names, and method names
      const methodPath = methodName.split(".");

      frames.push({ methodName, methodPath, filePath, line, fileName });
    }

    return frames;
  };
  const stackFrames = (exception: string) => parseExceptionString(exception);

  return (
    <div className="">
      <Helmet>
        <title>Job {jobNumberTitle}</title>
      </Helmet>

      <div className="">
        <div className="pt-3">
          <div className="flex items-center justify-start w-full space-x-4">
            <h1 className="flex items-center justify-start text-4xl font-bold tracking-tight scroll-m-20">
              {job?.jobName}
            </h1>
            {job && <JobStatusBadge status={job?.status as JobStatus} />}
          </div>
          <div
            className={cn(
              "space-x-3 space-y-2",
              (job?.tags.length ?? 0) > 0 && "pt-2"
            )}
          >
            {job?.tags.map((tag) => {
              return <Badge text={tag} variant="gray" />;
            })}
          </div>
          <div className="text-sm text-slate-800 items-center space-x-2 group inline-flex">
            <label htmlFor={`id-${job?.id}`} className="font-bold">
              Id:
            </label>
            <p
              className="text-sm text-slate-500"
              aria-label={`id name: ${job?.id}`}
              id={`id-${job?.id}`}
            >
              {job?.id ? job?.id : "-"}
            </p>
            {job?.id && (
              <CopyButton textToCopy={job?.id} className="rounded-full" />
            )}
          </div>
          <div className="text-sm text-slate-800 flex space-x-2">
            <label htmlFor={`queue-${job?.queue}`} className="font-bold">
              Queue:
            </label>
            <p
              className="text-sm text-slate-500"
              aria-label={`Queue name: ${job?.queue}`}
              id={`queue-${job?.queue}`}
            >
              {job?.queue ? job?.queue : "-"}
            </p>
          </div>
        </div>

        <div className="pt-3">
          <h3 className="text-xl font-bold tracking-tight">Data</h3>
          <div className="pt-1 pl-3 ">
            <div className="max-h-48 overflow-auto border rounded-lg border-slate-300 shadow p-1 text-sm">
              <JsonSyntaxHighlighter json={job?.payload || "[]"} />
            </div>
          </div>
        </div>

        <div className="pt-3">
          <h3 className="text-xl font-bold tracking-tight">History</h3>
          <div className="pt-3 pl-3 space-y-4">
            {/* {jobData?.history.map((history, index) => { */}
            {jobData?.history.map((history) => {
              return (
                <div
                  className={cn(
                    "border rounded-lg border-slate-300 shadow text-sm overflow-hidden "
                  )}
                >
                  <div
                    className={cn(
                      "flex justify-between items-center",
                      // history?.status === "Failed" &&
                      //   "bg-rose-200 text-rose-800",
                      // history?.status === "Completed" &&
                      //   "bg-emerald-200 text-emerald-800",
                      // history?.status === "Processing" &&
                      //   index === 0 &&
                      //   "bg-amber-200 text-amber-800",
                      // history?.status === "Enqueued" &&
                      //   index === 0 &&
                      //   "bg-violet-200 text-violet-800",

                      // history?.status === "Failed" && " text-rose-800",
                      // history?.status === "Completed" && " text-emerald-800",
                      // history?.status === "Processing" &&
                      //   index === 0 &&
                      //   " text-amber-800",
                      // history?.status === "Enqueued" &&
                      //   index === 0 &&
                      //   " text-violet-800",

                      "bg-slate-100",
                      "py-4 px-4 rounded-t-lg"
                    )}
                  >
                    <p className="text-lg font-bold tracking-tight">
                      {history.status}
                    </p>
                    <p className="font-medium tracking-tight">
                      {history?.occurredAt &&
                        `occurred ${formatDistanceToNow(
                          new Date(history.occurredAt),
                          { addSuffix: true }
                        )} at ${format(
                          new Date(history.occurredAt),
                          "yyyy-MM-dd'T'HH:mm:ss"
                        )}`}
                    </p>
                  </div>

                  {(history?.details?.length ?? 0) > 0 && (
                    <div className="py-4 px-4 rounded-lg text-sm">
                      <div className="text-sm">{history.comment}</div>
                      <div className="font-bold">Stack Trace:</div>
                      <ul className="list-inside">
                        {stackFrames(history.details!).map((frame, index) => (
                          <li
                            key={index}
                            className="pl-4 text-sm text-gray-800 space-x-1"
                          >
                            {frame.methodPath.map((part, partIndex) => (
                              <span
                                key={partIndex}
                                className={`font-semibold ${
                                  partIndex === frame.methodPath.length - 1
                                    ? "text-emerald-400"
                                    : "text-slate-800"
                                }`}
                              >
                                {part}
                                {partIndex < frame.methodPath.length - 1
                                  ? "."
                                  : ""}
                              </span>
                            ))}{" "}
                            in
                            <span className="text-emerald-400">
                              {" "}
                              {frame.fileName}
                            </span>{" "}
                            :
                            <span className="text-violet-600">
                              {frame.line}
                            </span>
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}
