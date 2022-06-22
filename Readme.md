# Free azure demo

This is a repository that shows a demo on how to use azure for free.

## Step by step guid

### add SWA CLI to your project

install swa cli in your project

```bash
npm install -D @azure/static-web-apps-cli
```

connect swa cli to your azure subscription and tenant

```bash
npx swa login
```

> Warning: This creates an `.env` file that contains kinda secret information. So maybe you should add this file to your gitignore

### init a full dotnet project

> For the following steps you need the most latest version of [dotnet](dot.net) and the [function cli](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)

first create some folders

```bash
mkdir ./src/free-azure.api
mkdir ./src/free-azure.frontend
```

then init the frontend in the folder `./src/free-azure.frontend`

```bash
dotnet new blazorwasm
```

and the backend in the folder `./src/free-azure.api`

```bash
func init --dotnet
```

combine the project in a visual studio solution

```bash
dotnet new sln
dotnet sln add ./src/free-azure.api/free-azure_api.csproj
dotnet sln add ./src/free-azure.frontend/free-azure.frontend.csproj
```

> Information: If you didn't generate the HTTPS cert do it with `dotnet dev-certs https --trust`.

### add the SWA configuraiton for the project

We run the following assistant:

```bash
npx swa init
```

This will ask for an configuration name. That is not really important. After that it will hopefully find all paths correctly. It should look like:

![swa config wizard, with all the values automatic detected](/docs/.assets/swa-config.png)

Now you will find a new file in your workspace that is called `swa-cli.config.json`. This will contains all the important information for the static web apps cli to work later.

Now you can simply start the project with the following line:

```bash
npx swa start
```

This will start this project. Currently only the blazor frontend is usable, because the function API is empty. We will change this now.

### Intruduction to the project

This project is a simple time planing app. So you can create events at locations and a location can only be used for a single event to a given time.

### add some functions

> If you just use the Codespace or the remote containers extension then you don't need to install anything. But for all the other people, you will need for the following steps a `CosmosDB` host. It can be the emulater or a real CosmosDB.

First open a terminal an go to the API project

```bash
cd ./src/free-azure.api/
```

Then let add a simple function to list all events:

```bash
func new --template "Http Trigger" --name Events --force
```

This creates a new `Events.cs` file. In this file we now want to make a connection to the configured cosmos db. First let add some EF Core to make lifes easyier:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Cosmos
```

Now you can create a DBContext. The DbContext manges the connection to the database and translates the LINQ C# Syntax to SQL. An example of an DBConetext can be found in `src/free-azure.api/Models/FreeAzureContext.cs`

Now we have a database defintion and a functions project. Now we can continue writing our function. The sample file can be found here: `src/free-azure.api/Events.cs`.

In this file is the function `Events` that accepts GET and POST HTTP Requests. If the caller sends a post, it will create an event and if the caller sends a get it will list all events.

```csharp
[FunctionName("Events")]
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
    ILogger log)
{
    try
    {
        if (req.Method == HttpMethods.Post)
        {
            var postEvent = await JsonSerializer.DeserializeAsync<Event>(req.Body);
            await this.dbContext.Events.AddAsync(postEvent);
            await this.dbContext.SaveChangesAsync();
            return new OkResult();
        }
        else
        {
            log.LogInformation("try to create a list of events now.");
            var result = await this.dbContext
                .Events
                .ToListAsync();
            return new OkObjectResult(result);
        }
    }
    catch (Exception ex)
    {
        throw ex;
    }
}
```

### Build the frontend

The frontend is pretty easy because it's just C# in this sample. So we can easyly share all the models from the backend in the frontend. And there is another good thing: the sample Blazor project has a `Fetch data` page alread.(src/free-azure.frontend/Pages/FetchData) Let's rewrite this page quickly.

First lets change all types from `WeatherForcast` to `Event`.
And change the request to `api/Events`. Also make some changes to the html table:

```csharp
@page "/fetchdata"
@inject HttpClient Http

<PageTitle>Events</PageTitle>

<h1>Events</h1>

<p>This component demonstrates fetching Events from the server.</p>

@if (sampleEvents == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Start</th>
                <th>End</th>
                <th>Name</th>
                <th>Locations</th>
                <th>Duration</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var sampleEvent in sampleEvents)
            {
                <tr>
                    <td>@sampleEvent.Start.ToShortDateString()</td>
                    <td>@sampleEvent.End.ToShortDateString()</td>
                    <td>@sampleEvent.Name</td>
                    <td>@sampleEvent.Locations</td>
                    <td>@sampleEvent.Duration</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private free_azure.shared.Event[]? sampleEvents;

    protected override async Task OnInitializedAsync()
    {
        sampleEvents = await Http.GetFromJsonAsync<free_azure.shared.Event[]>("api/Events");
    }
}
```

## Lets deploy

### First configure routes

In the frontend folder you will find a `wwwroot` folder. This folder should contain a file with the name `staticwebapp.config.json`. This configures your the "server". We want to do three things there:

- set our backend framework to `dotnet` in version 6
- configure that the default behaivior for all routes should be a redirect to `index.html`
   - That is always the case if you want to deploy an single page application
- from the route rule, we want one exlusion, that allow us to call the api

An example of this file can look like that:

```json
{
    "platform": {
        "apiRuntime": "dotnet:6.0"
    },
    "navigationFallback": {
        "rewrite": "/index.html",
        "exclude": [
            "/api/*"
        ]
    }
}
```

### prepare deployment

To be able to deploy we need to create a resource group and in this resource group a cosmos db.

After this we need to change the configuration of the file `swa-cli.config.json`.
In this file we must define the new name of our webapp and the resource group we created. It could look like that:

```json
{
  "$schema": "https://aka.ms/azure/static-web-apps-cli/schema",
  "configurations": {
    "free-azure": {
      "appLocation": "src/free-azure.frontend",
      "apiLocation": "src/free-azure.api",
      "outputLocation": "bin/wwwroot",
      "appBuildCommand": "dotnet publish -c Release -o bin",
      "apiBuildCommand": "dotnet publish -c Release",
      "run": "dotnet watch run",
      "appDevserverUrl": "https://localhost:7120",
      "appName": "free-azure",
      "resourceGroupName": "free-azure-rg"
    }
  }
}
```

### Lets deploy

Now we are ready to deploy. In two steps:

- build
- deploy

#### build

To build the app run simply:

```bash
npx swa build
```

### deploy

```bash
 npx swa deploy --no-use-keychain --api-location src/free-azure.api/bin/Release/net6.0/publish/
```

### deploy to production

```bash
 npx swa deploy --no-use-keychain --api-location src/free-azure.api/bin/Release/net6.0/publish/ --env production
```

### last step after deployment

Now we must get the connection string from our cosmos db and must add that string as an environment variable in our static webapp. The name of the variable is:

`SqlConnectionString`

If we now open the app we should be ready.