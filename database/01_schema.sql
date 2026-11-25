CREATE TABLE contracts (
	id SERIAL PRIMARY KEY,
	marketplace_name VARCHAR(100) NOT NULL UNIQUE,
	commission_rate NUMERIC(5,4) NOT NULL CHECK(commission_rate >= 0 AND commission_rate <= 1),
	payout_days INT NOT NULL CHECK(payout_days >= 0),
	created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE orders (
	id SERIAL PRIMARY KEY,
	contract_id INT NOT NULL REFERENCES contracts(id),
	order_code VARCHAR(50) NOT NULL UNIQUE,
	order_date DATE NOT NULL,
	gross_amount NUMERIC(12,2) NOT NULL CHECK(gross_amount >= 0),
	created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE payout_batches(
	id SERIAL PRIMARY KEY,
	contract_id INT NOT NULL REFERENCES contracts(id),
	file_name VARCHAR(255) NOT NULL,
	file_hash VARCHAR(64) NOT NULL UNIQUE,
	status VARCHAR(20) NOT NULL CHECK(status IN ('PROCESSING','COMPLETED','FAILED')),
	created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE payout_staging(
	id SERIAL PRIMARY KEY,
	batch_id INT NOT NULL REFERENCES payout_batches(id),
	external_transaction_id VARCHAR(100) NOT NULL,
	order_code VARCHAR(50) NOT NULL,
	net_amount NUMERIC(12,2) NOT NULL CHECK(net_amount >= 0),
	payout_date DATE NOT NULL,
	status VARCHAR(20) NOT NULL CHECK(status IN ('PENDING','PROCESSED','REJECTED')),
	rejection_reason VARCHAR(255) NULL
);

CREATE TABLE discrepancies(
	id SERIAL PRIMARY KEY,
	order_id INT REFERENCES orders(id),
	staging_id INT REFERENCES payout_staging(id),
	discrepancy_type VARCHAR(30) NOT NULL CHECK(discrepancy_type IN('MISSING_PAYOUT','ORPHAN_PAYOUT','DUPLICATE_PAYOUT','NET_AMOUNT_MISMATCH')),
	expected_amount NUMERIC(12,2) NOT NULL CHECK(expected_amount >= 0),
	actual_amount NUMERIC(12,2) NOT NULL CHECK(actual_amount >= 0),
	difference_amount NUMERIC(12,2) NOT NULL,
	detected_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
	CHECK (order_id IS NOT NULL OR staging_id IS NOT NULL)
);

CREATE INDEX idx_orders_contract_id ON orders(contract_id);
CREATE INDEX idx_payout_batches_contract_id ON payout_batches(contract_id);
CREATE INDEX idx_payout_staging_batch_id ON payout_staging(batch_id);
CREATE INDEX idx_payout_staging_external_tx_id ON payout_staging(external_transaction_id);
CREATE INDEX idx_discrepancies_order_id ON discrepancies(order_id);
CREATE INDEX idx_discrepancies_staging_id ON discrepancies(staging_id);