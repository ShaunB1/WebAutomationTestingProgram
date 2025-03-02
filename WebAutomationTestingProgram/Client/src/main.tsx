import { createRoot } from "react-dom/client";
import App from "./App.tsx";

import { PublicClientApplication } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import { msalConfig } from "@auth/authConfig.ts";

const msalInstance = new PublicClientApplication(msalConfig);

createRoot(document.getElementById("root")!).render(
  <MsalProvider instance={msalInstance}>
      <App />
  </MsalProvider>
);
