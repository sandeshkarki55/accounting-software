export const formatCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n);

export const formatDate = (d: string) =>
  new Date(d).toLocaleDateString();

export const toISODate = (d: Date | null) =>
  d ? d.toISOString().split('T')[0] : undefined;
