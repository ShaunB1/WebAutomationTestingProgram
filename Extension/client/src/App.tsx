import './App.css'
import HomePage from "@modules/home/pages/HomePage.tsx";
import NavBar from "@modules/core/components/NavBar/NavBar.tsx";
import AutoLoginPage from "@modules/autoLogin/pages/AutoLoginPage";
import RecorderPage from "@modules/testRecorder/pages/RecorderPage.tsx";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import ToolsPage from "@modules/tools/pages/ToolsPage.tsx";
import { useEffect, useState } from 'react';
import { getToken } from '@auth/authConfig';
import { HOST } from '@/constants.ts';
import { useMsal } from '@azure/msal-react';
import AuthGuard from '@modules/core/components/AuthGuard/AuthGuard.tsx';

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
  }, [instance, accounts]);

  return (
    <>
      <BrowserRouter>
        <div className="main-container">
          <NavBar name={name} email={email} />
          <div className="content-container">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/recorder" element={
                <AuthGuard>
                  <RecorderPage />
                </AuthGuard>
              } />
              <Route path="/autoLogin" element={
                <AuthGuard>
                  <AutoLoginPage />
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
