# Test Credentials & Quick Start Guide

## Pre-seeded Test Accounts

### Admin Account
```
Username: admin
Password: admin123
Email: admin@loanmanagement.com
Role: Admin
```

### Regular User Account
```
Username: testuser
Password: user123
Email: user@loanmanagement.com
Role: User
```

## Quick Testing Steps

### 1. Start the Application

```bash
# Backend + Database
docker-compose up -d

# Frontend (in another terminal)
cd frontend
npm install
ng serve
```

### 2. Test Authentication (Swagger)

1. Open http://localhost:5000/swagger
2. Click **Authorize** button (top right with lock icon)
3. Use this workflow:
   - **POST /api/auth/login** with admin credentials
   - Copy the `token` from response
   - Click **Authorize** button
   - Enter: `Bearer <your-token-here>`
   - Click **Authorize**, then **Close**

### 3. Test Protected Endpoints

Now you can test:
- **POST /api/loans** - Create a new loan (requires auth)
- **POST /api/loans/{id}/payment** - Make payment (requires auth)

### 4. Test Public Endpoints

No authentication needed:
- **GET /api/loans** - List all loans
- **GET /api/loans/{id}** - Get specific loan

## Example cURL Commands

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

### Create Loan (with auth)
```bash
curl -X POST http://localhost:5000/api/loans \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "amount": 20000,
    "applicantName": "Test User"
  }'
```

### Make Payment (with auth)
```bash
curl -X POST http://localhost:5000/api/loans/1/payment \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "amount": 1000
  }'
```

## Viewing Logs

### Console Logs
```bash
docker-compose logs -f backend
```

### File Logs
```bash
# Logs are saved in backend/logs/ directory
ls backend/logs/
cat backend/logs/loan-api-$(date +%Y%m%d).log
```

## Sample Log Output

```
[22:15:30 INF] Starting Loan Management API
[22:15:31 INF] Applying database migrations...
[22:15:32 INF] Database migrations applied successfully
[22:15:33 INF] Application started successfully
[22:15:40 INF] Login attempt for user admin
[22:15:40 INF] User admin logged in successfully
[22:15:45 INF] Creating new loan for Test User with amount 20000
[22:15:45 INF] Successfully created loan 6 for Test User with amount 20000
```

## Testing Checklist

- [ ] Login with admin account
- [ ] Create a new loan (authenticated)
- [ ] View all loans (public)
- [ ] Make a payment (authenticated)
- [ ] Register a new user
- [ ] Login with new user
- [ ] View logs in console
- [ ] Check log file in backend/logs/
- [ ] Test with frontend at http://localhost:4200
- [ ] Try accessing protected endpoint without token (should get 401)

## Troubleshooting

**401 Unauthorized Error:**
- Make sure you're logged in and have a valid token
- Check that Authorization header is: `Bearer <token>`
- Token expires after 24 hours

**Cannot login:**
- Verify database migrations ran successfully
- Check docker-compose logs: `docker-compose logs backend`
- Ensure credentials are correct (case-sensitive)

**Logs not appearing:**
- Check backend/logs/ directory exists
- Ensure write permissions on logs directory
- View console logs: `docker-compose logs -f backend`
