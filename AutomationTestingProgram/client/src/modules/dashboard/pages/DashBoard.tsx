import {Box, Typography} from "@mui/material";

const DashBoard: React.FC = () => {
    return (
        <>
            <Box
                sx={{
                    width: "100%",
                    height: "100%",
                    display: "flex",
                    gap: 6,
                }}
            >
                <Box
                    sx={{
                        height: "85vh",
                        width: "100%",
                        // outline: "2px solid red",
                        overflow: "hidden",
                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "space-between",
                    }}
                >
                    <Box
                        sx={{
                            width: "100%",
                            height: "40vh",
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
                    </Box>
                    <Box
                        sx={{
                            width: "100%",
                            height: "40vh",
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
                        height: "85vh",
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