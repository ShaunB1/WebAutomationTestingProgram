import Home from "../Pages/Home.tsx";
import NavBar from "./NavBar/NavBar.tsx";
import EnvsPage from "../Pages/EnvsPage.tsx";
import RecorderPage from "../Pages/RecorderPage.tsx";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";

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
                                <Route path="testRecorder" element={<RecorderPage />} />
                            </Route>
                        </Routes>
                    </div>
                </div>
            </BrowserRouter>
        </>
    );
}

export default App
