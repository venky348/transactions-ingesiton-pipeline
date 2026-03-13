# Transaction Ingestion Pipeline

## Overview

This project implements a reliable transaction ingestion pipeline for a retail payments platform.
The system processes hourly snapshots of transactions from multiple point-of-sale systems and reconciles them with the database.

The pipeline supports:

- Transaction upserts
- Change detection and audit tracking
- Revocation of missing transactions
- Finalization of transactions older than 24 hours
- Idempotent execution
- Automated tests

---

## Architecture

The application is a .NET 10 Console application built using Entity Framework Core with SQLite.

Core components:

**Models**

- `Transaction` – main transaction record
- `TransactionAudit` – tracks field-level changes

**Services**

- `TransactionSnapshotService` – loads snapshot data (mock JSON feed)
- `TransactionReconciliationService` – performs ingestion logic:
  - Upsert transactions
  - Detect field changes
  - Write audit entries
  - Handle revocations
  - Finalize older transactions

**Data**

- `AppDbContext` – EF Core database context

**Tests**

- Unit tests implemented using xUnit and EF Core InMemory provider.

---

## Data Flow

1. Application starts
2. Database is initialized
3. Snapshot JSON is loaded
4. Transactions are reconciled:
   - Insert new transactions
   - Detect updates
   - Record audit entries
   - Revoke missing transactions
   - Finalize transactions older than 24 hours
5. Results are persisted in SQLite

---

## Running the Application

From the repository root:

```
cd src/TransactionIngestion
dotnet run
```

Example output:

```
Database initialized.
Loaded 2 transactions from snapshot.
Snapshot processed successfully.
```

---

## Running Tests

From the repository root:

```
dotnet test
```

Example output:

```
Passed! 4 tests
```

---

## Configuration

Configuration is stored in `appsettings.json`.

Example:

```
{
  "Database": {
    "ConnectionString": "Data Source=transactions.db"
  },
  "MockApi": {
    "SnapshotFile": "mock_transactions.json"
  }
}
```

---

## Assumptions

- Snapshot contains transactions from the last 24 hours.
- TransactionId is the stable unique identifier.
- Card numbers are stored only as the last 4 digits for privacy.
- Transactions older than 24 hours are finalized and not modified.

---

## Idempotency

The ingestion job is idempotent because:

- Transactions are uniquely identified by `TransactionId`
- Updates are only recorded when values change
- Revoked transactions are not revoked again
- Finalized transactions are immutable

---

## Estimated vs Actual Time

Estimated time: `6 hours`

Actual time spent: `7–8 hours`

Breakdown:

- Project setup and EF Core configuration
- Snapshot ingestion logic
- Reconciliation and audit tracking
- Revocation and finalization logic
- Unit tests
- Documentation

---

## Highlights

- Clean service-based architecture
- Full audit trail for transaction changes
- Idempotent ingestion pipeline
- Automated unit tests
- Configurable snapshot source
- SQLite-based persistence using EF Core
