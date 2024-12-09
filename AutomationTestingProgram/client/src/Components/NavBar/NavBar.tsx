import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Button from "@mui/material/Button";
import { Link } from "react-router-dom";

const NavBar = () => {
    return (
        <>
            <AppBar position="sticky">
                <Toolbar>
                    <Button component={Link} to={"/"} color="inherit" style={{ fontWeight: 'bold', fontSize: '25px', marginRight: '30px' }}>TAP</Button>
                    <Button component={Link} to={"/environments"} color="inherit" style={{ marginRight: '30px' }} >Environments</Button>
                    <Button component={Link} to={"/pivottable"} color="inherit">Pivot Table</Button>
                </Toolbar>
            </AppBar>
        </>
    );
}

export default NavBar;