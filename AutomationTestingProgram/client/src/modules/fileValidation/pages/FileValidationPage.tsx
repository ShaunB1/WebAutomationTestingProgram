import {Box, Button, List, ListItem, Typography} from "@mui/material";
import React, {useState} from "react";
import {AgGridReact} from "ag-grid-react";
import {ColDef} from "ag-grid-community";
import * as XLSX from "xlsx";

interface TestStep {
    testcasename: string;
    description: string;
    actiononobject: string;
    object: string;
    value: string;
    comments: string;
    attempts: string;
    timeout: string;
    control: string;
    steptype: string;
    goto: string;
    group: string;
}

const FileValidationPage = ()=> {
    const [rowData, setRowData] = useState<TestStep[]>([]);
    const [colDefs] = useState<ColDef[]>([
        {field: "testcasename", headerName: "TESTCASENAME"},
        { field: "description", headerName: "TESTSTEPDESCRIPTION"},
        { field: "actiononobject", headerName: "ACTIONONOBJECT"},
        { field: "object", headerName: "OBJECT"},
        { field: "value", headerName: "VALUE"},
        { field: "comments", headerName: "COMMENTS"},
        { field: "attempts", headerName: "LOCAL_ATTEMPTS"},
        { field: "timeout", headerName: "LOCAL_TIMEOUT"},
        { field: "control", headerName: "CONTROL"},
        { field: "steptype", headerName: "TESTSTEPTYPE"},
        { field: "goto", headerName: "GOTOSTEP"},
        { field: "group", headerName: "CYCLEGROUP"},
    ]);

    const [file, setFile] = useState("");
    const [unknownActions, setUnknownActions] = useState<Set<string> | null>(null);

    const handleFileVerification = async (event: any) => {
        const file = event.target.files[0];
        setFile(file);
        if (file) {
            try {
                const reader = new FileReader();
                reader.onload = async (event) => {
                    const data = event.target?.result;
                    const workbook = XLSX.read(data, { type: "binary" });
                    const firstSheetName = workbook.SheetNames[0];
                    const worksheet = workbook.Sheets[firstSheetName];
                    const actionOnObjects: any[] = []

                    await fetch('/static/actions.json')
                        .then(response => {
                            console.log(response);
                            if (!response.ok) {
                                throw new Error(`HTTP error! status: ${response.status}`);
                            }
                            return response.text(); // Read as text to handle potential HTML response
                        })
                        .then(data => {
                            try {
                                const json = JSON.parse(data); // Attempt to parse JSON
                                Object.keys(json).forEach((key) => {
                                    actionOnObjects.push(key);
                                })
                            } catch (error) {
                                console.error('Error parsing JSON:', error, data); // Log the raw response
                            }
                        })
                        .catch(error => console.error('Error fetching actions.json:', error));

                    const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, { defval: "" });

                    if (jsonData.length === 0) {
                        alert("The selected Excel file is empty.");
                        return;
                    }

                    const unknownActions = new Set<string>();
                    const mappedData: TestStep[] = jsonData.map((row) => {
                        const action = row.ACTIONONOBJECT.toLowerCase().replace(/\s+/g, "").trim();
                        if (!actionOnObjects.includes(action)) {
                            unknownActions.add(action);
                            return null
                        }
                        return row;
                    });

                    setUnknownActions(unknownActions);
                    setRowData(prevRows => [...prevRows, ...mappedData]);
                }

                reader.onerror = (error) => {
                    console.error(`Error reading file: ${error}`);
                }

                reader.readAsArrayBuffer(file);
                event.target.value = "";
            } catch (e) {
                console.log(e);
            }
        }
    }

    return (
        <>
            <Box
                className="ag-theme-quartz"
                sx={{
                    width: "95vw",
                    height: "80vh",
                }}
            >
                <Box>
                    <Button variant="contained" component="label">
                        Verify File
                        <input type="file" hidden accept=".xlsxm, .xls, .xlsx" onChange={handleFileVerification} />
                    </Button>
                    <Typography variant={"body2"} color={"textSecondary"}>
                        {file ? (file as any).name : "No file chosen"}
                    </Typography>
                </Box>
                <Typography variant="h6">Unknown ActionOnObjects ({unknownActions?.size})</Typography>
                <Box
                    sx={{
                        width: "80vw",
                        height: "10vh",
                        border: "2px solid gray",
                        borderRadius: 1,
                        display: "flex",
                        flexWrap: "wrap",
                        gap: 2,
                    }}
                >
                    {unknownActions && Array.from(unknownActions as any).map((action: any, index) => (
                        <Box
                            sx={{
                                display: "flex",
                                width: "auto",
                                minHeight: "auto",
                            }}
                        >
                            <Typography key={index}>{action}</Typography>
                        </Box>
                    ))}
                </Box>
                {/*<AgGridReact*/}
                {/*    rowData={rowData}*/}
                {/*    columnDefs={colDefs}*/}
                {/*    animateRows={true}*/}
                {/*    rowSelection="multiple"*/}
                {/*    pagination={false}*/}
                {/*    domLayout="normal"*/}
                {/*/>*/}
            </Box>
        </>
    );
}

export default FileValidationPage;