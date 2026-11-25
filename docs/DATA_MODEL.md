# Modelo de Dados — Marketplace Reconciliation API

![Diagrama ER](./data_model.png)

<details>
<summary>Ver código Mermaid (erDiagram)</summary>

```mermaid
erDiagram
    contracts ||--o{ orders : "governs"
    contracts ||--o{ payout_batches : "receives"
    payout_batches ||--|{ payout_staging : "contains"
    orders |o--o{ discrepancies : "referenced_in"
    payout_staging |o--o{ discrepancies : "flagged_in"

    contracts {
        serial id PK
        varchar marketplace_name UK "NOT NULL"
        numeric commission_rate "NOT NULL"
        int payout_days "NOT NULL"
        timestamptz created_at "NOT NULL"
    }

    orders {
        serial id PK
        int contract_id FK "NOT NULL"
        varchar order_code UK "NOT NULL"
        date order_date "NOT NULL"
        numeric gross_amount "NOT NULL"
        timestamptz created_at "NOT NULL"
    }

    payout_batches {
        serial id PK
        int contract_id FK "NOT NULL"
        varchar file_name "NOT NULL"
        varchar file_hash UK "NOT NULL"
        varchar status "NOT NULL"
        timestamptz created_at "NOT NULL"
    }

    payout_staging {
        serial id PK
        int batch_id FK "NOT NULL"
        varchar external_transaction_id "NOT NULL"
        varchar order_code "NOT NULL"
        numeric net_amount "NOT NULL"
        date payout_date "NOT NULL"
        varchar status "NOT NULL"
        varchar rejection_reason "NULL"
    }

    discrepancies {
        serial id PK
        int order_id FK "NULL"
        int staging_id FK "NULL"
        varchar discrepancy_type "NOT NULL"
        numeric expected_amount "NOT NULL"
        numeric actual_amount "NOT NULL"
        numeric difference_amount "NOT NULL"
        timestamptz detected_at "NOT NULL"
    }
```

</details>

## Decisões de Modelagem

- **`payout_batches.file_hash` como `UNIQUE`:** Garante a idempotência da importação, fazendo o PostgreSQL bloquear automaticamente o reprocessamento caso o mesmo arquivo `.csv` seja enviado mais de uma vez.
- **`payout_staging.order_code` apenas como `VARCHAR` (sem `FK` para `orders`):** Escolha proposital para não bloquear repasses vinculados a pedidos inexistentes na base interna, permitindo que o registro entre na triagem e seja auditado como `ORPHAN_PAYOUT`.
- **`payout_staging.external_transaction_id` sem `UNIQUE`:** Permite que uma transação externa repetida (seja em outra linha do mesmo arquivo ou em uma importação futura) seja gravada na área de triagem para que o motor detecte e registre a divergência `DUPLICATE_PAYOUT`.
- **`order_id` e `staging_id` anuláveis (`NULL`) em `discrepancies`:** Necessário porque existem cenários em que apenas um dos lados existe — em `ORPHAN_PAYOUT` não há pedido interno correspondente (`order_id` fica nulo), enquanto em `MISSING_PAYOUT` não há linha de repasse no extrato (`staging_id` fica nulo).