<template>
  <div>
    <h1 class="text-3xl font-bold text-gray-800 mb-6">Dashboard</h1>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
      <Card class="overflow-hidden">
        <template #content>
          <div class="flex items-center gap-4 p-0">
            <div class="w-16 h-16 rounded-xl flex items-center justify-center text-2xl text-white bg-gradient-primary flex-shrink-0">
              <i class="pi pi-dollar"></i>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm text-gray-600 mb-1">Ventas Hoy</div>
              <div class="text-2xl font-semibold text-gray-800 mb-1">{{ formatCurrency(todaySales) }}</div>
              <div class="text-sm text-success flex items-center gap-1">
                <i class="pi pi-arrow-up"></i> 12.5%
              </div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="overflow-hidden">
        <template #content>
          <div class="flex items-center gap-4 p-0">
            <div class="w-16 h-16 rounded-xl flex items-center justify-center text-2xl text-white flex-shrink-0 bg-gradient-to-br from-pink-400 to-rose-500">
              <i class="pi pi-receipt"></i>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm text-gray-600 mb-1">Facturas Emitidas</div>
              <div class="text-2xl font-semibold text-gray-800 mb-1">{{ invoicesIssued }}</div>
              <div class="text-sm text-success flex items-center gap-1">
                <i class="pi pi-arrow-up"></i> 8.2%
              </div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="overflow-hidden">
        <template #content>
          <div class="flex items-center gap-4 p-0">
            <div class="w-16 h-16 rounded-xl flex items-center justify-center text-2xl text-white flex-shrink-0 bg-gradient-to-br from-blue-400 to-cyan-400">
              <i class="pi pi-box"></i>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm text-gray-600 mb-1">Productos Registrados</div>
              <div class="text-2xl font-semibold text-gray-800 mb-1">{{ totalProducts }}</div>
              <div class="text-sm text-gray-500 flex items-center gap-1">
                <i class="pi pi-minus"></i> 0%
              </div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="overflow-hidden">
        <template #content>
          <div class="flex items-center gap-4 p-0">
            <div class="w-16 h-16 rounded-xl flex items-center justify-center text-2xl text-white flex-shrink-0 bg-gradient-to-br from-green-400 to-teal-400">
              <i class="pi pi-users"></i>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm text-gray-600 mb-1">Clientes Activos</div>
              <div class="text-2xl font-semibold text-gray-800 mb-1">{{ activeCustomers }}</div>
              <div class="text-sm text-success flex items-center gap-1">
                <i class="pi pi-arrow-up"></i> 5.4%
              </div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <!-- Charts Row -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
      <Card>
        <template #title>Ventas Mensuales</template>
        <template #content>
          <div class="h-[300px]">
            <Line :data="salesChartData" :options="chartOptions" />
          </div>
        </template>
      </Card>

      <Card>
        <template #title>Productos Más Vendidos</template>
        <template #content>
          <div class="h-[300px]">
            <Bar :data="topProductsChartData" :options="barChartOptions" />
          </div>
        </template>
      </Card>
    </div>

    <!-- Recent Activity -->
    <div class="mb-6">
      <Card>
        <template #title>Facturas Recientes</template>
        <template #content>
          <DataTable
            :value="recentInvoices"
            :paginator="true"
            :rows="5"
            responsiveLayout="scroll"
          >
            <Column field="documentNumber" header="Número" />
            <Column field="customer" header="Cliente" />
            <Column field="date" header="Fecha">
              <template #body="slotProps">
                {{ formatDate(slotProps.data.date) }}
              </template>
            </Column>
            <Column field="total" header="Total">
              <template #body="slotProps">
                {{ formatCurrency(slotProps.data.total) }}
              </template>
            </Column>
            <Column field="status" header="Estado">
              <template #body="slotProps">
                <Tag
                  :value="getStatusLabel(slotProps.data.status)"
                  :severity="getStatusSeverity(slotProps.data.status)"
                />
              </template>
            </Column>
          </DataTable>
        </template>
      </Card>
    </div>

    <!-- Low Stock Alert -->
    <div>
      <Card>
        <template #title>
          <div class="flex items-center gap-3">
            Productos con Stock Bajo
            <Badge :value="lowStockProducts.length" severity="warning" />
          </div>
        </template>
        <template #content>
          <DataTable
            :value="lowStockProducts"
            :paginator="true"
            :rows="5"
            responsiveLayout="scroll"
          >
            <Column field="code" header="Código" />
            <Column field="name" header="Producto" />
            <Column field="currentStock" header="Stock Actual">
              <template #body="slotProps">
                <span class="text-error font-semibold">{{ slotProps.data.currentStock }}</span>
              </template>
            </Column>
            <Column field="minStock" header="Stock Mínimo" />
            <Column field="warehouse" header="Bodega" />
            <Column header="Acción">
              <template #body>
                <Button
                  icon="pi pi-plus"
                  label="Reabastecer"
                  size="small"
                  outlined
                />
              </template>
            </Column>
          </DataTable>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Line, Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Badge from 'primevue/badge'
import Button from 'primevue/button'
import { useCurrency } from '@/composables/useCurrency'
import { format } from 'date-fns'

// Register Chart.js components
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Title,
  Tooltip,
  Legend
)

const { formatCurrency } = useCurrency()

// Stats data
const todaySales = ref(12450.75)
const invoicesIssued = ref(35)
const totalProducts = ref(248)
const activeCustomers = ref(156)

// Chart data
const salesChartData = ref({
  labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
  datasets: [
    {
      label: 'Ventas 2026',
      data: [45000, 52000, 48000, 61000, 55000, 67000, 0, 0, 0, 0, 0, 0],
      borderColor: '#667eea',
      backgroundColor: 'rgba(102, 126, 234, 0.1)',
      tension: 0.4
    }
  ]
})

const topProductsChartData = ref({
  labels: ['Producto A', 'Producto B', 'Producto C', 'Producto D', 'Producto E'],
  datasets: [
    {
      label: 'Unidades Vendidas',
      data: [150, 125, 98, 87, 75],
      backgroundColor: [
        'rgba(102, 126, 234, 0.8)',
        'rgba(118, 75, 162, 0.8)',
        'rgba(237, 100, 166, 0.8)',
        'rgba(255, 154, 158, 0.8)',
        'rgba(250, 208, 196, 0.8)'
      ]
    }
  ]
})

const chartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom' as const
    }
  },
  scales: {
    y: {
      beginAtZero: true
    }
  }
})

const barChartOptions = ref({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    }
  },
  scales: {
    y: {
      beginAtZero: true
    }
  }
})

// Recent invoices
const recentInvoices = ref([
  {
    id: '1',
    documentNumber: '001-001-000000035',
    customer: 'Juan Pérez',
    date: new Date('2026-02-05'),
    total: 1250.50,
    status: 'authorized'
  },
  {
    id: '2',
    documentNumber: '001-001-000000034',
    customer: 'María González',
    date: new Date('2026-02-05'),
    total: 890.25,
    status: 'authorized'
  },
  {
    id: '3',
    documentNumber: '001-001-000000033',
    customer: 'Carlos Ramírez',
    date: new Date('2026-02-04'),
    total: 2150.00,
    status: 'pending'
  },
  {
    id: '4',
    documentNumber: '001-001-000000032',
    customer: 'Ana Rodríguez',
    date: new Date('2026-02-04'),
    total: 675.80,
    status: 'authorized'
  },
  {
    id: '5',
    documentNumber: '001-001-000000031',
    customer: 'Luis Martínez',
    date: new Date('2026-02-03'),
    total: 3420.15,
    status: 'authorized'
  }
])

// Low stock products
const lowStockProducts = ref([
  {
    code: 'PROD001',
    name: 'Producto A',
    currentStock: 5,
    minStock: 20,
    warehouse: 'Bodega Principal'
  },
  {
    code: 'PROD015',
    name: 'Producto B',
    currentStock: 8,
    minStock: 15,
    warehouse: 'Bodega Principal'
  },
  {
    code: 'PROD032',
    name: 'Producto C',
    currentStock: 3,
    minStock: 10,
    warehouse: 'Bodega Secundaria'
  }
])

// Helper functions
const formatDate = (date: Date): string => {
  return format(date, 'dd/MM/yyyy')
}

const getStatusLabel = (status: string): string => {
  const labels: Record<string, string> = {
    authorized: 'Autorizada',
    pending: 'Pendiente',
    rejected: 'Rechazada'
  }
  return labels[status] || status
}

const getStatusSeverity = (status: string): string => {
  const severities: Record<string, string> = {
    authorized: 'success',
    pending: 'warning',
    rejected: 'danger'
  }
  return severities[status] || 'info'
}
</script>
