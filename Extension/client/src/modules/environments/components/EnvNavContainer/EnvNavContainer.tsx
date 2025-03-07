import { useState, useRef, useEffect, useMemo } from "react";
import data from "@assets/environment_list.json";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import "ag-grid-community/styles/ag-grid.css";
import "ag-grid-community/styles/ag-theme-quartz.css";
import { EnvRowData } from "@/interfaces/interfaces";
import { TextField } from "@mui/material";

function EnvNavContainer(props: any) {
    const [envFilter, setEnvFilter] = useState("");
    const gridRef: any = useRef();

    useEffect(() => {
        chrome.storage.local.get("envFilter", (result) => {
            if (result.envFilter) {
                setEnvFilter(result.envFilter);
            }
        });
    }, []);

    useEffect(() => {
        if (gridRef.current && gridRef.current.api) {
            gridRef.current.api.setFilterModel({
                environment: {
                    filterType: 'text',
                    type: 'contains',
                    filter: envFilter,
                }
            })
            gridRef.current.api.onFilterChanged();
        }
    }, [envFilter]);

    const columnDefs: ColDef[] = [
        {
            field: "environment", headerName: "Environment", width: 150,
            filter: true, suppressHeaderFilterButton: true
        },
        {
            field: "ops_bps", headerName: "OPS BPS",
            cellRenderer: (params: any) => (
                <a href={params.value} onClick={(e) => props.handleUrlClick(e, params.value)}>
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
    
    const rowData: EnvRowData[] = useMemo<EnvRowData[]>(() => {
        return (data.map((env) => {
            return {
                environment: env.ENVIRONMENT,
                ops_bps: env.URL,
                aad: env.URL2,
            }
        }));
    }, []);

    const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setEnvFilter(event.target.value);
        chrome.storage.local.set({ envFilter: event.target.value });
    };

    return (
        <>
            <TextField id="outlined-basic" label="Search for environment" variant="outlined" value={envFilter} onChange={handleFilterChange} />

            <div className="ag-theme-quartz" style={{ width: 301, height: 500, marginTop: 10 }} >
                <AgGridReact ref={gridRef} rowHeight={50} rowData={rowData} columnDefs={columnDefs} enableCellTextSelection={true} ></AgGridReact>
            </div>
        </>
    );
}

export default EnvNavContainer;