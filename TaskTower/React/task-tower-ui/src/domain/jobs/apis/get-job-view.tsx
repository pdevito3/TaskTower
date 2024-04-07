import { isStandaloneEnv } from "@/utils";
import { useQuery } from "@tanstack/react-query";
import axios, { AxiosResponse } from "axios";
import { TaskTowerJobView } from "../types";
import { JobKeys } from "./job.keys";

export const getJobView = async (jobId: string) => {
  const url = isStandaloneEnv()
    ? `http://localhost:5130/api/v1/jobs/${jobId}/view`
    : `/api/v1/jobs/view`;
  return axios
    .get(url)
    .then((response: AxiosResponse<TaskTowerJobView>) => response.data);
};

export const useGetJobView = (jobId: string) => {
  return useQuery({
    queryKey: JobKeys.detail(jobId),
    queryFn: () => getJobView(jobId),
    enabled: !!jobId,
    gcTime: 0,
    staleTime: 0,
    refetchInterval: (data) => {
      const status = data.state?.data?.job.status;
      return status === "Processing" ||
        status === "Pending" ||
        status === "Enqueued"
        ? 3000
        : false;
    },
  });
};
