import React from "react";
import CredsTable from "../CredsTable/CredsTable";

interface CredsContainerProps {
    onSecretName: (secretName: string) => void;
    onSecretValue: (secretValue: string) => void;
}

function CredsContainer({ onSecretValue, onSecretName }: CredsContainerProps) {
    const [searchUser, setSearchUser] = React.useState("");
    const [newUser, setNewUser] = React.useState("");
    const [secretName, setSecretName] = React.useState("");
    const [secretValue, setSecretValue] = React.useState("");

    async function handleFormSubmit(e: any) {
        e.preventDefault();
        if (searchUser !== "") {
            //const res = window.electronAPI.getUser(searchUser);
            setSearchUser("");
        }
    }

    async function handleFindUserSecret() {
        try {
            const formattedEmail = secretName
                .replace(/_/g, "---")
                .replace(/@/g, "--")
                .replace(/\./g, "-");

            //const result = await window.electronAPI.getSecret(formattedEmail);
            //setSecretValue(result);

            onSecretName(secretName);
            //onSecretValue(result);

            setSecretName("");
        } catch (e) {
            throw e;
        }
    }

    function handleUserSecretChange(e: any) {
        setSecretName(e.target.value);
    }

    function handleInputChange(e: any) {
        setSearchUser(e.target.value);
    }

    function handleNewUserChange(e: any) {
        setNewUser(e.target.value);
    }

    async function handleAddUser() {
        if (newUser.trim() !== "") {
            try {
                //const result = await window.electronAPI.addUser(newUser);
                //console.log("User added: ", result);
                setNewUser("");
            } catch (e) {
                console.error("Error adding user: ", e);
            }
        } else {
            console.log("Please enter a user name");
        }
    }

    return (
        <>
            <input type={"text"} placeholder={"Add User..."} value={newUser} onChange={handleNewUserChange} />
            <button onClick={handleAddUser}>Add User</button>
            <input type="text" placeholder={"Find User Secret..."} value={secretName} onChange={handleUserSecretChange} />
            <button onClick={handleFindUserSecret}>Find Secret</button>
            <form onSubmit={handleFormSubmit}>
                <input type={"search"} placeholder={"Search..."} value={searchUser} onChange={handleInputChange} />
                <button type={"submit"}>Search</button>
            </form>
            <CredsTable />
            {/*<h2>OUI</h2>*/}
            {/*<ul>*/}
            {/*    <li>oui_min_a@ontarioemail.ca</li>*/}
            {/*    <li>oui_min_c@ontarioemail.ca</li>*/}
            {/*    <li>oui_min_u@ontarioemail.ca</li>*/}
            {/*    <li>oui_office_a@ontarioemail.ca</li>*/}
            {/*    <li>oui_office_c@ontarioemail.ca</li>*/}
            {/*    <li>oui_s@ontarioemail.ca</li>*/}
            {/*</ul>*/}
        </>
    );
}

export default CredsContainer;