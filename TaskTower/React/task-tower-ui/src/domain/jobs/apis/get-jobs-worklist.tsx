import { Job } from "@/domain/jobs/types";
import { PagedResponse, Pagination } from "@/types/apis";
import { isStandaloneEnv } from "@/utils";
import { useQuery } from "@tanstack/react-query";
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
  filters?: string;
  sortOrder?: SortingState;
}

export const useJobs = ({
  pageNumber = 1,
  pageSize = 10,
  filters,
  sortOrder,
  delayInMs = 0,
}: JobsListHookProps = {}) => {
  const queryParams = queryString.stringify({
    pageNumber,
    pageSize,
    filters,
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
