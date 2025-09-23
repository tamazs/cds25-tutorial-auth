# CDS.Security 2025 - Auth

<!--toc:start-->
- [CDS.Security 2025 - Auth](#cdssecurity-2025-auth)
  - [Introduction](#introduction)
  - [Getting started](#getting-started)
    - [Database](#database)
    - [Client](#client)
    - [Server](#server)
  - [How to use](#how-to-use)
    - [Web servers](#web-servers)
    - [Database](#database)
    - [Users](#users)
  - [Exercises](#exercises)
<!--toc:end-->

## Introduction

This repository contains a skeleton application and a series of guides on how
to manually implement authentication, session management and authorization with
ASP.NET as backend and React as frontend.

The skeleton application is a simple blog with posts and comments.

In real-world systems, I recommend using [ASP.NET
Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-9.0)
instead of implementing authentication and sessions manually.
However, it is important that you know how this stuff works, and I believe in
learning by coding.

## Getting started

Fork the repository and clone it to your computer.

### Database

There is a script named `setup.sh` and you simply run the script.

```sh
./setup.sh <some-password>
```

Replace `<some-password>` with what you want to use as password for the
preconfigured users.

In case you forget the password, or just want to reset, simply run the script
again.

### Client

Before first run, you need to install dependencies with:

```sh
npm ci --prefix client
```

Then start development web server with:

```sh
npm run dev --prefix client
```

The `--prefix client` tells npm to behave as if you were running it from
`client/` folder.
That way do don't have to `cd` around all the time.

### Server

```sh
dotnet watch --project server/Api
```

The `watch` part makes it hot-reload when you change the code. Not all changes
works with hot-reloading, in which case you need to manually restart it with
<kbd>CTRL</kbd>+<kbd>R</kbd>.

## How to use

### Web servers

| Sub-systems | URL |
| - | - |
| Client / frontend | <http://localhost:5173/> |
| Server / backend | <http://localhost:5153/scalar/> |

_The project uses [Scalar](https://scalar.com/), which is an alternative to
[Swagger UI](https://swagger.io/tools/swagger-ui/)._

### Database

| Parameter         | Value                                                           |
| ----------------- | --------------------------------------------------------------- |
| URL               | jdbc:postgresql://localhost:5432/postgres                       |
| Username          | postgres                                                        |
| Password          | mysecret                                                        |
| Connection string | HOST=localhost;DB=postgres;UID=postgres;PWD=mysecret;PORT=5432; |

### Users

The application ships with some test data, including blog posts and a couple of
users.
After implementing authentication, you will be able to log-in using these credentials:

| Email / Username | Role   |
|-------------------------| - |
| <admin@example.com> |  Admin  |
| <editor@example.com> |  Editor |
| <othereditor@example.com> |  Editor |
| <reader@example.com> |  Reader |

## Exercises

Complete these exercises in order.
Since each builds on the previous.
Make a commit each time you complete an exercise.

0. [Authentication](tutorials/00_authentication.md)
1. [Sessions](/tutorials/01_session.md)
2. [Authorization](tutorials/02_authorization.md)
