import Cookies from 'universal-cookie';

/**
 * async function that makes a call to an API.
 * @param {Object} props - The properties of the API call.
 * @param {string} props.url - The URL of the API endpoint.
 * @param {string} [props.method="GET"] - The HTTP method of the API call (e.g. GET, POST, PUT).
 * @param {string} [props.contentType="application/json"] - The content type of the request body.
 * @param {boolean} [props.hasAuth=false] - Whether authentication is required for the API call.
 * @param {Object} [props.body=null] - The request body.
 * @param {Object} props.status - An object that contains the status code of the API response.
 * @returns {(Object|string)} The API response, parsed as JSON if the content type is 'application/json', or as plain text if the content type is 'text/html'. If the content type is not specified, an empty string is returned.
 */
export async function makeCall(props)
{
  let {
    url,
    method = "GET",
    contentType = "application/json",
    hasAuth = false,
    basicAuthCredentials = null,
    body = null,
    status } = props;

  let authorization = btoa(basicAuthCredentials);
  let headers = {
    'Content-Type': contentType
  }
  let options = {
    method: method,
    headers: headers
  };

  if (hasAuth)
  {
    if (basicAuthCredentials)
    {
      headers.Authorization = 'Basic ' + authorization;
    }
    else
    {
      var cookies = new Cookies()
      headers["X-AUTH-TOKEN"] = cookies.get('authToken');
    }
  }


  if (body)
  {
    options.body = body;
  }
  let response = await fetch(url, options);

  status.code = response.status;

  let respContentType = response.headers.get('Content-Type');
  if (!respContentType)

    return "";

  if (respContentType.split(";")[0] === 'application/json')
    return await response.json();
  if (respContentType.split(";")[0] === 'text/html')
    return await response.text();
}