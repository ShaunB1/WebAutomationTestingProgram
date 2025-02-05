import {Box, Button, Typography} from "@mui/material";
import GridViewIcon from '@mui/icons-material/GridView';
import "./SidePanel.css"
import {useNavigate} from "react-router-dom";
import {CheckCircle, Checklist, Dns, Edit, FindInPage, FolderZip, PlayCircle} from "@mui/icons-material";

const SidePanel: React.FC = () => {
    const navigate = useNavigate();

    return (
        <>
            <Box
                sx={{
                    width: "250px",
                    height: "100vh",
                    background: "white",
                    p: 2,
                    position: "fixed",
                    top: 70,
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
                        <Typography>Dashboard</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/")}>
                        <PlayCircle fontSize="small"/>
                        <Typography>Test Runs</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/filevalidation")}>
                        <FindInPage fontSize="small"/>
                        <Typography>File Validation</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/edittestfile")}>
                        <Edit fontSize="small"/>
                        <Typography>Edit Test File</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/environments")}>
                        <Dns fontSize="small"/>
                        <Typography>Environments</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/taskboard")}>
                        <Checklist fontSize="small"/>
                        <Typography>Tasks</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/completedtasks")}>
                        <CheckCircle fontSize="small"/>
                        <Typography>Completed Tasks</Typography>
                    </button>
                    <button className="nav-button" onClick={() => navigate("/extension")}>
                        <FolderZip fontSize="small"/>
                        <Typography>Extension</Typography>
                    </button>
                </Box>
            </Box>
        </>
    );
}

export default SidePanel;