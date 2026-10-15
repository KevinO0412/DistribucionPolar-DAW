import { Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import type { DashboardResponse } from '../../api/types';
import { useChartColors } from '../../hooks/useChartColors';
import { formatInt, formatMoney, formatMoneyShort } from '../../lib/format';

interface Props {
  data: DashboardResponse['salesByCategory'];
  selectedId: string;
  onSelect: (categoryId: string) => void;
}

/** Barras por categoría. Al pulsar una barra se filtra todo el dashboard por esa categoría (y otra vez para quitar el filtro). */
export function CategorySalesChart({ data, selectedId, onSelect }: Props) {
  const colors = useChartColors();
  const height = Math.max(160, data.length * 52);

  return (
    <div style={{ height }} className="w-full">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} layout="vertical" margin={{ top: 0, right: 16, bottom: 0, left: 0 }}>
          <CartesianGrid stroke={colors.line} strokeDasharray="3 3" horizontal={false} />
          <XAxis type="number" tickFormatter={formatMoneyShort} stroke={colors.muted} fontSize={12} tickLine={false} axisLine={false} />
          <YAxis type="category" dataKey="categoryName" stroke={colors.muted} fontSize={13} tickLine={false} axisLine={false} width={110} />
          <Tooltip
            cursor={{ fill: colors.line, opacity: 0.35 }}
            contentStyle={{ background: colors.surface, border: `1px solid ${colors.line}`, borderRadius: 6, color: colors.ink }}
            formatter={(value: number, _name, item) => [`${formatMoney(value)}, ${formatInt(item.payload.units)} und`, 'Ventas']}
          />
          <Bar
            dataKey="total"
            radius={[0, 4, 4, 0]}
            isAnimationActive={false}
            cursor="pointer"
            onClick={(entry: { categoryId: string }) => onSelect(entry.categoryId === selectedId ? '' : entry.categoryId)}
          >
            {data.map((entry) => (
              <Cell key={entry.categoryId} fill={!selectedId || entry.categoryId === selectedId ? colors.brand : colors.line} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
