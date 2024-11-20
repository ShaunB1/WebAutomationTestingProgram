import React, {useState} from "react";
import FileUpload from "../Components/FileUpload/FileUpload.tsx";
import LogDisplay from "../Components/LogDisplay/LogDisplay.tsx";
import DataTable from "../Components/DataTable/DataTable.tsx";
import {Box, Button, TextField} from "@mui/material";

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
            <FileUpload />
            <LogDisplay />
            <Box
                sx={{
                    display: "flex", // Flex layout to align items
                    alignItems: "center", // Vertically center items
                    gap: "10px", // Add spacing between items
                    marginBottom: "20px", // Add spacing below the box
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