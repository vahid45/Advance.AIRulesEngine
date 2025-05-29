# API Usage Guide

## Overview

The Rules Engine API provides endpoints for managing and evaluating business rules. The API is documented using Swagger UI, which can be accessed at `https://localhost:7011/swagger`.

## Authentication

The API uses JWT Bearer token authentication. Include the token in the Authorization header:

```http
Authorization: Bearer your-token-here
```

## Endpoints

### Rules Management

#### Get All Rules
```http
GET /api/rules?entityName={entityName}
```
- Optional query parameter: `entityName` to filter rules by entity
- Returns: List of rules

#### Get Rule by ID
```http
GET /api/rules/{id}
```
- Returns: Single rule or 404 if not found

#### Create Rule
```http
POST /api/rules
```
Request body:
```json
{
  "name": "Rule Name",
  "description": "Rule Description",
  "entityName": "EntityName",
  "rootCondition": {
    "fieldName": "FieldName",
    "operator": "EQUALS",
    "fieldType": "STRING",
    "value": "Value"
  }
}
```

#### Update Rule
```http
PUT /api/rules/{id}
```
- Request body: Same as Create Rule
- Returns: 204 No Content on success

#### Delete Rule
```http
DELETE /api/rules/{id}
```
- Returns: 204 No Content on success

#### Evaluate Rule
```http
POST /api/rules/{id}/evaluate
```
Request body:
```json
{
  "field1": "value1",
  "field2": "value2"
}
```
Returns:
```json
{
  "isSuccess": true,
  "message": "Rule evaluation result",
  "executedActions": []
}
```

## Error Handling

The API uses standard HTTP status codes:

- 200: Success
- 201: Created
- 204: No Content
- 400: Bad Request
- 401: Unauthorized
- 403: Forbidden
- 404: Not Found
- 500: Internal Server Error

Error responses include a message:
```json
{
  "message": "Error description"
}
```

## Examples

### Creating a Rule

```bash
curl -X POST "https://localhost:7011/api/rules" \
     -H "Authorization: Bearer your-token" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "High Value Account Rule",
       "description": "Rule for high-value accounts",
       "entityName": "Account",
       "rootCondition": {
         "fieldName": "Revenue",
         "operator": "GREATER_THAN",
         "fieldType": "MONEY",
         "value": 1000000
       }
     }'
```

### Evaluating a Rule

```bash
curl -X POST "https://localhost:7011/api/rules/{id}/evaluate" \
     -H "Authorization: Bearer your-token" \
     -H "Content-Type: application/json" \
     -d '{
       "Revenue": 1500000,
       "AccountType": "Enterprise"
     }'
```

## Best Practices

1. **Error Handling**
   - Always check response status codes
   - Handle errors gracefully
   - Implement retry logic for transient failures

2. **Performance**
   - Use appropriate HTTP methods
   - Minimize payload size
   - Cache responses when possible

3. **Security**
   - Always use HTTPS
   - Keep tokens secure
   - Validate input data

4. **Monitoring**
   - Use the health check endpoint: `GET /health`
   - Monitor API response times
   - Log important events

## Rate Limiting

The API implements rate limiting to ensure fair usage:
- 100 requests per minute per client
- 1000 requests per hour per client

## Support

For API support:
- Check the [Swagger Documentation](https://localhost:7011/swagger)
- Review the [Installation Guide](installation.md)
- Contact support at support@example.com 