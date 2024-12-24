import './NavBar.css'
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { Link } from "react-router-dom";
import {
    AuthenticatedTemplate,
    UnauthenticatedTemplate,
    useMsal,
} from "@azure/msal-react";
import { login, logout } from "../../authConfig.ts";

const NavBar = () => {
    const { instance, accounts } = useMsal();

    const handleLogin = async () => {
        await login(instance);
    };

    const handleLogout = async () => {
        await logout(instance, accounts);
    };

    // TSX conditionally renders AuthenticatedTemplate if user is logged in,
    // and Unauthenticated template otherwise.

    return (
        <>
            <AppBar position="fixed">
                <AuthenticatedTemplate>
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
                            component={Link}
                            to={"/environments"}
                            color="inherit"
                            className="button"
                        >
                            Environments
                        </Button>
                        <Button component={Link} to={"/pivottable"} color="inherit" className="button">
                            Pivot Table
                        </Button>
                        <Button onClick={handleLogout} color="inherit" className="button">
                            Logout
                        </Button>
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