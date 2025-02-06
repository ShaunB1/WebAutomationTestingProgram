import {Box, Button, IconButton, Typography} from "@mui/material";
import GridViewIcon from '@mui/icons-material/GridView';
import "./SidePanel.css"
import {useNavigate} from "react-router-dom";
import {
    CheckCircle,
    Checklist,
    Dns,
    Edit,
    FindInPage,
    FolderZip,
    KeyboardDoubleArrowLeft,
    PlayCircle
} from "@mui/icons-material";
import {useState} from "react";

interface SidePanelProps {
    collapsed: boolean;
    setCollapsed: (collapsed: boolean) => void;
}

const SidePanel: React.FC<SidePanelProps> = ({ collapsed, setCollapsed }) => {
    const navigate = useNavigate();

    return (
        <>
            <Box
                sx={{
                    width: collapsed ? "20px" : "200px",
                    transition: "width 0.3s ease-in-out",
                    height: "100vh",
                    background: "white",
                    p: 2,
                    position: "fixed",
                    top: 70,
                    display: "flex",
                    flexDirection: "column",
                    alignItems: collapsed ? "center" : "flex-start",
                }}
            >
                <Box
                    sx={{
                        width: "100%",
                        height: "85%",
                    }}
                >
                    <Box
                        sx={{
                            width: "100%",
                            height: "100%",
                            display: "flex",
                            flexDirection: "column",
                            alignItems: "center",
                            gap: 2,
                        }}
                    >
                        <button className="nav-button" onClick={() => navigate("/dashboard")}>
                            <GridViewIcon fontSize="small"/>
                            { collapsed ? null : <Typography>Dashboard</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/testruns")}>
                            <PlayCircle fontSize="small"/>
                            { collapsed ? null : <Typography>Test Runs</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/filevalidation")}>
                            <FindInPage fontSize="small"/>
                            { collapsed ? null : <Typography>File Validation</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/edittestfile")}>
                            <Edit fontSize="small"/>
                            { collapsed ? null : <Typography>Edit Test File</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/environments")}>
                            <Dns fontSize="small"/>
                            { collapsed ? null : <Typography>Environments</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/taskboard")}>
                            <Checklist fontSize="small"/>
                            { collapsed ? null : <Typography>Tasks</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/completedtasks")}>
                            <CheckCircle fontSize="small"/>
                            { collapsed ? null : <Typography>Completed Tasks</Typography> }
                        </button>
                        <button className="nav-button" onClick={() => navigate("/extension")}>
                            <FolderZip fontSize="small"/>
                            { collapsed ? null : <Typography>Extension</Typography> }
                        </button>
                    </Box>
                    <Box
                        sx={{
                            width: "100%",
                            height: "5%",
                            color: "black",
                            display: "flex",
                            justifyContent: collapsed ? "center" : "flex-end",
                        }}
                    >
                        <IconButton onClick={() => setCollapsed(!collapsed)}>
                            <KeyboardDoubleArrowLeft
                                fontSize="small"
                                sx={{
                                    transform: collapsed ? "rotate(180deg)" : "rotate(0deg)",
                                    transition: "transform 0.3s ease-in-out"
                                }}
                            />
                        </IconButton>
                    </Box>
                </Box>
            </Box>
        </>
    );
}

export default SidePanel;