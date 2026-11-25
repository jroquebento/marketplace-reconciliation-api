INSERT INTO contracts 
	(marketplace_name, commission_rate, payout_days)
VALUES 
	('Mercado Livre', 0.1500, 14), 
	('Amazon BR', 0.1250, 30);

INSERT INTO orders
	(contract_id, order_code, order_date,gross_amount)
VALUES
	(1, 'ORD-1001','2026-11-01',1000.00),
	(1, 'ORD-1002','2026-11-02',500.00),
	(1, 'ORD-1003','2026-11-02',200.00),
	(1, 'ORD-1004','2026-11-03',300.00);

INSERT INTO payout_batches
	(contract_id, file_name, file_hash, status)
VALUES 
	(1, 'meli_payout_2026_11_20.csv', 'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855', 'PROCESSING');

INSERT INTO payout_staging
	(batch_id, external_transaction_id, order_code, net_amount, payout_date, status, rejection_reason)
VALUES 
	(1, 'TXN-9001', 'ORD-1001', 850.00, '2026-11-20', 'PENDING', NULL),
	(1, 'TXN-9002', 'ORD-1002', 400.00, '2026-11-20', 'PENDING', NULL),
	(1, 'TXN-9003', 'ORD-1003', 170.00, '2026-11-20', 'PENDING', NULL),
	(1, 'TXN-9003', 'ORD-1003', 170.00, '2026-11-20', 'PENDING', NULL),
	(1, 'TXN-9004', 'ORD-9999', 120.00, '2026-11-20', 'PENDING', NULL);