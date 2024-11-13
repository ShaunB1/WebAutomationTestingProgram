import React from "react";
import EnvNavContainer from "../Components/EnvNavContainer/EnvNavContainer";
import CredsContainer from "../Components/CredsContainer/CredsContainer";

function EnvPage() {
    const [secretName, setSecretName] = React.useState("");
    const [secretValue, setSecretValue] = React.useState("");

    const handleSecretName = (name: string) => {
        setSecretName(name);
    }

    const handleSecretValue = (value: string) => {
        setSecretValue(value);
    }

    return (
        <>
            <h1>Environments</h1>
            <EnvNavContainer secretName={secretName} secretValue={secretValue} />
            <h1>Search User Credentials</h1>
            <CredsContainer onSecretName={handleSecretName} onSecretValue={handleSecretValue} />
        </>
    );
}

export default EnvPage;