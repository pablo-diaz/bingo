# Bingo game with Domain Driven Design

## Purposes

- My main purpose with this game is to join families and friends around a friendly Bingo game, that can be accessed via both Mobile phones as well as regular PCs
- Secondary purposes were:
  - To excercise my DDD skills with a Bingo game
  - To play with WebSockets communication via SignalR
  - To play with Blazor

## Deployment specs

When deploying this to any IIS-with-dotnet-core-3.1 enabled http service, please make sure to:
- Set the right password for the Admin
- Set the right *webroot* folder
  - Leave this empty, if you are publishing to an Azure WebSite
  - If you are publishing to an IIS WebSite, under a specific WebApplication, then set this to that WebApplication's name
