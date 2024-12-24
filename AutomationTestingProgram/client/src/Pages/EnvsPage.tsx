import { useRef, useState } from 'react';
import EnvNavContainer from "../Components/EnvNavContainer/EnvNavContainer";
import CredsContainer from "../Components/CredsContainer/CredsContainer";
import { Button, CircularProgress, TextField, Typography } from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { getToken } from "../authConfig";

interface ApiResponse {
    message: string;
    success: boolean;
    email: string;
}

function EnvPage() {
    const [resetEmail, setResetEmail] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<string | null>(null);
    const [fileName, setFileName] = useState<string | null>(null);
    const [fileLines, setFileLines] = useState<string[]>([]);
    const [fileResults, setFileResults] = useState<ApiResponse[]>([]);
    const fileInputRef = useRef<HTMLInputElement | null>(null);
    const { instance, accounts } = useMsal();

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

    const handleClick = async () => {
        setFileResults([]);
        setResult(null);
        setLoading(true);
        setError(null);

        const token = await getToken(instance, accounts);
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${token}`);
        headers.append('Content-Type', 'application/json');

        if (fileLines?.length > 0) {
            for (const item of fileLines) {
                const response = await fetch('/api/environments/resetPassword', {
                    method: 'POST',
                    body: JSON.stringify(item),
                    headers: headers,
                });
                const result = await response.json();
                console.log(JSON.stringify(result));
                setFileResults(prevFileResults => [...prevFileResults, result]);
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
                const response = await fetch("api/environments/resetPassword", {
                    method: 'POST',
                    body: JSON.stringify(resetEmail),
                    headers: headers
                });
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json();
                    setError(`Failed to reset password for ${errorData.email}`);
                    throw new Error(errorData.message);
                }
                const result = await response.json();
                console.log(JSON.stringify(result));
                setResult(`Successfully reset password for ${result.email}`);
            } catch (err) {
                console.error(err);
            }
        }
    }

    const handleChange = (e: any) => {
        setResetEmail(e.target.value);
    }

    return (
        <>
            <div>
                <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "10px" }}>
                    <h2>Reset Password for Email:</h2>
                    <TextField label="Email" onChange={handleChange} value={resetEmail}></TextField>
                    <input
                        accept=".txt"
                        id="file-upload"
                        type="file"
                        style={{ display: 'none' }}
                        onChange={handleFileChange}
                        ref={fileInputRef}
                    />
                    <label htmlFor="file-upload">
                        <Button variant="contained" component="span" color="primary">
                            Upload File
                        </Button>
                        {fileName && <Typography variant="body2" onClick={(event) => {
                            setFileName(null);
                            setFileLines([]);
                        }}>Selected File: {fileName}</Typography>}
                    </label>
                    {loading ?
                        <CircularProgress size={24} /> :
                        <Button color="secondary" variant="contained" onClick={handleClick}>Reset</Button>
                    }
                </div>
                <div style={{ display: "flex", alignItems: "center", justifyContent: "center" }}>
                    {fileResults?.length > 0 ? (
                        <ul>
                            {fileResults.map((item, index) => (
                                <li key={index} style={{ color: (item.success ? 'black' : 'red') }}>
                                    {item.success ?
                                        `Successfully reset password for ${item.email}` :
                                        `Failed to reset password for ${item.email}`
                                    }
                                </li>
                            ))}
                        </ul>
                    ) :
                        error ? <span style={{ color: "red" }}>{error}</span> : result && <span>{result}</span>}
                </div>
                <div style={{ display: "flex", height: "100%" }}>
                    <div style={{ marginRight: "100px" }}>
                        <h1>Environments</h1>
                        <EnvNavContainer />
                    </div>
                    <div style={{ height: "100%" }}>
                        <h1>Search User Credentials</h1>
                        <CredsContainer />
                    </div>
                </div>
            </div >
        </>
    );
}

export default EnvPage;