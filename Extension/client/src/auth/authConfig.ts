import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { VITE_PROD_AZ_CLIENT, VITE_AZ_TENANT } from "../constants";

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
    iframeHashTimeout: 3000
  }
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

let tokenPromise: Promise<string> | null = null;

export const getToken = async (instance: any, accounts: any) => {
  console.log("tokenPromise", tokenPromise)
  if (!tokenPromise) {
    tokenPromise = instance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    }).then((token: any) => {
      return token.accessToken;
    }).catch(async (error: any) => {
      if (error instanceof InteractionRequiredAuthError) {
        console.log(error);
        const acquireTokenUrl = await getAcquireTokenUrl(instance);
        const response = await launchWebAuthFlow(acquireTokenUrl, instance);
        return (response as { accessToken: string }).accessToken
      }
      return null;
    });
  }

  return (tokenPromise as any).then((token: any) => {
    tokenPromise = null;
    return token;
  });
}



export const login = async (instance: any) => {
  const loginUrl = await getLoginUrl(instance);
  await launchWebAuthFlow(loginUrl, instance);
}

export const logout = async (instance: any, accounts: any) => {
  const logoutUrl = await getLogoutUrl(instance, accounts);
  await launchWebAuthFlow(logoutUrl, instance);
}
