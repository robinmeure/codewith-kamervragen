import { FluentProvider, teamsLightTheme, webDarkTheme, webLightTheme } from "@fluentui/react-components";
import { ChatPage } from './pages/ChatPage';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MsalProvider, MsalAuthenticationTemplate } from "@azure/msal-react";
import { Configuration, PublicClientApplication, InteractionType } from "@azure/msal-browser";

const queryClient = new QueryClient()

const configuration: Configuration = {
  auth: {
    clientId: process.env.VITE_PUBLIC_APP_ID || "",
    authority: process.env.VITE_PUBLIC_AUTHORITY_URL || "",
    redirectUri: "/",
    postLogoutRedirectUri: "/",
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
  },
};
const pca = new PublicClientApplication(configuration);

const authRequest = {
  scopes: ["openid", "profile", process.env.VITE_BACKEND_SCOPE|| ""]
};

function App() {

  return (
    <MsalProvider instance={pca}>
      <MsalAuthenticationTemplate
        interactionType={InteractionType.Redirect}
        authenticationRequest={authRequest}
      >
        <QueryClientProvider client={queryClient}>
          <FluentProvider theme={webLightTheme}>
            <ChatPage />
          </FluentProvider>
        </QueryClientProvider>
      </MsalAuthenticationTemplate>
    </MsalProvider>
  );
}

export default App
