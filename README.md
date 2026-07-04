# LienHub County-Held Certificate Scraper

## Overview

This project is a C# Selenium automation tool that logs into LienHub, navigates through every supported Florida county's **County-Held Certificates** page, extracts certificate information, and stores the results in a Microsoft SQL Server database.

The scraper is designed to run unattended, collecting the latest available data across multiple counties in a single execution while handling pagination automatically.

---

## Features

* Automated login to LienHub
* Visits 30+ Florida county certificate pages
* Automatically detects counties with available records
* Changes page size to display **100 records per page**
* Navigates through every page until the last record
* Extracts certificate information into strongly typed objects
* Saves all collected records into SQL Server
* Clears previous data before importing fresh records
* Handles counties with no available certificates
* Basic exception handling to skip malformed rows without stopping execution

---

## Technologies Used

* **C# (.NET)**
* **Selenium WebDriver**
* **ChromeDriver**
* **Microsoft SQL Server**
* **ADO.NET (SqlClient)**

---

## Data Collected

For every certificate the scraper collects:

| Field           | Description                    |
| --------------- | ------------------------------ |
| County Name     | County being scraped           |
| Tax Sale URL    | Direct certificate link        |
| Account         | Parcel/Account number          |
| Tax Year        | Tax year                       |
| Certificate     | Certificate number             |
| Issued Date     | Date issued                    |
| Expiration Date | Certificate expiration         |
| Purchase Amount | Purchase amount                |
| Date Captured   | Timestamp of scraper execution |

---

## How It Works

### 1. Login

The scraper launches Chrome using Selenium and authenticates using the supplied LienHub credentials.

---

### 2. Visit Each County

A predefined list of county URLs is maintained within the application.

The scraper iterates through each county and navigates directly to its County-Held Certificates page.

---

### 3. Detect Available Records

Before scraping begins, the application checks whether the page contains the DataTables page-size selector.

If it is missing, that county is skipped automatically.

This avoids unnecessary errors when a county has no available certificate listings.

---

### 4. Pagination

For counties containing records:

* Page size is changed to **100**
* Every page is scraped
* The scraper clicks the **Next** button until it becomes disabled
* Records from every page are merged into a single in-memory collection

---

### 5. Data Extraction

Each table row is converted into a `Record` object.

The scraper extracts:

* Account Number
* Tax Year
* Certificate Number
* Certificate URL
* Issued Date
* Expiration Date
* Purchase Amount
* County Name
* Capture Timestamp

Rows that cannot be parsed are skipped so that the scraper continues processing the remaining records.

---

### 6. Database Import

Once all counties have been processed:

1. Connect to SQL Server
2. Truncate the destination table
3. Insert every collected record
4. Report the total number of imported records

This guarantees that the database always reflects the latest scrape.

---

## Project Structure

```text
Program.cs
│
├── Main()
│   ├── Login()
│   ├── Visit every county
│   ├── Pagination()
│   │     └── Scrape()
│   ├── Save()
│   └── Logout()
│
└── Record.cs
    └── Data model representing a certificate
```

---

## Design Decisions

### In-Memory Collection

Instead of inserting records into the database page-by-page, all records are collected into a single list first.

This approach:

* Reduces database connections
* Keeps scraping independent from storage
* Makes validation easier before saving

---

### Automatic Pagination

Rather than assuming a fixed number of pages, the scraper checks the **Next** button's `aria-disabled` attribute to determine when the last page has been reached.

This makes the scraper resilient to changing record counts.

---

### CSS Selectors

The scraper primarily relies on CSS selectors because they are:

* Faster
* Easier to maintain
* More readable than XPath

---

### Fault Tolerance

Individual row parsing is wrapped in exception handling.

If one record contains unexpected data, the scraper skips that row instead of terminating the entire run.

---

## Future Improvements

Potential enhancements include:

* Explicit waits instead of `Thread.Sleep()`
* Bulk SQL inserts using `SqlBulkCopy`
* Configuration via `appsettings.json`
* Parallel county processing
* Logging framework (Serilog/NLog)
* Automatic retry for transient failures
* Export to CSV or Excel
* Scheduled execution using Windows Task Scheduler or SQL Agent

---

## Disclaimer

This project is intended for educational and automation purposes. Ensure you have authorization to access and automate interactions with the target website and comply with its Terms of Service.

---

## Author

Developed using **C#**, **Selenium WebDriver**, and **Microsoft SQL Server** to automate county-held certificate collection from LienHub.
