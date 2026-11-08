# Ewallet

## API Endpoints

| Endpoint Path                  | HTTP Method | Description                        | Available |
| ------------------------------ | ----------- | ---------------------------------- | --------- |
| `/api/users`                   | GET         | Get all users                      |           |
| `/api/users/{id}`              | GET         | Get user detail by ID              |           |
| `/api/users`                   | POST        | Create user and initialize wallet  |           |
| `/api/users/{id}`              | PUT         | Update user profile                |           |
| `/api/wallets/{userId}`        | GET         | Get wallet information by User ID  |           |
| `/api/wallets/{userId}/topups` | POST        | Top up wallet balance              |           |
| `/api/transfers`               | POST        | Transfer balance to another wallet |           |
| `/api/users/{id}/transactions` | GET         | Get user transaction history       |           |
