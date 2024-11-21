import React from "react";
import EnvNavContainer from "../Components/EnvNavContainer/EnvNavContainer";
import CredsContainer from "../Components/CredsContainer/CredsContainer";

function EnvPage() {
    return (
        <>
            <div style={{display: "flex", height: "100%"}}>
                <div style={{ marginRight: "100px" }}>
                    <h1>Environments</h1>
                    <EnvNavContainer />
                </div>
                <div style={{height: "100%"}}>
                    <h1>Search User Credentials</h1>
                    <CredsContainer  />
                </div>
            </div>
        </>
    );
}

export default EnvPage;