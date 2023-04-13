using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;

const string BaseUrl = "https://cdkey.lilith.com/api/";

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

HttpClient client = new HttpClient();

int id = int.Parse(config.GetSection("UserId").Value);

Console.WriteLine(id);

client.DefaultRequestHeaders.Add("Accept", "application/json");
client.DefaultRequestHeaders.Add("AcceptEncoding", "gzip, deflate, br");
client.DefaultRequestHeaders.Add("AcceptLanguage", "ru,en;q=0.9");
client.DefaultRequestHeaders.Add("ContentType", "application/json");
client.DefaultRequestHeaders.Add("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 " +
    "YaBrowser/23.3.1.895 Yowser/2.5 Safari/537.36");

AuthData authData = new AuthData()
{
    Uid = id,
    Code = Console.ReadLine()
};

JsonContent jsonContent = JsonContent.Create(authData);

HttpResponseMessage response = await client.PostAsync(BaseUrl + "verify-afk-code", jsonContent);

Console.WriteLine("StatusCode " + response.StatusCode);

if (response.StatusCode != HttpStatusCode.OK)
{
    return;
}

foreach (var header in response.Headers)
{
    Console.Write($"{header.Key}:");
    foreach (var headerValue in header.Value)
    {
        Console.WriteLine(headerValue);
    }
}

Console.WriteLine();
Console.WriteLine("response.Content!!!!!!!!!!!");

foreach (var header in response.Content.Headers)
{
    Console.Write($"{header.Key}:");
    foreach (var headerValue in header.Value)
    {
        Console.WriteLine(headerValue);
    }
}

Console.WriteLine(await response.Content.ReadAsStringAsync());

Console.WriteLine();
Console.WriteLine("Смотрим кукисы");

Uri uri = new Uri(BaseUrl);

CookieContainer cookies = new CookieContainer();
// получаем из запроса все элементы с заголовком Set-Cookie
foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
    // добавляем заголовки кук в CookieContainer
    cookies.SetCookies(uri, cookieHeader);

// получение всех куки
foreach (Cookie cookie in cookies.GetCookies(uri))
    Console.WriteLine($"{cookie.Name}: {cookie.Value}");

Console.WriteLine();
Console.WriteLine("Закончили смотреть кукисы");


jsonContent = JsonContent.Create(new GetUserDataReuest() { Uid = id });
response = await client.PostAsync(BaseUrl + "users", jsonContent);

if (response.StatusCode != HttpStatusCode.OK)
{
    Console.WriteLine("StatusCode " + response.StatusCode);
    return;
}

Console.WriteLine(await response.Content.ReadAsStringAsync());

Root root = await response.Content.ReadFromJsonAsync<Root>();

Console.WriteLine(root.data.users.Count);

foreach (var user in root.data.users)
{
    Console.WriteLine(user.name + " " + user.uid);
}

Console.WriteLine("Введите код возмещения: ");
string key = Console.ReadLine();

for (int i = 0; i < root.data.users.Count; i++)
{
    jsonContent = JsonContent.Create(new CodeRequest()
    {
        uid = root.data.users[i].uid,
        cdkey = key
    });

    response = await client.PostAsync(BaseUrl + "cd-key/consume", jsonContent);

    Console.WriteLine("From User " + root.data.users[i].name + " " + response.StatusCode);
    //тут нужно смотреть ответ response.Content.ReadAsStringAsync() или ReadFromJsonAsync
}

class AuthData
{
    public int Uid { get; set; }

    public string Game { get; set; } = "afk";

    public string Code { get; set; }
}

public class CodeRequest
{
    public string type { get; set; } = "cdkey_web";
    public string game { get; set; } = "afk";
    public int uid { get; set; }
    public string cdkey { get; set; }
}

class GetUserDataReuest
{
    public int Uid { get; set; }

    public string Game { get; set; } = "afk";
}

public class Data
{
    public List<User> users { get; set; }
}

public class Root
{
    public int ret { get; set; }
    public string info { get; set; }
    public Data data { get; set; }
}

public class User
{
    public bool is_main { get; set; }
    public int svr_id { get; set; }
    public int level { get; set; }
    public string name { get; set; }
    public int uid { get; set; }
}