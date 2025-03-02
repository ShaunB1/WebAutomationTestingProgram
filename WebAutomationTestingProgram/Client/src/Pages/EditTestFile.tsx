import {ColDef} from "ag-grid-community";
import {useEffect, useRef, useState} from "react";
import {AgGridReact} from "ag-grid-react";
import {Box, Button, Typography} from "@mui/material";
import * as XLSX from "xlsx";

interface TestStep {
    TESTCASENAME: string;
    TESTSTEPDESCRIPTION: string;
    ACTIONONOBJECT: string;
    OBJECT: string;
    VALUE: string;
    COMMENTS: string;
    LOCAL_ATTEMPTS: string;
    LOCAL_TIMEOUT: string;
    CONTROL: string;
    TESTSTEPTYPE: string;
    GOTOSTEP: string;
    CYCLEGROUP: string;
}

const EditTestFile = () => {
    const [file, setFile] = useState<string>("")
    const [rowData, setRowData] = useState<TestStep[]>([]);
    const [validGrouping, setValidGrouping] = useState("");
    const [colDefs] = useState<ColDef[]>([
        { field: "TESTCASENAME", editable: true },
        { field: "TESTSTEPDESCRIPTION", editable: true },
        { field: "ACTIONONOBJECT", editable: true },
        { field: "OBJECT", editable: true },
        { field: "VALUE", editable: true },
        { field: "COMMENTS", editable: true },
        { field: "LOCAL_ATTEMPTS", editable: true },
        { field: "LOCAL_TIMEOUT", editable: true },
        { field: "CONTROL", editable: true },
        { field: "TESTSTEPTYPE", editable: true },
        { field: "GOTOSTEP", editable: true },
        { field: "CYCLEGROUP", editable: true },
    ])

    const gridRef = useRef<any>(null);

    useEffect(() => {
        if (gridRef.current) {
            gridRef.current.api?.sizeColumnsToFit();
        }
    }, []);

    const handleImport = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;
        if (files && files[0]) {
            try {
                const reader = new FileReader();

                reader.onload = (event) => {
                    const data = event.target?.result;
                    if (typeof data === "string" || data instanceof ArrayBuffer) {
                        const workbook = XLSX.read(data, { type: "binary" });
                        const firstSheetName = workbook.SheetNames[0];
                        const worksheet = workbook.Sheets[firstSheetName];

                        const jsonData: TestStep[] = XLSX.utils.sheet_to_json(worksheet, { defval: "" });

                        if (jsonData.length === 0) {
                            alert("The selected excel file is empty.");
                            return;
                        }

                        const mappedData: TestStep[] = jsonData.map(row => ({
                            TESTCASENAME: String(row.TESTCASENAME || ""),
                            TESTSTEPDESCRIPTION: String(row.TESTSTEPDESCRIPTION || ""),
                            ACTIONONOBJECT: String(row.ACTIONONOBJECT || ""),
                            OBJECT: String(row.OBJECT || ""),
                            VALUE: String(row.VALUE || ""),
                            COMMENTS: String(row.COMMENTS || ""),
                            LOCAL_ATTEMPTS: String(row.LOCAL_ATTEMPTS || ""),
                            LOCAL_TIMEOUT: String(row.LOCAL_TIMEOUT || ""),
                            CONTROL: String (row.CONTROL || ""),
                            TESTSTEPTYPE: String(row.TESTSTEPTYPE),
                            GOTOSTEP: String(row.GOTOSTEP),
                            CYCLEGROUP: String(row.CYCLEGROUP || ""),
                        }));

                        setRowData(prevRowData => [...prevRowData, ...mappedData]);
                    }
                }

                reader.onerror = (error) => {
                    console.error(`Error reading file: ${error}`);}

                reader.readAsArrayBuffer(files[0]);

                e.target.value = "";
            } catch (e) {
                console.log(e);
            }
        }
    }

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files.length > 0) {
            const file = event.target.files[0];
            setFile(file.name);
        }

        handleImport(event);
    }

    const handleGroupValidation = () => {
        let stack = 0

        rowData.forEach(testStep => {
            const group = testStep.CYCLEGROUP;
            if (group.toLowerCase() === "start") {
                stack++;
            } else if (group.toLowerCase() === "end") {
                stack--;
                if (stack < 0) {
                    setValidGrouping("Invalid Grouping");
                }
            }
        });

        const valid = stack === 0 ? "Valid Grouping" : "Invalid Grouping";
        setValidGrouping(valid);
    }

    return (
        <>
            <Box sx={{ width: "100%", height: "auto" }}>
                <Typography variant="h6">Edit Test File</Typography>
                <Box
                    sx={{
                        width: "100%",
                        height: "100%",
                        display: "flex",
                        alignItems: "center",
                        gap: 2,
                        mb: 2,
                    }}
                >
                    <Button variant="contained" component="label">
                        Upload File
                        <input type="file" hidden accept=".xlsx, .xls" onChange={handleFileChange} />
                    </Button>
                    <Typography variant="body2" color="textSecondary">
                        {file || "No file chosen"}
                    </Typography>
                    <Button variant="contained" onClick={handleGroupValidation} >
                        Validate Cycle Groups
                    </Button>
                    <Typography>
                        {validGrouping}
                    </Typography>
                </Box>
                <Box
                    className="ag-theme-quartz"
                    sx={{
                        width: "100%",
                        height: "40vh",
                        overflowY: "auto",
                    }}
                >
                    <AgGridReact
                        ref={gridRef}
                        columnDefs={colDefs}
                        rowData={rowData}
                        onGridReady={() => gridRef.current.api.sizeColumnsToFit()}
                    />
                </Box>
                <Box
                    sx={{
                        mt: 2,
                        width: "50%",
                        height: "60vh",
                        outline: "2px solid red",
                    }}
                >

                </Box>
            </Box>
        </>
    );
}

export default EditTestFile;