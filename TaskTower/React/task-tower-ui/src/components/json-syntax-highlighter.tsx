import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { coy as style } from "react-syntax-highlighter/dist/esm/styles/prism";

export const JsonSyntaxHighlighter = ({ json }: { json: string }) => {
  try {
    const formattedJson = JSON.stringify(JSON.parse(json), null, 2);
    return (
      <SyntaxHighlighter language="json" style={style}>
        {formattedJson}
      </SyntaxHighlighter>
    );
  } catch (error) {
    return <div>Error displaying JSON</div>;
  }
};
