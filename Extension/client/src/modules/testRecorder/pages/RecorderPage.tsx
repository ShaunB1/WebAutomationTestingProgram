import React, {useCallback, useEffect, useRef, useState} from "react";
import { AgGridReact } from "ag-grid-react";
import "ag-grid-community/styles/ag-grid.css"
import "ag-grid-community/styles/ag-theme-quartz.css";
import { ColDef } from "ag-grid-community";
import {
    Box,
    Button,
    MenuItem,
    Select,
    SelectChangeEvent,
    TextField,
    Toolbar,
    useTheme
} from "@mui/material";
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

interface TestCaseTable {
    testCaseName: string;
    rowData: TestStep[];
}

interface OrderedTable {
    TESTCASE: string;
    STEPS: number;
}

interface ElementDetails {
    ATTRIBUTE: string;
    VALUE: any;
}

const TEST_CASES_KEY = "savedTableData";
const SORTED_CASES_KEY = "sortedTableData";
const SAVED_STEPS_KEY = "savedStepsData"
const START_KEY = "startState";
const SELECTED_CASE_KEY = "selectedState";

const RecorderPage: React.FC = () => {
    const theme = useTheme();
    const [inputValue, setInputValue] = useState("");
    const [testCase, setTestCase] = useState("");
    const [altVerify, setAltVerify] = useState(false);
    const [start, setStart] = useState(false);
    const [savedRowData, setSavedRowData] = useState<TestStep[]>([]);
    const [testCases, setTestCases] = useState<TestCaseTable[]>([]);
    const [sortedTestCases, setSortedTestCases] = useState<OrderedTable[]>([])
    const [selectedTestCase, setSelectedTestCase] = useState<string | null>(null);
    const [selectedAction, setSelectedAction] = useState<string | null>(null);
    const [attributes, setAttributes] = useState<ElementDetails[]>([])
    const [attributeColDefs] = useState<ColDef[]>([
        { field: "ATTRIBUTE" },
        { field: "VALUE" },
    ])
    const [testCaseColDefs] = useState<ColDef[]>([
        {
            field: "TESTCASE",
            rowDrag: true,
            checkboxSelection: true,
            headerCheckboxSelection: true,
        },
        { field: "STEPS" }
    ]);
    const [colDefs, setColDefs] = useState<ColDef[]>([
        {
            field: "TESTCASENAME",
            checkboxSelection: true,
            headerCheckboxSelection: true,
            rowDrag: true,
            filter: true,
        },
        { field: "TESTSTEPDESCRIPTION", editable: true, filter: true },
        { field: "ACTIONONOBJECT", editable: true, filter: true },
        { field: "OBJECT", editable: true, filter: true },
        { field: "VALUE", editable: true, filter: true },
        { field: "COMMENTS", editable: true, filter: true },
        { field: "LOCAL_ATTEMPTS", editable: true, filter: true },
        { field: "LOCAL_TIMEOUT", editable: true, filter: true },
        { field: "CONTROL", editable: true, filter: true },
        { field: "TESTSTEPTYPE", editable: true, filter: true },
        { field: "GOTOSTEP", editable: true, filter: true },
        { field: "CYCLEGROUP", editable: true, filter: true },
    ])
    // const [tabIndex, setTabIndex] = useState(0);

    const testCaseRef = useRef(testCase);
    const recordingRef = useRef<AgGridReact>(null);

    useEffect(() => {
        chrome.storage.local.get([TEST_CASES_KEY, SORTED_CASES_KEY, SAVED_STEPS_KEY, SELECTED_CASE_KEY], (result) => {
            if (result[TEST_CASES_KEY]) { setTestCases(result[TEST_CASES_KEY]) }
            if (result[SORTED_CASES_KEY]) { setSortedTestCases(result[SORTED_CASES_KEY]) }
            if (result[SAVED_STEPS_KEY]) { setSavedRowData(result[SAVED_STEPS_KEY]) }
            if (result[SELECTED_CASE_KEY]) { setSelectedTestCase(result[SELECTED_CASE_KEY]) }
        });
    }, []);

    useEffect(() => {
        chrome.storage.local.set({
            [TEST_CASES_KEY]: testCases,
            [SORTED_CASES_KEY]: sortedTestCases,
            [SAVED_STEPS_KEY]: savedRowData,
            [SELECTED_CASE_KEY]: selectedTestCase,
        });
    }, [testCases, sortedTestCases, savedRowData, selectedTestCase]);

    const handleDeleteSelectedRows = () => {
        const selectedRecordedSteps = recordingRef.current?.api.getSelectedRows();
        const selectedSavedSteps = savedRowsRef.current?.api.getSelectedRows();

        if (selectedRecordedSteps && selectedRecordedSteps.length > 0) {
            setTestCases(prevTestCases =>
                prevTestCases.map(tc => tc.testCaseName === selectedTestCase
                    ? {...tc, rowData: tc.rowData.filter(row => !selectedRecordedSteps.includes(row))}
                    : tc
            ));
        }
        if (selectedSavedSteps && selectedSavedSteps.length > 0) {
            setSavedRowData(prevTestSteps => prevTestSteps.filter(row => !selectedSavedSteps?.includes(row)));
        }
    }

    useEffect(() => {
        testCaseRef.current = testCase;
    }, [testCase]);

    useEffect(() => {
        const handleMessage = (message: any) => {
            if (message.action === "RECORD_TEST_STEP") {
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

                setTestCases(prevTestCases =>
                    prevTestCases.map(tc => tc.testCaseName === selectedTestCase
                        ? {...tc, rowData: [...tc.rowData, testStep]}
                        : tc
                    )
                );
                setSortedTestCases((prevTestCases: any) => prevTestCases.map(
                    (tc: any) => tc.TESTCASE === selectedTestCase
                        ? {...tc, STEPS: parseInt(tc.STEPS, 10)+1}
                        : tc
                ))
            }
            if (message.action === "SEND_ELEMENT_DETAILS") {
                const attributes = message.attributes;
                setAttributes(attributes);
            }
        }

        chrome.runtime.onMessage.addListener(handleMessage);

        return () => {
            chrome.runtime.onMessage.removeListener(handleMessage);
        };
    }, [selectedTestCase]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setInputValue(e.target.value);
    }

    const handleCreateTestCase = () => {
        const existingTestCase = testCases.find(tc => tc.testCaseName === inputValue.trim());

        if (existingTestCase) {
            alert("Test case with this name already exists.");
            return;
        }

        const newTestCaseTable: TestCaseTable = {
            testCaseName: inputValue.trim(),
            rowData: [],
        }

        const newTestCase: OrderedTable = {
            TESTCASE: inputValue.trim(),
            STEPS: 0,
        }

        setTestCase(inputValue.trim())
        setTestCases(prevTestCases => [...prevTestCases, newTestCaseTable]);
        setSortedTestCases(prevTestCases => [...prevTestCases, newTestCase]);
        setSelectedTestCase(inputValue.trim());
        setInputValue("");
    }

    const handleTestCaseChange = (event: SelectChangeEvent) => {
        const testCase = event.target.value as string;
        setSelectedTestCase(testCase);
        setTestCase(testCase)
    }

    const currentTestCaseData = selectedTestCase ? testCases.find(tc => tc.testCaseName === selectedTestCase)?.rowData : [];

    const handleVerifySwitch = () => {
        setAltVerify(!altVerify);
    }

    const scrollToBottom = () => {
        const gridApi = recordingRef.current?.api;
        if (gridApi) {
            const rowCount = gridApi.getDisplayedRowCount();
            gridApi.ensureIndexVisible(rowCount - 1, "bottom");
        }
    }

    useEffect(() => {
        scrollToBottom();
    }, [testCases]);

    const onRecordingGridReady = (params: any) => {
        // @ts-expect-error / works
        recordingRef.current = params;
        params.api.autoSizeAllColumns();
    }

    const onSortedTestCasesGridReady = (params: any) => {
        // @ts-expect-error / works
        sortedTestCasesRef.current = params;
        params.api.autoSizeAllColumns();
    }

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
        chrome.storage.local.set({ [START_KEY]: start });
    }, [start]);

    const handleStartSwitch = () => {
        if (selectedTestCase && selectedTestCase !== "") {
            console.log(selectedTestCase);
            setStart(!start);
        } else {
            alert("Please select a test case.");
        }
    }

    useEffect(() => {
        const port = chrome.runtime.connect({ name: "sidepanel" })
        return () => {
            port.disconnect();
        };
    }, []);

    const handleExport = () => {
        try {
            const allTestSteps: TestStep[] = [];
            const testCaseMap = new Map(
                testCases.map((testCase) => [testCase.testCaseName, testCase.rowData])
            )

            sortedTestCases.forEach((testCaseOrdered) => {
                const matchingRowData = testCaseMap.get(testCaseOrdered.TESTCASE);
                if (matchingRowData) {
                    allTestSteps.push(...matchingRowData.map((testStep) => ({ ...testStep })));
                }
            });

            const worksheet = XLSX.utils.json_to_sheet(allTestSteps);
            const workbook = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1")
            XLSX.writeFile(workbook, "recorded_test.xlsx");
        } catch (e) {
            console.log(e);
        }
    }

    const handleImport = (e: React.ChangeEvent<HTMLInputElement>) => {
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

                        const mappedData: TestStep[] = jsonData.map((row) => ({
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

                        const caseSteps: number[] = [];

                        setTestCases(prevTestCases => {
                            const updatedTestCases = [...prevTestCases];

                            mappedData.forEach(testStep => {
                                const testCaseName = testStep.TESTCASENAME.trim();
                                const existingTestCase = updatedTestCases.find(tc => tc.testCaseName === testCaseName);

                                if (existingTestCase) {
                                    if (!existingTestCase.rowData.some(row => JSON.stringify(row) === JSON.stringify(testStep))) {
                                        existingTestCase.rowData.push(testStep);
                                    }
                                } else {
                                    updatedTestCases.push({
                                        testCaseName: testCaseName,
                                        rowData: [testStep],
                                    });
                                }
                            });

                            updatedTestCases.forEach(testCase => {
                                caseSteps.push(testCase.rowData.length);
                            });

                            console.log(caseSteps);

                            return updatedTestCases;
                        });

                        setSortedTestCases(prevSortedTestCases => {
                            const newSortedCases = [...prevSortedTestCases];

                            mappedData.forEach(testStep => {
                                const testCaseName = testStep.TESTCASENAME.trim();
                                if (!newSortedCases.some(tc => tc.TESTCASE === testCaseName)) {
                                    newSortedCases.push({
                                        TESTCASE: testCaseName,
                                        STEPS: 0,
                                    });
                                }
                            });
                            return newSortedCases;
                        });



                        setSortedTestCases((prevSortedTestCases: any) => prevSortedTestCases.map((tc: any, index: number) => ({
                            ...tc,
                            STEPS: caseSteps[index],
                        })));

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
                                    rowDrag: true,
                                };
                            } else {
                                return {
                                    headerName: key,
                                    field: key,
                                    sortable: true,
                                    filter: true,
                                    resizable: true,
                                    editable: true,
                                }
                            }
                        });

                        setColDefs(cols);
                    }
                }

                reader.onerror = (error) => {
                    console.error(`Error reading file: ${error}`);}

                reader.readAsArrayBuffer(file);

                e.target.value = "";
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

    const onSelectionChanged = () => {
        const selectedRows = recordingRef.current?.api.getSelectedRows();
        const length = selectedRows ? selectedRows.length : -1;
        if (length > 0) {
            chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                if (tabs[0].id) {
                    chrome.tabs.sendMessage(
                        tabs[0].id,
                        {
                            action: "ROW_SELECTED",
                            selectedRow: selectedRows ? selectedRows[0] : null,
                        }
                    )
                }
            });
        } else {
            chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                if (tabs[0].id) {
                    chrome.tabs.sendMessage(
                        tabs[0].id,
                        {
                            action: "ROW_UNSELECTED",
                            selectedRow: selectedRows ? selectedRows[0] : null,
                        }
                    )
                }
            });
        }
    }

    const attributesRef = useRef<AgGridReact>(null);

    const navigateToNextCell = (params: any) => {
        const previousCell = params.previousCellPosition;
        const suggestedNextCell = params.nextCellPosition;
        const gridApi = recordingRef.current?.api;

        if (previousCell && suggestedNextCell) {
            if (params.key === "ArrowDown" || params.key === "ArrowUp") {
                gridApi?.deselectAll();

                const rowNode = gridApi?.getDisplayedRowAtIndex(suggestedNextCell.rowIndex);

                if (rowNode) {
                    rowNode.setSelected(true);
                }
            }
        }

        return suggestedNextCell;
    }

    const handleExecuteStep = (event: KeyboardEvent) => {
        const key = event.key;
        if (key === "Enter") {
            const selectedRows = recordingRef.current?.api.getSelectedRows();

            if (selectedRows && selectedRows.length > 0) {
                chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                    if (tabs[0].id) {
                        chrome.tabs.sendMessage(
                            tabs[0].id,
                            {
                                action: "EXECUTE_TEST_STEP", stepData: selectedRows ? selectedRows[0] : null
                            },
                        )
                    }
                })
            }
        }
    }

    useEffect(() => {
        document.addEventListener("keydown", handleExecuteStep);

        return () => {
            document.removeEventListener("keydown", handleExecuteStep);
        }
    }, [handleExecuteStep]);

    const [desc, setDesc] = useState("");
    const [action, setAction] = useState("");
    const [object, setObject] = useState("");
    const [value, setValue] = useState("");
    const [comment, setComment] = useState("");

    const handleDescChange = (event: any) => {
        setDesc(event.target.value);
    }

    const handleObjectChange = (event: any) => {
        setObject(event.target.value);
    }

    const handleValueChange = (event: any) => {
        setValue(event.target.value);
    }

    const handleCommentChange = (event: any) => {
        setComment(event.target.value);
    }

    const handleAddStep = () => {
        const newStep: TestStep = {
            TESTCASENAME: testCaseRef.current,
            TESTSTEPDESCRIPTION: desc,
            ACTIONONOBJECT: action,
            OBJECT: object,
            VALUE: value,
            COMMENTS: comment,
            LOCAL_ATTEMPTS: "",
            LOCAL_TIMEOUT: "",
            CONTROL: "",
            TESTSTEPTYPE: "",
            GOTOSTEP: "",
            CYCLEGROUP: ""
        }

        setTestCases(prevTestCases =>
            prevTestCases.map(tc => tc.testCaseName === selectedTestCase
                ? {...tc, rowData: [...tc.rowData, newStep]}
                : tc
            )
        );
        setSortedTestCases(prevTestCases =>
            prevTestCases.map(tc => tc.TESTCASE === selectedTestCase
                ? {...tc, STEPS: tc.STEPS+1}
                : tc
            )
        );
    }

    const handleCopyRows = () => {
        const selectedRows = recordingRef.current?.api.getSelectedRows();
        if (selectedRows && selectedRows.length > 0) {
            setSavedRowData(prevTestSteps => [...prevTestSteps, ...selectedRows]);
            setTestCases(prevTestCases => prevTestCases.map(
                tc => tc.testCaseName === selectedTestCase
                    ? {...tc, rowData: tc.rowData.filter(tc => !selectedRows.includes(tc))}
                    : tc
            ));
        }
    }

    const savedRowsRef = useRef<AgGridReact>(null);

    const handlePasteRows = () => {
        const selectedRows = savedRowsRef.current?.api.getSelectedRows();
        if (selectedRows && selectedRows.length > 0) {
            const updatedRows: any = selectedRows.map(row => ({...row, TESTCASENAME: selectedTestCase}))
            setTestCases(prevTestCases => prevTestCases.map(
                tc => tc.testCaseName === selectedTestCase
                    ? {...tc, rowData: [...tc.rowData, ...updatedRows]}
                    : tc
            ));
        }
    };

    const handleDeleteTestCase = () => {
        const selectedTestCases = sortedTestCasesRef.current?.api.getSelectedRows();
        if (selectedTestCases  && selectedTestCases.length > 0) {
            setTestCases(prevTestCases => prevTestCases.filter(tc => !selectedTestCases.some(selected => selected.TESTCASE === tc.testCaseName)));
            setSortedTestCases(prevTestCases => prevTestCases.filter(tc => !selectedTestCases.includes(tc)));
            setSelectedTestCase(null);
        }
    }

    const sortedTestCasesRef = useRef<AgGridReact>(null);

    const onRowDragEnd = () => {
        const newRowOrder: any = [];

        sortedTestCasesRef.current?.api.forEachNode((node: any) => {
            newRowOrder.push(node.data);
        })

        setSortedTestCases(newRowOrder);
    }

    const testStepDrag = () => {
        const newStepOrder: TestStep[] = [];

        recordingRef.current?.api.forEachNode((node: any) => {
            newStepOrder.push(node.data);
        });

        setTestCases(prevTestCases => prevTestCases.map(
            tc => tc.testCaseName === selectedTestCase
                ? {...tc, rowData: newStepOrder}
                : tc
        ));
    }

    const handleSelectActionChange = (event: SelectChangeEvent) => {
        const action = event.target.value as string;
        setSelectedAction(action);
        setAction(action);
    }

    const actions = [
        "ClickWebElement",
        "PopulateWebElement",
        "Login",
        "WaitInSeconds",
        "SelectDDL",
        "VerifyWebElementAvailability",
        "VerifyWebElementContent",
        "UploadFile",
    ]
    const actionsWithObject = [
        "ClickWebElement",
        "PopulateWebElement",
        "Login",
        "SelectDDL",
        "VerifyWebElementAvailability",
        "VerifyWebElementContent",
        "UploadFile",
    ];
    const actionsWithValue = [
        "PopulateWebElement",
        "Login",
        "WaitInSeconds",
        "SelectDDL",
        "VerifyWebElementAvailability",
        "VerifyWebElementContent",
        "UploadFile",
    ];

    const resetRef = useRef<HTMLButtonElement>(null);
    const [confirmation, setConfirmation] = useState(false);

    const handleReset = () => {
        if (confirmation) {
            setTestCases([]);
            setSelectedTestCase("");
            setSavedRowData([]);
            setSortedTestCases([]);
            setConfirmation(false);
        } else {
            setConfirmation(true);
        }
    }

    useEffect(() => {
        const handleCancel = (event: MouseEvent) => {
            if (resetRef.current && !resetRef.current.contains(event.target as Node)) {
                setConfirmation(false);
            }
        };

        document.addEventListener("click", handleCancel, { capture: true });

        return () => {
            document.addEventListener("click", handleCancel, { capture: true });
        };
    }, []);

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
                <Box sx={{ width: "100%" }}>
                    <Box
                        sx={{
                            display: "flex",
                            alignItems: "center",
                            width: "100%",
                            mb: 2,
                            gap: 2,
                        }}
                    >
                        <Box
                            sx={{
                                display: "flex",
                                gap: 1,
                                flexGrow: 1,
                                alignItems: "center",
                            }}
                        >
                            <Select
                                value={selectedTestCase || ""}
                                onChange={handleTestCaseChange}
                                displayEmpty
                                variant="outlined"
                                size="small"
                                sx={{
                                    flexShrink: 0,
                                    minWidth: "150px",
                                    maxWidth: "75%",
                                }}
                            >
                                <MenuItem value="" disabled>
                                    Select Test Case
                                </MenuItem>
                                {testCases.map((testCase) => (
                                    <MenuItem key={testCase.testCaseName} value={testCase.testCaseName}>
                                        {testCase.testCaseName}
                                    </MenuItem>
                                ))}
                            </Select>

                            <TextField
                                size="small"
                                label="Test Case Name"
                                variant="outlined"
                                value={inputValue}
                                onChange={handleInputChange}
                                sx={{
                                    flexGrow: 1,
                                    minWidth: "100px",
                                }}
                            />
                        </Box>
                        <Box
                            sx={{
                                display: "flex",
                                gap: 1,
                                justifyContent: "flex-end",
                                flexShrink: 0,
                            }}
                        >
                            <Button variant="contained" color="primary" onClick={handleCreateTestCase}>
                                Create
                            </Button>
                            <Button variant="contained" color="primary" onClick={handleDeleteTestCase}>
                                Delete TestCase
                            </Button>
                            <Button variant="contained" color="error" onClick={handleReset} sx={{ width: "87px" }}>
                                {confirmation ? "Confirm" : "Reset"}
                            </Button>
                        </Box>
                    </Box>

                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                        <Box display="flex" gap={1}>
                            <Button variant="contained" color="primary" onClick={handleStartSwitch} sx={{ width: 4 }}>
                                {start ? "STOP" : "START"}
                            </Button>
                            <Button variant="contained" color="primary" onClick={handleVerifySwitch} sx={{ width: 195 }}>
                                {altVerify ? "VERIFY: Content" : "VERIFY: Availability"}
                            </Button>
                            <Button variant="contained" color="primary" onClick={handleCopyRows}>
                                Copy Rows
                            </Button>
                            <Button variant="contained" color="primary" onClick={handlePasteRows}>
                                Paste Rows
                            </Button>
                            <Button variant="contained" color="primary" onClick={handleDeleteSelectedRows}>
                                Delete Selected
                            </Button>
                        </Box>
                        <Box display="flex" gap={1}>
                            <Button variant="contained" component="label">
                                Import
                                <input type="file" accept=".xlsxm, .xls, .xlsx" hidden onChange={handleImport}/>
                            </Button>
                            <Button variant="contained" color="primary" onClick={handleExport}>
                                Export
                            </Button>
                        </Box>
                    </Box>
                </Box>
            </Toolbar>
            <Box sx={{ height: 575, display: "flex", flexDirection: "column" }}>
                <Box display="flex" className="ag-theme-quartz" sx={{ width: "100%", height: "50%" }}>
                    <Box className="ag-theme-quartz" sx={{ width: "75%", height: "100%" }}>
                        <AgGridReact
                            ref={recordingRef}
                            rowData={currentTestCaseData}
                            columnDefs={colDefs}
                            animateRows={true}
                            rowSelection="multiple"
                            pagination={false}
                            domLayout="normal"
                            navigateToNextCell={navigateToNextCell}
                            onSelectionChanged={onSelectionChanged}
                            rowDragManaged={true}
                            onRowDragEnd={testStepDrag}
                            onGridReady={onRecordingGridReady}
                        />
                    </Box>
                    <Box className="ag-theme-quartz" sx={{ width: "25%", height: "100%" }}>
                        <AgGridReact
                            ref={sortedTestCasesRef}
                            rowData={sortedTestCases}
                            columnDefs={testCaseColDefs}
                            animateRows={true}
                            rowSelection="multiple"
                            pagination={false}
                            domLayout="normal"
                            rowDragManaged={true}
                            onRowDragEnd={onRowDragEnd}
                            onGridReady={onSortedTestCasesGridReady}
                        />
                    </Box>
                </Box>
                <Box sx={{ height: "50%", display: "flex" }}>
                    <Box className="ag-theme-quartz" sx={{ width: "50%", height: "100%" }}>
                        <AgGridReact
                            columnDefs={attributeColDefs}
                            rowData={attributes}
                            ref={attributesRef}
                            domLayout="normal"
                            pagination={false}
                        />
                    </Box>
                    <Box className="ag-theme-quartz" sx={{ width: "50%", height: "100%" }}>
                        <AgGridReact
                            ref={savedRowsRef}
                            rowData={savedRowData}
                            columnDefs={colDefs}
                            animateRows={true}
                            rowSelection="multiple"
                            pagination={false}
                            domLayout="normal"
                        />
                    </Box>
                </Box>
            </Box>
            <Box sx={{ display: "flex", justifyContent: "space-between", width: "100%", mt: 2 }}>
                <Box sx={{ width: "100%" }}>
                    <Select
                        variant="outlined"
                        value={selectedAction || ""}
                        onChange={handleSelectActionChange}
                        displayEmpty
                        sx={{ width: "50%" }}
                        size="small"
                    >
                        <MenuItem value="" disabled>Select Action</MenuItem>
                        {actions.map(action => (
                            <MenuItem key={action} value={action}>
                                {action}
                            </MenuItem>
                        ))}
                    </Select>
                    <Button variant="contained" color="primary" onClick={handleAddStep} sx={{ ml: 2 }}>
                        Add Step
                    </Button>
                </Box>
                <Box gap={1} sx={{ width: "100%", display: "flex", justifyContent: "flex-end" }}>
                    {actionsWithObject.includes(selectedAction as string)
                        ? <TextField label="Object" variant="outlined" onChange={handleObjectChange}/>
                        : null
                    }
                    {actionsWithValue.includes(selectedAction as string)
                        ? <TextField label="Value" variant="outlined" onChange={handleValueChange}/>
                        : null
                    }
                    <TextField size="small" label="Description" variant="outlined" onChange={handleDescChange}/>
                    <TextField size="small" label="Comment" variant="outlined" onChange={handleCommentChange}/>
                </Box>
            </Box>
        </>
    );
}

export default RecorderPage;