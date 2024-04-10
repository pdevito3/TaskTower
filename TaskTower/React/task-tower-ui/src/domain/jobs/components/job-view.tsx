// import { JobStatusBadge } from "@/domain/jobs/components/job-status";
// import { JobStatus } from "@/domain/jobs/types";
import { Badge } from "@/components/badge";
import { JsonSyntaxHighlighter } from "@/components/json-syntax-highlighter";
import { Notification } from "@/components/notifications";
import { Button } from "@/components/ui/button";
import { CopyButton } from "@/components/ui/copy-button";
import { JobStatusBadge } from "@/domain/jobs/components/job-status";
import { JobStatus, TaskTowerJobView } from "@/domain/jobs/types";
import { cn } from "@/utils";
import { format, formatDistanceToNow } from "date-fns";
import { CheckIcon, XCircle } from "lucide-react";
import { useRequeueJob } from "../apis/requeue-job";

export function JobView({ jobData }: { jobData: TaskTowerJobView }) {
  const job = jobData.job;

  const requeueJobsApi = useRequeueJob();
  function handleRequeueJobs() {
    requeueJobsApi
      .mutateAsync(job.id)
      .then(() => {
        Notification.success("Job requeued successfully");
      })
      .catch((e) => {
        Notification.error("There was an error requeuing this job");
        console.error(e);
      });
  }

  return (
    <div className="">
      <div className="pt-3">
        <div className="flex items-center justify-between w-full space-x-4">
          <div className="flex items-center justify-start w-full space-x-4">
            <h1 className="flex items-center justify-start text-4xl font-bold tracking-tight scroll-m-20">
              {job?.jobName}
            </h1>
            {job && <JobStatusBadge status={job?.status as JobStatus} />}
          </div>
          <Button variant="outline" onClick={() => handleRequeueJobs()}>
            Requeue
          </Button>
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
        {job?.status === "Failed" && (
          <div className="text-sm text-slate-800 flex items-center space-x-2">
            <label htmlFor={`queue-${job?.queue}`} className="font-bold">
              Next Run:
            </label>
            <time
              dateTime={format(new Date(job.runAfter), "yyyy-MM-dd'T'HH:mm:ss")}
              className="flex-none py-0.5 text-sm leading-5 text-gray-500"
              aria-label={`Next Run: ${formatDistanceToNow(
                new Date(job.runAfter)
              )}`}
              id={`next-run-in-${formatDistanceToNow(new Date(job.runAfter))}`}
            >
              {`${formatDistanceToNow(new Date(job.runAfter), {
                addSuffix: true,
              })} at ${format(
                new Date(job.runAfter),
                "yyyy-MM-dd'T'HH:mm:ss"
              )}`}
            </time>
          </div>
        )}
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
          <HistoryList jobData={jobData} />
        </div>
      </div>
    </div>
  );
}

function HistoryList({ jobData }: { jobData: TaskTowerJobView }) {
  return (
    <ul role="list" className="space-y-6">
      {jobData?.history.map((history, historyIndex) => (
        <li key={history.id} className="relative flex gap-x-4">
          <div
            className={cn(
              historyIndex === jobData?.history.length - 1
                ? "h-6"
                : "-bottom-6",
              "absolute left-0 top-0 flex w-6 justify-center"
            )}
          >
            <div className="w-px bg-gray-200" />
          </div>
          {history.status === "Failed" ? (
            <FailureNode history={history} />
          ) : (
            <NormalNode history={history} isCurrentState={historyIndex === 0} />
          )}
        </li>
      ))}
    </ul>
  );
}

type HistoryItem = TaskTowerJobView["history"][0];

function NormalNode({
  history,
  isCurrentState,
}: {
  history: HistoryItem;
  isCurrentState: boolean;
}) {
  return (
    <>
      <div className="relative flex h-6 w-6 flex-none items-center justify-center bg-white">
        {history.status === "Completed" ? (
          <CheckIcon className="h-6 w-6 text-emerald-600" aria-hidden="true" />
        ) : isCurrentState ? (
          <div className="h-1.5 w-1.5 rounded-full bg-emerald-200 ring-1 ring-emerald-400 animate-pulse" />
        ) : (
          <div className="h-1.5 w-1.5 rounded-full bg-gray-100 ring-1 ring-gray-300" />
        )}
      </div>
      <p className="flex-auto py-0.5 text-sm leading-5 text-gray-500">
        <span className="font-medium text-gray-900">{history.status}</span>
      </p>

      {history?.occurredAt && (
        <time
          dateTime={format(
            new Date(history.occurredAt),
            "yyyy-MM-dd'T'HH:mm:ss"
          )}
          className="flex-none py-0.5 text-sm leading-5 text-gray-500"
        >
          {`occurred ${formatDistanceToNow(new Date(history.occurredAt), {
            addSuffix: true,
          })} at ${format(
            new Date(history.occurredAt),
            "yyyy-MM-dd'T'HH:mm:ss"
          )}`}
        </time>
      )}
    </>
  );
}

function FailureNode({ history }: { history: HistoryItem }) {
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
    <>
      <div className="relative flex h-6 w-6 flex-none items-center justify-center bg-white">
        <XCircle className="h-6 w-6 text-rose-600" aria-hidden="true" />
      </div>
      <div className="flex-auto rounded-md p-3 ring-1 ring-inset ring-gray-200">
        <div className="flex justify-between gap-x-4">
          <div className="py-0.5 text-sm leading-5 text-gray-500">
            <span className="font-medium text-gray-900">{history.status}</span>
          </div>

          {history?.occurredAt && (
            <time
              dateTime={format(
                new Date(history.occurredAt),
                "yyyy-MM-dd'T'HH:mm:ss"
              )}
              className="flex-none py-0.5 text-sm leading-5 text-gray-500"
            >
              {`occurred ${formatDistanceToNow(new Date(history.occurredAt), {
                addSuffix: true,
              })} at ${format(
                new Date(history.occurredAt),
                "yyyy-MM-dd'T'HH:mm:ss"
              )}`}
            </time>
          )}
        </div>
        <p className="text-sm leading-6 text-gray-500">
          <p className="text-sm">{history.comment}</p>
          <h4 className="font-bold">Stack Trace:</h4>
          <ul className="list-inside">
            {stackFrames(history.details!).map((frame, index) => (
              <li key={index} className="pl-4 text-sm text-gray-800 space-x-1">
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
                    {partIndex < frame.methodPath.length - 1 ? "." : ""}
                  </span>
                ))}{" "}
                in
                <span className="text-emerald-400"> {frame.fileName}</span> :
                <span className="text-violet-600">{frame.line}</span>
              </li>
            ))}
          </ul>
        </p>
      </div>
    </>
  );
}
