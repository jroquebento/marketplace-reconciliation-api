# Marketplace Reconciliation API — Escopo do Projeto (MVP)

## 1. Problema e Público-Alvo
- **Público-alvo:** Pequenas e médias operações de e-commerce que vendem em marketplaces e registram suas vendas em um sistema interno (ERP).
- **Dor operacional:** Os marketplaces enviam extratos financeiros (arquivos `.csv`) descontando comissões e aplicando prazos de repasse ($D+N$). Conferir manualmente milhares de vendas contra os repasses gera falhas invisíveis, como vendas não pagas, repasses duplicados e descontos de taxas acima do contratado.
- **Solução:** Um motor back-end de ingestão, conciliação e auditoria financeira que cruza os pedidos internos do ERP com os extratos externos e isola automaticamente qualquer divergência financeira.

## 2. Fluxo Macro de Dados
1. **Base Interna (ERP):** Tabelas no SQL Server contendo os contratos dos canais de venda (taxa de comissão e prazo de repasse em dias) e os pedidos realizados.
2. **Ingestão Externa (Python):** Um cliente Python lê o arquivo `.csv` do marketplace, realiza saneamento estrutural básico (formatação de datas, valores e campos obrigatórios) e envia o lote para a API.
3. **Processamento e Conciliação (.NET 10 + Dapper + T-SQL):**
   - A API valida a idempotência do arquivo (via hash SHA-256) para impedir reprocessamento do mesmo lote.
   - Os registros recebidos são gravados em uma área de preparação (*Staging*) para preservar os dados brutos.
   - Consultas T-SQL cruzam os repasses normalizados com os pedidos internos e registram as inconsistências encontradas.

## 3. Regras de Negócio Invioláveis (MVP)
O motor de conciliação deve detectar e classificar exatamente **4 cenários de divergência**:

1. **Venda sem repasse (`MISSING_PAYOUT`):** Um pedido registrado no ERP não possui repasse correspondente no extrato e o prazo contratual de liquidação ($DataPedido + DiasContrato$) já venceu. Pedidos dentro do prazo normal não são considerados divergência.
2. **Repasse sem venda (`ORPHAN_PAYOUT`):** O extrato do marketplace informa um pagamento vinculado a um código de pedido inexistente na base interna do ERP.
3. **Repasse duplicado (`DUPLICATE_PAYOUT`):** O mesmo identificador de transação externa (`ExternalTransactionId`) é enviado mais de uma vez (no mesmo arquivo ou em importações posteriores), ou o mesmo arquivo `.csv` é submetido novamente.
4. **Valor líquido divergente (`NET_AMOUNT_MISMATCH`):** O valor líquido efetivamente repassado pelo marketplace difere do valor líquido esperado calculado pelo ERP ($ValorBrutoPedido - (ValorBrutoPedido \times \%ComissaoContrato)$) a partir de R$ 0,01 de diferença.

## 4. Tratamento de Erros vs. Transações (`ROLLBACK`)
- **Erros estruturais por linha:** Se uma linha do `.csv` vier com data inválida, valor negativo ou campo obrigatório vazio, apenas aquela linha é marcada como rejeitada na tabela de *Staging* com o motivo do erro, permitindo que o restante do lote válido seja processado.
- **Falhas sistêmicas de processamento:** Se ocorrer falha de conexão ou violação inesperada durante a gravação transacional de um lote, o banco executa `ROLLBACK` completo da operação para impedir dados gravados pela metade.

## 5. Fora de Escopo (MVP)
Para manter o foco em modelagem relacional, T-SQL puro e integridade transacional, ficam fora desta versão:
- Interface visual (front-end) — a demonstração será feita via Swagger/Scalar, scripts SQL e testes automatizados.
- Tabela de itens do pedido (a conciliação ocorre no nível do pedido total).
- Devoluções parciais, *chargebacks*, parcelamentos múltiplos e cálculo tributário.
- Arquitetura distribuída, microsserviços, filas (RabbitMQ/Kafka), cache distribuído ou integração com IA.