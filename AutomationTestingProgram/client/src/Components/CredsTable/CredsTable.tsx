import React from "react"

interface CredsTableProps {
    email: string,
    role: string,
    roleDesc: string,
    orgs: string,
}

function CredsTable () {
    const [accountDetails, setAccountDetails] = React.useState<CredsTableProps[]>([]);

    React.useEffect(() => {
        async function getAccountDetails() {
            //const details = await window.electronAPI.getAccountDetails();
            //setAccountDetails(details);
        }
        getAccountDetails();
    }, []);

    return (
        <>
            <table>
                <thead>
                    <tr>
                        {/*<th>Application Code</th>*/}
                        <th>E-mail Address</th>
                        <th>Role</th>
                        <th>Role Description</th>
                        <th>Organizations</th>
                    </tr>
                </thead>
                <tbody>
                    {accountDetails.map((account, index) => (
                        <tr key={index}>
                            <td>{account.email}</td>
                            <td>{account.role}</td>
                            <td>{account.roleDesc}</td>
                            <td>{account.orgs}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </>
    );
}

export default CredsTable;