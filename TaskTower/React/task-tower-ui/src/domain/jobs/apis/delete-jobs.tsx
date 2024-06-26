import { isStandaloneEnv } from "@/utils";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { JobKeys } from "./job.keys";

export const deleteJobs = async (jobIds: string[]) => {
  const url = isStandaloneEnv()
    ? `http://localhost:5130/api/v1/jobs/bulkDelete`
    : `/api/v1/jobs/bulkDelete`;
  return axios.post(url, {
    jobIds: jobIds,
  });
};

export const useDeleteJobs = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteJobs,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: JobKeys.all });
    },
  });
};
