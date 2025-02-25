import { ActiveRun } from "@interfaces/interfaces";
import { Box, Typography } from "@mui/material";
import { useMsal } from "@azure/msal-react";
import { useEffect, useState } from "react";
import { getToken } from "@auth/authConfig";
import ActiveRuns from "@/modules/testruns/components/ActiveRuns/ActiveRuns";

const DashBoard = (props: any) => {
    const [testRuns, setTestRuns] = useState<ActiveRun[]>([]);
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
                        filterValue: "AutomationTestingProgram.Modules.TestRunner.Backend.Requests.TestController.ProcessRequest"
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

    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    height: "100%",
                    display: "flex",
                    gap: 3,
                }}
            >
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
                            height: "38vh",
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
                                Current Test Runs
                            </Typography>
                        </Box>
                        <Box
                            sx={{
                                width: "100%",
                            }}
                        >
                            {testRuns.map(testRun => (
                                <ActiveRuns key={testRun.id} testRun={testRun} showButton={false} /*handleJoinRun={handleJoinRun}*/ />
                            ))}
                        </Box>
                    </Box>
                    <Box
                        sx={{
                            width: "100%",
                            height: "47vh",
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
                                Test Run Details
                            </Typography>
                        </Box>
                    </Box>
                </Box>
                <Box
                    sx={{
                        height: "87vh",
                        width: "40%",
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
                                Previous Test Runs
                            </Typography>
                        </Box>
                    </Box>
                </Box>
            </Box>
        </>
    );
}

export default DashBoard;