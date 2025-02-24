import { Autocomplete, Box, Button, Stack, TextField, Typography } from "@mui/material";
import LogDisplay from "../components/LogDisplay/LogDisplay";
import { useEffect, useMemo, useState } from "react";
import envData from "@assets/environment_list.json";
import { TestRun, ActiveRun } from '@interfaces/interfaces';
import { getToken } from "@auth/authConfig";
import { useMsal } from "@azure/msal-react";
import ActiveRuns from "../components/ActiveRuns/ActiveRuns";

const TestRuns = (props: any) => {
    const [env, setEnv] = useState<string | null>(null);
    const [envInputValue, setEnvInputValue] = useState('');
    const [browser, setBrowser] = useState<string | null>(null);
    const [browserInputValue, setBrowserInputValue] = useState('');
    const [delay, setDelay] = useState(0);
    const browserOptions = ["Chrome", "Edge", "Firefox"];
    const [testRuns, setTestRuns] = useState<ActiveRun[]>([]);
    const [testRunLogs, setTestRunLogs] = useState<TestRun[]>([]);
    const [file, setFile] = useState<File | null>(null);
    const { instance, accounts } = useMsal();

    useEffect(() => {
        const getActiveTestRuns = async () => {
            try {
                await instance.initialize();
                const token = await getToken(instance, accounts);
                if (token) {
                    const headers = new Headers();
                    headers.append("Authorization", `Bearer ${token}`);
                    headers.append("Content-Type", "application/json");

                    const data = {
                        filterType: "Type",
                        filterValue: "AutomationTestingProgram.Modules.TestRunnerModule.ProcessRequest"
                    }
                    const response = await fetch("/api/core/retrieve", {
                        method: "POST",
                        body: JSON.stringify(data),
                        headers: headers,
                    });
                    const result = await response.json();
                    if (response.ok) {
                        const activeRuns: ActiveRun[] = result.result.map((activeRun: any) => ({
                            id: activeRun.id
                        }));
                        setTestRuns(activeRuns);
                    } else {
                        throw new Error(result.error);
                    }
                }
            } catch (err) {
                console.error(err);
            }
        }
        getActiveTestRuns();
    }, [instance, accounts]);

    useEffect(() => {
        if (props.connection) {
            props.connection.on("BroadcastLog", (testRunId: any, message: any) => {
                const normalizedLog = message.replace(/\r\n/g, '\n').replace(/\r/g, '\n');
                setTestRunLogs(prevTestRunLogs =>
                    prevTestRunLogs.map(testRunLog =>
                        testRunLog.id === testRunId
                            ? { ...testRunLog, logs: [...testRunLog.logs, ...normalizedLog.split('\n')] }
                            : testRunLog
                    )
                );
            });

            props.connection.on('AddClient', (testRunId: any, message: any) => {
                console.log(message);
            });

            props.connection.on('RemoveClient', (testRunId: any, message: any) => {
                console.log(message);
            });

            props.connection.on('NewRun', (testRunId: any, message: any) => {
                console.log(message)
                setTestRuns(prevTestRuns => [...prevTestRuns, { id: testRunId }]);
            });

            props.connection.on('RunFinished', (testRunId: any, message: any) => {
                console.log(message)
            });

            props.connection.on('RunPaused', (testRunId: any, message: any) => {
                console.log(message)
            });

            props.connection.on('RunUnpaused', (testRunId: any, message: any) => {
                console.log(message)
            });

            props.connection.on('RunStopped', (testRunId: any, message: any) => {
                console.log(message)
            });

            props.connection.on('GetRuns', (testRunIds: any, message: any) => {
                console.log(message, `: [${testRunIds.toString()}]`);
                for (var testRunId of testRunIds) {
                    handleJoinRun(null, testRunId);
                }
            });

            props.connection.invoke('GetRuns');
        }

        return () => {
            if (props.connection) {
                props.connection.off('BroadcastLog');
                props.connection.off('AddClient');
                props.connection.off('RemoveClient');
                props.connection.off('NewRun');
                props.connection.off('RunFinished');
                props.connection.off('RunPaused');
                props.connection.off('RunUnpaused');
                props.connection.off('RunStopped');
            }
        }
    }, [props.connection]);

    useEffect(() => {
        const browserChoice = localStorage.getItem('browser');
        if (browserChoice != null) {
            setBrowser(browserChoice);
        }

        const envChoice = localStorage.getItem('environment');
        if (envChoice != null) {
            setEnv(envChoice);
        }

        const delayChoice = localStorage.getItem('delay');
        if (delayChoice != null) {
            setDelay(parseInt(delayChoice, 10));
        }
    }, []);

    const handleEnvChange = (e: any, newValue: string | null) => {
        setEnv(newValue);
        if (newValue) {
            localStorage.setItem('environment', newValue);
        } else {
            localStorage.setItem('environment', '');
        }
    }

    const envOptions: string[] = useMemo<string[]>(() => {
        return envData.map((env: any) => {
            return env.ENVIRONMENT
        });
    }, []);

    const handleBrowserChange = (e: any, newValue: string | null) => {
        setBrowser(newValue);
        if (newValue) {
            localStorage.setItem('browser', newValue);
        } else {
            localStorage.setItem('browser', '');
        }
    }

    const handleDelayChange = (e: any) => {
        setDelay(e.target.value);
        localStorage.setItem('delay', e.target.value);
    }

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

        const formData = new FormData();
        formData.append("File", file);
        formData.append("Browser", browser.toLowerCase());
        formData.append("BrowserVersion", "113");
        formData.append("Environment", env);
        formData.append("Delay", delay.toString());

        try {
            const token = await getToken(instance, accounts);
            const headers = new Headers();
            headers.append("Authorization", `Bearer ${token}`);

            const response = await fetch("/api/test/run", {
                method: "POST",
                body: formData,
                headers: headers,
            });
            const result = await response.json();
            if (response.ok) {
                const testRunId = result.result;
                props.connection.invoke("AddClient", testRunId);
                const testRunLog: TestRun = {
                    id: testRunId,
                    logs: []
                }
                setTestRunLogs(prevTestRunLogs => [...prevTestRunLogs, testRunLog]);
            } else {
                throw new Error(result.error);
            }
        } catch (e) {
            console.error("Error uploading file: ", e);
            alert("An error occurred while uploading the file.");
        }
    }

    const handleJoinRun = async (e: any, id: string) => {
        if (!testRunLogs.some(testRunLog => testRunLog.id === id)) {
            const token = await getToken(instance, accounts);
            const headers = new Headers();
            headers.append("Authorization", `Bearer ${token}`);

            let lines: string[] = [];

            try {
                const response = await fetch(`/api/core/retrieveLogFile?ID=${id}`, {
                    method: "GET",
                    headers: headers,
                });
                if (!response.ok) {
                    const result = await response.json();
                    throw new Error(result.error);
                }
                const text = await response.text();
                const normalizedLog = text.replace(/\r\n/g, '\n').replace(/\r/g, '\n');
                lines = normalizedLog.split('\n');
            } catch (err) {
                console.error(err)
            }

            const testRunLog: TestRun = {
                id: id,
                logs: lines
            }
            setTestRunLogs(prevTestRunLogs => [...prevTestRunLogs, testRunLog]);
            props.connection.invoke("AddClient", id)
        }
    }

    const handlePauseRun = async (e: any, id: string) => {
        const token = await getToken(instance, accounts);
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${token}`);

        try {
            const response = await fetch(`/api/test/pause?ID=${id}`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify({ id: id })
            });
            const result = await response.json();
            if (!response.ok) {
                throw new Error(result.error);
            }
            console.log(result.result);
        } catch (err) {
            console.error(err)
        }
    }

    const handleUnpauseRun = async (e: any, id: string) => {
        const token = await getToken(instance, accounts);
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${token}`);

        try {
            const response = await fetch(`/api/test/unpause?ID=${id}`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify({ id: id })
            });
            const result = await response.json();
            if (!response.ok) {
                throw new Error(result.error);
            }
            console.log(result.result);
        } catch (err) {
            console.error(err)
        }
    }

    const handleStopRun = async (e: any, id: string) => {
        const token = await getToken(instance, accounts);
        const headers = new Headers();
        headers.append("Authorization", `Bearer ${token}`);
        headers.append("Content-Type", "application/json")

        try {
            const response = await fetch(`/api/core/stop`, {
                method: "POST",
                headers: headers,
                body: JSON.stringify({ id: id })
            });
            const result = await response.json();
            if (!response.ok) {
                throw new Error(result.error);
            }
            console.log(result.result);
        } catch (err) {
            console.error(err)
        }
    }

    const handleCloseRun = async (e: any, id: string) => {
        setTestRunLogs(prevtestRunLogs => prevtestRunLogs.filter(testRunLog => testRunLog.id !== id));
        props.connection.invoke("RemoveClient", id);
    }

    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    height: "87vh",
                    display: "flex",
                    gap: 3,
                }}
            >
                <Box
                    sx={{
                        width: "50%",
                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "space-between",
                    }}
                >
                    <Box
                        sx={{
                            height: "44.5vh",
                            width: "100%",
                            overflow: "hidden",
                            display: "flex",
                            flexDirection: "column",
                        }}
                    >
                        <Box
                            sx={{
                                width: "100%",
                                height: "100%",
                                background: "white",
                                borderRadius: 4,
                            }}
                        >
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "50px",
                                    background: "#313D4F",
                                    borderRadius: "10px 10px 0 0",
                                    display: "flex",
                                    alignItems: "center",
                                    color: "white",
                                }}
                            >
                                <Typography
                                    sx={{
                                        fontSize: "20px",
                                        ml: 2,
                                    }}
                                >
                                    Create Test Run
                                </Typography>
                            </Box>
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "100%",
                                    display: "flex",
                                    flexDirection: "column",
                                    alignItems: "center",
                                    justifyContent: "flex-start",
                                    marginTop: "10px",
                                    gap: "10px"
                                }}
                                component="form" onSubmit={handleSubmit}
                            >
                                <Box
                                    sx={{
                                        display: "flex",
                                        flexWrap: "wrap",
                                        justifyContent: "center",
                                        alignItems: "flex-start",
                                        gap: "10px"
                                    }}
                                >
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
                                    <TextField
                                        label="Enter Delay (seconds)"
                                        type="number"
                                        value={delay}
                                        sx={{ width: 200 }}
                                        onChange={handleDelayChange}
                                    />
                                </Box>
                                <Box sx={{ display: 'flex', flexDirection: 'row', alignItems: 'flex-start' }}>
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

                                    <Button
                                        variant="contained"
                                        color="success"
                                        type="submit"
                                        sx={{ ml: 2 }}
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
                        </Box>
                    </Box>
                    <Box
                        sx={{
                            height: "40vh",
                            width: "100%",
                            overflow: "hidden",
                            display: "flex",
                            flexDirection: "column",
                            justifyContent: "space-between",
                        }}
                    >
                        <Box
                            sx={{
                                width: "100%",
                                height: "100%",
                                background: "white",
                                borderRadius: 4,
                                overflowY: "auto",
                            }}
                        >
                            <Box
                                sx={{
                                    width: "100%",
                                    height: "50px",
                                    background: "#313D4F",
                                    borderRadius: "10px 10px 0 0",
                                    display: "flex",
                                    alignItems: "center",
                                    color: "white",
                                }}
                            >
                                <Typography
                                    sx={{
                                        fontSize: "20px",
                                        ml: 2,
                                    }}
                                >
                                    Test Runs
                                </Typography>
                            </Box>
                            <Box
                                sx={{
                                    width: "100%",
                                }}
                            >
                                {testRuns.map(testRun => (
                                    <ActiveRuns key={testRun.id} testRun={testRun} handleJoinRun={handleJoinRun} showButton={true}/>
                                ))}
                            </Box>
                        </Box>
                    </Box>
                </Box>
                <Box
                    sx={{
                        height: "87vh",
                        width: "100%",
                        overflow: "hidden",
                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "space-between",
                    }}
                >
                    <Box
                        sx={{
                            width: "100%",
                            height: "100%",
                            background: "white",
                            borderRadius: 4,
                            overflowY: "auto",
                        }}
                    >
                        <Box
                            sx={{
                                width: "100%",
                                height: "50px",
                                background: "#313D4F",
                                borderRadius: "10px 10px 0 0",
                                display: "flex",
                                alignItems: "center",
                                color: "white",
                            }}
                        >
                            <Typography
                                sx={{
                                    fontSize: "20px",
                                    ml: 2,
                                }}
                            >
                                Logs
                            </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                            {testRunLogs.map(testRunLog => (
                                <LogDisplay
                                    key={testRunLog.id} testRunLog={testRunLog}
                                    handleStopRun={handleStopRun} handleCloseRun={handleCloseRun}
                                    handlePauseRun={handlePauseRun} handleUnpauseRun={handleUnpauseRun}
                                />
                            ))}
                        </Box>
                    </Box>
                </Box>
            </Box>
        </>
    );
}

export default TestRuns;