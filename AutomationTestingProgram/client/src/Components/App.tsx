import Home from "../Pages/Home.tsx";
import Table from "./Table/Table.tsx";

function App() {
    const defaultColDef = {
        editable: true,
    }

    const rowData = [
        { testcasename: "Test", testdesc: "Test", stepnum: "Test", action: "Test", object: "Test", value: "Test", comments: "Test", release: "Test", attempts: "Test", timeout: "Test", control: "Test", collection: "Test", steptype: "Test", goto: "Test"},
    ];

    // Define the column definitions
    const columnDefs = [
        { field: "testcasename", headerName: "TESTCASENAME" },
        { field: "testdesc", headerName: "TESTDESCRIPTION" },
        { field: "stepnum", headerName: "STEPNUM" },
        { field: "action", headerName: "ACTIONONOBJECT"},
        { field: "object", headerName: "OBJECT" },
        { field: "value", headerName: "VALUE"},
        { field: "comments", headerName: "COMMENTS"},
        { field: "release", headerName: "RELEASE"},
        { field: "attempts", headerName: "LOCAL_ATTEMPTS"},
        { field: "timeout", headerName: "LOCAL_TIMEOUT"},
        { field: "control", headerName: "CONTROL"},
        { field: "collection", headerName: "COLLECTION"},
        { field: "steptype", headerName: "TESTSTEPTYPE" },
        { field: "goto", headerName: "GOTOSTEP"},
        {

        }
    ];

    return (
        <>
            <Home />
            <Table rowData={rowData} columnDefs={columnDefs} defaultColDef={defaultColDef} />
        </>
    );
}

export default App
