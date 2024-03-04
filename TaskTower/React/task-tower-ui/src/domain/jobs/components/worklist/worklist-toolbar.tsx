"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useDebouncedValue } from "@/hooks/useDebouncedValue";
import { useEffect, useState } from "react";
import { useJobsTableStore } from "./jobs-worklist.store";

export function JobsWorklistToolbar() {
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
          {/* <FilterControl title="Status" options={statuses} /> */}
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
        </div>
      </div>
    </div>
  );
}

// const statuses = [
//   {
//     value: "Draft",
//     label: "Draft",
//     icon: CircleIcon,
//   },
//   {
//     value: "Ready For Testing",
//     label: "Ready For Testing",
//     icon: TimerIcon,
//   },
//   {
//     value: "Testing",
//     label: "Testing",
//     icon: TimerIcon,
//   },
//   {
//     value: "Testing Complete",
//     label: "Testing Complete",
//     icon: TimerIcon,
//   },
//   {
//     value: "Report Pending",
//     label: "Report Pending",
//     icon: TimerIcon,
//   },
//   {
//     value: "Report Complete",
//     label: "Report Complete",
//     icon: TimerIcon,
//   },
//   {
//     value: "Completed",
//     label: "Completed",
//     icon: TimerIcon,
//   },
//   {
//     value: "Abandoned",
//     label: "Abandoned",
//     icon: TimerIcon,
//   },
//   {
//     value: "Cancelled",
//     label: "Cancelled",
//     icon: TimerIcon,
//   },
//   {
//     value: "Qns",
//     label: "QNS",
//     icon: TimerIcon,
//   },
// ] as { value: JobStatus; label: string; icon: any }[];
