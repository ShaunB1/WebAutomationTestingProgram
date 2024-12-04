let isSidePanelOpen = false;

// const socketUrl = "ws://localhost:5223/ws/recorder"
// let socket: WebSocket | null = null;

// function initWebSocket() {
//     socket = new WebSocket(socketUrl);

//     socket.onopen = () => {
//         console.log("WebSocket connection established.");
//     }

//     socket.onmessage = (event) => {
//         console.log("Message from server: ", event.data);
//     }

//     socket.onclose = (event) => {
//         console.log(`WebSocket closed: ${event.code} - ${event.reason}`);
//     }

//     socket.onerror = (error) => {
//         console.error(`WebSocket error: `, error);
//     }
// }

// initWebSocket();

// function sendInteractionData(eventData: any) {
//     if (socket?.readyState === WebSocket.OPEN) {
//         const message = JSON.stringify({ type: "interaction", data: eventData });
//         socket.send(message);
//     } else {
//         console.error("WebSocket is not open.");
//     }
// }

// chrome.runtime.onMessage.addListener(sendInteractionData);
chrome.runtime.onMessage.addListener((message) => {
    if (message.action === "actiononobject") {
        chrome.runtime.sendMessage({ action: "RECORD_TEST_STEP", locator: message.locator, stepValues: message.stepValues });
    }
});

chrome.commands.onCommand.addListener((command) => {
    if (command === "toggle-side-panel") {
        if (!isSidePanelOpen) {
            chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                if (tabs[0]) {
                    const tabId = tabs[0].id as any;
                    chrome.sidePanel.open({ tabId });
                    isSidePanelOpen = true;
                }
            });
        } else {
            // There's no sidePanel.close() method in chrome API so this is a workaround
            chrome.sidePanel.setOptions({ enabled: false });
            chrome.sidePanel.setOptions({ enabled: true });
            isSidePanelOpen = false;
        }
    }
});

chrome.runtime.onMessage.addListener(async (message) => {
    if (message.action === "openTabAndLogin") {
        const { url, username, password } = message;

        // Logout of OPS BPS before auto-login.
        try {
            await fetch("https://stage.login.security.gov.on.ca/oam/server/logout", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
            });
            console.log("Successfully logged out of OPS BPS");
        } catch (err) {
            console.error("Error logging out of OPS BPS: " + err);
        }

        const parsedUrl = new URL(url);
        const protocol = parsedUrl.protocol;
        const domain = parsedUrl.hostname;
        chrome.cookies.getAll({ domain: domain }, function (cookies) {
            cookies.forEach(cookie => {
                chrome.cookies.remove({ url: `${protocol}//${domain}${cookie.path}`, name: cookie.name }, function (removed) {
                    if (removed) {
                        console.log(`Removed cookie: ${cookie.name}`);
                    } else {
                        console.log(`Failed to remove cookie: ${cookie.name}`);
                    }
                });
            })
        });

        chrome.cookies.getAll({ domain: "stage.login.security.gov.on.ca" }, function (cookies) {
            cookies.forEach(cookie => {
                chrome.cookies.remove({ url: `https://stage.login.security.gov.on.ca${cookie.path}`, name: cookie.name }, function (removed) {
                    if (removed) {
                        console.log(`Removed gov.on.ca cookie: ${cookie.name}`);
                    } else {
                        console.log(`Failed to remove gov.on.ca cookie: ${cookie.name}`);
                    }
                });
            })
        });

        chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
            const activeTab = tabs[0];
            if (!activeTab || !activeTab.id) {
                console.error("No active tab found.");
                return;
            }
            chrome.tabs.onUpdated.addListener(function onUpdated(tabId, changeInfo) {
                if (tabId === activeTab.id && changeInfo.status === 'complete') {
                    chrome.tabs.onUpdated.removeListener(onUpdated);
                    chrome.scripting.executeScript(
                        {
                            target: { tabId: activeTab.id },
                            func: autofillLogin,
                            args: [username, password],
                        }
                    );
                }
            })
            chrome.tabs.update(activeTab.id, { url });
        });

        // Can maybe detect middle click or right click and create new tab
        //
        // chrome.tabs.create({ url }, (tab: any) => {
        //     chrome.tabs.update(tab.id, { url }, () => {
        //         chrome.tabs.onUpdated.addListener(function onUpdated(tabId, changeInfo) {
        //             if (tabId === tab.id && changeInfo.status === 'complete') {
        //                 chrome.tabs.onUpdated.removeListener(onUpdated);
        //                 chrome.scripting.executeScript(
        //                     {
        //                         target: { tabId: tab.id },
        //                         func: autofillLogin,
        //                         args: [username, password],
        //                     }
        //                 );
        //             }
        //         })
        //     })
        // });
    }
    return true;
});

function autofillLogin(username: string, password: string) {
    const usernamexpath = "//*[@id='username'] | //input[@name='loginfmt'] | //input[@id='email']";
    const passwordxpath = "//*[@id='password'] | //input[@name='passwd']";
    const loginxpath = "//*[@id='signin'] | //input[@type='submit'] | //button[contains(text(), 'Sign In')]";
    const usernameField = document.evaluate(usernamexpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue as HTMLInputElement;
    const passwordField = document.evaluate(passwordxpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue as HTMLInputElement;
    const loginButton = document.evaluate(loginxpath, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue as HTMLButtonElement;

    if (usernameField && passwordField) {
        usernameField.value = username;
        passwordField.value = password;
        usernameField.dispatchEvent(new Event("input", { bubbles: true }));
        passwordField.dispatchEvent(new Event("input", { bubbles: true }));

        if (loginButton) {
            loginButton.click();
        } else {
            console.error("Login button not found");
        }
    } else {
        console.error("Login form not found");
    }
}