# satisfactory-accounting-data

This is a web api for transforming and storing the community docs for Satisfactory.

## Live site

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

## Deployment
```
func azure functionapp publish satisfactory-accounting-data
```