import React, { useState, useEffect, useCallback } from 'react';
import { usePageTitle } from '../../hooks/usePageTitle';
import { dashboardService, DashboardStats } from '../../services/dashboardService';
import { SimpleGrid, Paper, Text, Group, Stack, Skeleton, Alert, Title, ThemeIcon, Loader, Center } from '@mantine/core';
import { AreaChart, BarChart, PieChart, DonutChart } from '@mantine/charts';
import { IconCurrencyDollar, IconClock, IconUsers, IconChartBar, IconAlertTriangle, IconArrowUp, IconArrowDown } from '@tabler/icons-react';

const formatCurrency = (amount: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

interface KpiCardProps {
  title: string;
  value: string;
  subtitle?: string;
  change?: number;
  icon: React.ReactNode;
  color: string;
  loading?: boolean;
}

const KpiCard: React.FC<KpiCardProps> = ({ title, value, subtitle, change, icon, color, loading }) => (
  <Paper p="lg" radius="md" withBorder>
    {loading ? (
      <Stack gap="sm">
        <Skeleton height={28} circle />
        <Skeleton height={16} width="60%" />
        <Skeleton height={24} width="80%" />
      </Stack>
    ) : (
      <Group wrap="nowrap" justify="space-between">
        <Stack gap={4}>
          <Text size="xs" c="dimmed" tt="uppercase" fw={700}>{title}</Text>
          <Text size="xl" fw={700}>{value}</Text>
          {subtitle && <Text size="xs" c="dimmed">{subtitle}</Text>}
          {change !== undefined && (
            <Group gap={4}>
              {change >= 0 ? <IconArrowUp size={14} color="green" /> : <IconArrowDown size={14} color="red" />}
              <Text size="xs" c={change >= 0 ? 'green' : 'red'}>{Math.abs(change).toFixed(1)}% from last month</Text>
            </Group>
          )}
        </Stack>
        <ThemeIcon variant="light" size="lg" radius="md" color={color}>
          {icon}
        </ThemeIcon>
      </Group>
    )}
  </Paper>
);

const ChartCard: React.FC<{ title: string; loading: boolean; error: string | null; children: React.ReactNode }> =
  ({ title, loading, error, children }) => (
    <Paper p="md" radius="md" withBorder>
      <Text size="sm" fw={600} mb="sm">{title}</Text>
      {loading ? <Skeleton height={250} /> :
        error ? <Alert icon={<IconAlertTriangle size="1rem" />} color="red" variant="light">{error}</Alert> :
        children}
    </Paper>
  );

const DashboardPage: React.FC = () => {
  usePageTitle('Dashboard');

  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [statsLoading, setStatsLoading] = useState(true);
  const [statsError, setStatsError] = useState<string | null>(null);

  const [invoiceStatus, setInvoiceStatus] = useState<any>(null);
  const [invStatusLoading, setInvStatusLoading] = useState(true);
  const [invStatusError, setInvStatusError] = useState<string | null>(null);

  const [monthlyRevenue, setMonthlyRevenue] = useState<any>(null);
  const [revLoading, setRevLoading] = useState(true);
  const [revError, setRevError] = useState<string | null>(null);

  const [topCustomers, setTopCustomers] = useState<any>(null);
  const [tcLoading, setTcLoading] = useState(true);
  const [tcError, setTcError] = useState<string | null>(null);

  const [revVsExp, setRevVsExp] = useState<any>(null);
  const [rveLoading, setRveLoading] = useState(true);
  const [rveError, setRveError] = useState<string | null>(null);

  const [paymentTrend, setPaymentTrend] = useState<any>(null);
  const [ptLoading, setPtLoading] = useState(true);
  const [ptError, setPtError] = useState<string | null>(null);

  const [accountBalance, setAccountBalance] = useState<any>(null);
  const [abLoading, setAbLoading] = useState(true);
  const [abError, setAbError] = useState<string | null>(null);

  const loadAll = useCallback(async () => {
    try { setStatsLoading(true); const d = await dashboardService.getDashboardStats(); setStats(d); setStatsError(null); }
    catch { setStatsError('Failed to load'); } finally { setStatsLoading(false); }

    try { setInvStatusLoading(true); const d = await dashboardService.getInvoiceStatusDistribution(); setInvoiceStatus(transformPie(d)); setInvStatusError(null); }
    catch { setInvStatusError('Failed to load'); } finally { setInvStatusLoading(false); }

    try { setRevLoading(true); const d = await dashboardService.getMonthlyRevenueTrend(); setMonthlyRevenue(transformLine(d, 'month', 'amount')); setRevError(null); }
    catch { setRevError('Failed to load'); } finally { setRevLoading(false); }

    try { setTcLoading(true); const d = await dashboardService.getTopCustomers(5); setTopCustomers(transformBar(d, 'name', 'value')); setTcError(null); }
    catch { setTcError('Failed to load'); } finally { setTcLoading(false); }

    try { setRveLoading(true); const d = await dashboardService.getRevenueVsExpenses(); setRevVsExp(transformRevExp(d)); setRveError(null); }
    catch { setRveError('Failed to load'); } finally { setRveLoading(false); }

    try { setPtLoading(true); const d = await dashboardService.getPaymentTrend(6); setPaymentTrend(transformLine(d, 'month', 'amount')); setPtError(null); }
    catch { setPtError('Failed to load'); } finally { setPtLoading(false); }

    try { setAbLoading(true); const d = await dashboardService.getAccountBalanceOverview(); setAccountBalance(transformPie(d)); setAbError(null); }
    catch { setAbError('Failed to load'); } finally { setAbLoading(false); }
  }, []);

  useEffect(() => { loadAll(); }, [loadAll]);

  if (statsLoading) return <Center h={400}><Loader size="lg" color="navy" /></Center>;
  if (statsError || !stats) return <Alert icon={<IconAlertTriangle size="1rem" />} color="red" variant="light">{statsError}</Alert>;

  return (
    <Stack gap="lg">
      <Stack gap={4}>
        <Title order={3}>Accounting Dashboard</Title>
        <Text c="dimmed" size="sm">Welcome back! Here's your business overview</Text>
      </Stack>

      <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
        <KpiCard title="Total Revenue" value={formatCurrency(stats.totalRevenue)} change={stats.revenueChange}
          icon={<IconCurrencyDollar size={20} />} color="navy" loading={statsLoading} />
        <KpiCard title="Outstanding Invoices" value={formatCurrency(stats.outstandingInvoices)}
          subtitle={`${stats.totalInvoiceCount - stats.paidInvoicesCount} invoices pending`}
          icon={<IconClock size={20} />} color="yellow" loading={statsLoading} />
        <KpiCard title="Active Customers" value={String(stats.activeCustomers)}
          icon={<IconUsers size={20} />} color="teal" loading={statsLoading} />
        <KpiCard title="Average Invoice" value={formatCurrency(stats.averageInvoiceValue)}
          subtitle={`${stats.totalInvoiceCount} total invoices`}
          icon={<IconChartBar size={20} />} color="grape" loading={statsLoading} />
      </SimpleGrid>

      <SimpleGrid cols={{ base: 1, md: 2 }}>
        <ChartCard title="Invoice Status" loading={invStatusLoading} error={invStatusError}>
          {invoiceStatus && <PieChart data={invoiceStatus} withLabels withTooltip h={250} />}
        </ChartCard>
        <ChartCard title="Monthly Revenue" loading={revLoading} error={revError}>
          {monthlyRevenue && <AreaChart data={monthlyRevenue} dataKey="month" series={[{ name: 'amount', color: 'navy.6' }]} curveType="monotone" h={250} />}
        </ChartCard>
        <ChartCard title="Top Customers" loading={tcLoading} error={tcError}>
          {topCustomers && <BarChart data={topCustomers} dataKey="name" series={[{ name: 'value', color: 'teal.6' }]} h={250} orientation="horizontal" />}
        </ChartCard>
        <ChartCard title="Revenue vs Expenses" loading={rveLoading} error={rveError}>
          {revVsExp && <BarChart data={revVsExp} dataKey="month" series={[{ name: 'revenue', color: 'navy.6' }, { name: 'expenses', color: 'red.6' }]} h={250} />}
        </ChartCard>
        <ChartCard title="Payment Trend" loading={ptLoading} error={ptError}>
          {paymentTrend && <AreaChart data={paymentTrend} dataKey="month" series={[{ name: 'amount', color: 'teal.6' }]} curveType="monotone" h={250} />}
        </ChartCard>
        <ChartCard title="Account Balance Overview" loading={abLoading} error={abError}>
          {accountBalance && <DonutChart data={accountBalance} withLabels withTooltip h={250} />}
        </ChartCard>
      </SimpleGrid>
    </Stack>
  );
};

// Transform helpers — adapt backend DTO shape to Mantine Charts expected shape
function transformPie(dto: any): any[] {
  if (!dto || !dto.labels) return [];
  return dto.labels.map((label: string, i: number) => ({ name: label, value: dto.data?.[i] ?? 0, color: dto.backgroundColors?.[i] }));
}
function transformLine(dto: any, xKey: string, yKey: string): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.months || [];
  const data = dto.data || dto.amounts || dto.values || [];
  return labels.map((label: string, i: number) => ({ [xKey]: label, [yKey]: data[i] ?? 0 }));
}
function transformBar(dto: any, xKey: string, yKey: string): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.names || [];
  const data = dto.data || dto.values || [];
  return labels.map((label: string, i: number) => ({ [xKey]: label, [yKey]: data[i] ?? 0 }));
}
function transformRevExp(dto: any): any[] {
  if (!dto) return [];
  const labels = dto.labels || dto.months || [];
  const revData = dto.revenue || dto.revenues || [];
  const expData = dto.expenses || [];
  return labels.map((label: string, i: number) => ({ month: label, revenue: revData[i] ?? 0, expenses: expData[i] ?? 0 }));
}

export default DashboardPage;
