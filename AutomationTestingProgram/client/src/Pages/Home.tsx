import React, { useState } from "react";
import FileUpload from "../Components/SetArguments/SetArguments.tsx";
import LogDisplay from "../Components/LogDisplay/LogDisplay.tsx";
import DataTable from "../Components/DataTable/DataTable.tsx";
import { Box, Button, TextField, Select } from "@mui/material";

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
        </>
    );
}

export default Home;