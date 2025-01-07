import './App.css'
import Home from "./Pages/Home.tsx";
import NavBar from "./NavBar/NavBar";
import EnvPage from "./Pages/EnvPage";
import RecorderTable from "./RecorderTable/RecorderTable.tsx";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import ToolsPage from "../Pages/ToolsPage/ToolsPage.tsx";
import { useEffect, useState } from 'react';
import AuthGuard from './AuthGuard/AuthGuard.tsx';

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [accessToken, setAccessToken] = useState(null);

  const fallBackElement =
    window.location.pathname !== "/signin-oidc" ? <Navigate to={"/"} /> : <></>;

  useEffect(() => {
    chrome.storage.local.get("accessToken", (token) => {
      if (token.accessToken) {
        setAccessToken(token.accessToken);
        setIsAuthenticated(true);
      }
    });
  }, []);

  return (
    <>
      <BrowserRouter>
        <div className="main-container">
          <NavBar isAuthenticated={isAuthenticated} setIsAuthenticated={setIsAuthenticated} setAccessToken={setAccessToken} />
          <div className="content-container">
            <Routes>
              <Route path="/" element={<Home isAuthenticated={isAuthenticated} />} />
              <Route path="/recorder" element={
                <AuthGuard isAuthenticated={isAuthenticated}>
                  <RecorderTable />
                </AuthGuard>
              } />
              <Route path="/environments" element={
                <AuthGuard isAuthenticated={isAuthenticated}>
                  <EnvPage accessToken={accessToken} />
                </AuthGuard>
              } />
              <Route path="/tools" element={
                <AuthGuard isAuthenticated={isAuthenticated}>
                  <ToolsPage />
                </AuthGuard>
              } />
              <Route path="/executescripts" />
              <Route path="*" element={fallBackElement} />
            </Routes>
          </div>
        </div>
      </BrowserRouter>
    </>
  )
}

export default App
