import config from './config/config.json'

export async function makeCall(url, method, contentType, hasAuth, body, status) {
    let authorization = btoa(config.authCredentials);
    let headers = {
      'Content-Type': contentType
  }
    let options = {
        method: method,
        headers: headers
    };

    if(hasAuth) {
      headers.Authorization ='Basic ' + authorization;
    }

    if (body) {
        options.body = body;
    }
    let response = await fetch(url, options);

    status.code = response.status;

    console.log(status);
    if(contentType === 'application/json')
      return await response.json();
    if(contentType === 'text/html')
      return await response.text();
}