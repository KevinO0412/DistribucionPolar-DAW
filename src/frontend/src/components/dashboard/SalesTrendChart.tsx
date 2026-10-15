import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardResponse } from '../../api/types';
import { useChartColors } from '../../hooks/useChartColors';
import { formatDate, formatInt, formatMoney, formatMoneyShort } from '../../lib/format';

export function SalesTrendChart({ data }: { data: DashboardResponse['salesByDay'] }) {
  const colors = useChartColors();
  const tickFormat = (value: string) => formatDate(value, { day: 'numeric', month: 'short' });

  return (
    <div className="h-72 w-full">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
          <defs>
            <linearGradient id="salesFill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor={colors.brand} stopOpacity={0.28} />
              <stop offset="100%" stopColor={colors.brand} stopOpacity={0} />
            </linearGradient>
          </defs>
          <CartesianGrid stroke={colors.line} strokeDasharray="3 3" vertical={false} />
          <XAxis dataKey="date" tickFormatter={tickFormat} stroke={colors.muted} fontSize={12} tickLine={false} axisLine={false} minTickGap={24} />
          <YAxis tickFormatter={formatMoneyShort} stroke={colors.muted} fontSize={12} tickLine={false} axisLine={false} width={64} />
          <Tooltip
            cursor={{ stroke: colors.muted, strokeDasharray: '4 4' }}
            contentStyle={{ background: colors.surface, border: `1px solid ${colors.line}`, borderRadius: 6, color: colors.ink }}
            labelFormatter={(label: string) => formatDate(label, { weekday: 'long', day: 'numeric', month: 'long' })}
            formatter={(value: number, _name, item) => [
              `${formatMoney(value)} en ${formatInt(item.payload.invoices)} facturas`,
              'Ventas',
            ]}
          />
          <Area type="monotone" dataKey="total" stroke={colors.brand} strokeWidth={2} fill="url(#salesFill)" activeDot={{ r: 5 }} isAnimationActive={false} />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
