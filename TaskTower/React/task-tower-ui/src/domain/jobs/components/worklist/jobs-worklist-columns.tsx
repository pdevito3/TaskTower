import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { JobStatusBadge } from "@/domain/jobs/components/job-status";
import { Job, JobStatus } from "@/domain/jobs/types";
import { ColumnDef } from "@tanstack/react-table";
import { formatDistanceToNow, parseISO } from "date-fns";

type Columns = ColumnDef<Job>;
export const createColumns = (): Columns[] => [
  {
    accessorKey: "id",
    header: "Id",
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
