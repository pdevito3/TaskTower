export type JobStatus =
  | "Pending"
  | "Enqueued"
  | "Processing"
  | "Completed"
  | "Failed"
  | "Dead";

export interface Job {
  id: string;
  jobName: string;
  queue: string | null;
  status: JobStatus;
  type: string;
  method: string;
  parameterTypes: string[];
  payload: string;
  retries: number;
  maxRetries: number | null;
  runAfter: Date;
  ranAt: Date | null;
  createdAt: Date;
  deadline: Date | null;
}

export type TaskTowerJobView = {
  job: {
    id: string;
    queue: string | null;
    status: string;
    type: string;
    method: string;
    parameterTypes: string[];
    payload: string;
    retries: number;
    maxRetries: number | null;
    runAfter: Date;
    ranAt: Date | null;
    createdAt: Date;
    deadline: Date | null;
    jobName: string;
    tagNames: string | null;
    tags: string[];
  };
  history: {
    id: string;
    jobId: string;
    status: string;
    comment: string | null;
    details: string | null;
    occurredAt: Date | null;
  }[];
};
