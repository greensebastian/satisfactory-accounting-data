# Satisfactory Accounting

This is a web app for tracking and planning satisfactory factories.

It consists of two parts:

* An api for uploading, processing and storing game information.
* A client for planning and recording your factories.

[![Build and deploy API to Azure Functions](https://github.com/greensebastian/satisfactory-accounting-data/actions/workflows/azure-functions-api.yml/badge.svg?branch=master)](https://github.com/greensebastian/satisfactory-accounting-data/actions/workflows/azure-functions-api.yml)

[![Build and deploy Client to Azure Web Apps](https://github.com/greensebastian/satisfactory-accounting-data/actions/workflows/azure-webapps-client.yml/badge.svg?branch=master)](https://github.com/greensebastian/satisfactory-accounting-data/actions/workflows/azure-webapps-client.yml)

## Client

https://satisfactory-accounting.azurewebsites.net/

The plan here is to make and plan reusable factory groups, to calculate as well as track efficiencies and resource requirements for various production lines.

If you've ideas for what to put here, please let me know!

## API

There is only one endpoint which doesn't require auth, and it retrieves the json docs included with the game installation, only sliced a little differently.

### Swagger

The ui is slow, as it freezes when trying to render the large output json.

https://satisfactory-accounting-data.azurewebsites.net/api/swagger/ui

### Get

Retrieve the current model.

https://satisfactory-accounting-data.azurewebsites.net/api/get

### Update
Computes and updates the stored model from the Docs.json file found in the installation files of Satisfactory.

Requires a valid `X-Key: <key>` header.

https://satisfactory-accounting-data.azurewebsites.net/api/update
