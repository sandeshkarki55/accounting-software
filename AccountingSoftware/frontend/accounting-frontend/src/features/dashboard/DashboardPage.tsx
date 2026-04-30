import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePageTitle } from '../../hooks/usePageTitle';
import { useDashboardData } from '../../hooks/useDashboardData';
import {
  Paper,
  Text,
  Group,
  Stack,
  Skeleton,
  Alert,
  Title,
  ThemeIcon,
  Center,
  SimpleGrid,
  Button,
  Select,
  ActionIcon,
  Tooltip,
  Box,
} from '@mantine/core';
import { AreaChart, BarChart, DonutChart } from '@mantine/charts';
import {
  IconCurrencyDollar,
  IconClock,
  IconUsers,
  IconChartBar,
  IconAlertTriangle,
  IconArrowUp,
  IconArrowDown,
  IconReceipt,
  IconUserPlus,
  IconFileText,
  IconRefresh,
  IconExclamationCircle,
  IconPercentage,
} from '@tabler/icons-react';

const formatCurrency = (amount: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

const valueFormatter = (value: number) => {
  if (Math.abs(value) >= 1000) {
    return `$${(value / 1000).toFixed(1)}k`;
  }
  return `$${value.toFixed(0)}`;
};

// ─── KPI Card ───────────────────────────────────────────────────────────────

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

// ─── Chart Card ─────────────────────────────────────────────────────────────

const ChartCard: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <Paper p="md" radius="md" withBorder>
    <Text size="sm" fw={600} mb="sm">{title}</Text>
    {children}
  </Paper>
);

// ─── Dashboard Page ─────────────────────────────────────────────────────────

const DashboardPage: React.FC = () => {
  usePageTitle('Dashboard');
  const navigate = useNavigate();
  const [period, setPeriod] = useState<string>('6');
  const { data, isLoading, error, refresh } = useDashboardData(Number(period));

  // ── Loading State ──────────────────────────────────────────────────────
  if (isLoading) {
    return (
      <Stack gap="lg">
        <Stack gap={4}>
          <Title order={3}>Accounting Dashboard</Title>
          <Text c="dimmed" size="sm">Loading your business overview…</Text>
        </Stack>
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
          {Array.from({ length: 4 }).map((_, i) => (
            <Paper key={i} p="lg" radius="md" withBorder>
              <Stack gap="sm">
                <Skeleton height={28} circle />
                <Skeleton height={16} width="60%" />
                <Skeleton height={24} width="80%" />
              </Stack>
            </Paper>
          ))}
        </SimpleGrid>
        <SimpleGrid cols={{ base: 1, md: 2 }}>
          {Array.from({ length: 2 }).map((_, i) => (
            <Paper key={i} p="md" radius="md" withBorder>
              <Skeleton height={20} width="30%" mb="sm" />
              <Skeleton height={250} />
            </Paper>
          ))}
        </SimpleGrid>
      </Stack>
    );
  }

  // ── Error State ────────────────────────────────────────────────────────
  if (error || !data.stats) {
    return (
      <Center h={400}>
        <Alert icon={<IconAlertTriangle size="1rem" />} color="red" variant="light" title="Error">
          {error || 'Unable to load dashboard data.'}
        </Alert>
      </Center>
    );
  }

  const stats = data.stats;
  const pendingCount = stats.totalInvoiceCount - stats.paidInvoicesCount;

  // ── Render ─────────────────────────────────────────────────────────────
  return (
    <Stack gap="lg">
      {/* Header */}
      <Group justify="space-between" align="flex-end">
        <Stack gap={4}>
          <Title order={3}>Accounting Dashboard</Title>
          <Text c="dimmed" size="sm">Welcome back! Here's your business overview</Text>
        </Stack>
        <Group gap="xs">
          <Select
            size="xs"
            value={period}
            onChange={(v) => v && setPeriod(v)}
            data={[
              { value: '6', label: 'Last 6 months' },
              { value: '12', label: 'Last 12 months' },
            ]}
            w={140}
          />
          <Tooltip label="Refresh data">
            <ActionIcon variant="subtle" color="gray" onClick={refresh}>
              <IconRefresh size="1.1rem" />
            </ActionIcon>
          </Tooltip>
        </Group>
      </Group>

      {/* KPI Row */}
      <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
        <KpiCard
          title="Total Revenue"
          value={formatCurrency(stats.totalRevenue)}
          change={stats.revenueChange}
          icon={<IconCurrencyDollar size={20} />}
          color="navy"
        />
        <KpiCard
          title="Outstanding Invoices"
          value={formatCurrency(stats.outstandingInvoices)}
          subtitle={`${pendingCount} invoices pending`}
          icon={<IconClock size={20} />}
          color="yellow"
        />
        <KpiCard
          title="Active Customers"
          value={String(stats.activeCustomers)}
          icon={<IconUsers size={20} />}
          color="teal"
        />
        <KpiCard
          title="Average Invoice"
          value={formatCurrency(stats.averageInvoiceValue)}
          subtitle={`${stats.totalInvoiceCount} total invoices`}
          icon={<IconChartBar size={20} />}
          color="grape"
        />
      </SimpleGrid>

      {/* Secondary KPI Row */}
      <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
        <KpiCard
          title="Overdue Amount"
          value={formatCurrency(stats.overdueAmount)}
          icon={<IconExclamationCircle size={20} />}
          color="red"
        />
        <KpiCard
          title="Payment Rate"
          value={`${(stats.paymentRate * 100).toFixed(1)}%`}
          subtitle={`${stats.paidInvoicesCount} of ${stats.totalInvoiceCount} invoices paid`}
          icon={<IconPercentage size={20} />}
          color="teal"
        />
      </SimpleGrid>

      {/* Quick Actions */}
      <Paper p="md" radius="md" withBorder>
        <Group gap="sm">
          <Button variant="light" color="navy" leftSection={<IconReceipt size="1rem" />} onClick={() => navigate('/invoices')}>
            New Invoice
          </Button>
          <Button variant="light" color="teal" leftSection={<IconUserPlus size="1rem" />} onClick={() => navigate('/customers')}>
            New Customer
          </Button>
          <Button variant="light" color="slate" leftSection={<IconFileText size="1rem" />} onClick={() => navigate('/journal')}>
            New Journal Entry
          </Button>
        </Group>
      </Paper>

      {/* Charts Row 1: Monthly Revenue (large) + Invoice Status (small) */}
      <Group grow align="stretch" preventGrowOverflow={false} gap="md" wrap="wrap" styles={{ root: { '& > *': { flex: '1 1 300px' } } }}>
        <Box maw="100%">
          <ChartCard title="Monthly Revenue Trend">
            {data.monthlyRevenue.length > 0 ? (
              <AreaChart
                data={data.monthlyRevenue}
                dataKey="month"
                series={[{ name: 'amount', color: 'navy.6', label: 'Revenue' }]}
                curveType="monotone"
                h={250}
                valueFormatter={valueFormatter}
                withTooltip
              />
            ) : (
              <Text c="dimmed" size="sm" ta="center" py="xl">No revenue data available</Text>
            )}
          </ChartCard>
        </Box>
        <Box maw="100%">
          <ChartCard title="Invoice Status Distribution">
            {data.invoiceStatus.length > 0 ? (
              <Box h={250}>
                <Center h="100%">
                  <DonutChart
                    data={data.invoiceStatus}
                    withLabels
                    withTooltip
                    size={220}
                    thickness={24}
                  />
                </Center>
              </Box>
            ) : (
              <Text c="dimmed" size="sm" ta="center" py="xl">No invoice data available</Text>
            )}
          </ChartCard>
        </Box>
      </Group>

      {/* Charts Row 2: Revenue vs Expenses + Top Customers + Payment Trend */}
      <SimpleGrid cols={{ base: 1, md: 3 }}>
        <ChartCard title="Revenue vs Expenses">
          {data.revVsExp.length > 0 ? (
            <BarChart
              data={data.revVsExp}
              dataKey="month"
              series={[
                { name: 'revenue', color: 'navy.6', label: 'Revenue' },
                { name: 'expenses', color: 'red.6', label: 'Expenses' },
              ]}
              h={250}
              valueFormatter={valueFormatter}
              withTooltip
            />
          ) : (
            <Text c="dimmed" size="sm" ta="center" py="xl">No data available</Text>
          )}
        </ChartCard>
        <ChartCard title="Top 5 Customers">
          {data.topCustomers.length > 0 ? (
            <BarChart
              data={data.topCustomers}
              dataKey="name"
              series={[{ name: 'value', color: 'teal.6', label: 'Revenue' }]}
              h={250}
              orientation="horizontal"
              valueFormatter={valueFormatter}
              withTooltip
            />
          ) : (
            <Text c="dimmed" size="sm" ta="center" py="xl">No customer data available</Text>
          )}
        </ChartCard>
        <ChartCard title="Payment Trend">
          {data.paymentTrend.length > 0 ? (
            <AreaChart
              data={data.paymentTrend}
              dataKey="month"
              series={[{ name: 'amount', color: 'teal.6', label: 'Payments' }]}
              curveType="monotone"
              h={250}
              valueFormatter={valueFormatter}
              withTooltip
            />
          ) : (
            <Text c="dimmed" size="sm" ta="center" py="xl">No payment data available</Text>
          )}
        </ChartCard>
      </SimpleGrid>
    </Stack>
  );
};

export default DashboardPage;
