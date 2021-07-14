namespace Mesawer.PresentationLayer
{
    public static class ApiDocs
    {
        public const string Description =
            @"
==> Headers
- ** Version ** -
    - The Version request HTTP header uses to determine the client's (Web App | Mobile App) version, so, that our APIs
    can determine if there's a problem with the version and ask for an update by sending 505 response instead of 500 response
    in case of system failures. See the Server Errors section below.

- ** Accept-Language ** -
    - The Accept-Language request HTTP header advertises which languages the client is able to understand,
    and which locale variant is preferred.
    (By languages, we mean natural languages, such as English, and not programming languages).

- ** Mac-Address ** -
    - The Mac-Address request HTTP header advertises what the client's mac address is.

==> Success Responses `2xx`

- ** 200 Ok **
    - The request has succeeded.
    - [rfc2068#section-10.2.1](https://tools.ietf.org/html/rfc2068#section-10.2.1)

- ** 202 Accepted **
    - The request has been accepted for processing, but the processing has not been completed.
    - The response may contain the current status and a reference id.
    - [rfc2068#section-10.2.3](https://tools.ietf.org/html/rfc2068#section-10.2.3)

- ** 204 NoContent **
    - The request has succeeded. But there is no new information to send (Response not have message-body).
    - [rfc2068#section-10.2.5](https://tools.ietf.org/html/rfc2068#section-10.2.5)

==> Handling Errors

Requests made to our APIs can result in several different error responses.
The following document describes the common errors values.
API errors responses is based on [RFC7807](https://tools.ietf.org/html/rfc7807).

==> Client Errors `4xx`

- ** 400 Bad Request **
    - The request could not be understood by the server (not parsable) due to malformed syntax.
    - The request is trying to do something that is logically invalid.
    - [rfc2068#section-10.4.1](https://tools.ietf.org/html/rfc2068#section-10.4.1)

- ** 401 Unauthorized **
    - Access token is missing or invalid.
    - [rfc2068#section-10.4.4](https://tools.ietf.org/html/rfc2068#section-10.4.4)

- ** 403 Forbidden **
    - Doesn't have the necessary permissions.
    - [rfc2068#section-10.4.2](https://tools.ietf.org/html/rfc2068#section-10.4.2)

- ** 404 Not Found **
    - The requested resource was not found. This response will be returned if the URL
    is entirely invalid (i.e. /request)
    - Or, if it is a URL that could be valid but is referencing something that does not exist (i.e. /items/12344).
    - [rfc2068#section-10.4.5](https://tools.ietf.org/html/rfc2068#section-10.4.5)

- ** 405 Method Not Allowed **
    - A request was made of a resource using a request method not supported by that resource;
    for example, using GET on a form which requires data to be presented via POST,
    or using PUT on a read-only resource.
    - Or, the request body is empty.
    - [rfc2068#section-10.4.6](https://tools.ietf.org/html/rfc2068#section-10.4.6)

- ** 415 Unsupported Media Type **
    - The server is refusing to service the request because the entity of
    the request is in a format not supported by the requested resource
    for the requested method.
    - [rfc2068#section-10.4.16](https://tools.ietf.org/html/rfc2068#section-10.4.16)

- ** 422 Unprocessable Entity **
    - Means the server understands the content type of the request entity (parseable)
    but have a semantic errors (some parameters were missing or otherwise invalid).
    - [rfc2518#section-10.3](https://tools.ietf.org/html/rfc2518#section-10.3)

==> Server Errors `5xx`

- ** 500 Internal Error **
    - The request could not be understood by the server (not parsable) due to malformed syntax.
    - [rfc2068#section-10.4.1](https://tools.ietf.org/html/rfc2068#section-10.4.1)

- ** 505 HTTP Version Not Supported **
    - The known use: The server does not support, or refuses to support, the HTTP protocol
    version that was used in the request message.
    - Our use: The server does not support, or refuses to support, the client version that was used in the request header.
    - [rfc2068#section-10.5.6](https://tools.ietf.org/html/rfc2068#section-10.5.6)
";
    }
}
