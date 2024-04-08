import { isStandaloneEnv } from "@/utils";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { JobKeys } from "./job.keys";

export const requeueJobs = async (jobId: string) => {
  const url = isStandaloneEnv()
    ? `http://localhost:5130/api/v1/jobs/${jobId}/requeue`
    : `/api/v1/jobs/${jobId}/requeue`;
  return axios.put(url);
};

export const useRequeueJob = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: requeueJobs,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: JobKeys.all });
    },
  });
};
