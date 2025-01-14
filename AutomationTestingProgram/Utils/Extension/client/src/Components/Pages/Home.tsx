const Home = (props: any) => {
    return (
        <>
            {
                props.isAuthenticated
                    ?
                    <p>Home Page for QA Regression Team Extension</p>
                    :
                    <p>Welcome to the QA Regression Team Extension. Please sign in to continue.</p>
            }
        </>
    );
}

export default Home;