import React, { useEffect } from "react"
import { Button } from "react-bootstrap";
import Cookies from 'universal-cookie';
import configData from "../config/config.json";
import authConfig from "../config/microsoft-auth-config.json";

import { makeCall } from "../MakeCall";

export default function (props)
{
  const [username, setUsername] = React.useState("");
  const [password, setPassword] = React.useState("");
  const [alert, setAlert] = React.useState("");
  const logIn = async () =>
  {
    if (!username || !password)
    {
      setAlert("Inserisci nome utente e password");
      return;
    }

    if (username.includes(" ") || password.includes(" "))
    {
      setAlert("Nome utente e password non possono contenere spazi");
      return;
    }

    let status = { code: 0 };
    let result = await makeCall({
      url: configData.botApiUrl + '/login', method: "POST",
      hasAuth: true,
      basicAuthCredentials: username + ":" + password,
      status: status
    });

    if (status.code !== 200)
    {
      setAlert("Nome utente o password errati");
      return;
    }

    if (!result)
    {
      setAlert("Errore nel login.");
      return;
    }

    saveTokenAndRefresh(result.token);
    // await makeCall({ url: configData.botApiUrl + '/authenticate', method: "POST", hasAuth: true, status: status });
    // console.log(status.code);
  }

  useEffect(() =>
  {
    async function ssoAuthCallBack()
    {
      let code = new URL(window.location.href).searchParams.get("code");
      let state = new URL(window.location.href).searchParams.get("state");
      if (!code || !state) return;

      let status = { code: 0 };
      let url = new URL(configData.botApiUrl + '/auth/callback');
      url.searchParams.append("code", code);
      url.searchParams.append("state", state);

      let result = await makeCall({ url: url, method: "GET", hasAuth: false, status: status });

      if (!result)
      {
        setAlert("Errore nel login.");
        return;
      }

      saveTokenAndRefresh(result.token);
    }
    ssoAuthCallBack();
  }, []);


  const saveTokenAndRefresh = (token) =>
  {
    const cookies = new Cookies();
    // Save the token to a cookie
    cookies.set('authToken', token, {
      maxAge: 60 * 60 * 24 * 29, // expires in 29 days
      path: '/',
      sameSite: 'Strict',
      secure: true,
      httpOnly: false
    });

    props.refresh();

    window.location.pathname = "/PoliTutorBot/";
    window.location.hash = "/reservations";
  }

  const loginWithPolimi = () =>
  {
    let url = new URL("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");
    url.searchParams.append("client_id", authConfig.clientId);
    url.searchParams.append("scope", "openid offline_access");
    url.searchParams.append("response_type", "code");
    url.searchParams.append("state", "10020");
    url.searchParams.append("login_hint", "nome@mail.polimi.it");
    url.searchParams.append("redirect_uri", authConfig.redirectUri);

    window.location.href = url.href;

  }

  return (
    <div className="Auth-form-container">
      <form className="Auth-form">
        <div className="Auth-form-content">
          <h3 className="Auth-form-title"></h3>
          <div className="form-group mt-3">
            <label>Nome Utente</label>
            <input
              type="username"
              className="form-control mt-1"
              placeholder="nome utente"
              onChange={(e) => setUsername(e.target.value)}
            />
          </div>
          <div className="form-group mt-3">
            <label>Password</label>
            <input
              type="password"
              className="form-control mt-1"
              placeholder="password"
              onChange={(e) => setPassword(e.target.value)}
            />
          </div>
          <div className="d-grid gap-2 mt-3">
            <Button type="button" onClick={() => logIn()} className="btn btn-primary">
              Accedi
            </Button>
          </div>
          <>
            <Button
              className="btnSsoLogin"
              onClick={loginWithPolimi}>Accedi con Polimi</Button>
          </>
          <div className="alertText">{alert}</div>
        </div>
      </form>
    </div>
  )
}