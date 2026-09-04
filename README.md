# Tazora

Tazora is a modern grocery shopping and rapid-delivery application built with **.NET MAUI** and **SQLite**. It demonstrates a complete mobile shopping flow with reusable UI components, persistent basket management, discounts, orders, statistics, authentication, and AI-assisted recipe suggestions.

The project was developed as a .NET MAUI case study and extended beyond the original requirements with additional user-facing features.

## Features

### Shopping experience

- Browse products by category
- Search and filter categories and products
- View product details, prices, units, descriptions, and images
- Discover popular and discounted products
- Add products to the basket from multiple screens
- Increase, decrease, or remove basket items
- Persist basket contents locally with SQLite
- Calculate discounts, delivery fees, and basket totals dynamically
- Display empty states and loading indicators

### Orders and account

- Local user registration and login
- Password hashing with PBKDF2, SHA-256, and a unique salt
- Create orders from the current basket in a database transaction
- View previous orders and order details
- Display user-specific order statistics
- Log out and clear the active application session

### Additional features

- AI recipe assistant that matches recipe ingredients with products in the local catalog
- Add AI-suggested products directly to the basket
- Spin wheel with demo discount rewards and coupon codes
- Dynamic statistics for products, categories, discounts, basket totals, and orders
- Reusable four-tab bottom navigation with active-tab highlighting
- Responsive cards, custom fonts, icons, and consistent visual styling

## Technology Stack

- **.NET 9**
- **.NET MAUI**
- **C#**
- **XAML**
- **SQLite**
- **sqlite-net-pcl**
- **Microsoft.Extensions.DependencyInjection**
- **Material Symbols**
- **Inter font**
- **Gemini API integration**

## Application Pages

- Login
- Register
- Home
- Categories
- Product List
- Product Detail
- Discounts
- Basket
- Orders
- Order Detail
- Profile
- Statistics
- Spin Wheel

## Data Model

Tazora stores its application data locally in SQLite.

| Model | Purpose |
|---|---|
| `Category` | Product categories and their display information |
| `Product` | Product details, prices, images, units, and category relationship |
| `Discount` | Active product and campaign discounts |
| `BasketItem` | Product quantities stored in the basket |
| `User` | Locally registered users and password hashes |
| `CustomerOrder` | Order totals, delivery fee, discount amount, status, and user relationship |
| `OrderItem` | Product snapshots belonging to an order |

All database access is centralized in `DatabaseService`. The service handles initialization, seed data, product queries, authentication, basket operations, discounts, and transactional order creation.

## Project Structure

```text
Tazora/
├── Controls/        # Reusable navigation and AI chat controls
├── Data/            # Local data-related resources
├── Helpers/         # Icon constants and shared helpers
├── Models/          # SQLite entities and application models
├── Pages/           # XAML pages and code-behind
├── Resources/
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   ├── Raw/
│   ├── Splash/
│   └── Styles/
├── Services/        # Database, session, and AI services
└── ViewModels/      # UI-specific display models
```

## Core Workflows

### Basket management

When the same product is added more than once, Tazora updates the existing basket row instead of creating duplicates. Reducing an item's quantity to zero removes it from the basket.

### Checkout

Checkout is executed inside a SQLite transaction:

1. Basket items and active products are loaded.
2. Product discounts are calculated.
3. Delivery fees and order totals are calculated.
4. The order and its order items are inserted.
5. The basket is cleared only after the order is created successfully.

### Authentication

Passwords are never stored as plain text. Tazora derives password hashes using PBKDF2 with SHA-256, a per-user random salt, and a configurable iteration count.

### AI recipe assistant

The assistant sends the user's recipe request together with a simplified local product catalog to the configured AI service. The response contains a recipe message and matching product IDs. Only IDs that exist in the local catalog are converted into product suggestions.

> [!IMPORTANT]
> The repository does not contain an API key. Keep credentials out of source control. For a production mobile application, call the AI provider through a protected backend rather than embedding a permanent secret in the app package.

## Getting Started

### Prerequisites

- Visual Studio 2022 with the **.NET Multi-platform App UI development** workload
- .NET 9 SDK
- An Android emulator, Android device, iOS/macOS environment, or Windows target supported by .NET MAUI

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/ismailbarankarasu/Tazora.git
   ```

2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Select a supported target such as Windows or Android.
5. Build and run the application.

The local SQLite database is initialized by the application and populated with sample catalog data when required.

## Case Study Requirements

The project covers the primary case-study requirements:

- Classic .NET MAUI project structure
- XAML-based responsive interface
- Custom font and icon integration
- Centralized SQLite service
- Dynamic categories, products, discounts, and statistics
- Two-column product layout
- Persistent basket management
- Parameterized navigation
- Four-section bottom navigation
- Real product and campaign images
- Search, live quantity changes, and empty states

It also extends the case with authentication, transactional checkout, order history, AI-assisted recipes, and a promotional spin wheel.

## Current Scope

Tazora is a portfolio and case-study project. Payment methods, addresses, coupon redemption, and production delivery tracking are currently represented as demo or UI-level features.

## Author

**İsmail Baran Karasu**

- [GitHub](https://github.com/ismailbarankarasu)
- [LinkedIn](https://www.linkedin.com/in/ismail-baran-karasu/)
