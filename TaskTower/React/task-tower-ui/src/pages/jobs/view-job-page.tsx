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
import { CheckIcon, XCircle } from "lucide-react";
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
  const activity = [
    {
      id: 1,
      type: "created",
      person: { name: "Chelsea Hagon" },
      date: "7d ago",
      dateTime: "2023-01-23T10:32",
    },
    {
      id: 2,
      type: "edited",
      person: { name: "Chelsea Hagon" },
      date: "6d ago",
      dateTime: "2023-01-23T11:03",
    },
    {
      id: 3,
      type: "sent",
      person: { name: "Chelsea Hagon" },
      date: "6d ago",
      dateTime: "2023-01-23T11:24",
    },
    {
      id: 4,
      type: "commented",
      person: {
        name: "Chelsea Hagon",
        imageUrl:
          "https://images.unsplash.com/photo-1550525811-e5869dd03032?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=facearea&facepad=2&w=256&h=256&q=80",
      },
      comment:
        "Called client, they reassured me the invoice would be paid by the 25th.",
      date: "3d ago",
      dateTime: "2023-01-23T15:56",
    },
    {
      id: 5,
      type: "viewed",
      person: { name: "Alex Curren" },
      date: "2d ago",
      dateTime: "2023-01-24T09:12",
    },
    {
      id: 6,
      type: "paid",
      person: { name: "Alex Curren" },
      date: "1d ago",
      dateTime: "2023-01-24T09:20",
    },
  ];

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
                    <>
                      <div className="relative flex h-6 w-6 flex-none items-center justify-center bg-white">
                        <XCircle
                          className="h-6 w-6 text-rose-600"
                          aria-hidden="true"
                        />
                      </div>
                      <div className="flex-auto rounded-md p-3 ring-1 ring-inset ring-gray-200">
                        <div className="flex justify-between gap-x-4">
                          <div className="py-0.5 text-sm leading-5 text-gray-500">
                            <span className="font-medium text-gray-900">
                              {history.status}
                            </span>
                          </div>

                          {history?.occurredAt && (
                            <time
                              dateTime={format(
                                new Date(history.occurredAt),
                                "yyyy-MM-dd'T'HH:mm:ss"
                              )}
                              className="flex-none py-0.5 text-sm leading-5 text-gray-500"
                            >
                              {`occurred ${formatDistanceToNow(
                                new Date(history.occurredAt),
                                { addSuffix: true }
                              )} at ${format(
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
                            {stackFrames(history.details!).map(
                              (frame, index) => (
                                <li
                                  key={index}
                                  className="pl-4 text-sm text-gray-800 space-x-1"
                                >
                                  {frame.methodPath.map((part, partIndex) => (
                                    <span
                                      key={partIndex}
                                      className={`font-semibold ${
                                        partIndex ===
                                        frame.methodPath.length - 1
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
                              )
                            )}
                          </ul>
                        </p>
                      </div>
                    </>
                  ) : (
                    <>
                      <div className="relative flex h-6 w-6 flex-none items-center justify-center bg-white">
                        {history.status === "Completed" ? (
                          <CheckIcon
                            className="h-6 w-6 text-indigo-600"
                            aria-hidden="true"
                          />
                        ) : (
                          <div className="h-1.5 w-1.5 rounded-full bg-gray-100 ring-1 ring-gray-300" />
                        )}
                      </div>
                      <p className="flex-auto py-0.5 text-sm leading-5 text-gray-500">
                        <span className="font-medium text-gray-900">
                          {history.status}
                        </span>
                      </p>

                      {history?.occurredAt && (
                        <time
                          dateTime={format(
                            new Date(history.occurredAt),
                            "yyyy-MM-dd'T'HH:mm:ss"
                          )}
                          className="flex-none py-0.5 text-sm leading-5 text-gray-500"
                        >
                          {`occurred ${formatDistanceToNow(
                            new Date(history.occurredAt),
                            { addSuffix: true }
                          )} at ${format(
                            new Date(history.occurredAt),
                            "yyyy-MM-dd'T'HH:mm:ss"
                          )}`}
                        </time>
                      )}
                    </>
                  )}
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}
