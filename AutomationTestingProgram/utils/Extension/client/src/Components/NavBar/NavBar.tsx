import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { Link } from "react-router-dom";

const NavBar = () => {
    return (
        <>
            <AppBar position="fixed">
                <Toolbar>
                    <Button component={Link} to={"/"} color="inherit" style={{ marginRight: '30px' }}>Test Recorder</Button>
                    <Button component={Link} to={"/environments"} color="inherit" style={{ marginRight: '30px' }} >Environments</Button>
                </Toolbar>
            </AppBar>
        </>
    );
}

export default NavBar;