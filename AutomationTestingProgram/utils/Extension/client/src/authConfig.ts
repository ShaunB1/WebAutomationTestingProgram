import { VITE_PROD_AZ_CLIENT, VITE_AZ_TENANT } from "./constants";

const clientId = VITE_PROD_AZ_CLIENT;
const tenantId = VITE_AZ_TENANT;

export const msalConfig = {
  auth: {
    clientId: `${clientId}`,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: `https://${chrome.runtime.id}.chromiumapp.org/`,
    postLogoutRedirectUri: `https://${chrome.runtime.id}.chromiumapp.org/`,
    scope: `api://${clientId}`,
    clientCapabilities: ["CP1"],
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (_level: any, message: any, containsPii: any) => {
        if (containsPii) {
          return;
        }
        console.log(message);
      },
    },
  },
};

const loginRequest = {
  scopes: [`api://${clientId}/Test.Run`],
};

const getLoginUrl = async (instance: any) => {
  return new Promise((resolve, reject) => {
    instance.loginRedirect({
      ...loginRequest,
      onRedirectNavigate: (url: any) => {
        resolve(url);
        return false;
      }
    }).catch(reject);
  });
}

const getAcquireTokenUrl = (instance: any) => {
  return new Promise((resolve, reject) => {
    instance.acquireTokenRedirect({
      ...loginRequest,
      onRedirectNavigate: (url: any) => {
        resolve(url);
        return false;
      }
    }).catch(reject);
  });
}

const launchWebAuthFlow = async (url: any, instance: any) => {
  return new Promise((resolve, reject) => {
    chrome.identity.launchWebAuthFlow({
      interactive: true,
      url
    }, (responseUrl: any) => {
      if (responseUrl.includes("#")) {
        instance.handleRedirectPromise(`#${responseUrl.split("#")[1]}`)
          .then(resolve)
          .catch(reject)
      } else {
        resolve(null);
      }
    })
  })
}

const getLogoutUrl = async (instance: any, accounts: any) => {
  const logoutRequest = {
    account: instance.getAccountByHomeId(accounts[0].homeAccountId),
    postLogoutRedirectUri: `https://${chrome.runtime.id}.chromiumapp.org/`,
  };

  return new Promise((resolve, reject) => {
    instance.logout({
      ...logoutRequest,
      onRedirectNavigate: (url: any) => {
        resolve(url);
        return false;
      }
    }).catch(reject);
  });
}

export const getToken = async (instance: any, accounts: any) => {
  const tokenRequest = {
    ...loginRequest,
    account: accounts[0],
  };

  try {
    const token = await instance.acquireTokenSilent(tokenRequest);
    return token.accessToken;
  } catch (error: any) {
    if (error.name === "InteractionRequiredAuthError") {
      const acquireTokenUrl = await getAcquireTokenUrl(instance);
      const loginResult = await launchWebAuthFlow(acquireTokenUrl, instance);
      return (loginResult as { accessToken: string }).accessToken;
    }
    throw error;
  }
}

export const login = async (instance: any) => {
  const loginUrl = await getLoginUrl(instance);
  await launchWebAuthFlow(loginUrl, instance);
}

export const logout = async (instance: any, accounts: any) => {
  const logoutUrl = await getLogoutUrl(instance, accounts);
  await launchWebAuthFlow(logoutUrl, instance);
}
