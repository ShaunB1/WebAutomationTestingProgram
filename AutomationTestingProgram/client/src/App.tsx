import { useEffect, useState } from "react";
import "./App.css";
import NavBar from "@modules/core/components/NavBar/NavBar.tsx";
import HomePage from "@modules/home/pages/HomePage.tsx";
import EnvsPage from "@modules/environments/pages/EnvPage.tsx";
import PivotTablePage from "@modules/pivotTable/pages/PivotTablePage.tsx";
import TaskBoardPage from "@modules/tasks/pages/TaskBoardPage.tsx";
import CompletedTasksPage from "@modules/tasks/pages/CompletedTasksPage.tsx";
import ChatBot from "@modules/core/components/ChatBot/ChatBot.tsx";
import FileValidationPage from "@modules/fileValidation/pages/FileValidationPage.tsx";
import { getToken } from "@auth/authConfig.ts";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { MsalAuthenticationTemplate, useMsal } from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import ExtensionPage from "@modules/extension/pages/ExtensionPage.tsx";

function App() {
    // Kenny implemented this fallback element but this can be removed/updated
    // because the /signin-oidc route doesn't seem to work right now
    /*
    Fallback element first determines if requested URL is signin-oidc, and if not then
    catches all requests and reroutes it back to home page.

    /signin-oidc is the default redirect for Azure AAD to post the login token for
    the authenticated user.
    */
    const fallBackElement =
        window.location.pathname !== "/signin-oidc" ? <Navigate to={"/"} /> : <></>;
    const { instance, accounts } = useMsal();
    const [name, setName] = useState<string | null>(null);
    const [email, setEmail] = useState<string | null>(null);

    useEffect(() => {
        const getAccountInfo = async () => {
            try {
                await instance.initialize();
                const token = await getToken(instance, accounts);
                const headers = new Headers();
                headers.append("Authorization", `Bearer ${token}`);
                const response = await fetch("/api/auth/getAccountInfo", {
                    method: "GET",
                    headers: headers,
                });
                const result = await response.json();
                setName(result.name);
                setEmail(result.email);
            } catch (err) {
                console.error(err);
            }
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
                            <Route path="/" element={<HomePage />} />
                            <Route
                                path="/environments"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <EnvsPage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/pivottable"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <PivotTablePage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/taskboard"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <TaskBoardPage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/completedtasks"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <CompletedTasksPage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/filevalidation"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <FileValidationPage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/extension"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <ExtensionPage />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route path="*" element={fallBackElement} />
                        </Routes>
                    </div>
                </div>
            </BrowserRouter>
        </>
    );
}

export default App

