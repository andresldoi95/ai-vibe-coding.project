<script setup lang="ts">
definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const { t } = useI18n()
const { hasPermission } = usePermissions()

// Breadcrumbs
const breadcrumbs = [
  { label: t('nav.settings'), url: '/settings' },
]

// Available settings sections
const settingsSections = computed(() => {
  const sections = []

  if (hasPermission('roles.read')) {
    sections.push({
      title: t('nav.roles'),
      description: 'Manage roles and permissions for your organization',
      icon: 'pi pi-shield',
      route: '/settings/roles',
      color: 'text-blue-500',
      bgColor: 'bg-blue-50 dark:bg-blue-900/20',
    })
  }

  return sections
})
</script>

<template>
  <div class="p-6">
    <PageHeader
      :title="$t('nav.settings')"
      subtitle="Configure your system settings and preferences"
      :breadcrumbs="breadcrumbs"
    />

    <div class="mt-6">
      <div v-if="settingsSections.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <Card
          v-for="section in settingsSections"
          :key="section.route"
          class="hover:shadow-lg transition-shadow cursor-pointer"
          @click="navigateTo(section.route)"
        >
          <template #content>
            <div class="flex items-start gap-4">
              <div class="p-3 rounded-lg" :class="[section.bgColor]">
                <i class="text-2xl" :class="[section.icon, section.color]" />
              </div>
              <div class="flex-1">
                <h3 class="text-lg font-semibold mb-2">
                  {{ section.title }}
                </h3>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ section.description }}
                </p>
              </div>
              <i class="pi pi-chevron-right text-gray-400" />
            </div>
          </template>
        </Card>
      </div>

      <EmptyState
        v-else
        icon="pi pi-lock"
        :title="$t('messages.no_access')"
        :description="$t('messages.no_settings_access')"
      />
    </div>
  </div>
</template>
