import { useState } from 'react';
import EnvNavContainer from "../components/EnvNavContainer/EnvNavContainer";
import CredsContainer from "../components/CredsContainer/CredsContainer";

function EnvPage() {
    const [currentCreds, setCurrentCreds] = useState({ username: "", password: "" });

    const handleUrlClick = (e: any, url: string) => {
        e.preventDefault();
        chrome.runtime.sendMessage(
            {
                action: "openTabAndLogin",
                url: url,
                username: currentCreds.username,
                password: currentCreds.password
            }
        )
    };

    return (
        <>
            <h2>Current Auto-Login Credentials: <span style={{ "fontWeight": "normal" }}>{currentCreds.username}</span></h2>
            <div style={{ display: "flex", height: "100%" }}>
                <div style={{ marginRight: "20px" }}>
                    <EnvNavContainer handleUrlClick={handleUrlClick} />
                </div>
                <div style={{ height: "100%" }}>
                    <CredsContainer setCurrentCreds={setCurrentCreds} />
                </div>
            </div>
        </>
    );
}

export default EnvPage;