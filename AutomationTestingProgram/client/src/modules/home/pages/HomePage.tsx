import React, {useEffect, useMemo, useRef, useState} from "react";
import SetArguments from "../components/SetArguments/SetArguments.tsx";
import DataTable from "../components/DataTable/DataTable.tsx";
import {Autocomplete, Box, Button, List, ListItem, Paper, Stack, TextField, Typography} from "@mui/material";
import {AuthenticatedTemplate, UnauthenticatedTemplate, useMsal} from "@azure/msal-react";
import envData from "@assets/environment_list.json";
import {getToken} from "@auth/authConfig.ts";
import * as test from "node:test";

interface TableData {
    name: string;
}

interface TestRun {
    id: string;
    logs: string[];
}

const Home: React.FC = () => {
    const [tables, setTables] = useState<TableData[]>([]);
    const [testRuns, setTestRuns] = useState<TestRun[]>([]);
    const [testCaseName, setTestCaseName] = useState<string>("");
    const [file, setFile] = useState<File | null>(null);
    const [env, setEnv] = useState<string | null>(null);
    const [envInputValue, setEnvInputValue] = useState('');
    const [browser, setBrowser] = useState<string | null>(null);
    const [browserInputValue, setBrowserInputValue] = useState('');

    const { instance, accounts } = useMsal();
    const logContainerRef = useRef<HTMLDivElement>(null);
    const host = process.env.NODE_ENV === "production" ? process.env.VITE_HOST : process.env.LOCAL_HOST;

    console.log("HOST: ", host);

    const envOptions: string[] = useMemo<string[]>(() => {
        return envData.map((env: any) => {
            return env.ENVIRONMENT
        });
    }, []);
    const browserOptions = ["Chrome", "Edge", "Firefox"];

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

        if (!file || !env || !browser) {
            alert("File or arguments missing");
            return;
        }

        const testRunId = crypto.randomUUID();
        const socket = new WebSocket(`wss://${host}/ws/logs?testRunId=${testRunId}`);

        const testRun: TestRun = {
            id: testRunId,
            logs: []
        }

        setTestRuns(prevTestRuns => [...prevTestRuns, testRun]);

        socket.onopen = (event) => {
            console.log(`WebSocket ${testRunId} connected.`)
        }

        socket.onmessage = (event) => {
            const message = event.data as string;
            if (message.startsWith("ClientId")) {
                // const clientId = message.split(":")[1];
                // testRun.id = clientId;
            } else {
                const data = JSON.parse(event.data);
                const { testRunId, logMessage } = data;
                console.log(testRunId, logMessage);
                console.log(testRun.id === testRunId);
                setTestRuns(prevTestRuns =>
                    prevTestRuns.map(testRun =>
                        testRun.id === testRunId
                            ? { ...testRun, logs: [...testRun.logs, logMessage] }
                            : testRun
                    )
                );
            }
        }

        socket.onclose = () => {
            console.log(`WebSocket ${testRun.id} closed `);
        }

        socket.onerror = (error) => {
            console.error("WebSocket error:", error);
        }

        const formData = new FormData();
        formData.append("file", file);
        formData.append('env', env);
        formData.append('browser', browser.toLowerCase());

        try {
            const token = await getToken(instance, accounts);
            const headers = new Headers();

            headers.append("Authorization", `Bearer ${token}`);
            headers.append("TestRunId", testRunId);

            const res = await fetch("/api/test/run", {
                method: "POST",
                body: formData,
                headers: headers,
            });

            if (res.ok) {
                alert("File uploaded successfully!");
            } else {
                alert("Failed to upload file.");
                socket.close();
            }
        } catch (e) {
            console.error("Error uploading file: ", e);
            alert("An error occurred while uploading the file.");
            socket.close();
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
        if (newValue) {
            localStorage.setItem('browser', newValue);
        } else {
            localStorage.setItem('browser', '');
        }
    }

    // useEffect(() => {
    //     const socket = new WebSocket(`wss://${host}/ws/logs`);
    //     console.log(`WS Host: ${host}`);
    //
    //     socket.onopen = (event) => {
    //         console.log("Connected to WebSocket server.");
    //     }
    //
    //     socket.onmessage = (event) => {
    //         const message = event.data;
    //
    //         if (message.startsWith("ClientId")) {
    //             const clientId = message.split(":")[1];
    //             console.log("Received ClientId: ", clientId);
    //         } else {
    //             setLogs((prevLogs) => [...prevLogs, event.data as string]);
    //         }
    //     }
    //
    //     socket.onerror = (error) => {
    //         console.error("WebSocket error:", error);
    //     }
    //
    //     socket.onclose = (event) => {
    //         console.log("Disconnected from WebSocket server");
    //     }
    //
    //     return () => {
    //         socket.close();
    //     }
    // }, [])

    const addTable = () => {
        if (testCaseName.trim() === "") return;
        setTables([...tables, { name: testCaseName }]);
        setTestCaseName("");
    }

    return (
        <>
            <AuthenticatedTemplate>
                <Box sx={{ display: "flex", flexDirection: "column", gap: "10px" }}>
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
                    <Box
                        sx={{
                            width: "100%",
                            display: "flex",
                            flexWrap: "wrap",
                            gap: 2,
                            justifyContent: "center",
                        }}
                    >
                        {testRuns.map((testRun, index) => (
                            <Box key={index} 
                                 sx={{
                                     width: "45%",
                                     display: "flex",
                                     flexWrap: "wrap",
                                     justifyContent: "center",
                                }}
                            >
                                <Paper
                                    elevation={3}
                                    ref={logContainerRef}
                                    sx={{
                                        width: '100%',
                                        height: 600,
                                        bgcolor: 'black',
                                        color: 'white',
                                        borderRadius: 2,
                                        overflowY: 'auto',
                                    }}
                                >
                                    <Box
                                        sx={{
                                            bgcolor: "#313d4f",
                                            p: 1,
                                            textAlign: 'center',
                                            position: 'sticky',
                                            top: 0,
                                            zIndex: 1
                                        }}
                                    >
                                        <Typography variant={"h6"} color={"white"}>
                                            Test Run {index+1}
                                        </Typography>
                                    </Box>
                                    <List>
                                        {testRun.logs.map((log, logIndex) => (
                                            <ListItem key={`${index}-${logIndex}`}>
                                                <Typography>{log}</Typography>
                                            </ListItem>
                                        ))}
                                    </List>
                                </Paper>
                            </Box>
                        ))}
                    </Box>
                </Box>
                {/*<Box*/}
                {/*    sx={{*/}
                {/*        display: "flex",*/}
                {/*        alignItems: "center",*/}
                {/*        gap: "10px",*/}
                {/*        marginBottom: "20px",*/}
                {/*        marginTop: "20px",*/}
                {/*    }}*/}
                {/*>*/}
                {/*    <TextField*/}
                {/*        value={testCaseName}*/}
                {/*        onChange={(e) => setTestCaseName(e.target.value)}*/}
                {/*        label="Cycle Group Name"*/}
                {/*        variant="outlined"*/}
                {/*        size="small"*/}
                {/*    />*/}
                {/*    <Button variant="contained" onClick={addTable}>*/}
                {/*        Add Table*/}
                {/*    </Button>*/}
                {/*</Box>*/}
                
                {/*{tables.map((table, index) => (*/}
                {/*    <div key={index} style={{ marginBottom: "40px" }}>*/}
                {/*        <DataTable testCaseName={table.name} />*/}
                {/*    </div>*/}
                {/*))}*/}
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                Welcome to the QA Regression Team's Automated Testing Program. Please
                sign in to continue.
            </UnauthenticatedTemplate>
        </>
    );
}

export default Home;