import { Badge, BadgeVariant } from "../../../components/badge";
import { JobStatus } from "../types";

interface JobStatusBadgeProps {
  status: JobStatus;
  className?: string;
  props?: React.HTMLProps<HTMLSpanElement>;
}

export const JobStatusBadge: React.FC<JobStatusBadgeProps> = ({
  status,
  className,
  props,
}) => {
  let variant: BadgeVariant;

  switch (status) {
    case "Pending":
      variant = "cyan";
      break;
    case "Enqueued":
      variant = "indigo";
      break;
    case "Processing":
      variant = "sky";
      break;
    case "Failed":
      variant = "rose";
      break;
    case "Completed":
      variant = "green";
      break;
    case "Dead":
      variant = "amber";
      break;
    default:
      variant = "gray";
  }

  return (
    <Badge text={status} variant={variant} className={className} {...props} />
  );
};
