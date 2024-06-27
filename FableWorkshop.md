# Real-time event visualization using F# and Fable

## Prerequisites
1. Visual Studio Code (running on Windows, Linux or MacOS)
2. Ionide F# plugin (Ionide-fsharp) by Ionide
3. Basic understanding of F#
4. .NET Core version 8.0 or later
5. npm JavaScript package manager

## Recommended tools and resources
1. [The Elmish Book](https://zaid-ajaj.github.io/the-elmish-book) by Zaid Ajaj (its sample is outdated and currently doesn't build with latest Fable packages but it it still a valuable resource that explains Fable/Elmish concepts)
2. [An Introduction to Elm](https://guide.elm-lang.org/)
3. REST Client plugin by Huachao Mao
4. Chrome browser
5. Redux DevTools Chrome extension (remember to enable it)

## Workshop plan
1. Scaffolding a project with SAFE template
2. Serving the content of a file from a Saturn Web service
3. Retrieving file content using HTTP requests in a Fable app
4. Implementing playback of individual lines of a file
5. Replacing direct calls to Fable.React with Feliz
6. Adding Bulma CSS and FontAwesome to a Fable app (using CSS F# type provider)
7. From file lines player to events player (parsing text lines with Thoth.Json decoder)
8. Implementing event subscriptions using Web sockets (and Elmish.Bridge)
9. Adding live tiles (presentation of state changes)
10. Using Redux DevTools with Fable applications

## 1. Scaffolding a project with SAFE template
Now that we verified out tools, let's create an empty Fable project that we will incrementally enhance with features we need. The easiest is to use one of available scaffolding templates, and we will use SAFE template. Read more about SAFE Stack [here](https://safe-stack.github.io/). SAFE stands for Saturn-Azure-Fable-Elmish, but we will only be using SFE from its stack.

First install SAFE templates:
```
dotnet new -i SAFE.Template
```
Now generate a new Saturn/Fable/Elmish project with minimal number of features (option "-m"):
```
dotnet new SAFE -m -o FableWorkshop
```
This will create a new folder FableWorkshop with project files. Checkout its README.md file and the `src` folder. The source folder contains three subfolders: `Client`, `Server` and `Shared`.

Let's build the server first. Open the project folder in Visual Studio Code, Start the Terminal (you can start two Terminal instances: for server and client), and do the following:
```
cd src/Server
dotnet run
```
The server should be up and running after the built. Now it's time to build the client (from a different Terminal window):
```
npm install
dotnet tool restore
dotnet fable watch src/Client --run npm run start
```
After the client application is built, go to http://localhost:8080 and you should see a welcome message from SAFE.

### Inspect the project files
Browse project files, all essential client code resides in `Client/Index.fs` while the whole server is implemented in `Server/Server.fs`. The `Shared` folder contains definitions that are common for both applications.

## 2. Serving the content of a file from a Saturn Web service
We will now modify our Server app to serve file content. First, open `src/Shared/Shared.fs` file and add a new route to module `Route` so it will look like this:
```
module Route =
    let hello = "/api/hello"
    let files = "/api/files"
```
Now we need to modify our server. Replace the content of `src/Server/Server.fs` with the one from this [gist](https://gist.github.com/object/72c91ecbc2f82b6402d231a61d46fdea). The code refers ThothSerializer so we need to add its Nuget package to the project:
```
dotnet add src/Server/Server.fsproj package Thoth.Json.Giraffe
```

Build and run the app. Navigate to http://localhost:5000/api/files in the browser. Now it only shows an empty list. We will create a couple of files that we will later use in the project and place them in server's public folder. [This gist](https://gist.github.com/object/e77cfc2a1956b318dcff60a4bdb9db5c) contains files `SingleProgram.txt` and `MultiplePrograms.txt`. Create files with these names in `src/Server/public` folder and paste the respective content. Go to http://localhost:5000/api/files/SingleProgram.txt, you should get the content of a file.

### Further reading
To learn more of Saturn framework, check its [Web site](https://saturnframework.org/tutorials/how-to-start.html). [SAFE Dojo project](https://github.com/CompositionalIT/SAFE-Dojo) is a great example of how it can be used to develop Web applications and services in F#.
