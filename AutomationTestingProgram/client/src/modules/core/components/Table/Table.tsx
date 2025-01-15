import React from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import "ag-grid-community/styles/ag-grid.css";
import "ag-grid-community/styles/ag-theme-quartz.css";
import { RowData } from "@interfaces/interfaces";

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