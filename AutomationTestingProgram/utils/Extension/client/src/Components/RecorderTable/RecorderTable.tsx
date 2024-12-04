import React, {useCallback, useEffect, useRef, useState} from "react";
import { AgGridReact } from "ag-grid-react";
import "ag-grid-community/styles/ag-grid.css"
import "ag-grid-community/styles/ag-theme-quartz.css";
import { ColDef } from "ag-grid-community";
import {Box, Button, TextField, Toolbar, useTheme} from "@mui/material";
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

const RecorderTable: React.FC = () => {
    const theme = useTheme();
    const [inputValue, setInputValue] = useState("");
    const [testCase, setTestCase] = useState("");
    const [altVerify, setAltVerify] = useState(false);
    const [start, setStart] = useState(false);
    const [rowData, setRowData] = useState<TestStep[]>([]);
    const [colDefs, setColDefs] = useState<ColDef[]>([
        {
            field: "TESTCASENAME",
            checkboxSelection: true,
            headerCheckboxSelection: true,
        },
        { field: "TESTSTEPDESCRIPTION" },
        { field: "ACTIONONOBJECT" },
        { field: "OBJECT" },
        { field: "VALUE" },
        { field: "COMMENTS" },
        { field: "LOCAL_ATTEMPTS" },
        { field: "LOCAL_TIMEOUT" },
        { field: "CONTROL" },
        { field: "TESTSTEPTYPE" },
        { field: "GOTOSTEP" },
        { field: "CYCLEGROUP" },
    ])

    const testCaseRef = useRef(testCase);

    const gridRef = useRef<AgGridReact>(null);

    const handleDeleteSelectedRows = () => {
        const selectedRows = gridRef.current?.api.getSelectedRows();
        if (selectedRows && selectedRows.length > 0) {
            setRowData(prevRows => prevRows.filter(row => !selectedRows.includes(row)));
        }
    }

    const renderToolbarButtons = () => (
        <Box>
            <Button
                variant="contained"
                color="secondary"
                onClick={handleDeleteSelectedRows}
            >
                Delete Selected
            </Button>
        </Box>
    );

    useEffect(() => {
        testCaseRef.current = testCase;
    }, [testCase]);

    useEffect(() => {
        const handleMessage = (message: any) => {
            if (message.action === "RECORD_TEST_STEP") {
                // const elDict = JSON.parse(message.locator);
                const testStep: TestStep = {
                    TESTCASENAME: testCaseRef.current,
                    TESTSTEPDESCRIPTION: message.stepValues.testdescription,
                    ACTIONONOBJECT: message.stepValues.actiononobject,
                    OBJECT: message.stepValues.object,
                    VALUE: message.stepValues.value,
                    COMMENTS: message.stepValues.comments,
                    LOCAL_ATTEMPTS: "",
                    LOCAL_TIMEOUT: "",
                    CONTROL: "",
                    TESTSTEPTYPE: "",
                    GOTOSTEP: "",
                    CYCLEGROUP: "",
                };

                console.log("TEST STEP: ", testStep);

                // @ts-ignore
                setRowData((prevRowData) => [...prevRowData, testStep])
            }
        }

        chrome.runtime.onMessage.addListener(handleMessage);

        return () => {
            chrome.runtime.onMessage.removeListener(handleMessage);
        };
    }, []);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(e.target.value);
    }

    const handleSubmit = () => {
        setTestCase(inputValue);
    }

    const handleVerifySwitch = () => {
        setAltVerify(!altVerify);
    }

    const handleClearTable = () => {
        setRowData([]);
    }

    const scrollToBottom = () => {
        const gridApi = gridRef.current?.api;

        if (gridApi) {
            const rowCount = gridApi.getDisplayedRowCount();
            gridApi.ensureIndexVisible(rowCount - 1, "bottom");
        }
    }

    useEffect(() => {
        scrollToBottom();
    }, [rowData]);

    const sendMessageToContentScript = useCallback(() => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "ALT_VERIFY", changeMode: altVerify },
                )
            }
        });
    }, [altVerify])

    const sendStartState = useCallback(() => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "CHANGE_START_STATE", start: start }
                )
            }
        })
    }, [start]);

    const handleStartSwitch = () => {
        setStart(!start);
    }

    const handleExport = () => {
        try {
            const rows: any[] = [];
            gridRef.current?.api.forEachNode((node) => {
                rows.push(node.data);
            });

            const worksheet = XLSX.utils.json_to_sheet(rows);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1")
            XLSX.writeFile(workbook, "recorded_test.xlsx");
        } catch (e) {
            console.log(e);
        }
    }

    // const insertRowsAfterSelected = (selectedRowIndex: any, newSteps: any) => {
    //     if (gridRef.current) {
    //         const api = gridRef.current.api;
    //         const addTransactions = newSteps.map((step, index) => ({
    //             add: [step],
    //             addIndex: selectedRowIndex + index + 1,
    //         }));
    //
    //         addTransactions.forEach((transaction: any) => {
    //             api.applyTransaction(transaction);
    //         })
    //
    //         setRowData(prevRows => {
    //
    //         });
    //     }
    // }

    const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = e.target.files;

        if (files && files[0]) {
            try {
                const file = files[0];
                const reader = new FileReader();

                reader.onload = (event) => {
                    const data = event.target?.result;

                    if (typeof data === "string" || data instanceof ArrayBuffer) {
                        const workbook = XLSX.read(data, { type: "binary" });
                        const firstSheetName = workbook.SheetNames[0];
                        const worksheet = workbook.Sheets[firstSheetName];

                        const jsonData: any[] = XLSX.utils.sheet_to_json(worksheet, { defval: "" });

                        if (jsonData.length === 0) {
                            alert("The selected Excel file is empty.");
                            return;
                        }

                        const mappedData: TestStep[] = jsonData.map((row) => {
                            const testStep: TestStep = {
                                TESTCASENAME: String(row.TESTCASENAME || "test"),
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
                            };

                            return testStep;
                        });

                        setRowData(mappedData);

                        const cols: any[] = Object.keys(mappedData[0]).map((key) => {
                            if (key === "TESTCASENAME") {
                                return {
                                    headerName: key,
                                    field: key,
                                    sortable: true,
                                    filter: true,
                                    resizable: true,
                                    checkboxSelection: true,
                                    headerCheckboxSelection: true,
                                };
                            } else {
                                return {
                                    headerName: key,
                                    field: key,
                                    sortable: true,
                                    filter: true,
                                    resizable: true,
                                }
                            }
                        });

                        setColDefs(cols);
                    }
                }

                reader.onerror = (error) => {
                    console.error(`Error reading file: ${error}`);}

                reader.readAsArrayBuffer(file);
            } catch (e: any) {
                console.log(e);
            }
        }
    }

    useEffect(() => {
        sendMessageToContentScript();
    }, [sendMessageToContentScript]);

    useEffect(() => {
        sendStartState();
    }, [sendStartState]);

    return (
        <>
            <Toolbar
                sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    marginBottom: 2,
                    backgroundColor: theme.palette.background.default,
                    borderRadius: 1,
                }}
            >
                <Box>
                    <TextField label="Test Case Name" variant="outlined" onChange={handleInputChange} value={inputValue} />
                    <Button variant="contained" color="primary" onClick={handleSubmit}>
                        Submit
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleVerifySwitch}>
                        {altVerify ? "VERIFY: Content" : "VERIFY: Availability"}
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleClearTable}>
                        Clear
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleStartSwitch}>
                        {start ? "STOP" : "START"}
                    </Button>
                    <Button variant="contained" color="primary" onClick={handleExport}>
                        EXPORT
                    </Button>
                    <Button variant="contained" component="label">
                        IMPORT
                        <input type="file" accept=".xlsxm, .xls, .xlsx" hidden onChange={handleFileUpload} />
                    </Button>
                    {renderToolbarButtons()}
                </Box>
            </Toolbar>
            <div className="ag-theme-quartz-dark" style={{ height: 500 }}>
                <AgGridReact
                    ref={gridRef}
                    rowData={rowData}
                    columnDefs={colDefs}
                    animateRows={true}
                    rowSelection="multiple"
                    pagination={false}
                    domLayout="normal"
                    onGridReady={() => {
                        scrollToBottom();
                    }}
                    onFirstDataRendered={() => {
                        scrollToBottom();
                    }}
                />
            </div>
        </>
    );
}

export default RecorderTable;