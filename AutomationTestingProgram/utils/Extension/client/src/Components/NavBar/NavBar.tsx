import { useEffect } from 'react';
import './NavBar.css';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { login, logout } from "../../authConfig.ts";
import { Link, useNavigate, useLocation } from "react-router-dom";

const NavBar = (props: any) => {
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogin = async () => {
        await login(props.setIsAuthenticated, props.setAccessToken);
    };

    const handleLogout = async () => {
        await logout();
        chrome.storage.local.remove('accessToken');
        props.setAccessToken(null);
        props.setIsAuthenticated(false);
        navigate('/');
    };

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
                {
                    props.isAuthenticated ?
                        <Toolbar className="navbar">
                            <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                            <Button component={Link} to={"/environments"} color="inherit" className="button">Environments</Button>
                            <Button component={Link} to={"/recorder"} color="inherit" className="button" >Test Recorder</Button>
                            <Button component={Link} to={"/tools"} color="inherit" className="button">Tools</Button>
                            <Button onClick={handleLogout} color="inherit" className="button">Logout</Button>
                        </Toolbar>
                        :
                        <Toolbar className="navbar">
                            <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                            <Button onClick={handleLogin} color="inherit" className="button">Login</Button>
                        </Toolbar>
                }
            </AppBar>
        </>
    );
}

export default NavBar;