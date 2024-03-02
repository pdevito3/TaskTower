import { DataTableColumnHeader } from "@/components/data-table/data-table-column-header";
import { JobStatusBadge } from "@/domain/jobs/components/job-status";
import { Job, JobStatus } from "@/domain/jobs/types";
import { ColumnDef } from "@tanstack/react-table";

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
      <DataTableColumnHeader column={column} title="Status" canSort={false} />
    ),
    cell: ({ row }) => {
      const status = row.getValue("status") as string;
      console.log(status);
      // const containerType = row.getValue("status") as string;
      console.log(row);
      return (
        <div className="flex flex-col">
          <p>
            {(status?.length ?? 0) > 0 ? (
              <JobStatusBadge status={status as JobStatus} />
            ) : (
              "—"
            )}
          </p>
        </div>
      );
    },
  },
  {
    accessorKey: "jobName",
    header: "Job Name",
  },
  {
    accessorKey: "method",
    header: "Method",
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
    header: "Run After",
  },
  {
    accessorKey: "ranAt",
    header: "Ran At",
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
