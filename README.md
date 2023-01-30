# Overview

Inspired by David Fowler using the async apis to create a trivia game
this repository contains some sample code for doing something similar using MassTransit.

It's not at all production ready but gives an example of how this might look.

# Usage

Run the server:

dotnet run --project server/src/trivia-server.csproj

Run the client in python:

add 127.0.0.1 triviagame.com to your hosts file

cd ./client
python -m http.server 50001
open browser and navigate to triviagame.com:50001/client.html

Wait 30 seconds for the first question to arrive
