import React, { useEffect, useRef, useState, memo, useMemo } from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import { Button, CircularProgress, TextField } from "@mui/material";

interface ErrorResponse {
    error: string;
}

function CredsContainer() {
    const [rowData, setRowData] = useState([]);
    const [loading, setLoading] = useState(true);
    const gridRef: any = useRef();

    useEffect(() => {
        const fetchRows = async () => {
            try {
                setLoading(true);
                const response = await fetch("api/environments/keychainAccounts", {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                });
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json() as ErrorResponse;
                    throw new Error(`${errorData.error}`);
                }
                const result = await response.json();
                setRowData(result);
            } catch (err) {
                console.error(err);
            }
        };

        fetchRows();
    }, []);

    const handleClick = async (email: string, setValue: (value: string) => void, setLoading: (value: boolean) => void, setError: (value: boolean) => void) => {
        try {
            setLoading(true);
            const response = await fetch(`api/environments/secretKey?email=${encodeURIComponent(email.trim())}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            setLoading(false);
            if (!response.ok) {
                setError(true);
                const errorData = await response.json() as ErrorResponse;
                throw new Error(`${errorData.error}`);
            }
            const result = await response.json();
            setError(false);
            setValue(result.message);
        } catch (err) {
            console.error(err);
        }
    }

    const SecretKeyCellRenderer = memo((params: any) => {
        const [value, setValue] = useState<string | null>(null);
        const [loading, setLoading] = useState<boolean>(false);
        const [error, setError] = useState<boolean>(false);

        return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
            {loading ? (
                <CircularProgress size={24} />
            ) : value ? (
                <span>{value}</span>
            ) : (
                <Button
                    variant="contained"
                    onClick={() => handleClick(params.data.email, setValue, setLoading, setError)}
                >
                    {error ? 'Retry' : 'Get'}
                </Button>
            )}
        </div>
    });

    const columnDefs: ColDef[] = useMemo<ColDef[]>(() => {
        return [
            { field: "email", headerName: "Email", width: 150, filter: true, suppressHeaderFilterButton: true },
            { field: "role", headerName: "Role", width: 150 },
            { field: "organization", headerName: "Organization", width: 150 },
            {
                field: "secretKey", headerName: "Get Secret Key", width: 150,
                cellRenderer: SecretKeyCellRenderer, cellStyle: { display: 'flex', justifyContent: 'center', alignItems: 'center' },
            }
        ];
    }, []);

    const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        gridRef.current?.api.setFilterModel({
            email: {
                filterType: 'text',
                type: 'contains',
                filter: event.target.value,
            }
        })
        gridRef.current?.api.onFilterChanged();
    };

    return (
        <>
            <TextField id="outlined-basic" label="Search for account" variant="outlined" onChange={handleFilterChange} />
            <div className="ag-theme-quartz" style={{ width: 650, height: 500, marginTop: 10 }}  >
                <AgGridReact loading={loading} rowData={rowData} columnDefs={columnDefs} rowHeight={50} ref={gridRef} enableCellTextSelection={true} ></AgGridReact>
            </div>
        </>
    );
}

export default CredsContainer;