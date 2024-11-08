import React from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import "ag-grid-community/styles/ag-grid.css";
import "ag-grid-community/styles/ag-theme-quartz.css";

interface RowData {
    testcasename: string;
    testdesc: string;
    stepnum: string;
    action: string;
    object: string;
    value: string;
    comments: string;
    release: string;
    attempts: string;
    timeout: string;
    control: string;
    collection: string;
    steptype: string;
    goto: string;
}

interface TableProps {
    rowData: RowData[];
    columnDefs: ColDef[];
    defaultColDef?: ColDef;
}

const Table: React.FC<TableProps> = ({ rowData, columnDefs, defaultColDef }) => {
    return (
        <div className="ag-theme-quartz" style={{ width: "100%", height: "100%" }} >
            <AgGridReact rowData={rowData} columnDefs={columnDefs} defaultColDef={defaultColDef} domLayout={"autoHeight"} />
        </div>
    )
}

export default Table;