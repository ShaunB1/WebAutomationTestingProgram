import './App.css'
import NavBar from "./NavBar/NavBar";
import EnvPage from "./Pages/EnvPage";
import RecorderTable from "./RecorderTable/RecorderTable.tsx";
import { Route, Routes } from "react-router-dom";
import ToolsPage from "../Pages/ToolsPage/ToolsPage.tsx";

function App() {
  return (
      <>
        <div className="main-container">
          <NavBar/>
          <div className="content-container">
            <Routes>
              <Route path="/">
                <Route path="recorder" element={<RecorderTable/>}/>
                <Route path="environments" element={<EnvPage/>}/>
                <Route path="tools" element={<ToolsPage/>}/>
              </Route>
            </Routes>
          </div>
        </div>
      </>
  )
}

export default App
