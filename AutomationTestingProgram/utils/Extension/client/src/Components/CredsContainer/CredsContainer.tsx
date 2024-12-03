import React, { useEffect, useRef, useState } from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import TextField from '@mui/material/TextField';
import { Button, CircularProgress } from "@mui/material";
import { HOST } from "../../constants";

interface ErrorResponse {
    error: string;
}

function CredsContainer(props: any) {
    const [rowData, setRowData] = useState([]);
    const [loading, setLoading] = useState(true);
    const [credFilter, setCredFilter] = useState("");

    const gridRef: any = useRef();

    const url = HOST;

    useEffect(() => {
        const fetchRows = async () => {
            try {
                setLoading(true);
                const response = await fetch(`${url}/api/environments/keychainAccounts`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                });
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json() as ErrorResponse;
                    console.log(errorData);
                    throw new Error(`${response.status}`);
                }
                const result = await response.json();
                setRowData(result);
            } catch (err) {
                console.log(err);
            }
        };

        fetchRows();
    }, []);

    useEffect(() => {
        chrome.storage.local.get("credFilter", (result) => {
            if (result.credFilter) {
                setCredFilter(result.credFilter);
            }
        });
    }, []);

    useEffect(() => {
        if (gridRef.current && gridRef.current.api) {
            gridRef.current.api.setFilterModel({
                email: {
                    filterType: 'text',
                    type: 'contains',
                    filter: credFilter,
                }
            })
            gridRef.current.api.onFilterChanged();
        }
    }, [credFilter]);

    const handleClick = async (email: string, setLoading: (value: boolean) => void, setError: (value: boolean) => void) => {
        try {
            setLoading(true);
            const response = await fetch(`${url}/api/environments/secretKey?email=${encodeURIComponent(email.trim())}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            setLoading(false);
            if (!response.ok) {
                setError(true);
                const errorData = await response.json() as ErrorResponse;
                console.log(errorData);
                throw new Error(`${errorData.error}`);
            }
            const result = await response.json();
            setError(false);
            props.setCurrentCreds({ username: email, password: result.secretKey });
        } catch (err) {
            console.log(err);
        }
    }

    const SecretKeyCellRenderer = (params: any) => {
        const [loading, setLoading] = useState<boolean>(false);
        const [error, setError] = useState<boolean>(false);

        return <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
            {loading ? (
                <CircularProgress size={24} />
            ) : (
                <Button
                    variant="contained"
                    onClick={() => handleClick(params.data.email, setLoading, setError)}
                >
                    {error ? 'Error' : 'Set'}
                </Button>
            )}
        </div>
    };

    const columnDefs: ColDef[] = [
        { field: "email", headerName: "Email", width: 150, filter: true, suppressHeaderFilterButton: true },
        {
            field: "secretKey", headerName: "Set Auto-Login Credentials", width: 150,
            cellRenderer: SecretKeyCellRenderer, cellStyle: { display: 'flex', justifyContent: 'center', alignItems: 'center' },
        }
    ];

    const handleFilterChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setCredFilter(event.target.value);
        chrome.storage.local.set({ credFilter: event.target.value });
    };

    return (
        <>
            <TextField id="outlined-basic" label="Search for account" variant="outlined" value={credFilter} onChange={handleFilterChange} />

            <div className="ag-theme-quartz" style={{ width: 301, height: 500, marginTop: 10 }}  >
                <AgGridReact loading={loading} rowData={rowData} columnDefs={columnDefs} rowHeight={50} ref={gridRef} enableCellTextSelection={true} ></AgGridReact>
            </div>
        </>
    );
}

export default CredsContainer;