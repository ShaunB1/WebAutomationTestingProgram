import Home from "../Pages/Home.tsx";
import NavBar from "./NavBar/NavBar.tsx";
import EnvsPage from "../Pages/EnvsPage.tsx";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import PivotTable from "./PivotTable/PivotTable.tsx";

function App() {
    return (
        <>
            <BrowserRouter>
                <div className="main-container">
                    <NavBar />
                    <div className="content-container">
                        <Routes>
                            <Route path="/">
                                <Route index element={<Home />} />
                                <Route path="environments" element={<EnvsPage />} />
                                <Route path="pivottable" element={<PivotTable />} />
                            </Route>
                        </Routes>
                    </div>
                </div>
            </BrowserRouter>
        </>
    );
}

export default App
