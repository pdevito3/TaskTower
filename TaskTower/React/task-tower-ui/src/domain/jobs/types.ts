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
