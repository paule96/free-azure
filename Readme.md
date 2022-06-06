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
