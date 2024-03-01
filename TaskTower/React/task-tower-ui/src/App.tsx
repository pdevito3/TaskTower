import { useQuery } from "@tanstack/react-query";
import axios from "axios";
import { getEnv } from "./utils/environment-utilities";

export default function App() {
  const environment = getEnv();
  return (
    <div className="p-8">
      <h1 className="text-xl font-bold text-violet-500">
        Hello Task Tower in ({environment})
      </h1>
      <Jobs />
    </div>
  );
}

function useJobs() {
  return useQuery({
    queryKey: ["jobs"],
    queryFn: async () =>
      axios
        .get(
          getEnv() === "Standalone"
            ? "https://localhost:5130/api/v1/jobs"
            : "/api/v1/jobs"
        )
        .then((response) => response.data),
  });
}

function Jobs() {
  const { isLoading, data } = useJobs();

  if (isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {data &&
        data?.map(
          (job: {
            id: string;
            imageLink: string;
            title: string;
            description: string;
            jobSourceLink: string;
          }) => (
            <div key={job.id} className="p-4 bg-white rounded-lg shadow-lg">
              <img
                src={job.imageLink}
                alt={job.title}
                className="object-cover w-full h-32 sm:h-48"
              />
              <div className="p-4">
                <h2 className="text-lg font-bold">{job.title}</h2>
                <p className="text-sm">{job.description}</p>
                <a
                  href={job.jobSourceLink}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Source
                </a>
              </div>
            </div>
          )
        )}
    </div>
  );
}
