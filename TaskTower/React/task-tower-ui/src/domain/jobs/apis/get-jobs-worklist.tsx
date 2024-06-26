import { Job } from "@/domain/jobs/types";
import { PagedResponse, Pagination } from "@/types/apis";
import { isStandaloneEnv } from "@/utils";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { SortingState } from "@tanstack/react-table";
import axios, { AxiosResponse } from "axios";
import queryString from "query-string";
import { JobKeys } from "./job.keys";

interface DelayProps {
  hasArtificialDelay?: boolean;
  delayInMs?: number;
}

interface JobsListApiProps extends DelayProps {
  queryString: string;
}

const getJobs = async ({
  queryString,
  hasArtificialDelay = false,
  delayInMs = 0,
}: JobsListApiProps) => {
  const url = isStandaloneEnv()
    ? `http://localhost:5130/api/v1/jobs/paginated?${queryString}`
    : `/api/v1/jobs/paginated?${queryString}`;

  const [result] = await Promise.all([
    axios.get(url).then((response: AxiosResponse) => {
      const jobs: Job[] = response.data;
      const pagination: Pagination = JSON.parse(
        response.headers["x-pagination"] ?? "{}"
      );
      return {
        data: jobs,
        pagination,
      } as PagedResponse<Job>;
    }),
    new Promise((resolve) =>
      setTimeout(resolve, hasArtificialDelay ? delayInMs : 0)
    ),
  ]);

  return result;
};

interface JobsListHookProps extends DelayProps {
  pageNumber?: number;
  pageSize?: number;
  sortOrder?: SortingState;
  status?: string[];
  filterText?: string;
  queue?: string[];
}

export const useJobs = ({
  pageNumber = 1,
  pageSize = 10,
  sortOrder,
  delayInMs = 0,
  status,
  filterText,
  queue,
}: JobsListHookProps = {}) => {
  const queryParams = queryString.stringify({
    pageNumber,
    pageSize,
    filterText: filterText && filterText.length > 0 ? filterText : undefined,
    statusFilter:
      status && status.length > 0
        ? // ? status?.map((status) => `StatusFilter=${status}`).join("&")
          status
        : undefined,
    queueFilter:
      queue && queue.length > 0
        ? // ? queue?.map((queue) => `StatusFilter=${queue}`).join("&")
          queue
        : undefined,
    sortOrder: generateSieveSortOrder(sortOrder),
  });

  const hasArtificialDelay = delayInMs > 0;

  return useQuery({
    queryKey: JobKeys.list(queryParams),
    queryFn: () =>
      getJobs({
        queryString: queryParams,
        hasArtificialDelay,
        delayInMs,
      }),
    gcTime: 0,
    staleTime: 0,
  });
};

// Assuming SortingState is defined elsewhere
// Adapt this function based on your actual sorting logic
export const generateSieveSortOrder = (sortOrder: SortingState | undefined) =>
  sortOrder && sortOrder.length > 0
    ? sortOrder.map((s) => (s.desc ? `-${s.id}` : s.id)).join(",")
    : undefined;

export const useInvalidateJobListQuery = () => {
  const queryClient = useQueryClient();

  const invalidateJobListQuery = () => {
    const queryKey = JobKeys.lists();
    queryClient.invalidateQueries({ queryKey });
  };

  return invalidateJobListQuery;
};
