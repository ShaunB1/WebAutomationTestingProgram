# API Documentation

## Table of Contents
1. [Test Controller](#test-controller)
2. [Environments Controller](#environments-controller)
3. [Auth Controller](#auth-controller)
4. [Task Controller](#task-controller)

# Test Controller

## Base URL `/api/test`

#### **POST** `/run`
- **Description**: Runs an automated test
- **Authentication**: Requires a valid `Authorization` token
- **Request Body**:
    - **Content-Type**: `multipart/form-data`
    - **Form Parameters**:

| Parameter | Type   | Required | Description                      |
|-----------|--------|----------|----------------------------------|
| `file`    | `file`  | Yes       |  The test file to run. Only `.xlsx` accepted |
| `env`    | `string`  | Yes       |  The environment to run the test (e.g. `EDCS-9`) |
| `browser`    | `string`  | Yes       | The browser to execute the test in (e.g. `chrome`) |
| `browserVersion`    |  |  |  |

# Environments Controller

## Base URL `/api/environments`

#### **GET** `/keychainAccounts`
- **Description**: Gets a list of test account emails
- **Authentication**: Requires a valid `Authorization` token

#### **GET** `/secretKey`
- **Description**: Fetches the secret key (password) of specified email from Azure Key Vault
- **Authentication**: Requires a valid `Authorization` token
- **Query Parameters**:

| Parameter | Type   | Required | Description                      |
|-----------|--------|----------|----------------------------------|
| `email`    | `string`  | Yes       |  Email to fetch secret key for |

#### **POST** `/resetPassword`
- **Description**: Resets the password for specified email in OPS BPS and Azure Key Vault
- **Authentication**: Requires a valid `Authorization` token
- **Request Body**:
    - **Content-Type**: `application/json`
    
```json
"example@ontarioemail.ca"
```

# Auth Controller

## Base URL `/api/auth`

#### **GET** `/validateToken`
- **Description**: Validates the access token in the `Authorization` header
- **Authentication**: Requires a valid `Authorization` token
- **Response**: Returns 200 if valid token and 401 otherwise

#### **GET** `/getAccountInfo`
- **Description**: Gets the account information of the user
- **Authentication**: Requires a valid `Authorization` token
- **Response**: Returns `name` and `email` of the user

# Task Controller
