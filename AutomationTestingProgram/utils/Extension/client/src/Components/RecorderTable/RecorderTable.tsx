import React, { useCallback, useEffect, useRef, useState} from "react";
import { AgGridReact } from "ag-grid-react";
import "ag-grid-community/styles/ag-grid.css"
import "ag-grid-community/styles/ag-theme-quartz.css";
import { ColDef } from "ag-grid-community";
import {Box, Button, TextField, Toolbar, useTheme} from "@mui/material";

interface TestStep {
    TESTCASENAME: string;
    TESTDESCRIPTION: string;
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
    const [fillerValue, setFillerValue] = useState("");
    const [testCase, setTestCase] = useState("");
    const [altVerify, setAltVerify] = useState(false);
    const [fillerText, setFillerText] = useState("");
    const [rowData, setRowData] = useState<TestStep[]>([]);
    const [colDefs] = useState<ColDef[]>([
        { field: "TESTCASENAME" },
        { field: "TESTDESCRIPTION" },
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
    const fillerTextRef = useRef(fillerText);
    const gridRef = useRef<AgGridReact>(null);

    useEffect(() => {
        testCaseRef.current = testCase;
        fillerTextRef.current = fillerText;
    }, [testCase, fillerText]);

    useEffect(() => {
        const handleMessage = (message: any) => {
            if (message.action === "RECORD_TEST_STEP") {
                // const elDict = JSON.parse(message.locator);
                const testStep: TestStep = {
                    TESTCASENAME: testCaseRef.current,
                    TESTDESCRIPTION: message.stepValues.testdescription,
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

    const handleFillerTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFillerValue(e.target.value);
    }

    const handleSubmitFillerText = () => {
        setFillerText(fillerValue);
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

    const sendFillerTextToContentScript = useCallback(() => {
        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            if (tabs[0].id) {
                chrome.tabs.sendMessage(
                    tabs[0].id,
                    { action: "FILL_TEXT_BOXES", fillerText: fillerTextRef.current },
                )
            }
        })
    }, [fillerText]);

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

    useEffect(() => {
        sendMessageToContentScript();
    }, [sendMessageToContentScript]);

    useEffect(() => {
        sendFillerTextToContentScript();
    }, [sendFillerTextToContentScript]);

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
                    <TextField label="Enter Filler Text..." variant="outlined" onChange={handleFillerTextChange} value={fillerValue} />
                    <Button variant="contained" color="primary" onClick={handleSubmitFillerText}>
                        Fill
                    </Button>
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