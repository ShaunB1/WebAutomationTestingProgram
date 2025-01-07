import { VITE_PROD_AZ_CLIENT, VITE_AZ_TENANT } from "./constants";

const clientId = VITE_PROD_AZ_CLIENT;
const tenantId = VITE_AZ_TENANT;

export const login = async (setIsAuthenticated: any, setAccessToken: any) => {
  const redirectUri = `https://${chrome.runtime.id}.chromiumapp.org/`;
  const authUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?` +
    `client_id=${encodeURIComponent(clientId)}` +
    `&response_type=token` +
    `&redirect_uri=${encodeURIComponent(redirectUri)}` +
    `&scope=api://${clientId}/Test.Run`;

  chrome.identity.launchWebAuthFlow(
    {
      url: authUrl,
      interactive: true
    },
    (redirectUrl) => {
      if (chrome.runtime.lastError) {
        console.error(chrome.runtime.lastError.message);
        return;
      }

      if (redirectUrl) {
        const urlParams = new URLSearchParams(new URL(redirectUrl).hash.substring(1));
        const accessToken = urlParams.get("access_token");

        setIsAuthenticated(true);
        setAccessToken(accessToken);
        chrome.storage.local.set({ "accessToken": accessToken });
      } else {
        console.error("Access token not found");
      }
    }
  );
};

export const logout = async () => {
  const redirectUri = `https://${chrome.runtime.id}.chromiumapp.org/`;
  const logoutUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/logout?` +
    `post_logout_redirect_uri=${encodeURIComponent(redirectUri)}`;

  chrome.identity.launchWebAuthFlow(
    {
      url: logoutUrl,
      interactive: true
    },
    function (_redirectUrl) {
      if (chrome.runtime.lastError) {
        console.error(chrome.runtime.lastError.message);
        return;
      }
      console.log("Successfully logged out");
    }
  );
};
