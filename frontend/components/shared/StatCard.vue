<script setup lang="ts">
interface Props {
  title: string
  value: string | number
  icon?: string
  trend?: number
  trendLabel?: string
  severity?: 'success' | 'warning' | 'danger' | 'info'
}

withDefaults(defineProps<Props>(), {
  severity: 'info',
})
</script>

<template>
  <Card class="h-full">
    <template #content>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
            {{ title }}
          </p>
          <p class="mt-2 text-3xl font-bold text-gray-900 dark:text-gray-50">
            {{ value }}
          </p>
          <div v-if="trend !== undefined" class="mt-2 flex items-center gap-1">
            <i
              class="pi text-sm"
              :class="[
                trend > 0
                  ? 'pi-arrow-up text-green-600'
                  : 'pi-arrow-down text-red-600',
              ]"
            />
            <span class="text-sm text-gray-600 dark:text-gray-400">
              {{ trendLabel }}
            </span>
          </div>
        </div>
        <div
          v-if="icon"
          class="rounded-lg p-3"
          :class="[
            severity === 'success'
              && 'bg-green-100 text-green-600 dark:bg-green-900 dark:text-green-400',
            severity === 'warning'
              && 'bg-amber-100 text-amber-600 dark:bg-amber-900 dark:text-amber-400',
            severity === 'danger'
              && 'bg-red-100 text-red-600 dark:bg-red-900 dark:text-red-400',
            (!severity || severity === 'info')
              && 'bg-teal-100 text-teal-600 dark:bg-teal-900 dark:text-teal-400',
          ]"
        >
          <i class="text-2xl" :class="[icon]" />
        </div>
      </div>
    </template>
  </Card>
</template>
