import { useRef, useState } from 'react';
import EnvNavContainer from "../components/EnvNavContainer/EnvNavContainer";
import CredsContainer from "../components/CredsContainer/CredsContainer";
import {Box, Button, CircularProgress, TextField, Typography} from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { getToken } from "@auth/authConfig";

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
    const [fileResults, setFileResults] = useState<string[]>([]);
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
        if (fileLines?.length > 0) {
            for (const item of fileLines) {
                const formData = new FormData();
                formData.append("Email", item);
                const response = await fetch("/api/environments/resetPassword", {
                    method: 'POST',
                    body: formData,
                    headers: headers
                });
                if (!response.ok) {
                    const errorData = await response.json();
                    setFileResults(prevFileResults => [...prevFileResults, `Failed password reset: ${errorData.error}`]);
                }
                else {
                    const result = await response.json();
                    setFileResults(prevFileResults => [...prevFileResults, `Successfully reset ${result.result}`]);
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
                setLoading(false);
                if (!response.ok) {
                    const errorData = await response.json();
                    setError(`Failed to reset password for ${resetEmail} becasue of ${errorData.error}`);
                    throw new Error(errorData.error);
                }
                const result = await response.json();
                setResult(`Successfully reset password for ${result.result}`);
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
            <div
                style={{
                    height: "87vh"
                }}
            >
                {/*<div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "10px" }}>*/}
                {/*    <h2>Reset Password for Email:</h2>*/}
                {/*    <TextField label="Email" onChange={handleChange} value={resetEmail}></TextField>*/}
                {/*    <input*/}
                {/*        accept=".txt"*/}
                {/*        id="file-upload"*/}
                {/*        type="file"*/}
                {/*        style={{ display: 'none' }}*/}
                {/*        onChange={handleFileChange}*/}
                {/*        ref={fileInputRef}*/}
                {/*    />*/}
                {/*    <label htmlFor="file-upload">*/}
                {/*        <Button variant="contained" component="span" color="primary">*/}
                {/*            Upload File*/}
                {/*        </Button>*/}
                {/*        {fileName && <Typography variant="body2" onClick={(event) => {*/}
                {/*            setFileName(null);*/}
                {/*            setFileLines([]);*/}
                {/*        }}>Selected File: {fileName}</Typography>}*/}
                {/*    </label>*/}
                {/*    {loading ?*/}
                {/*        <CircularProgress size={24} /> :*/}
                {/*        <Button color="secondary" variant="contained" onClick={handleClick}>Reset</Button>*/}
                {/*    }*/}
                {/*</div>*/}
                <div style={{ display: "flex", alignItems: "center", justifyContent: "center" }}>
                    {fileResults?.length > 0 ? (
                        <ul>
                            {fileResults.map((item, index) => (
                                <li key={index} >
                                    {item}
                                </li>
                            ))}
                        </ul>
                    ) :
                        error ? <span style={{ color: "red" }}>{error}</span> : result && <span>{result}</span>}
                </div>
                <div
                    style={{
                        display: "flex",
                        height: "100%",
                        width: "100%",
                        gap: "1rem",
                    }}
                >
                    <Box
                        sx={{
                            width: "70%",
                            height: "96%",
                            background: "white",
                            p: 2,
                            borderRadius: 2,
                        }}
                    >
                        <div
                            style={{
                                height: "100%",
                                width: "100%",
                                overflow: "hidden"
                            }}
                        >
                            <Box
                                sx={{
                                    background: "#313D4F",
                                    borderRadius: 2,
                                    mb: 2,
                                }}
                            >
                                <Typography variant="h4" color="white" sx={{ p: 1 }} >Environments</Typography>
                            </Box>
                            <EnvNavContainer/>
                        </div>
                    </Box>
                    <Box
                        sx={{
                            width: "100%",
                            height: "96%",
                            background: "white",
                            p: 2,
                            borderRadius: 2,
                        }}
                    >
                        <div
                            style={{
                                width: "100%",
                                overflow: "hidden",
                                height: "100%",
                            }}
                        >
                            {/*<h1>KeyChain Accounts</h1>*/}
                            <Box
                                sx={{
                                    background: "#313D4F",
                                    width: "100%",
                                    borderRadius: 2,
                                    mb: 2,
                                }}
                            >
                                <Typography color="white" variant="h4" sx={{ p: 1 }} >KeyChain Accounts</Typography>
                            </Box>
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "100%",
                                }}
                            >
                                <CredsContainer/>
                            </Box>
                        </div>
                    </Box>
                </div>
            </div>
        </>
    );
}

export default EnvPage;