/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { InteractionRequiredAuthError } from "@azure/msal-browser";

/**
 * Configuration object to be passed to MSAL instance on creation.
 * For a full list of MSAL.js configuration parameters, visit:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/configuration.md
 */

// Determine tenant ID based on environment
const clientId = process.env.VITE_AZ_CLIENT;
const tenantId = process.env.VITE_AZ_TENANT;

export const msalConfig = {
  auth: {
    clientId: `${clientId}`,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: "/",
    postLogoutRedirectUri: "/",
    scope: `api://${clientId}`,
    clientCapabilities: ["CP1"],
  },
  cache: {
    cacheLocation: "localStorage", // This configures where your cache will be stored
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: any, message: any, containsPii: any) => {
        if (containsPii) {
          return;
        }
        //console.log(message);
      },
    },
  },
};

/**
 * Scopes you add here will be prompted for user consent during sign-in.
 * By default, MSAL.js will add OIDC scopes (openid, profile, email) to any login request.
 * For more information about OIDC scopes, visit:
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#openid-connect-scopes
 */
export const loginRequest = {
  scopes: [`api://${clientId}/Test.Run`],
};

/**
 * Add here the scopes to request when obtaining an access token for MS Graph API. For more information, see:
 * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-browser/docs/resources-and-scopes.md
 */
export const graphConfig = {
  graphMeEndpoint: "https://graph.microsoft.com/v1.0/me",
};

export const login = async (instance: any) => {
  try {
    await instance.loginRedirect(loginRequest);
  } catch (error) {
    console.error("Login failed with error: " + error);
  }
};

let tokenPromise: Promise<string> | null = null;
let isRedirecting = false;

export const getToken = async (instance: any, accounts: any) => {
  if (!tokenPromise) {
    tokenPromise = instance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    }).then((token: any) => {
      return token.accessToken;
    }).catch(async (error: any) => {
      if (error instanceof InteractionRequiredAuthError) {
        if (!isRedirecting) {
          isRedirecting = true;
          console.log(error);
          const response = await instance.acquireTokenRedirect(loginRequest);
          isRedirecting = false;
          return response.accessToken;
        } else {
          return tokenPromise;
        }
      }
      return null;
    })
  }

  return (tokenPromise as any).then((token: any) => {
    tokenPromise = null;
    return token;
  });
};

export const logout = async (instance: any, accounts: any) => {
  const logoutRequest = {
    account: instance.getAccountByHomeId(accounts[0].homeAccountId),
    postLogoutRedirectUri: '/',
  };

  await instance.logoutRedirect(logoutRequest);
};
