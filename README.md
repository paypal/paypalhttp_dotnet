## ElCamino.PayPalHttp

Improved regex and threading performance over deprecated/forked [PayPalHttp library](https://github.com/paypal/paypalhttp_dotnet).

## PayPal HttpClient

PayPalHttp is a generic HTTP Client used with [generated server SDKs](https://github.braintreeps.com/dx/sdkgen).

In it's simplest form, an [`HttpClient`](./PayPalHttp-Dotnet/HttpClient.cs) exposes an `Execute` method which takes an [HTTP request](./PayPalHttp-Dotnet/HttpRequest.cs), executes it against the domain described in an [Environment](./PayPalHttp-Dotnet/Environment.cs), and returns an [HTTP response](./PayPalHttp-Dotnet/HttpResponse.cs).

### Environment

An [`Environment`](./PayPalHttp-Dotnet/Environment.cs) describes a domain that hosts a REST API, against which an `HttpClient` will make requests. `Environment` is a simple interface that wraps one method, `BaseUrl`.

```C#
var env = new Environment('https://example.com')
```

### Requests

HTTP requests contain all the information needed to make an HTTP request against the REST API. Specifically, one request describes a path, a method, any path/query/form parameters, headers, attached files for upload, and body data.

These objects are constructed in code generated by the [sdkgen](http://github.braintreeps.com/dx/sdkgen) project. Instructions for using generated HTTP request subclasses is provided in that project.

### Responses

HTTP responses contain information returned by a server in response to a request as described above. They are simple objects which contain a status code, headers, and any data returned by the server.

```C#
var client = new HttpClient(env);

var request = new HttpRequest("/", HttpMethod.Get);
request.Body = "some data";

var response = await client.Execute(request);

var statusCode = response.StatusCode;
var headers = response.Headers;
var data = response.Result<String>();
```

### Injectors

Injectors are blocks that can be used for executing arbitrary pre-flight logic, such as modifying a request or logging data. Injectors are attached to an `HttpClient` using the `AddInjector` method.

The `HttpClient` executes its injectors in a first-in, first-out order, before each request.

```C#
class LogInjector : IInjector
{
	public void Inject(HttpRequest request)
    {
        // Do some logging here
    }
}

var logInjector = new LogInjector();
client.AddInjector(logInjector);
...
```

### Error Handling

`HttpClient#Execute` may throw an `HttpException` if something went wrong during the course of execution. If the server returned a non-200 response, [HttpException](./PayPalHttp-Dotnet/HttpException.cs) will be thrown, that will contain a status code and headers you can use for debugging.

```C#
try
{
    client.Execute(request);
}
catch (HttpException ex)
{
	var statusCode = ex.StatusCode;
	var headers = ex.Headers;
	var message = ex.response<String>();
}
```

## License
PayPalHttp-Dotnet is open source and available under the MIT license. See the [LICENSE](./LICENSE) file for more information.

## Contributing
Pull requests and issues are welcome. Please see [CONTRIBUTING.md](./CONTRIBUTING.md) for more details.
