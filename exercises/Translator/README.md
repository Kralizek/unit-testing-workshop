# Translator

This exercise is focused on the differences between authoring unit tests with and without taking advantage of AutoFixture integrations with NUnit and Moq.

The application under test is a simple Nybus application running in .NET Core 2.2 and accepting a `TranslateEducationCommand`. Each command contains: the ID of an education published on Studentum.se, and a language to translate its text into.

The flow is very simple:
1. the application retrieves the profile of the course via an HTTP request
2. the content is parsed so that the content of the text paragraphs are extracted
3. the application uses Amazon Translate to translate the content from Swedish to the desired language
4. all translations are uploaded on Amazon S3 in a single document
5. an event to notify that the processing is over is raised

The application comes with two handlers for the same command. One, `SingleTranslateCommandHandler`, contains the whole processing logic and directly uses external services. The second one, `ImprovedTranslateCommandHandler`, coordinates a set of dependencies delegating to them parts of the business logic.

## How to run the application

### RabbitMQ backend

The application is using RabbitMQ for the message exchange and it's already configured to take advantage of a Docker container with the default setting.

You can create your RabbitMQ container executing this line
```powershell
# Creates the container
docker create --name rabbitmq -p 4369:4369 -p 35197:35197 -p 15672:15672 -p 5672:5672 rabbitmq:management
```
Once created, you can start and stop it by using the following commands
```powershell
# Starts the container
docker start rabbitmq
# Stops the container
docker stop rabbitmq
```
This container includes the web management interface, so you can point your browser to http://localhost:15672 and authenticate with the default credentials (`guest`, `guest`).

### Enqueuing messages

For convenience, the solution contains a console application that can be used to push a message to the queue.

You can execute this application from Visual Studio or from your console by executing the following command (assuming you are in the root folder of the solution)
```powershell
# Executes the Message sender application with a parameter
dotnet run -p .\src\MessageSender\MessageSender.csproj -- <<educationId>>
```
You can replace `<<educationId>>` with the ID of any course published on Studentum.se.

### Processing the queue messages

As for the `MessageSender` application, you can execute `QueueProcessor` from the command line by executing the following command
```powershell
# Starts the QueueProcessor
dotnet run -p .\src\QueueProcessor\QueueProcessor.csproj
```
Press `CTRL+C` to exit the program.

Alternatively, you can execute from Visual Studio, with or without the debugger attached.

### Execuing the tests

You can execute the tests as you prefer:
* Visual Studio native test interface
* Visual Studio + ReSharper test interface
* Visual Studio Code using [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
* `dotnet test` in the console

Alternatively, you can run the provided Cake script. The script will execute the tests, measure the code coverage, render a report and open the report in your default browser.

To execute the script, you need to install the Cake global tool
```powershell
dotnet tool install --global Cake.Tool
```
Once installed the tool, you can simply run the script
```powershell
dotnet cake
```

You will be required to execute the build script later, so you should set up your environment for it regardless of which way you prefer to execute the tests.

## The exercise

The solution includes two sets of unit tests

* `Tests.QueueProcessor.WithoutIntegration`
* `Tests.QueueProcessor.WithIntegration`

The name of the two projects should be self-explicative.

The first suite is not using any special integration between AutoFixture and NUnit and Moq. On the other hand, the second suite has the same unit tests but implemented using the different available integrations.

Each of the two suites have tests that have not been implemented or only partially implemented. Also, please notice that, while getting 100% coverage, they are not intended to cover all cases.

Finally, the two suites of tests are meant to be used together. While working on one assignement, it's ok to take a peak on the "sibling" project to get a pointer on where to go next. Same applies for other fixtures in the same project, feel free to take inspiration from them.

Googling and Stack Overflow are fine too, but I am not sure you'll find much about this exercise ;)

### Assignement 1: testing without integrations

In the `Tests.QueueProcessor.WithoutIntegration` project, perform the following activities
* Read through the tests in the fixture `ImprovedTranslateCommandHandlerTests` in the `Handlers` folder
* Complete the tests in the fixture `SingleTranslateCommandHandlerTests` in the `Handlers` folder
* Read through the tests in the fixtures in the `Services` folder

### Assignment 2: testing with integration

In the `Tests.QueueProcessor.WithIntegration` project, perform the following activities
* Read through the `AutoDataAttributes.cs` file in the root of the project
* Read through the tests in the fixture `ImprovedTranslateCommandHandlerTests` in the `Handlers` folder
* Read through the tests in the fixture `SingleTranslateCommandHandlerTests` in the `Handlers` folder
* Complete the tests in the fixtures in the `Services` folder

### Assignment 3: improving the test suites

In the `Tests.QueueProcessor.WithIntegration` project, perform the following activities
* Expand the `SingleTranslateCommandHandlerTests` fixture by adding tests to simulate the failure of each incoming dependencies
* Expand the `ImprovedTranslateCommandHandlerTests` fixture by adding tests to simulate the failure of each dependency
* Expand the fixtures in the `Services` folder by simulating the failure of each dependency
* _Optional_ Implement the same tests in the `Tests.QueueProcessor.WithoutIntegration` project.

You can skip testing for failures of `ILogger<T>`.

### Assignment 4: changing the application under test

In this assignment we will be introducing changes in the tested application. Each activity is considered concluded when the application can be correctly built and tested via the build script.

In the project under test, `QueueProcessor`, perform the following activities
* Add support in `ImprovedTranslateCommandHandler` for logging by adding a dependency to `ILogger<ImprovedTranslateCommandHandler>`
* Change `SingleTranslateCommandHandler` so that empty paragraphs are discarded before translation
* Change `SingleTranslateCommandHandler` so that the texts are aggregated before the translation and not after
* Change `ImprovedTranslateCommandHandler` so that empty paragraphs are discarded before translation
* Change `ImprovedTranslateCommandHandler` and its dependencies so that the texts are aggregated before the translation and not after
