import "./App.css";
import NavBar from "@modules/core/components/NavBar/NavBar.tsx";
import EnvsPage from "@modules/environments/pages/EnvPage.tsx";
import PivotTablePage from "@modules/pivotTable/pages/PivotTablePage.tsx";
import TaskBoardPage from "@modules/tasks/pages/TaskBoardPage.tsx";
import CompletedTasksPage from "@modules/tasks/pages/CompletedTasksPage.tsx";
import FileValidationPage from "@modules/fileValidation/pages/FileValidationPage.tsx";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthenticatedTemplate, MsalAuthenticationTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import { getToken } from "./auth/authConfig.ts";
import { useEffect, useState } from "react";
import EditTestFile from "./Pages/EditTestFile.tsx";
import ExtensionPage from "@modules/extension/pages/ExtensionPage.tsx";
import LandingPage from "./modules/home/pages/LandingPage.tsx";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import SidePanel from "@modules/core/components/SidePanel/SidePanel.tsx";
import DashBoard from "@modules/dashboard/pages/DashBoard.tsx";
import TestRuns from "@modules/testruns/pages/TestRuns.tsx";
import {Box} from "@mui/material";

function App() {
    const fallBackElement = window.location.pathname !== "/signin-oidc" ? <Navigate to={"/"} /> : <></>;
    const { instance, accounts } = useMsal();
    const [name, setName] = useState<string | null>(null);
    const [email, setEmail] = useState<string | null>(null);
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [collapsed, setCollapsed] = useState(true);

    useEffect(() => {
        const connectToSignalR = async () => {
            await instance.initialize();
            const token = await getToken(instance, accounts);

            if (token) {
                try {
                    const signalRConnection = new HubConnectionBuilder()
                        .withUrl("/testHub", {
                            accessTokenFactory: () => token
                        })
                        .withAutomaticReconnect()
                        .build();

                    await signalRConnection.start();

                    signalRConnection.on("OnConnected", (message: any) => {
                        console.log(message);
                    });

                    signalRConnection.on("OnDisconnected", (message: any) => {
                        console.log(message);
                    });
                    setConnection(signalRConnection);


                } catch (err) {
                    console.error(err)
                }
            }

        }
        connectToSignalR();

        return () => {
            connection?.stop().then(() => {
                console.log('SignalR connection stopped');
            }).catch((err) => {
                console.error('Error stopping SignalR connection:', err);
            });
        }
    }, [instance, accounts]);

    useEffect(() => {
        const getAccountInfo = async () => {
            try {
                await instance.initialize();
                const token = await getToken(instance, accounts);
                if (token) {
                    const headers = new Headers();
                    headers.append("Authorization", `Bearer ${token}`);
                    const response = await fetch("/api/auth/getAccountInfo", {
                        method: "GET",
                        headers: headers,
                    });
                    const result = await response.json();
                    if (response.ok) {
                        setName(result.name);
                        setEmail(result.email);
                    } else {
                        throw new Error(result.error);
                    }
                }
            } catch (err) {
                console.error(err);
            }
        }
        getAccountInfo();
    }, [instance, accounts]);

    return (
        <>
            <BrowserRouter>
                <div className="main-container">
                    <NavBar name={name} email={email} />
                    <SidePanel collapsed={collapsed} setCollapsed={setCollapsed} />
                    <Box
                        sx={{
                            width: collapsed ? "94.2%" : "84.7%",
                            height: "88vh",
                            marginLeft: collapsed ? "3%" : "12.5%",
                            transition: "margin-left 0.3s ease-in-out, width 0.3s ease-in-out",
                            paddingLeft: "25px",
                            paddingTop: "30px",
                            paddingRight: "25px",
                            marginTop: "4%",
                        }}
                    >
                        <Routes>
                            <Route path="/" element={
                                <div>
                                    <AuthenticatedTemplate>
                                        <DashBoard />
                                    </AuthenticatedTemplate>
                                    <UnauthenticatedTemplate>
                                        <LandingPage />
                                    </UnauthenticatedTemplate>
                                </div>
                            } />
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
                                path="/dashboard"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <DashBoard connection={connection} />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/testruns"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <TestRuns connection={connection} />
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
                                        <TaskBoardPage collapsed={collapsed} />
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
                            <Route
                                path="/edittestfile"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <EditTestFile />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route path="*" element={fallBackElement} />
                        </Routes>
                    </Box>
                </div>
            </BrowserRouter>
        </>
    );
}

export default App

