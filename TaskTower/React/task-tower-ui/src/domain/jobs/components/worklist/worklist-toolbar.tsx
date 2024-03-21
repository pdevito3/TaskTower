"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { JobStatus } from "@/domain/jobs/types";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { Trash2Icon } from "lucide-react";
import { useEffect, useState } from "react";
import { useInvalidateJobListQuery } from "../../apis/get-jobs-worklist";
import { useGetQueueNames } from "../../apis/get-queue-names";
import { QueueFilterControl } from "./job-queue-filter-control";
import { FilterControl } from "./job-status-filter-control";
import { useJobsTableStore } from "./jobs-worklist.store";

export function JobsWorklistToolbar({
  handleJobDeletion,
  hasRowsSelected,
}: {
  handleJobDeletion: () => void;
  hasRowsSelected: boolean;
}) {
  const { filterInput, setFilterInput, isFiltered, resetFilters } =
    useJobsTableStore();
  const [liveValue, setLiveValue] = useState(filterInput);
  const [debouncedFilterInput] = useDebouncedValue(liveValue, 400);

  useEffect(() => {
    setFilterInput(debouncedFilterInput);
    if (debouncedFilterInput === null) {
      resetFilters();
    }
  }, [debouncedFilterInput, resetFilters, setFilterInput]);

  const { data } = useGetQueueNames();
  const queues = data?.map((queue) => ({ label: queue, value: queue })) ?? [];

  const invalidateJobListQuery = useInvalidateJobListQuery();

  return (
    <div className="flex-col space-y-3 sm:flex-row sm:flex sm:items-center sm:justify-between sm:flex-1 sm:space-y-0">
      <div className="flex items-center justify-between">
        <div className="flex items-center flex-1 space-x-2">
          <Input
            autoFocus={true}
            placeholder="Filter jobs..."
            value={(liveValue as string) ?? ""}
            onChange={(event) => {
              setLiveValue(event.currentTarget.value);
            }}
            className="w-48 lg:w-54"
          />
          <FilterControl title="Status" options={statuses} />
          <QueueFilterControl title="Queue" options={queues} />
          {isFiltered.result() && (
            <Button
              variant="ghost"
              onClick={() => {
                setLiveValue(null);
                resetFilters();
              }}
            >
              Reset
              <div className="ml-2">
                <svg
                  className="h-5 w-5 text-rose-500"
                  xmlns="http://www.w3.org/2000/svg"
                  width="200"
                  height="200"
                  viewBox="0 0 32 32"
                >
                  <path
                    fill="currentColor"
                    d="M22.5 9a7.452 7.452 0 0 0-6.5 3.792V8h-2v8h8v-2h-4.383a5.494 5.494 0 1 1 4.883 8H22v2h.5a7.5 7.5 0 0 0 0-15Z"
                  />
                  <path
                    fill="currentColor"
                    d="M26 6H4v3.171l7.414 7.414l.586.586V26h4v-2h2v2a2 2 0 0 1-2 2h-4a2 2 0 0 1-2-2v-8l-7.414-7.415A2 2 0 0 1 2 9.171V6a2 2 0 0 1 2-2h22Z"
                  />
                </svg>
              </div>
            </Button>
          )}
          <Button variant="secondary" onClick={invalidateJobListQuery}>
            <p className="sr-only">Refresh</p>
            {/* https://iconbuddy.app/jam/refresh */}
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width={200}
              height={200}
              viewBox="-1.5 -2.5 24 24"
              className="w-5 h-5 text-slate-800"
            >
              <path
                fill="currentColor"
                d="m17.83 4.194l.42-1.377a1 1 0 1 1 1.913.585l-1.17 3.825a1 1 0 0 1-1.248.664l-3.825-1.17a1 1 0 1 1 .585-1.912l1.672.511A7.381 7.381 0 0 0 3.185 6.584l-.26.633a1 1 0 1 1-1.85-.758l.26-.633A9.381 9.381 0 0 1 17.83 4.194zM2.308 14.807l-.327 1.311a1 1 0 1 1-1.94-.484l.967-3.88a1 1 0 0 1 1.265-.716l3.828.954a1 1 0 0 1-.484 1.941l-1.786-.445a7.384 7.384 0 0 0 13.216-1.792a1 1 0 1 1 1.906.608a9.381 9.381 0 0 1-5.38 5.831a9.386 9.386 0 0 1-11.265-3.328z"
              />
            </svg>
          </Button>
          <Button
            variant="destructive"
            onClick={handleJobDeletion}
            disabled={!hasRowsSelected}
          >
            <p className="sr-only">Delete Jobs</p>
            <Trash2Icon className="w-4 h-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}

const statuses = [
  {
    value: "Pending",
    label: "Pending",
    icon: () => {
      return (
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={200}
          height={200}
          viewBox="0 0 24 24"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <path
            fill="currentColor"
            d="M18 3h-3.18C14.4 1.84 13.3 1 12 1s-2.4.84-2.82 2H6c-1.1 0-2 .9-2 2v15c0 1.1.9 2 2 2h6.11a6.743 6.743 0 0 1-1.42-2H6V5h2v1c0 1.1.9 2 2 2h4c1.1 0 2-.9 2-2V5h2v5.08c.71.1 1.38.31 2 .6V5c0-1.1-.9-2-2-2zm-6 2c-.55 0-1-.45-1-1s.45-1 1-1s1 .45 1 1s-.45 1-1 1zm5 7c-2.76 0-5 2.24-5 5s2.24 5 5 5s5-2.24 5-5s-2.24-5-5-5zm1.29 7l-1.65-1.65a.51.51 0 0 1-.15-.35v-2.49c0-.28.22-.5.5-.5s.5.22.5.5v2.29l1.5 1.5a.495.495 0 1 1-.7.7z"
          />
        </svg>
      );
    },
  },
  {
    value: "Enqueued",
    label: "Enqueued",
    icon: () => {
      return (
        // https://iconbuddy.app/heroicons/queue-list-16-solid
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={200}
          height={200}
          viewBox="0 0 16 16"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <path
            fill="currentColor"
            d="M2 4a2 2 0 0 1 2-2h8a2 2 0 1 1 0 4H4a2 2 0 0 1-2-2m0 5.25a.75.75 0 0 1 .75-.75h10.5a.75.75 0 0 1 0 1.5H2.75A.75.75 0 0 1 2 9.25m.75 3.25a.75.75 0 0 0 0 1.5h10.5a.75.75 0 0 0 0-1.5z"
          />
        </svg>
      );
    },
  },
  {
    value: "Processing",
    label: "Processing",
    icon: () => {
      return (
        // https://iconbuddy.app/icon-park-outline/loading-three
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={200}
          height={200}
          viewBox="0 0 48 48"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <path
            fill="none"
            stroke="currentColor"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={4}
            d="M24 44c11.046 0 20-8.954 20-20S35.046 4 24 4S4 12.954 4 24s8.954 20 20 20Zm0-32v3m8.485.515l-2.121 2.121M36 24h-3m-.515 8.485l-2.121-2.121M24 36v-3m-8.485-.515l2.121-2.121M12 24h3m.515-8.485l2.121 2.121"
          />
        </svg>
      );
    },
  },
  {
    value: "Completed",
    label: "Completed",
    icon: () => {
      return (
        <svg
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth="1.5"
          stroke="currentColor"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M10.125 2.25h-4.5c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125v-9M10.125 2.25h.375a9 9 0 0 1 9 9v.375M10.125 2.25A3.375 3.375 0 0 1 13.5 5.625v1.5c0 .621.504 1.125 1.125 1.125h1.5a3.375 3.375 0 0 1 3.375 3.375M9 15l2.25 2.25L15 12"
          />
        </svg>
      );
    },
  },
  {
    value: "Failed",
    label: "Failed",
    icon: () => {
      return (
        // https://iconbuddy.app/streamline/interface-arrows-synchronize-warning-arrow-fail-notification-sync-warning-failure-synchronize-error
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={200}
          height={200}
          viewBox="0 0 14 14"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <g
            fill="none"
            stroke="currentColor"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="m11 9l2-.5l.5 2" />
            <path d="M13 8.5A6.6 6.6 0 0 1 7 13h0a6 6 0 0 1-5.64-3.95M3 5l-2 .5l-.5-2" />
            <path d="M1 5.5A6.79 6.79 0 0 1 7 1h0a6 6 0 0 1 5.64 4M7 3.5v4" />
            <circle cx={7} cy={10} r=".5" />
          </g>
        </svg>
      );
    },
  },
  {
    value: "Dead",
    label: "Dead",
    icon: () => {
      return (
        // https://iconbuddy.app/si-glyph/skull
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={200}
          height={200}
          viewBox="0 0 17 17"
          className="w-4 h-4 mr-2 text-zinc-800"
        >
          <path
            fill="currentColor"
            fillRule="evenodd"
            d="M14.828 12.836C15.715 11.589 17 9.804 17 7.85C17 3.993 15.063 0 9.031 0c-6.031 0-8 3.992-8 7.85c0 1.947 1.391 3.728 2.186 4.973c.309.479.451 1.872.451 1.983c0 .082.012.161.029.24c.09.625.631.927 1.297.95c0 0 .496.009.571 0c.757-.087 1.344-.411 1.423-1.049c.064.631.668 1.049 2.027 1.049s1.943-.396 2.023-1.008c.107.634.723.947 1.496 1.008c.053.004.575 0 .575 0c.704-.054 1.261-.438 1.261-1.133l-.011-.004c.001-.022.007-.043.007-.064c.001-.122.148-1.516.462-1.959zM5.999 9a2 2 0 1 1 0-3.998A2 2 0 0 1 6 9zm3.011 3.848c-.824 0-1.493.439-1.493-.178c0-.616.669-1.891 1.493-1.891c.827 0 1.494 1.274 1.494 1.891c0 .618-.667.178-1.494.178zM11.999 9A2 2 0 1 1 12 5a2 2 0 0 1 0 4z"
          />
        </svg>
      );
    },
  },
] as { value: JobStatus; label: string; icon: React.ElementType }[];
