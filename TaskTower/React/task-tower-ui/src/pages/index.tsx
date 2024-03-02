import { Helmet } from "react-helmet";

export function IndexPage() {
  return (
    <>
      <div className="">
        <Helmet>
          <title>Dashboard</title>
        </Helmet>
        Hello dashboard
      </div>
    </>
  );
}
