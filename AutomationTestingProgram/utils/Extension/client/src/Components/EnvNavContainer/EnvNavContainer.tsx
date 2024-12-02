import { useRef } from "react";
import data from "./environment_list.json";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import "ag-grid-community/styles/ag-grid.css";
import "ag-grid-community/styles/ag-theme-quartz.css";
import { EnvRowData } from "../../interfaces";
import { TextField } from "@mui/material";

function EnvNavContainer(props: any) {
    const columnDefs: ColDef[] = [
        {
            field: "environment", headerName: "Environment", width: 150,
            filter: true, suppressHeaderFilterButton: true
        },
        {
            field: "ops_bps", headerName: "OPS BPS",
            cellRenderer: (params: any) => (
                <a href={params.value} onClick={(e) => handleUrlClick(e, params.value)}>
                    {params.value}
                </a>
            ),
            width: 150,
        },
        {
            field: "aad", headerName: "AAD",
            cellRenderer: (params: any) => (
                <a href={params.value} target="_blank">
                    {params.value}
                </a>
            ),
            width: 150
        }
    ];

    const rowData: EnvRowData[] = data.map((env) => {
        return {
            environment: env.ENVIRONMENT,
            ops_bps: env.URL,
            aad: env.URL2,
        }
    });

    const gridRef: any = useRef();

    const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        gridRef.current?.api.setFilterModel({
            environment: {
                filterType: 'text',
                type: 'contains',
                filter: event.target.value,
            }
        })
        gridRef.current?.api.onFilterChanged();
    };

    const handleUrlClick = (e: any, url: string) => {
        e.preventDefault();
        chrome.runtime.sendMessage(
            {
                action: "openTabAndLogin",
                url: url,
                username: props.currentCreds.username,
                password: props.currentCreds.password
            }
        )
    };

    return (
        <>
            <TextField id="outlined-basic" label="Search for environment" variant="outlined" onChange={handleFilterChange} />

            <div className="ag-theme-quartz" style={{ width: 301, height: 500, marginTop: 10 }} >
                <AgGridReact ref={gridRef} rowHeight={50} rowData={rowData} columnDefs={columnDefs} enableCellTextSelection={true} ></AgGridReact>
            </div>
        </>
    );
}

export default EnvNavContainer;