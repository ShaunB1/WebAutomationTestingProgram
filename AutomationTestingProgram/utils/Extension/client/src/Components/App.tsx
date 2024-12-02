import './App.css'
import NavBar from "./NavBar/NavBar";
import EnvPage from "./Pages/EnvPage";
import RecorderTable from "./RecorderTable/RecorderTable.tsx";
import { HashRouter, Route, Routes } from "react-router-dom";

function App() {
  return (
    <>
      <HashRouter>
        <div className="main-container">
          <NavBar />
          <div className="content-container">
            <Routes>
              <Route path="/">
                <Route index element={<RecorderTable />} />
                <Route path="environments" element={<EnvPage />} />
              </Route>
            </Routes>
          </div>
        </div>
      </HashRouter>
    </>
  )
}

export default App
