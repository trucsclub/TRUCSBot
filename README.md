# TRU CS Club Bot

![Build Status](https://github.com/trucsclub/TRUCSBot/workflows/Build%20Status/badge.svg)

This is the official Discord bot for the TRUSU Computing Science Club Discord channel, which you can
access [here](https://trucsclub.ca/discord).

It uses the [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) library to interact with Discord. It also
uses [IGDB .NET SDK](https://github.com/kamranayub/igdb-dotnet) for interacting with [IGDB.com](https://www.igdb.com/)
for our Game Night recommendations.

## Building

Build it like any other .NET Core project :)

## Running

You need to edit your settings file with your own Discord API key.

### Getting a Discord API Key

To get a Discord API key, you need to create an application
on [Discord's Developer Portal](https://discord.com/developers/applications).

### Getting the bot to join your test server

On the app page on Discord's Developer Portal, copy your bots client ID. Then go
to ``https://discordapp.com/oauth2/authorize?client_id=YOUR_CLIENT_ID&scope=bot&permissions=0``,
where ``YOUR_CLIENT_ID`` is your client ID. Then, select the server you want to join and press authorize.

## Pushing to our server (Club only)

Run publish.bat and copy those files over
