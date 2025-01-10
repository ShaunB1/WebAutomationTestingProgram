import React, { useState, useMemo, useEffect } from "react";
import { Box, Button, Stack, Typography, TextField, Autocomplete } from "@mui/material";
import envData from "../../assets/environment_list.json";
import { useMsal } from "@azure/msal-react";
import { getToken } from "../../authConfig";
import browserVerData from "../../assets/AllowedBrowserVersions.json";

const SetArguments = () => {
    const [file, setFile] = useState<File | null>(null);
    const [env, setEnv] = useState<string | null>(null);
    const [envInputValue, setEnvInputValue] = useState('');
    const [browser, setBrowser] = useState<string | null>(null);
    const [browserInputValue, setBrowserInputValue] = useState('');
    const { instance, accounts } = useMsal();
    const [browserVersions, setBrowserVersions] = useState<string[]>([]);
    const [selectedBrowserVersion, setSelectedBrowserVersion] = useState<string | null>(null);

    const envOptions: string[] = useMemo<string[]>(() => {
        return envData.map((env) => {
            return env.ENVIRONMENT
        });
    }, []);

    // Function to get versions based on selected browser
    const getBrowserVersions = (browser: string) => {
        const browserData = browserVerData.find((data) => data.browser === browser);
        return browserData ? browserData.versions : [];
    };

    /*// Function to remove comments from a JSON string
    const removeComments = (jsonString: string) => {
        return jsonString.replace(/\/\/.*$/gm, '').trim(); // Remove single-line comments
    };

    // Fetch and clean JSON data before parsing
    const getBrowserVersions = (browser: string) => {
        // Read the JSON string from the browserVerData file
        const cleanJsonString = JSON.stringify(browserVerData);
        const jsonWithoutComments = removeComments(cleanJsonString); // Remove comments

        try {
            const parsedData = JSON.parse(jsonWithoutComments); // Parse the cleaned JSON data
            const browserData = parsedData.find((data) => data.browser === browser);
            return browserData ? browserData.versions : [];
        } catch (error) {
            console.error("Error parsing the browser version data:", error);
            return [];
        }
    };*/

    const browserOptions = ["Chrome", "Edge", "Firefox"];

    useEffect(() => {
        if (browser) {
            const versions = getBrowserVersions(browser);
            setBrowserVersions(versions);
        }
    }, [browser]);

    useEffect(() => {
        const browserChoice = localStorage.getItem('browser');
        if (browserChoice != null) {
            setBrowser(browserChoice);
        }
    }, []);

    useEffect(() => {
        const envChoice = localStorage.getItem('environment');
        if (envChoice != null) {
            setEnv(envChoice);
        }
    }, []);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files.length > 0) {
            setFile(event.target.files[0]);
        }
    }

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!file || !env || !browser || !selectedBrowserVersion) {
            alert("File or arguments missing");
            return;
        }

        const formData = new FormData();
        formData.append("file", file);
        formData.append('env', env);
        formData.append('browser', browser.toLowerCase());
        formData.append('browserVersion', selectedBrowserVersion); 

        try {
            const token = await getToken(instance, accounts);
            const headers = new Headers();
            headers.append("Authorization", `Bearer ${token}`);
            const res = await fetch("/api/test/run", {
                method: "POST",
                body: formData,
                headers: headers,
            });

            if (res.ok) {
                alert("File uploaded successfully!");
            } else {
                alert("Failed to upload file.");
            }
        } catch (e) {
            console.error("Error uploading file: ", e);
            alert("An error occurred while uploading the file.");
        }
    }

    const handleEnvChange = (e: any, newValue: string | null) => {
        setEnv(newValue);
        if (newValue) {
            localStorage.setItem('environment', newValue);
        } else {
            localStorage.setItem('environment', '');
        }
    }

    const handleBrowserChange = (e: any, newValue: string | null) => {
        setBrowser(newValue);
        setSelectedBrowserVersion(null); // Reset browser version when changing browser
        if (newValue) {
            localStorage.setItem('browser', newValue);
        } else {
            localStorage.setItem('browser', '');
        }
    }

    const handleBrowserVersionChange = (e: any, newValue: string | null) => {
        setSelectedBrowserVersion(newValue);
        if (newValue) {
            localStorage.setItem('browserVersion', newValue);
        } else {
            localStorage.setItem('browserVersion', '');
        }
    }

    return (
        <>
            <Box>
                <Typography variant="h4" color="textSecondary" gutterBottom>
                    Run Tests
                </Typography>
                <Box component="form" onSubmit={handleSubmit} display="flex" flexDirection={"row"} alignItems="center" gap={2}>
                    <Stack direction={"column"} alignItems={"center"} justify-content={"center"} spacing={1}>
                        <Button variant={"contained"} color={"primary"} component={"label"}>
                            Upload File
                            <input
                                type='file'
                                accept={".xlsx, .xls"}
                                hidden
                                onChange={handleFileChange}
                            />
                        </Button>
                        <Typography variant={"body2"} color={"textSecondary"}
                            sx={{
                                maxWidth: '125px',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                            }}
                        >
                            {file ? file.name : "No file chosen"}
                        </Typography>
                    </Stack>
                    <Autocomplete
                        sx={{ width: 200 }}
                        value={env}
                        onChange={(e: any, newValue: string | null) => handleEnvChange(e, newValue)}
                        inputValue={envInputValue}
                        onInputChange={(e, newInputValue) => {
                            setEnvInputValue(newInputValue);
                        }}
                        renderInput={(params) => <TextField {...params} label="Environment" />}
                        options={envOptions}
                    />
                    <Autocomplete
                        sx={{ width: 200 }}
                        value={browser}
                        onChange={(e: any, newValue: string | null) => handleBrowserChange(e, newValue)}
                        inputValue={browserInputValue}
                        onInputChange={(e, newInputValue) => {
                            setBrowserInputValue(newInputValue);
                        }}
                        renderInput={(params) => <TextField {...params} label="Browser" />}
                        options={browserOptions}
                    />
                    <Autocomplete
                        sx={{ width: 220 }}
                        value={selectedBrowserVersion}
                        onChange={handleBrowserVersionChange}
                        renderInput={(params) => {
                            const label = browser ? `Select ${browser} Version` : 'Select Browser';
                            return <TextField {...params} label={label} />;
                        }}
                        options={browserVersions}
                    />
                    <Button
                        variant="contained"
                        color={"success"}
                        type={"submit"}
                        sx={{ mt: 2, mb: 2 }}
                    >
                        Run Test
                        <span
                            style={{
                                display: 'inline-block',
                                width: 0,
                                height: 0,
                                borderTop: '10px solid transparent',
                                borderBottom: '10px solid transparent',
                                borderLeft: '15px solid white',
                                marginLeft: '10px'
                            }}
                        ></span>
                    </Button>
                </Box>
            </Box>
        </>
    );
}

export default SetArguments;