import "./App.css";
import Home from "../Pages/Home.tsx";
import NavBar from "./NavBar/NavBar.tsx";
import EnvsPage from "../Pages/EnvsPage.tsx";
import PivotTable from "./PivotTable/PivotTable.tsx";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { MsalAuthenticationTemplate } from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import TaskBoard from "./TaskBoard/TaskBoard.tsx";
import CompletedTasks from "./CompletedTasks/CompletedTasks.tsx";
import ChatBot from "./ChatBot/ChatBot.tsx";
import FileValidation from "./FileValidation/FileValidation.tsx";

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

    return (
        <>
            <BrowserRouter>
                <div className="main-container">
                    <NavBar />
                    <div className="content-container">
                        <Routes>
                            <Route path="/" element={<Home />} />
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
                                        <PivotTable />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/taskboard"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <TaskBoard />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/completedtasks"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <CompletedTasks />
                                    </MsalAuthenticationTemplate>
                                }
                            />
                            <Route
                                path="/filevalidation"
                                element={
                                    <MsalAuthenticationTemplate
                                        interactionType={InteractionType.Redirect}>
                                        <FileValidation />
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
