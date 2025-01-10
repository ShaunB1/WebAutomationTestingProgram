import { AuthenticatedTemplate, UnauthenticatedTemplate } from "@azure/msal-react";

const AuthGuard = (props: any) => {
    return (
        <>
            <AuthenticatedTemplate>{props.children}</AuthenticatedTemplate>
            <UnauthenticatedTemplate>Please login to continue</UnauthenticatedTemplate>
        </>
    );
}

export default AuthGuard;