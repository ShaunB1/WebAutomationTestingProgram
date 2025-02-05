import {Autocomplete, Box, Button, IconButton, TextField, Typography} from "@mui/material";
import LogDisplay from "@modules/dashboard/components/LogDisplay/LogDisplay.tsx";
import {Delete, PlayCircle, StopCircle} from "@mui/icons-material";
import React, {useMemo, useState} from "react";
import envData from "@assets/environment_list.json";

const TestRuns = () => {
    const [env, setEnv] = useState<string | null>(null);
    const [envInputValue, setEnvInputValue] = useState('');
    const [browser, setBrowser] = useState<string | null>(null);
    const [browserInputValue, setBrowserInputValue] = useState('');
    const browserOptions = ["Chrome", "Edge", "Firefox"];

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
                                    justifyContent: "center",
                                }}
                            >
                                <Box
                                    sx={{
                                        display: "flex",
                                        flexWrap: "wrap",
                                        justifyContent: "center"
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
                                    />
                                </Box>
                                <Box>
                                    <Button variant="contained">Upload File</Button>
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
                                <Box
                                    sx={{
                                        width: "94%",
                                        height: "100%",
                                        background: "beige",
                                        display: "flex",
                                        alignItems: "center",
                                        justifyContent: "space-between",
                                        p: 2,
                                    }}
                                >
                                    <Typography>Test Run 1</Typography>
                                    <Box
                                        sx={{
                                            width: "30%",
                                            display: "flex",
                                            justifyContent: "space-evenly",
                                        }}
                                    >
                                        <IconButton>
                                            <PlayCircle />
                                        </IconButton>
                                        <IconButton>
                                            <StopCircle />
                                        </IconButton>
                                        <IconButton>
                                            <Delete />
                                        </IconButton>
                                    </Box>
                                </Box>
                                {/*<Box*/}
                                {/*    sx={{*/}
                                {/*        width: "100%",*/}
                                {/*        height: "100%",*/}
                                {/*        background: "beige",*/}
                                {/*        py: 2,*/}
                                {/*        pl: 4,*/}
                                {/*        outline: "2px solid lightgray"*/}
                                {/*    }}*/}
                                {/*>*/}
                                {/*    <Typography>Test Run 2</Typography>*/}
                                {/*</Box>*/}
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
                        <LogDisplay />
                        <LogDisplay />
                        <LogDisplay />
                    </Box>
                </Box>
            </Box>
        </>
    );
}

export default TestRuns;