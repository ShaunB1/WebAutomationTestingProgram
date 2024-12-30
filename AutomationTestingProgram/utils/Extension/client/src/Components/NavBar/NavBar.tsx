import { useEffect } from 'react';
import './NavBar.css';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { Link, useNavigate, useLocation } from "react-router-dom";

const NavBar = () => {
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        chrome.storage.local.get('lastPage', (result) => {
            if (result.lastPage) {
                navigate(result.lastPage);
            }
        });
    }, []);

    useEffect(() => {
        chrome.storage.local.set({ 'lastPage': location.pathname });
    }, [location]);

    return (
        <>
            <AppBar position="fixed">
                <Toolbar className="navbar">
                    <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                    <Button component={Link} to={"/environments"} color="inherit" className="button">Environments</Button>
                    <Button component={Link} to={"/recorder"} color="inherit" className="button" >Test Recorder</Button>
                    <Button component={Link} to={"/tools"} color="inherit" className="button">Tools</Button>
                </Toolbar>
            </AppBar>
        </>
    );
}

export default NavBar;