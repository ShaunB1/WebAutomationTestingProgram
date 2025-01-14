import './App.css'
import Home from "./Pages/Home.tsx";
import NavBar from "./NavBar/NavBar";
import EnvPage from "./Pages/EnvPage";
import RecorderTable from "./RecorderTable/RecorderTable.tsx";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import ToolsPage from "../Pages/ToolsPage/ToolsPage.tsx";
import { useEffect, useState } from 'react';
import { getToken } from '../authConfig.ts';
import { HOST } from '../constants.ts';
import { useMsal } from '@azure/msal-react';
import AuthGuard from './AuthGuard/AuthGuard.tsx';

function App() {
  const [name, setName] = useState(null);
  const [email, setEmail] = useState(null);
  const { instance, accounts } = useMsal();

  const fallBackElement =
    window.location.pathname !== "/signin-oidc" ? <Navigate to={"/"} /> : <></>;

    useEffect(() => {
      const getAccountInfo = async () => {
          await instance.initialize();
          const token = await getToken(instance, accounts);
          const headers = new Headers();
          headers.append("Authorization", `Bearer ${token}`);
          const response = await fetch(`${HOST}/api/auth/getAccountInfo`, {
              method: "GET",
              headers: headers,
          });
          const result = await response.json();
          setName(result.name);
          setEmail(result.email);
      }
      getAccountInfo();
  }, []);

  return (
    <>
      <BrowserRouter>
        <div className="main-container">
          <NavBar name={name} email={email} />
          <div className="content-container">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/recorder" element={
                <AuthGuard>
                  <RecorderTable />
                </AuthGuard>
              } />
              <Route path="/environments" element={
                <AuthGuard>
                  <EnvPage />
                </AuthGuard>
              } />
              <Route path="/tools" element={
                <AuthGuard>
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
