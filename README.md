# BigData + Desktop Application Programming Project - Warehouse Management System

## I. IDE Configuration & Setup (.NET 10.0)

### I.1. Project Creation

**Visual Studio 2026 UI Setup**

* Open Visual Studio 2026 and select **Create a new project**
* Type **WPF** in the search bar
* Select **WPF Application** (C#, XAML, Windows, Desktop). Do not select the option with "(.NET Framework)" in the title.
* Click **Next**, enter your project name (`Warehouse`), and choose your location
* Click **Next** to open the "Additional information" window
* Select **.NET 10.0 (Long Term Support)** from the Framework dropdown
* Click **Create**

### I.2. Library Dependencies (NuGet Packages)

**Visual Studio 2026 UI Setup**

* Right-click on your project name (`Warehouse`) in the **Solution Explorer** panel on the right
* Select **Manage NuGet Packages...** from the context menu
* Navigate to the **Browse** tab
* Search for and install the following three packages (ensure you select the latest stable versions):
  * `MongoDB.Driver`
  * `CommunityToolkit.Mvvm`
  * `Microsoft.Extensions.DependencyInjection`
  * `FluentValidation.DependencyInjectionExtensions`

### I.3. In case of error: `"This solution contains packages with vulnerabilities..."

E.g. Dependencies >  Packages > MongoDB.Driver > `SharpCompress` (Warning icon)

* Go to `Manage NuGet Packages`
* Click on the **Installed** tab.
* Find the package with the vulnerability warning (e.g. `SharpCompress`).
* Select the `Warehouse` project in the right panel.
* Choose the latest stable version and click **Install**

## II. Project Overview

### II.1. Desktop Application

**Parameter**       | **Value**
---                 | ---
**IDE**             | Visual Studio 2026
**Name**            | Warehouse
**Type**            | WPF App (.Net)
**Language**        | C#
**Framework**       | .NET 10.0 (LTS)

### II.2. Installed Packages and Versions

**Package Name**                                | **Version**
:---                                            | :---
CommunityToolkit.Mvvm                           | 8.4.2
Microsoft.Extensions.DependencyInjection        | 10.0.8
MongoDB.Driver                                  | 3.8.1
SharpCompress                                   | 0.48.1
FluentValidation.DependencyInjectionExtensions  | 12.1.1

### II.3. Big Data

**Parameter**         | **Value**
:---                  | :---
**Engine**            | MongoDB
**Database Name**     | WarehouseDB
**Collection Names**  | Category, Product, InventoryTransaction
**Server**            | localhost
**Port**              | 27017


## III. Features

### III.1. UseCases

**Product Management** | **Product Presenter**      | **Inventory Tracking**
:---                   | :---                       | :---
Add new product        | Search products            | Log stock in/out
Delete product         | View product list          | Generate movement reports
Edit product details   | Generate inventory status  | Aggregate historical data


## IV. Code Structure

### IV.1. Folder and File Organization

```bash
Warehouse/
├── Models/
│   ├── Category.cs
│   ├── InventoryTransaction.cs
│   ├── Product.cs
│   ├── Price.cs
│   ├── ProductTag.cs
│   └── Types/
│       ├── Currency.cs
│       └── Unit.cs
├── Services/
│   ├── Infrastructure/
│   │   ├── IMongoService.cs
│   │   └── MongoService.cs
│   └── Application/
│       ├── ProductService.cs
│       ├── InventoryService.cs
│       └── ReportService.cs
├── Validators/
│   ├── ProductValidator.cs
│   └── TransactionValidator.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   └── ProductDetailsViewModel.cs
├── UserControls/
│   ├── SearchBarControl.xaml
│   ├── ProductListControl.xaml
│   └── PaginationControl.xaml
├── Views/
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ProductDetailsWindow.xaml
│   └── ProductDetailsWindow.xaml.cs
├── App.xaml
└── App.xaml.cs
```

**Crucial Note:** Because `MainWindow.xaml` has been moved from the root directory to the `Views/` folder for better architecture, the `StartupUri` property inside `App.xaml` (or the startup initialization logic inside `App.xaml.cs`) must be manually updated to point to the new path (`Views/MainWindow.xaml`) to prevent application crash on startup.

### IV.1. UML Class Diagram

```mermaid
classDiagram
  direction TB

```

## V. Database Structure & Big Data Patterns

### V.1. Collections Overview & Single Source of Truth

**Collection Name**       | **Description**
:---                      | :---
**Category**              | Stores the hierarchical category tree using Parent References
**Product**               | Stores POCO product models. The `Quantity` field acts strictly as a **Materialized View / Cache** for rapid UI presentation
**InventoryTransaction**  | An immutable append-only ledger. This is the **Single Source of Truth** for stock levels |


### V.2. Document Schemas

**Category Collection**

```json
{
  "_id": "65f1a2b3c4d5e6f7a8b9c001",
  "Name": "Cleaning and Household",
  "Code": "CLEAN-01",
  "ParentCategoryId": null
}

```

**Product Collection**

```json
{
  "_id": "65f1a2b3c4d5e6f7a8b9c100",
  "Name": "Bosque Verde Laundry Detergent",
  "CategoryId": "65f1a2b3c4d5e6f7a8b9c002",
  "Quantity": 150,
  "Price": {
    "Amount": 3.40,
    "Currency": "EUR"
  },
  "Unit": "Piece",
  "Tags": [
    "NaturalSoap",
    "WhiteOnly"
  ]
}

```

**InventoryTransaction Collection**

```json
{
  "_id": "65f1a2b3c4d5e6f7a8b9c999",
  "ProductId": "65f1a2b3c4d5e6f7a8b9c100",
  "TransactionType": "IN",
  "QuantityChanged": 150,
  "Timestamp": "2026-05-19T10:00:00Z",
  "UserId": "admin_1",
  "Metadata": {
    "Source": "Supplier_A",
    "BatchNumber": "B-7781"
  }
}

```

### V.3. MongoDB Indexes

Indexes are mandatory to ensure the application scales effectively under Big Data conditions

**Collection**            | **Index Type**  | **Target Fields**
:---                      | :---            | :---
**Product**               | Text Index      | `Name`
**Product**               | Single Field    | `CategoryId`
**Product**               | Multikey Index  | `Tags`
**InventoryTransaction**  | Compound Index  | `ProductId`, `Timestamp`

### V.4. MongoDB Aggregation Pipelines

Complex data analysis is offloaded to the database engine via the Aggregation Framework

* **Stock Trends:** Groups `InventoryTransaction` records by `$dateToString` (day/month) and sums `QuantityChanged` to visualize daily movement
* **Top Movers:** Uses `$group` and `$sum` on `ProductId` within a specific date range, sorted by volume, followed by a `$lookup` to fetch product names
* **Materialized View Reconciliation:** Recalculates total actual stock purely from the `InventoryTransaction` ledger and updates the cached `Quantity` field in the `Product` collection


## VI. Architectural Decisions

1. **Immutable Ledger Data Model:** Financial and inventory integrity is achieved by making `InventoryTransaction` append-only
2. **UI Virtualization & Async-Everywhere:** The UI thread is never blocked. `VirtualizingStackPanel` handles massive data sets on the frontend, while `async/await` paired with MongoDB `Skip()` and `Limit()` handles memory efficiently on the backend
3. **Application Services:** Business logic is consolidated into cohesive domain services (`ProductService`, `InventoryService`) instead of highly fragmented UseCases
4. **Validation Layer:** Input integrity is enforced using `FluentValidation` prior to database execution

## VI. User Interface (UI) Architecture

The User Interface strictly follows a modular, component-based design. Instead of building monolithic windows, independent `UserControls` are created and assembled inside the main Views to maximize reusability

### VI.1 **Main Window (Composed View)**

* **SearchBarControl:** An independent component handling text input for product/category/tag searches
* **ProductListControl:** Displays products using UI Virtualization (`VirtualizingStackPanel`) to handle large datasets efficiently without memory leaks
* **PaginationControl:** Component containing Next/Previous buttons to navigate through database chunks utilizing `Skip()` and `Limit()`
* **Action Panel:** Contains buttons to open the "Generate Report" dialog and the "Product Management" routing

### VI.2 **Product Details Window**

* **Data Presentation:** Displays full detailed data fetched from the database for the selected item
* **Action Buttons:** Triggers for "Add Product", "Edit Product", and "Delete Product"

### VI.3 **Add/Edit Product Window**

* **Product ID:** EAN code from the product
* **Product Name:** TextBox for entering the product name
* **Category:** ComboBox for selecting the product category
* **Subcategory:** ComboBox dynamically populated based on the selected category
* **Price / Currency:** Text input and dropdown (Default: PLN)
* **Quantity:** Initial quantity injection (creates an automatic "IN" transaction upon creation)
* **Tags:** Dynamic CheckBoxes for selecting applicable tags based on the product type

## V. Building of Database (MongoDB)

### V.1. Create the `WarehouseDB` 

```javascript
use WarehouseDB
```

### V.2. Create Collections

```javascript
db.createCollection("Category")
db.createCollection("Product")
```

### V.3. Create Category

```javascript
db.Category.insertMany([
  { Id: "sau_01", Group: "Olej, przyprawy i sosy", Name: "Olej, ocet i sól" },
  { Id: "sau_02", Group: "Olej, przyprawy i sosy", Name: "Przyprawy" },
  { Id: "sau_03", Group: "Olej, przyprawy i sosy", Name: "Majonez, keczup i musztarda" },
  { Id: "sau_04", Group: "Olej, przyprawy i sosy", Name: "Inne sosy" },

  { Id: "dri_01", Group: "Woda i napoje bezalkoholowe", Name: "Woda" },
  { Id: "dri_02", Group: "Woda i napoje bezalkoholowe", Name: "Izotoniczny i energetyczny" },
  ...
])
```

