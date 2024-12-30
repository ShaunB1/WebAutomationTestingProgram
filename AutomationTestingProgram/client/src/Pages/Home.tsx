import React, { useState } from "react";
import FileUpload from "../Components/SetArguments/SetArguments.tsx";
import LogDisplay from "../Components/LogDisplay/LogDisplay.tsx";
import DataTable from "../Components/DataTable/DataTable.tsx";
import { Box, Button, TextField } from "@mui/material";
import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";

interface TableData {
    name: string;
}

const Home: React.FC = () => {
    const [tables, setTables] = useState<TableData[]>([]);
    const [testCaseName, setTestCaseName] = useState<string>("");

    const addTable = () => {
        if (testCaseName.trim() === "") return;
        setTables([...tables, { name: testCaseName }]);
        setTestCaseName("");
    }

    return (
        <>
            <AuthenticatedTemplate>
                <Box sx={{ display: "flex", flexDirection: "column", gap: "10px" }}>
                    <FileUpload />
                    <LogDisplay />
                </Box>
                <Box
                    sx={{
                        display: "flex",
                        alignItems: "center",
                        gap: "10px",
                        marginBottom: "20px",
                        marginTop: "20px",
                    }}
                >
                    <TextField
                        value={testCaseName}
                        onChange={(e) => setTestCaseName(e.target.value)}
                        label="Cycle Group Name"
                        variant="outlined"
                        size="small"
                    />
                    <Button variant="contained" onClick={addTable}>
                        Add Table
                    </Button>
                </Box>

                {tables.map((table, index) => (
                    <div key={index} style={{ marginBottom: "40px" }}>
                        <DataTable testCaseName={table.name} />
                    </div>
                ))}
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                Welcome to the QA Regression Team's Automated Testing Program. Please
                sign in to continue.
            </UnauthenticatedTemplate>
        </>
    );
}

export default Home;