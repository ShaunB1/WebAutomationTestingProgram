import React, { useRef } from "react";
import Table from "../Components/RecorderTable/Table";
import ToolBar from "../Components/ToolBar/ToolBar";
import * as XLSX from "xlsx";
import { ActionDetails } from "../interfaces";
import "./RecorderPage.css";

const headerMapping = [
    { header: "TESTCASENAME", key: "caseName" },
    { header: "TESTDESCRIPTION", key: "desc" },
    { header: "STEPNUM", key: "stepNum" },
    { header: "ACTIONONOBJECT", key: "action" },
    { header: "OBJECT", key: "object" },
    { header: "VALUE", key: "value" },
    { header: "COMMENTS", key: "comments" },
    { header: "RELEASE", key: "release" },
    { header: "LOCAL_ATTEMPTS", key: "attempts" },
    { header: "LOCAL_TIMEOUT", key: "timeout" },
    { header: "CONTROL", key: "control" },
    { header: "COLLECTION", key: "collection" },
    { header: "TESTSTEPTYPE", key: "stepType" },
    { header: "GOTOSTEP", key: "goto" },
]

function RecorderPage() {
    const [rows, setRows] = React.useState<ActionDetails[]>([]);
    const [testDescription, setTestDescription] = React.useState("");
    const [actionOnObject, setActionOnObject] = React.useState("");
    const [object, setObject] = React.useState("");
    const [value, setValue] = React.useState("");
    const [constants, setConstants] = React.useState({
        caseName: "",
        release: "",
        collection: "",
        stepType: "",
    });

    const handleClearTable = () => {
        // window.electronAPI.clearTable();
        setRows([]);
    }
    const handleExport = () => {
        const formattedData = rows.map((row) => {
            const formattedRow: { [key: string]: string } = {};
            headerMapping.forEach(({ header, key }) => {
                formattedRow[header] = row[key];
            });
            return formattedRow;
        });
        const worksheet = XLSX.utils.json_to_sheet(formattedData, {
            header: headerMapping.map((h) => h.header),
        });
        worksheet["!ref"] = XLSX.utils.encode_range({
            s: { c: 0, r: 0 },
            e: { c: headerMapping.length - 1, r: rows.length }
        });
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Sheet1");
        const wbout = XLSX.write(workbook, { bookType: "xlsx", type: "base64" });
        // window.electronAPI.saveFile(wbout);
    }

    const handleImport = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = (event.target.files as FileList)[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            const data = new Uint8Array((e.target as FileReader).result as ArrayBuffer);
            // @ts-ignore
            const workbook = XLSX.read(data, { type: "array" });
            const sheetName = workbook.SheetNames[0];
            const sheet = workbook.Sheets[sheetName];
            // @ts-ignore
            const excelRows: any[][] = XLSX.utils.sheet_to_json(sheet, { header: 1 });
            const newRows: ActionDetails[] = [];

            for (let i = 1; i < excelRows.length; i++) {
                const row = excelRows[i];
                const actionDetails: ActionDetails = {
                    caseName: "",
                    desc: "",
                    stepNum: 1,
                    action: "",
                    object: "",
                    value: "",
                    comments: "",
                    release: "",
                    attempts: "",
                    timeout: "",
                    control: "",
                    collection: "",
                    stepType: "",
                    goto: "",
                    uniqueLocator: false
                };
                const keys = Object.keys(actionDetails);
                for (let j = 0; j < row.length; j++) {
                    actionDetails[keys[j]] = row[j];
                }
                newRows.push(actionDetails);
            }

            setRows(prevRows => [...prevRows, ...newRows]);
            (event.target as any).value = "";
        }

        reader.readAsArrayBuffer(file);
    }

    const handleSave = () => {
        localStorage.setItem("tableData", JSON.stringify(rows));
    }

    const handleLoad = () => {
        const savedData = localStorage.getItem("tableData");
        if (savedData) {
            const savedRows = JSON.parse(savedData);
            setRows(prevRows => [...prevRows, ...savedRows]);
        }
    }

    const handleNewRow = () => {
        const actionDetails: ActionDetails = {
            caseName: constants.caseName,
            desc: testDescription,
            stepNum: 1,
            action: actionOnObject,
            object: object,
            value: value,
            comments: "",
            release: constants.release,
            attempts: "",
            timeout: "",
            control: "",
            collection: constants.collection,
            stepType: constants.stepType,
            goto: "",
            uniqueLocator: false
        };
        setRows([...rows, actionDetails])
    }

    return (
        <>
            <div className="header">
                <h1>TAP Test Recorder</h1>
                <div className="import-container">
                    <label htmlFor="importFile">Import Test File: </label>
                    <input type="file" id="importFile" name="import" onChange={handleImport} />
                </div>
            </div>
            <ToolBar constants={constants} setConstants={setConstants} onClearTable={handleClearTable} onExport={handleExport} onSave={handleSave} onLoad={handleLoad} />
            <Table rows={rows} setRows={setRows} />
            <div id="newrow-container">
                <input type="text" name="newRowDesc" placeholder="TESTDESCRIPTION" onChange={(e) => setTestDescription(e.target.value)} />
                <input type="text" name="newRowAction" placeholder="ACTIONONOBJECT" onChange={(e) => setActionOnObject(e.target.value)} />
                <input type="text" name="newRowObject" placeholder="OBJECT" onChange={(e) => setObject(e.target.value)} />
                <input type="text" name="newRowValue" placeholder="VALUE" onChange={(e) => setValue(e.target.value)} />
                <button id="addRow" onClick={handleNewRow}>ADD NEW ROW</button>
            </div>
        </>
    );
}

export default RecorderPage;