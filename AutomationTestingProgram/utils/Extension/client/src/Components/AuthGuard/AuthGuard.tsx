const AuthGuard = (props: any) => {
    return (
        <>
            {props.isAuthenticated ? props.children : "Please login to continue"}
        </>
    );
}

export default AuthGuard;