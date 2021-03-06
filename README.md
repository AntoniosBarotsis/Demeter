# Project Demeter

[![.NET](https://github.com/AntoniosBarotsis/Demeter/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/AntoniosBarotsis/Demeter/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/AntoniosBarotsis/Demeter/branch/master/graph/badge.svg?token=1NT47WCG0Y)](https://codecov.io/gh/AntoniosBarotsis/Demeter)
[![CodeFactor](https://www.codefactor.io/repository/github/antoniosbarotsis/demeter/badge)](https://www.codefactor.io/repository/github/antoniosbarotsis/demeter)

Project Demeter is a personal toy project I decided to work on to improve my knowledge of ASP NET Core, C# and Domain Driven Development.

## The Idea

This is very likely to change slightly in the future but here's what I have in mind for this project so far:

- I will not be deploying this anywhere unlike my last few projects. Reason being that I want to not be limited in the services that I want to use
  (for example, I would usually host Heroku which is a great platform to use as long as you have 1 container running which is very likely to
  not be the case for this project by the end of it). Instead I will be using a local database (probably SQLite instead of a container
  since its lighter) and have a `docker-compose` file to spin up any other services that I end up using (such as a Redis cache)
- The main idea of the app is a food ordering service; Restaurant owners can create an account and post their available meals and normal users can
  query those and order what they want.

Some of the things that I want to learn out of this project include:
- Caching
- How to properly structure a big application
- Testing in C#
- Role based authentication with JWTs
- Async programming
- Take a lot of services that I've used individually and combine them (Redis and RabbitMQ are the 2 main examples of this, I might also add 
  Elasticsearch and Kibana).

Keep in mind that as this will never be a full product (as this is not the point) some stuff in it will just do nothing. For example: I am thinking
of implementing a Pub/Sub system with RabbitMQ for order placements but once these get received nothing will actually happen.

Another thing to note is that this will be a backend only project as that is what I am interested in and it would also simply be way too much work
for one person to do all of this anyway. You are definitely welcome to provide your own UIs for this project as I will be trying to document
everything thoroughly.

## Some Technical notes

If you want to see the executed SQL queries in the logs you can change `"Microsoft": "Warning",` to `Information` in the `appsettings.Development.json`
file.

I have written a few powershell scripts for `dotnet ef` so I don't have to add the paths manually every time I run the commands

- Run
- ListMigrations
- AddMigration [MIGRATION NAME]
- PushMigrations