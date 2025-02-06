import { useRef, useMemo } from "react";
import envData from "@assets/environment_list.json";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import "ag-grid-community/styles/ag-grid.css";
import "ag-grid-community/styles/ag-theme-quartz.css";
import { EnvRowData } from "@interfaces/interfaces";
import { TextField } from "@mui/material";

function EnvNavContainer() {
    const columnDefs: ColDef[] = useMemo<ColDef[]>(() => {
        return [
            {
                field: "environment", headerName: "Environment", width: 150,
                filter: true, suppressHeaderFilterButton: true
            },
            {
                field: "ops_bps", headerName: "OPS BPS",
                cellRenderer: (params: any) => (
                    <a href={params.value} target="_blank">
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
            },
            { field: "db_name", headerName: "Database", width: 150 },
        ];
    }, []);

    const rowData: EnvRowData[] = useMemo<EnvRowData[]>(() => {
        return envData.map((env) => {
            return {
                environment: env.ENVIRONMENT,
                ops_bps: env.URL,
                aad: env.URL2,
                db_name: env.DB_NAME
            }
        });
    }, []);

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

    return (
        <>
            <TextField fullWidth size="small" id="outlined-basic" label="Search..." variant="outlined" onChange={handleFilterChange} />
            <div className="ag-theme-quartz" style={{ width: "100%", height: "87%"}} >
                <AgGridReact
                    ref={gridRef}
                    rowHeight={50}
                    rowData={rowData}
                    columnDefs={columnDefs}
                    enableCellTextSelection={true}
                    onGridReady={() => gridRef.current.api.sizeColumnsToFit()}
                />
            </div>
        </>
    );
}

export default EnvNavContainer;