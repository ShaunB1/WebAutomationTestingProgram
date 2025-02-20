import { useEffect, useState } from 'react';
import './NavBar.css';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { login, logout } from "@auth/authConfig";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { Avatar, Box, Divider, IconButton, Menu, MenuItem, Tooltip, Typography } from '@mui/material';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from '@azure/msal-react';

const NavBar = (props: any) => {
    const navigate = useNavigate();
    const location = useLocation();
    const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null);
    const { instance, accounts } = useMsal();

    const handleLogin = async () => {
        await login(instance);
    };

    const handleLogout = async () => {
        setAnchorElUser(null);
        await logout(instance, accounts);
    };

    const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorElUser(event.currentTarget);
    };

    const handleCloseUserMenu = () => {
        setAnchorElUser(null);
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
                <AuthenticatedTemplate>
                    <Toolbar className="navbar">
                        <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                        <Button component={Link} to={"/autoLogin"} color="inherit" className="button">Auto Login</Button>
                        <Button component={Link} to={"/recorder"} color="inherit" className="button" >Test Recorder</Button>
                        <Button component={Link} to={"/tools"} color="inherit" className="button">Tools</Button>
                        <Box style={{ marginLeft: "auto" }} sx={{ flexGrow: 0 }}>
                            <Tooltip title="Open settings">
                                <IconButton onClick={handleOpenUserMenu} sx={{ p: 0 }}>
                                    <Avatar></Avatar>
                                </IconButton>
                            </Tooltip>
                            <Menu
                                sx={{ mt: '45px' }}
                                id="menu-appbar"
                                anchorEl={anchorElUser}
                                anchorOrigin={{
                                    vertical: 'top',
                                    horizontal: 'right',
                                }}
                                keepMounted
                                transformOrigin={{
                                    vertical: 'top',
                                    horizontal: 'right',
                                }}
                                open={Boolean(anchorElUser)}
                                onClose={handleCloseUserMenu}
                            >
                                <Typography sx={{ padding: '4px 16px', fontWeight: 'bold' }}>
                                    {props.name}
                                </Typography>
                                <Typography sx={{ padding: '4px 6px 16px 16px' }}>
                                    {props.email}
                                </Typography>
                                <Divider />
                                <MenuItem key="Logout" onClick={handleLogout}>
                                    <Typography sx={{ textAlign: 'center' }}>Logout</Typography>
                                </MenuItem>
                            </Menu>
                        </Box>
                    </Toolbar>
                </AuthenticatedTemplate>
                <UnauthenticatedTemplate>
                    <Toolbar className="navbar">
                        <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                        <Button onClick={handleLogin} color="inherit" className="button">Login</Button>
                    </Toolbar>
                </UnauthenticatedTemplate>
            </AppBar>
        </>
    );
}

export default NavBar;