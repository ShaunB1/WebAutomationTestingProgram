import './NavBar.css'
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import {Link, useNavigate} from "react-router-dom";
import {
    AuthenticatedTemplate,
    UnauthenticatedTemplate,
    useMsal,
} from "@azure/msal-react";
import { login, logout } from "@auth/authConfig";
import {
    Avatar,
    Box,
    Divider,
    IconButton,
    InputAdornment,
    Menu,
    MenuItem,
    TextField,
    Tooltip,
    Typography
} from '@mui/material';
import { useState } from 'react';
import {ContentPasteSearch, Search} from "@mui/icons-material";

const NavBar = (props: any) => {
    const { instance, accounts } = useMsal();

    const handleLogin = async () => {
        await login(instance);
    };

    const handleLogout = async () => {
        setAnchorElUser(null);
        await logout(instance, accounts);
    };

    const [anchorElUser, setAnchorElUser] = useState<null | HTMLElement>(null);

    const handleOpenUserMenu = (event: React.MouseEvent<HTMLElement>) => {
        setAnchorElUser(event.currentTarget);
    };

    const handleCloseUserMenu = () => {
        setAnchorElUser(null);
    };

    const navigate = useNavigate();

    return (
        <>
            <AppBar
                position="fixed"
                sx={{
                    background: "white",
                    boxShadow: "none",
                    height: "70px",
                }}
            >
                <AuthenticatedTemplate>
                    <Toolbar
                        className="navbar"
                    >
                        <Box
                            sx={{
                                width: "100px",
                                height: "100%",
                                display: "flex",
                                alignItems: "center",
                                ml: 2,
                            }}
                        >
                            <ContentPasteSearch sx={{ fontSize: "36px", color: "turquoise" }} />
                            <button onClick={() => useNavigate("/")} style={{ all: "unset", marginLeft: "16px" }}>
                                <Typography variant="h6" sx={{ color: "black", fontWeight: "bold" }}>TAP</Typography>
                            </button>

                        </Box>
                        <Box
                            sx={{
                                width: "100%",
                                height: "100%",
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                            }}
                        >
                            <TextField
                                variant="outlined"
                                size="small"
                                placeholder="Search..."
                                sx={{
                                    width: "600px",
                                    background: "#F5F4F6",
                                }}
                                InputProps={{
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <Search color="disabled" />
                                        </InputAdornment>
                                    )
                                }}
                            />
                        </Box>
                        {/*<Button*/}
                        {/*    component={Link}*/}
                        {/*    to={"/environments"}*/}
                        {/*    color="inherit"*/}
                        {/*    className="button"*/}
                        {/*>*/}
                        {/*    Environments*/}
                        {/*</Button>*/}
                        {/*<Button component={Link} to={"/pivottable"} color="inherit" className="button">*/}
                        {/*    Pivot Table*/}
                        {/*</Button>*/}
                        {/*<Button component={Link} to={"/taskboard"} color="inherit" className="button">*/}
                        {/*    Tasks*/}
                        {/*</Button>*/}
                        {/*<Button component={Link} to={"/completedtasks"} color="inherit" className="button">*/}
                        {/*    Completed Tasks*/}
                        {/*</Button>*/}
                        {/*<Button component={Link} to={"/filevalidation"} color="inherit" className="button">*/}
                        {/*    File Validation*/}
                        {/*</Button>*/}
                        {/*<Button component={Link} to={"/extension"} color="inherit" className="button">Extension</Button>*/}
                        {/*<Button component={Link} to={"/edittestfile"} color="inherit" className="button">*/}
                        {/*    Edit Test File*/}
                        {/*</Button>*/}
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
                                disableScrollLock
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
                                <MenuItem key="Account" onClick={handleCloseUserMenu}>
                                    <Typography sx={{ textAlign: 'center' }}>Account</Typography>
                                </MenuItem>
                                <MenuItem key="Settings" onClick={handleCloseUserMenu}>
                                    <Typography sx={{ textAlign: 'center' }}>Settings</Typography>
                                </MenuItem>
                                <MenuItem key="Logout" onClick={handleLogout}>
                                    <Typography sx={{ textAlign: 'center' }}>Logout</Typography>
                                </MenuItem>
                            </Menu>
                        </Box>
                    </Toolbar>
                </AuthenticatedTemplate>
                <UnauthenticatedTemplate>
                    <Toolbar className="navbar">
                        <Button
                            component={Link}
                            to={"/"}
                            color="inherit"
                            className="button"
                        >
                            TAP
                        </Button>
                        <Button
                            onClick={handleLogin}
                            color="inherit"
                            className="button"
                        >
                            Login
                        </Button>
                    </Toolbar>
                </UnauthenticatedTemplate>
            </AppBar>
        </>
    );
}

export default NavBar;