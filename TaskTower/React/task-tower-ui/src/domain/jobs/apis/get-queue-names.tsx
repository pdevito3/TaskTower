import { isStandaloneEnv } from "@/utils";
import { useQuery } from "@tanstack/react-query";
import axios, { AxiosResponse } from "axios";
import { JobKeys } from "./job.keys";

export const getQueueNames = async () => {
  const url = isStandaloneEnv()
    ? `http://localhost:5130/api/v1/jobs/queueNames`
    : `/api/v1/jobs/queueNames`;
  return axios
    .get(url)
    .then((response: AxiosResponse<string[]>) => response.data);
};

export const useGetQueueNames = () => {
  return useQuery({
    queryKey: JobKeys.queueNames(),
    queryFn: () => getQueueNames(),
    gcTime: 0,
    staleTime: 0,
  });
};
