import React, { useEffect, useRef, useState, memo, useMemo } from "react";
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from "ag-grid-community";
import {Box, Button, CircularProgress, TextField, Typography} from "@mui/material";
import { getToken } from "@auth/authConfig";
import { useMsal } from "@azure/msal-react";
import CheckIcon from "@mui/icons-material/Check";
import {CancelRounded, CheckCircleRounded} from "@mui/icons-material";

interface ErrorResponse {
    error: string;
}

function CredsContainer() {
    const [rowData, setRowData] = useState([]);
    const [resetEmail, setResetEmail] = useState("");
    const [loading, setLoading] = useState(false);
    const [spinner, setSpinner] = useState(false);
    const [success, setSuccess] = useState(false);
    const [failure, setFailure] = useState(false);
    const [fileLines, setFileLines] = useState<string[]>([]);
    const [fileResults, setFileResults] = useState<string[]>([]);
    const [result, setResult] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const gridRef: any = useRef(null);

    const [fileName, setFileName] = useState<string | null>(null);
    const { instance, accounts } = useMsal();

    const handleChange = (e: any) => {
        setResetEmail(e.target.value);
    }

    useEffect(() => {
        const fetchRows = async () => {
            try {
                setLoading(true);
                await instance.initialize();
                const token = await getToken(instance, accounts);
                const headers = new Headers();
                headers.append("Authorization", `Bearer ${token}`);
                const response = await fetch("/api/environments/keychainAccounts", {
                    method: 'GET',
                    headers: headers
                });
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json() as ErrorResponse;
                    throw new Error(`${errorData.error}`);
                }
                const result = await response.json();
                setRowData(result.result);
            } catch (err) {
                console.error(err);
            }
        };

        fetchRows();
    }, [instance, accounts]);


    const handleClick = async () => {
        setFileResults([]);
        setResult(null);
        setLoading(true);
        setError(null);
        setSpinner(true);
        setFailure(false);
        setSuccess(false);

        const token = await getToken(instance, accounts);
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${token}`);
        if (fileLines?.length > 0) {
            for (const item of fileLines) {
                const formData = new FormData();
                formData.append("Email", item);
                const response = await fetch("/api/environments/resetPassword", {
                    method: 'POST',
                    body: formData,
                    headers: headers
                });
                setSpinner(false);
                if (!response.ok) {
                    const errorData = await response.json();
                    setFileResults(prevFileResults => [...prevFileResults, `Failed password reset: ${errorData.error}`]);
                    setFailure(true);
                }
                else {
                    const result = await response.json();
                    setFileResults(prevFileResults => [...prevFileResults, `Successfully reset ${result.result}`]);
                    setSuccess(true);
                }
            }
            setFileLines([]);
            setFileName(null);
            setLoading(false);
        } else {
            if (resetEmail === "") {
                setLoading(false);
                return;
            }
            try {
                const formData = new FormData();
                formData.append("Email", resetEmail);
                const response = await fetch("/api/environments/resetPassword", {
                    method: 'POST',
                    body: formData,
                    headers: headers
                });
                setSpinner(false);
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json();
                    setError(`Failed to reset password for ${resetEmail} becasue of ${errorData.error}`);
                    setFailure(true);
                    throw new Error(errorData.error);
                }
                const result = await response.json();
                setSuccess(true);
                // setResult(`Successfully reset password for ${result.result}`);
            } catch (err) {
                console.error(err);
            }
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
                    onClick={() => handleClick2(params.data.email, setValue, setLoading, setError)}
                >
                    {error ? 'Retry' : 'Get'}
                </Button>
            )}
        </div>
    });

    const handleClick2 = async (email: string, setValue: (value: string) => void, setLoading: (value: boolean) => void, setError: (value: boolean) => void) => {
        try {
            setLoading(true);
            const token = await getToken(instance, accounts);
            const headers = new Headers();
            headers.append("Authorization", `Bearer ${token}`);
            const response = await fetch(`/api/environments/secretKey?email=${encodeURIComponent(email.trim())}`, {
                method: 'GET',
                headers: headers
            });
            setLoading(false);
            if (!response.ok) {
                setError(true);
                const errorData = await response.json() as ErrorResponse;
                throw new Error(`${errorData.error}`);
            }
            const result = await response.json();
            setError(false);
            setValue(result.result);
        } catch (err) {
            console.error(err);
        }
    }

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

    const fileInputRef = useRef<HTMLInputElement | null>(null);

    const handleFileChange = async (event: any) => {
        if (event.target.files && event.target.files[0]) {
            const file = event.target.files[0];

            if (file.type !== "text/plain") {
                alert("Please upload a valid .txt file.");
                return;
            }

            setFileName(file.name);
            const reader = new FileReader();
            reader.onload = async (e) => {
                const content = (e.target as any).result as string;
                setFileLines(content.split(/\r?\n/).filter(line => line.trim() !== ""));
            };
            reader.readAsText(file);
        }
        if (fileInputRef.current) {
            fileInputRef.current.value = '';
        }
    };

    const handleFileUploadClick = () => {
        if (fileInputRef.current) {
            fileInputRef.current.click();
        }
    };

    return (
        <>
            <Box
               sx={{
                   width: "100%",
                   height: "100%",
               }}
            >
                <Box
                    sx={{
                        width: "100%",
                        display: "flex",
                        alignItems: "center",
                        gap: 1,
                        mb: 2,
                    }}
                >
                    <TextField size="small" id="outlined-basic" label="Search..." variant="outlined" onChange={handleFilterChange}/>
                    <div
                        style={{
                            display: "flex",
                            alignItems: "center",
                            width: "100%",
                            justifyContent: "space-between",
                        }}
                    >
                        <Box
                            sx={{
                                width: "100%",
                                display: "flex",
                                alignItems: "center",
                            }}
                        >
                            <TextField onChange={handleChange} size="small" label="Reset Email..." sx={{ width: "90%" }}></TextField>
                            <Box
                                sx={{
                                    ml: 2,
                                }}
                            >
                                { spinner ?
                                    <CircularProgress size="24px" sx={{ ml: 2 }} /> : success ?
                                    <CheckCircleRounded sx={{ color: "green" }} /> : failure ?
                                    <CancelRounded sx={{ color: "red" }} /> : null
                                }
                            </Box>
                            <input
                                accept=".txt"
                                id="file-upload"
                                type="file"
                                style={{display: 'none'}}
                                onChange={handleFileChange}
                                ref={fileInputRef}
                            />
                        </Box>
                        <Box
                            sx={{
                                width: "40%",
                                display: "flex",
                                justifyContent: "flex-end",
                                gap: 1,
                            }}
                        >
                            <input
                                type="file"
                                ref={fileInputRef}
                                onChange={handleFileChange}
                                style={{ display: "none" }}
                            />
                            <Button size="small" variant="contained" component="span" color="primary" onClick={handleClick}>
                                Reset
                            </Button>
                            <Button size="small" variant="contained" component="span" color="primary" onClick={handleFileUploadClick}>
                                Upload File
                            </Button>
                            {/*<label*/}
                            {/*    htmlFor="file-upload"*/}
                            {/*    style={{*/}
                            {/*        display: "flex",*/}
                            {/*        height: "100%",*/}
                            {/*        width: "100%",*/}
                            {/*    }}*/}
                            {/*>*/}
                            {/*    <Button variant="contained" component="span" color="primary">*/}
                            {/*        Upload File*/}
                            {/*    </Button>*/}
                            {/*    <Button variant="contained" component="span" color="primary">*/}
                            {/*        Reset*/}
                            {/*    </Button>*/}
                            {/*</label>*/}
                        </Box>
                    </div>
                </Box>
                <div
                    className="ag-theme-quartz"
                    style={{
                        width: "100%",
                        height: "87%",
                    }}
                >
                    <AgGridReact
                        loading={loading}
                        rowData={rowData}
                        columnDefs={columnDefs}
                        rowHeight={50}
                        ref={gridRef}
                        enableCellTextSelection={true}
                        onGridReady={() => gridRef.current.api.sizeColumnsToFit()}
                    />
                </div>
            </Box>
        </>
    );
}

export default CredsContainer;