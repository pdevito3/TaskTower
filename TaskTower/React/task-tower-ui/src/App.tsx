import { getEnv } from "./utils/environment-utilities";

export default function App() {
  const environment = getEnv();
  return (
    <div className="p-8">
      <h1 className="text-xl font-bold text-violet-500">
        Hello Task Tower in ({environment})
      </h1>
    </div>
  );
}
