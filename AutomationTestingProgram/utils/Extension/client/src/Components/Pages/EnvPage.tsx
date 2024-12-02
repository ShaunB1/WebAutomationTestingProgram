import { useState } from 'react';
import EnvNavContainer from "../EnvNavContainer/EnvNavContainer";
import CredsContainer from "../CredsContainer/CredsContainer";

function EnvPage() {
    const [currentCreds, setCurrentCreds] = useState({ username: "", password: "" });

    return (
        <>
            <h2>Current Auto-Login Credentials: <span style={{ "fontWeight": "normal" }}>{currentCreds.username}</span></h2>
            <div style={{ display: "flex", height: "100%" }}>
                <div style={{ marginRight: "20px" }}>
                    <EnvNavContainer currentCreds={currentCreds} />
                </div>
                <div style={{ height: "100%" }}>
                    <CredsContainer setCurrentCreds={setCurrentCreds} />
                </div>
            </div>
        </>
    );
}

export default EnvPage;