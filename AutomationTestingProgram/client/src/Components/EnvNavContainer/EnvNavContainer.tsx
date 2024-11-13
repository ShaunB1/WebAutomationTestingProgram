import Button from "../Button/Button";
import data from "./environment_list.json";

interface EnvNavContainerProps {
    secretName: string;
    secretValue: string;
}

function EnvNavContainer({ secretName, secretValue }: EnvNavContainerProps) {
    async function handleEnvNav (link: string) {
        // const res = await fetch("/api/testRecorder/navigate", {
        //     method: "POST",
        //     body: link,
        // });

        // if (res.ok) {
        //     alert("File uploaded successfully!");
        // } else {
        //     alert("Failed to upload file.")
        // }
    }

    return (
        <>
            <h1>OPS BPS</h1>
            {data.map((env, index) => {
                if (env.URL && env.URL.trim() !== "") {
                    return (
                        <Button
                            key={index}
                            content={env.ENVIRONMENT}
                            onClick={() => {handleEnvNav(env.URL)}}
                        />
                    )
                }
            })}
            <h1>AAD</h1>
            {data.map((env, index) => {
                if (env.URL2 && env.URL2.trim() !== "") {
                    return (
                        <Button
                            key={index}
                            content={env.ENVIRONMENT}
                            onClick={() => {handleEnvNav(env.URL2)}}
                        />
                    )
                }
            })}
        </>
    );
}

export default EnvNavContainer;