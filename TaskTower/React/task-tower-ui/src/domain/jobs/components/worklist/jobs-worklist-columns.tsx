import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { JobStatusBadge } from "@/domain/jobs/components/job-status";
import { Job, JobStatus } from "@/domain/jobs/types";
import { cn } from "@/utils";
import { Link } from "@tanstack/react-router";
import { ColumnDef } from "@tanstack/react-table";
import { formatDistanceToNow, parseISO } from "date-fns";
import React, { HTMLProps } from "react";

type Columns = ColumnDef<Job>;
export const createColumns = (): Columns[] => [
  {
    id: "selection",
    header: ({ table }) => (
      <div>
        <IndeterminateCheckbox
          {...{
            checked: table.getIsAllRowsSelected(),
            indeterminate: table.getIsSomeRowsSelected(),
            onChange: table.getToggleAllRowsSelectedHandler(),
          }}
        />
      </div>
    ),
    cell: ({ row }) => (
      <div>
        <IndeterminateCheckbox
          {...{
            checked: row.getIsSelected(),
            indeterminate: row.getIsSomeSelected(),
            onChange: row.getToggleSelectedHandler(),
          }}
        />
      </div>
    ),
  },
  {
    accessorKey: "id",
    header: "Id",
  },
  {
    accessorKey: "job-identification",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Job" canSort={false} />
    ),
    cell: ({ row }) => {
      const id = row.getValue("id") as string;
      const jobName = row.getValue("jobName") as string;
      return (
        // <p>
        //   <span className="text-violet-500">{id}</span> - {jobName}
        // </p>

        <div className="flex space-x-3">
          <div className="inline-flex flex-col">
            <Link
              to={`/tasktower/jobs/${row.getValue("id")}`}
              params={{ id: row.getValue("id") }}
              className="block text-sky-600 hover:text-sky-500 hover:underline"
            >
              {jobName}
            </Link>
            <p className="block text-xs text-slate-700">{id}</p>
          </div>
        </div>
      );
    },
  },
  // {
  //   accessorKey: "status",
  //   header: "Status",
  // },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Status" canSort={true} />
    ),
    cell: ({ row }) => {
      const status = row.getValue("status") as string;
      return (
        <p>
          {(status?.length ?? 0) > 0 ? (
            <JobStatusBadge status={status as JobStatus} />
          ) : (
            "—"
          )}
        </p>
      );
    },
  },
  {
    accessorKey: "jobName",
    header: "Job Name",
  },
  {
    accessorKey: "queue",
    header: "Queue",
  },
  {
    accessorKey: "payload",
    header: "Payload",
  },
  {
    accessorKey: "retries",
    header: "Retries",
  },
  {
    accessorKey: "maxRetries",
    header: "Max Retries",
  },
  {
    accessorKey: "retried",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Retried" canSort={false} />
    ),
    cell: ({ row }) => {
      const maxRetries = row.getValue("maxRetries") as number;
      const retries = row.getValue("retries") as number;
      const status = row.getValue("status") as string;

      if (status === "Completed") {
        return <p>—</p>;
      }

      const retriesRemaining = maxRetries - retries;

      return status === "Dead" || status === "Failed" ? (
        <p>
          {retries}/{maxRetries}
        </p>
      ) : (
        <p>{retriesRemaining >= 0 ? `${retries}/${maxRetries}` : "—"}</p>
      );
    },
  },
  {
    accessorKey: "runAfter",
    header: ({ column }) => (
      <DataTableColumnHeader
        column={column}
        title="Next Run In"
        canSort={true}
      />
    ),
    cell: ({ row }) => {
      const runAfter = row.getValue("runAfter") as string;

      if (runAfter < new Date().toISOString()) {
        return <p>—</p>;
      }

      return (
        <p>
          {(runAfter?.length ?? 0) > 0
            ? formatDistanceToNow(parseISO(runAfter), { addSuffix: true })
            : "—"}
        </p>
      );
    },
  },
  {
    accessorKey: "ranAt",
    header: ({ column }) => (
      <DataTableColumnHeader column={column} title="Ran" canSort={true} />
    ),
    cell: ({ row }) => {
      const ranAt = row.getValue("ranAt") as string;

      if (!ranAt) {
        return <p>—</p>;
      }

      return (
        <p>
          {(ranAt?.length ?? 0) > 0
            ? formatDistanceToNow(parseISO(ranAt), { addSuffix: true })
            : "—"}
        </p>
      );
    },
  },
  // {
  //   accessorKey: "createdAt",
  //   header: "Created At",
  // },
  // {
  //   accessorKey: "deadline",
  //   header: "Deadline",
  // },
];

// const formatCountdown = ({ isoDate }: { isoDate: string }) => {
//   const targetDate = parseISO(isoDate);
//   return formatDistanceToNow(targetDate, { addSuffix: true });
// };

// const formatCountdown = ({ isoDate }: { isoDate: string }) => {
//   const now = new Date();
//   const targetDate = parseISO(isoDate);
//   const secondsDiff = differenceInSeconds(targetDate, now);

//   if (secondsDiff === 1) {
//     return `in 1 second`;
//   }

//   // If the difference is less than 60 seconds, format it as "in xx seconds"
//   if (secondsDiff >= 0 && secondsDiff <= 60) {
//     return `in ${secondsDiff} seconds`;
//   }

//   return formatDistanceToNow(targetDate, { addSuffix: true });
// };

// const useCountdown = ({ isoDate }: { isoDate: string }) => {
//   const [countdown, setCountdown] = useState(() =>
//     formatCountdown({ isoDate })
//   );

//   useEffect(() => {
//     const intervalId = setInterval(() => {
//       setCountdown(formatCountdown({ isoDate }));
//     }, 1000);

//     return () => clearInterval(intervalId);
//   }, [isoDate]);

//   return countdown;
// };

// const CountdownTimer = ({ isoDate }: { isoDate: string }) => {
//   const countdown = useCountdown({ isoDate });

//   return <div>{countdown}</div>;
// };

function IndeterminateCheckbox({
  indeterminate,
  className = "",
  ...rest
}: { indeterminate?: boolean } & HTMLProps<HTMLInputElement>) {
  const ref = React.useRef<HTMLInputElement>(null!);

  React.useEffect(() => {
    if (typeof indeterminate === "boolean") {
      ref.current.indeterminate = !rest.checked && indeterminate;
    }
  }, [ref, indeterminate, rest.checked]);

  return (
    <input
      type="checkbox"
      ref={ref}
      className={cn("cursor-pointer accent-emerald-300", className)}
      {...rest}
    />
  );
}
