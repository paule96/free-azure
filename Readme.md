# Free azure demo

This is a repository that shows a demo on how to use azure for free.

## Step by step guid

install swa cli in your project

```bash
npm install -D @azure/static-web-apps-cli
```

connect swa cli to your azure subscription and tenant

```bash
npx swa login
```

> Warning: This creates an `.env` file that contains kinda secret information. So maybe you should add this file to your gitignore

