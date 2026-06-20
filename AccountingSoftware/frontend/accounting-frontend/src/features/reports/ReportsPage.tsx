import React, { useState, useEffect, useCallback } from 'react';
import { Tabs, Paper, Table, Text, Stack, Group, Alert, Center, Loader, Badge, Select, Button, SimpleGrid } from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { usePageTitle } from '../../hooks/usePageTitle';
import { reportService, TrialBalance, IncomeStatement, BalanceSheet, GeneralLedger, AgedReceivables } from '../../services/reportService';
import { accountService } from '../../services/api';
import { formatCurrency, formatDate, toISODate } from '../../utils';

const ReportsPage: React.FC = () => {
  usePageTitle('Reports');
  const [activeTab, setActiveTab] = useState<string | null>('trial-balance');

  const today = new Date();
  const [asOfDate, setAsOfDate] = useState<Date | null>(today);
  const [pStart, setPStart] = useState<Date | null>(new Date(today.getFullYear(), 0, 1));
  const [pEnd, setPEnd] = useState<Date | null>(today);
  const [glAccountId, setGlAccountId] = useState<string>('');
  const [accounts, setAccounts] = useState<{ value: string; label: string }[]>([]);

  useEffect(() => { accountService.getAccounts().then(a => setAccounts(a.filter(x => x.isActive).map(x => ({ value: String(x.id), label: `${x.accountCode} - ${x.accountName}` })))).catch(() => {}); }, []);

  return (
    <Stack gap="md">
      <Text component="h1" size="xl" fw={700}>Financial Reports</Text>
      <Tabs value={activeTab} onChange={setActiveTab}>
        <Tabs.List mb="md">
          <Tabs.Tab value="trial-balance">Trial Balance</Tabs.Tab>
          <Tabs.Tab value="income-statement">Income Statement</Tabs.Tab>
          <Tabs.Tab value="balance-sheet">Balance Sheet</Tabs.Tab>
          <Tabs.Tab value="general-ledger">General Ledger</Tabs.Tab>
          <Tabs.Tab value="aged-receivables">Aged Receivables</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="trial-balance">
          <TrialBalancePanel asOfDate={asOfDate} setAsOfDate={setAsOfDate} />
        </Tabs.Panel>
        <Tabs.Panel value="income-statement">
          <IncomeStatementPanel startDate={pStart} setStartDate={setPStart} endDate={pEnd} setEndDate={setPEnd} />
        </Tabs.Panel>
        <Tabs.Panel value="balance-sheet">
          <BalanceSheetPanel asOfDate={asOfDate} setAsOfDate={setAsOfDate} />
        </Tabs.Panel>
        <Tabs.Panel value="general-ledger">
          <GeneralLedgerPanel startDate={pStart} setStartDate={setPStart} endDate={pEnd} setEndDate={setPEnd} accountId={glAccountId} setAccountId={setGlAccountId} accounts={accounts} />
        </Tabs.Panel>
        <Tabs.Panel value="aged-receivables">
          <AgedReceivablesPanel asOfDate={asOfDate} setAsOfDate={setAsOfDate} />
        </Tabs.Panel>
      </Tabs>
    </Stack>
  );
};

// ── Trial Balance ──
const TrialBalancePanel: React.FC<{ asOfDate: Date | null; setAsOfDate: (d: Date | null) => void }> = ({ asOfDate, setAsOfDate }) => {
  const [data, setData] = useState<TrialBalance | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { const r = await reportService.getTrialBalance(toISODate(asOfDate)); setData(r); } catch { setError('Failed to load'); } finally { setLoading(false); }
  }, [asOfDate]);

  useEffect(() => { load(); }, [load]);

  return (
    <Stack gap="md">
      <Group><DateInput label="As of Date" value={asOfDate} onChange={(v) => setAsOfDate(v ? new Date(v) : null)} /><Button onClick={load} loading={loading}>Refresh</Button></Group>
      {loading ? <Center h={200}><Loader /></Center> :
       error ? <Alert color="red">{error}</Alert> :
       data ? (
         <Paper withBorder>
           <Table striped highlightOnHover>
             <Table.Thead><Table.Tr><Table.Th>Account Code</Table.Th><Table.Th>Account Name</Table.Th><Table.Th>Type</Table.Th><Table.Th ta="right">Debit</Table.Th><Table.Th ta="right">Credit</Table.Th></Table.Tr></Table.Thead>
             <Table.Tbody>
               {data.lines.map(l => (
                 <Table.Tr key={l.accountId}><Table.Td fw={500}>{l.accountCode}</Table.Td><Table.Td>{l.accountName}</Table.Td><Table.Td><Badge variant="light" size="sm">{l.accountType}</Badge></Table.Td><Table.Td ta="right">{l.debitBalance > 0 ? formatCurrency(l.debitBalance) : '-'}</Table.Td><Table.Td ta="right">{l.creditBalance > 0 ? formatCurrency(l.creditBalance) : '-'}</Table.Td></Table.Tr>
               ))}
             </Table.Tbody>
             <Table.Tfoot>
               <Table.Tr><Table.Td colSpan={3} fw={700}>Totals:</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.totalDebits)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.totalCredits)}</Table.Td></Table.Tr>
               <Table.Tr><Table.Td colSpan={5} ta="right" c={data.isBalanced ? 'green' : 'red'} fw={700}>{data.isBalanced ? '✓ Balanced' : '✗ Out of Balance'}</Table.Td></Table.Tr>
             </Table.Tfoot>
           </Table>
         </Paper>
       ) : null}
    </Stack>
  );
};

// ── Income Statement ──
const IncomeStatementPanel: React.FC<{ startDate: Date | null; setStartDate: (d: Date | null) => void; endDate: Date | null; setEndDate: (d: Date | null) => void }> = ({ startDate, setStartDate, endDate, setEndDate }) => {
  const [data, setData] = useState<IncomeStatement | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!startDate || !endDate) return;
    setLoading(true); setError(null);
    try { const r = await reportService.getIncomeStatement(toISODate(startDate)!, toISODate(endDate)!); setData(r); } catch { setError('Failed to load'); } finally { setLoading(false); }
  }, [startDate, endDate]);

  useEffect(() => { load(); }, [load]);

  const Section: React.FC<{ title: string; items: { accountCode: string; accountName: string; amount: number }[]; total: number; bold?: boolean }> = ({ title, items, total, bold }) => (
    items.length === 0 ? null :
    <Stack gap={0}>
      <Text fw={600} size="sm" mt="sm">{title}</Text>
      {items.map((item, i) => (
        <Group key={i} justify="space-between" px="md"><Text size="sm">{item.accountCode} - {item.accountName}</Text><Text size="sm">{formatCurrency(item.amount)}</Text></Group>
      ))}
      <Group justify="space-between" px="md" pt="xs" style={{ borderTop: '1px solid var(--mantine-color-gray-3)' }}>
        <Text size="sm" fw={bold ? 700 : 600}>Total {title}</Text><Text size="sm" fw={bold ? 700 : 600}>{formatCurrency(total)}</Text>
      </Group>
    </Stack>
  );

  return (
    <Stack gap="md">
      <Group><DateInput label="From" value={startDate} onChange={(v) => setStartDate(v ? new Date(v) : null)} /><DateInput label="To" value={endDate} onChange={(v) => setEndDate(v ? new Date(v) : null)} /><Button onClick={load} loading={loading}>Refresh</Button></Group>
      {loading ? <Center h={200}><Loader /></Center> :
       error ? <Alert color="red">{error}</Alert> :
       data ? (
         <Paper p="md" withBorder>
           <Text size="lg" fw={700} ta="center" mb="md">Income Statement</Text>
           <Section title="Revenue" items={data.revenue} total={data.totalRevenue} />
           {data.costOfGoodsSold.length > 0 && <Section title="Cost of Goods Sold" items={data.costOfGoodsSold} total={data.totalCOGS} />}
           <Group justify="space-between" px="md" py="xs" style={{ background: 'var(--mantine-color-gray-0)', borderTop: '2px solid var(--mantine-color-gray-5)', borderBottom: '1px solid var(--mantine-color-gray-3)' }}><Text size="sm" fw={700}>Gross Profit</Text><Text size="sm" fw={700}>{formatCurrency(data.grossProfit)}</Text></Group>
           <Section title="Operating Expenses" items={data.operatingExpenses} total={data.totalOperatingExpenses} />
           <Group justify="space-between" px="md" py="xs" style={{ background: 'var(--mantine-color-gray-0)', borderTop: '2px solid var(--mantine-color-gray-5)', borderBottom: '1px solid var(--mantine-color-gray-3)' }}><Text size="sm" fw={700}>Operating Income</Text><Text size="sm" fw={700}>{formatCurrency(data.operatingIncome)}</Text></Group>
           <Section title="Other Income" items={data.otherIncome} total={data.totalOtherIncome} />
           <Section title="Other Expenses" items={data.otherExpenses} total={data.totalOtherExpenses} />
           <Group justify="space-between" px="md" py="md" style={{ background: data.netIncome >= 0 ? 'var(--mantine-color-green-0)' : 'var(--mantine-color-red-0)', borderTop: '3px double var(--mantine-color-gray-7)' }}><Text size="md" fw={700}>NET INCOME</Text><Text size="md" fw={700} c={data.netIncome >= 0 ? 'green' : 'red'}>{formatCurrency(data.netIncome)}</Text></Group>
         </Paper>
       ) : null}
    </Stack>
  );
};

// ── Balance Sheet ──
const BalanceSheetPanel: React.FC<{ asOfDate: Date | null; setAsOfDate: (d: Date | null) => void }> = ({ asOfDate, setAsOfDate }) => {
  const [data, setData] = useState<BalanceSheet | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { const r = await reportService.getBalanceSheet(toISODate(asOfDate)); setData(r); } catch { setError('Failed to load'); } finally { setLoading(false); }
  }, [asOfDate]);
  useEffect(() => { load(); }, [load]);

  const Section: React.FC<{ title: string; items: { accountCode: string; accountName: string; balance: number }[]; total: number }> = ({ title, items, total }) => (
    <Stack gap={0}>
      <Text fw={600} size="sm" mt="sm">{title}</Text>
      {items.map((item, i) => (
        <Group key={i} justify="space-between" px="md"><Text size="sm">{item.accountCode} - {item.accountName}</Text><Text size="sm">{formatCurrency(item.balance)}</Text></Group>
      ))}
      <Group justify="space-between" px="md" pt="xs" style={{ borderTop: '1px solid var(--mantine-color-gray-3)' }}>
        <Text size="sm" fw={600}>Total {title}</Text><Text size="sm" fw={600}>{formatCurrency(total)}</Text>
      </Group>
    </Stack>
  );

  return (
    <Stack gap="md">
      <Group><DateInput label="As of Date" value={asOfDate} onChange={(v) => setAsOfDate(v ? new Date(v) : null)} /><Button onClick={load} loading={loading}>Refresh</Button></Group>
      {loading ? <Center h={200}><Loader /></Center> :
       error ? <Alert color="red">{error}</Alert> :
       data ? (
         <Paper p="md" withBorder>
           <Text size="lg" fw={700} ta="center" mb="md">Balance Sheet</Text>
           <Section title="Assets" items={data.assets} total={data.totalAssets} />
           <Group justify="space-between" px="md" py="xs" style={{ borderTop: '2px solid var(--mantine-color-gray-5)' }}><Text size="sm" fw={700}>Total Assets</Text><Text size="sm" fw={700}>{formatCurrency(data.totalAssets)}</Text></Group>
           <Section title="Liabilities" items={data.liabilities} total={data.totalLiabilities} />
           <Section title="Equity" items={data.equity} total={data.totalEquity} />
           <Group justify="space-between" px="md" py="xs" style={{ borderTop: '2px solid var(--mantine-color-gray-5)' }}><Text size="sm" fw={700}>Total Liabilities & Equity</Text><Text size="sm" fw={700}>{formatCurrency(data.totalLiabilitiesAndEquity)}</Text></Group>
           <Group justify="space-between" px="md" py="md" style={{ borderTop: '3px double var(--mantine-color-gray-7)' }}>
             <Text size="md" fw={700} c={data.isBalanced ? 'green' : 'red'}>{data.isBalanced ? '✓ Balanced' : '✗ Out of Balance'}</Text>
             <Text size="sm" c="dimmed">Assets ({formatCurrency(data.totalAssets)}) = Liabilities + Equity ({formatCurrency(data.totalLiabilitiesAndEquity)})</Text>
           </Group>
         </Paper>
       ) : null}
    </Stack>
  );
};

// ── General Ledger ──
const GeneralLedgerPanel: React.FC<{ startDate: Date | null; setStartDate: (d: Date | null) => void; endDate: Date | null; setEndDate: (d: Date | null) => void; accountId: string; setAccountId: (v: string) => void; accounts: { value: string; label: string }[] }> = ({ startDate, setStartDate, endDate, setEndDate, accountId, setAccountId, accounts }) => {
  const [data, setData] = useState<GeneralLedger | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!startDate || !endDate || !accountId) return;
    setLoading(true); setError(null);
    try { const r = await reportService.getGeneralLedger(Number(accountId), toISODate(startDate)!, toISODate(endDate)!); setData(r); } catch { setError('Failed to load'); } finally { setLoading(false); }
  }, [startDate, endDate, accountId]);
  useEffect(() => { load(); }, [load]);

  return (
    <Stack gap="md">
      <Group><Select data={accounts} searchable placeholder="Select account" value={accountId} onChange={v => setAccountId(v || '')} style={{ width: 300 }} /><DateInput label="From" value={startDate} onChange={(v) => setStartDate(v ? new Date(v) : null)} /><DateInput label="To" value={endDate} onChange={(v) => setEndDate(v ? new Date(v) : null)} /><Button onClick={load} loading={loading} disabled={!accountId}>Refresh</Button></Group>
      {!accountId ? <Text c="dimmed" ta="center">Select an account to view the general ledger</Text> :
       loading ? <Center h={200}><Loader /></Center> :
       error ? <Alert color="red">{error}</Alert> :
       data ? (
         <Stack gap="md">
           <Paper p="md" withBorder>
             <Group justify="space-between"><Text fw={700}>{data.accountCode} - {data.accountName}</Text><Badge variant="light">{data.accountType}</Badge></Group>
             <SimpleGrid cols={2} mt="sm"><Text size="sm">Opening Balance: <strong>{formatCurrency(data.openingBalance)}</strong></Text><Text size="sm">Closing Balance: <strong>{formatCurrency(data.closingBalance)}</strong></Text></SimpleGrid>
           </Paper>
           <Paper withBorder>
             <Table striped highlightOnHover>
               <Table.Thead><Table.Tr><Table.Th>Date</Table.Th><Table.Th>Entry #</Table.Th><Table.Th>Description</Table.Th><Table.Th ta="right">Debit</Table.Th><Table.Th ta="right">Credit</Table.Th><Table.Th ta="right">Balance</Table.Th></Table.Tr></Table.Thead>
               <Table.Tbody>
                 {data.lines.length === 0 ? <Table.Tr><Table.Td colSpan={6}><Center py="md"><Text c="dimmed">No transactions in this period</Text></Center></Table.Td></Table.Tr> :
                   data.lines.map((l, i) => (
                   <Table.Tr key={i}><Table.Td>{formatDate(l.transactionDate)}</Table.Td><Table.Td fw={500}>{l.entryNumber}</Table.Td><Table.Td>{l.description}</Table.Td><Table.Td ta="right">{l.debitAmount > 0 ? formatCurrency(l.debitAmount) : '-'}</Table.Td><Table.Td ta="right">{l.creditAmount > 0 ? formatCurrency(l.creditAmount) : '-'}</Table.Td><Table.Td ta="right" fw={500}>{formatCurrency(l.runningBalance)}</Table.Td></Table.Tr>
                 ))}
               </Table.Tbody>
             </Table>
           </Paper>
         </Stack>
       ) : null}
    </Stack>
  );
};

// ── Aged Receivables ──
const AgedReceivablesPanel: React.FC<{ asOfDate: Date | null; setAsOfDate: (d: Date | null) => void }> = ({ asOfDate, setAsOfDate }) => {
  const [data, setData] = useState<AgedReceivables | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true); setError(null);
    try { const r = await reportService.getAgedReceivables(toISODate(asOfDate)); setData(r); } catch { setError('Failed to load'); } finally { setLoading(false); }
  }, [asOfDate]);
  useEffect(() => { load(); }, [load]);

  return (
    <Stack gap="md">
      <Group><DateInput label="As of Date" value={asOfDate} onChange={(v) => setAsOfDate(v ? new Date(v) : null)} /><Button onClick={load} loading={loading}>Refresh</Button></Group>
      {loading ? <Center h={200}><Loader /></Center> :
       error ? <Alert color="red">{error}</Alert> :
       data ? (
         <Stack gap="md">
           <SimpleGrid cols={5}>
             <Paper p="sm" withBorder ta="center"><Text size="xs" c="dimmed" tt="uppercase">Current</Text><Text fw={700}>{formatCurrency(data.summary.totalCurrent)}</Text></Paper>
             <Paper p="sm" withBorder ta="center"><Text size="xs" c="dimmed" tt="uppercase">1-30 Days</Text><Text fw={700}>{formatCurrency(data.summary.totalDays1To30)}</Text></Paper>
             <Paper p="sm" withBorder ta="center"><Text size="xs" c="dimmed" tt="uppercase">31-60 Days</Text><Text fw={700} c={data.summary.totalDays31To60 > 0 ? 'orange' : undefined}>{formatCurrency(data.summary.totalDays31To60)}</Text></Paper>
             <Paper p="sm" withBorder ta="center"><Text size="xs" c="dimmed" tt="uppercase">61-90 Days</Text><Text fw={700} c={data.summary.totalDays61To90 > 0 ? 'orange' : undefined}>{formatCurrency(data.summary.totalDays61To90)}</Text></Paper>
             <Paper p="sm" withBorder ta="center"><Text size="xs" c="dimmed" tt="uppercase">90+ Days</Text><Text fw={700} c={data.summary.totalDaysOver90 > 0 ? 'red' : undefined}>{formatCurrency(data.summary.totalDaysOver90)}</Text></Paper>
           </SimpleGrid>
           <Paper withBorder>
             <Table striped highlightOnHover>
               <Table.Thead><Table.Tr><Table.Th>Customer</Table.Th><Table.Th ta="right">Current</Table.Th><Table.Th ta="right">1-30</Table.Th><Table.Th ta="right">31-60</Table.Th><Table.Th ta="right">61-90</Table.Th><Table.Th ta="right">90+</Table.Th><Table.Th ta="right">Total</Table.Th></Table.Tr></Table.Thead>
               <Table.Tbody>
                 {data.customers.length === 0 ? <Table.Tr><Table.Td colSpan={7}><Center py="md"><Text c="dimmed">No outstanding receivables</Text></Center></Table.Td></Table.Tr> :
                   data.customers.map(c => (
                   <Table.Tr key={c.customerId}><Table.Td fw={500}>{c.customerName} <Text size="xs" c="dimmed" span>{c.customerCode}</Text></Table.Td><Table.Td ta="right">{c.currentAmount > 0 ? formatCurrency(c.currentAmount) : '-'}</Table.Td><Table.Td ta="right">{c.days1To30Amount > 0 ? formatCurrency(c.days1To30Amount) : '-'}</Table.Td><Table.Td ta="right">{c.days31To60Amount > 0 ? formatCurrency(c.days31To60Amount) : '-'}</Table.Td><Table.Td ta="right">{c.days61To90Amount > 0 ? formatCurrency(c.days61To90Amount) : '-'}</Table.Td><Table.Td ta="right" c={c.daysOver90Amount > 0 ? 'red' : undefined}>{c.daysOver90Amount > 0 ? formatCurrency(c.daysOver90Amount) : '-'}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(c.totalOutstanding)}</Table.Td></Table.Tr>
                 ))}
                 <Table.Tr key="tfoot"><Table.Td fw={700}>Grand Total</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.totalCurrent)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.totalDays1To30)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.totalDays31To60)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.totalDays61To90)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.totalDaysOver90)}</Table.Td><Table.Td ta="right" fw={700}>{formatCurrency(data.summary.grandTotal)}</Table.Td></Table.Tr>
               </Table.Tbody>
             </Table>
           </Paper>
         </Stack>
       ) : null}
    </Stack>
  );
};

export default ReportsPage;
