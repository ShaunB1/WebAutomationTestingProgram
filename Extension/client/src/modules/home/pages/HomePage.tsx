import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";

const HomePage = () => {
    return (
        <>
            <AuthenticatedTemplate>
                <p>Home Page for QA Regression Team Extension</p>
            </AuthenticatedTemplate>
            <UnauthenticatedTemplate>
                <p>Welcome to the QA Regression Team Extension. Please sign in to continue.</p>
            </UnauthenticatedTemplate>
        </>
    );
}

export default HomePage;