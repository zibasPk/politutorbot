
import configData from "./config/config.json"


export async function MakeCall(callMethod ,endPoint , hasCache, hasAuth, callBack) {
  const method = callMethod !== undefined ? callMethod : 'GET';
  const cache = hasCache ? 'default' : 'no-cache';
  const auth = hasAuth ? 'Basic ' + btoa(configData.authCredentials) : '';
  
  fetch(configData.botApiUrl + endPoint,
    {
      method: method, // *GET, POST, PUT, DELETE, etc.
      mode: 'cors', // no-cors, *cors, same-origin
      cache: cache, // *default, no-cache, reload, force-cache, only-if-cached
      credentials: 'same-origin', // include, *same-origin, omit
      headers: {
        'Authorization': auth,
        // 'Content-Type': 'application/x-www-form-urlencoded',
      }
    }).then((response)=> 
      response.json()
    ).then((data) => callBack(data));
}