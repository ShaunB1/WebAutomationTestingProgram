import './NavBar.css'
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { Link } from "react-router-dom";

const NavBar = () => {
    return (
        <>
            <AppBar position="fixed">
                <Toolbar className="navbar">
                    <Button component={Link} to={"/"} color="inherit" className="button">TAP</Button>
                    <Button component={Link} to={"/environments"} color="inherit" className="button">Environments</Button>
                    <Button component={Link} to={"/pivottable"} color="inherit" className="button">Pivot Table</Button>
                </Toolbar>
            </AppBar>
        </>
    );
}

export default NavBar;