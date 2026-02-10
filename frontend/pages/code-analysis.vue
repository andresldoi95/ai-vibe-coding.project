<script setup lang="ts">
/**
 * Code Analysis Demo Page
 * Demonstrates the repetitive code detection utility
 */
import { analyzeCodePatterns, generateAnalysisReport, printAnalysisSummary } from '~/utils/codeAnalysis'

definePageMeta({
  middleware: ['auth', 'tenant'],
  layout: 'default',
})

const analysis = computed(() => analyzeCodePatterns())
const reportText = ref('')
const showReport = ref(false)

function generateReport() {
  reportText.value = generateAnalysisReport()
  showReport.value = true
}

function printToConsole() {
  printAnalysisSummary()
}

function getImpactColor(impact: string) {
  switch (impact) {
    case 'high':
      return 'danger'
    case 'medium':
      return 'warning'
    case 'low':
      return 'info'
    default:
      return 'secondary'
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Page Header -->
    <PageHeader
      title="Code Repetition Analysis"
      description="Automated detection of repetitive code patterns in the frontend application"
    />

    <!-- Summary Card -->
    <Card>
      <template #header>
        <div class="p-6 pb-0">
          <h2 class="text-2xl font-semibold text-slate-900 dark:text-white">
            Analysis Summary
          </h2>
        </div>
      </template>

      <template #content>
        <div class="grid grid-cols-1 gap-6 md:grid-cols-3">
          <!-- Total Patterns -->
          <div class="text-center">
            <div class="text-4xl font-bold text-teal-600 dark:text-teal-400">
              {{ analysis.totalPatterns }}
            </div>
            <div class="mt-2 text-sm text-slate-600 dark:text-slate-400">
              Total Patterns Detected
            </div>
          </div>

          <!-- High Impact -->
          <div class="text-center">
            <div class="text-4xl font-bold text-orange-600 dark:text-orange-400">
              {{ analysis.highImpactPatterns }}
            </div>
            <div class="mt-2 text-sm text-slate-600 dark:text-slate-400">
              High Impact Patterns
            </div>
          </div>

          <!-- Lines to Reduce -->
          <div class="text-center">
            <div class="text-4xl font-bold text-green-600 dark:text-green-400">
              ~{{ analysis.estimatedLinesToReduce }}
            </div>
            <div class="mt-2 text-sm text-slate-600 dark:text-slate-400">
              Lines to Reduce
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="mt-6 flex justify-center gap-3">
          <Button
            label="Generate Full Report"
            icon="pi pi-file-pdf"
            @click="generateReport"
          />
          <Button
            label="Print to Console"
            icon="pi pi-print"
            severity="secondary"
            outlined
            @click="printToConsole"
          />
        </div>
      </template>
    </Card>

    <!-- Patterns List -->
    <Card>
      <template #header>
        <div class="p-6 pb-0">
          <h2 class="text-2xl font-semibold text-slate-900 dark:text-white">
            Detected Patterns
          </h2>
        </div>
      </template>

      <template #content>
        <div class="space-y-4">
          <div
            v-for="(pattern, index) in analysis.patterns"
            :key="pattern.name"
            class="rounded-lg border border-slate-200 p-4 dark:border-slate-700"
          >
            <!-- Pattern Header -->
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-semibold text-slate-900 dark:text-white">
                    {{ index + 1 }}. {{ pattern.name }}
                  </h3>
                  <Tag :severity="getImpactColor(pattern.impact)" :value="pattern.impact.toUpperCase()" />
                </div>
                <p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
                  {{ pattern.description }}
                </p>
              </div>
              <Badge :value="pattern.occurrences" severity="info" class="ml-4" />
            </div>

            <!-- Solution -->
            <div class="mt-4">
              <div class="text-xs font-semibold uppercase text-slate-500 dark:text-slate-400">
                Solution
              </div>
              <p class="mt-1 text-sm text-teal-700 dark:text-teal-300">
                {{ pattern.solution }}
              </p>
            </div>

            <!-- Sample Locations -->
            <div class="mt-4">
              <div class="text-xs font-semibold uppercase text-slate-500 dark:text-slate-400">
                Sample Locations
              </div>
              <ul class="mt-2 space-y-1">
                <li
                  v-for="location in pattern.locations.slice(0, 3)"
                  :key="location"
                  class="text-xs font-mono text-slate-600 dark:text-slate-400"
                >
                  <i class="pi pi-file mr-2 text-[10px]" />
                  {{ location }}
                </li>
              </ul>
              <div
                v-if="pattern.locations.length > 3"
                class="mt-2 text-xs text-slate-500 dark:text-slate-400"
              >
                ... and {{ pattern.locations.length - 3 }} more
              </div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <!-- Full Report Dialog -->
    <Dialog
      v-model:visible="showReport"
      header="Full Analysis Report"
      :modal="true"
      :style="{ width: '80vw', maxHeight: '80vh' }"
    >
      <div class="overflow-auto" style="max-height: 60vh">
        <pre class="whitespace-pre-wrap rounded bg-slate-100 p-4 text-xs dark:bg-slate-800">{{ reportText }}</pre>
      </div>
      <template #footer>
        <Button label="Close" @click="showReport = false" />
      </template>
    </Dialog>

    <!-- Info Card -->
    <Card>
      <template #content>
        <div class="prose prose-slate dark:prose-invert max-w-none">
          <h3>About This Analysis</h3>
          <p>
            This analysis tool automatically detects repetitive code patterns across the frontend application.
            The patterns are categorized by impact level:
          </p>
          <ul>
            <li><strong>High Impact:</strong> Patterns that appear frequently and significantly reduce code maintainability</li>
            <li><strong>Medium Impact:</strong> Patterns that appear moderately and could benefit from extraction</li>
            <li><strong>Low Impact:</strong> Patterns that appear occasionally but are still worth addressing</li>
          </ul>
          <h3>Available Solutions</h3>
          <p>
            For each detected pattern, we've created reusable composables and components:
          </p>
          <ul>
            <li><code>useCrudPage()</code> - CRUD page setup and logic</li>
            <li><code>useStatus()</code> - Status label and severity helpers</li>
            <li><code>useDataLoader()</code> - Async data loading with error handling</li>
            <li><code>useFilters()</code> - Filter state management</li>
            <li><code>DeleteConfirmDialog</code> - Reusable delete confirmation</li>
            <li><code>ExportDialog</code> - Configurable export dialog</li>
          </ul>
          <p>
            See <a href="/docs/REPETITIVE_CODE_DETECTION.md" target="_blank">REPETITIVE_CODE_DETECTION.md</a> for detailed documentation
            and <a href="/docs/REFACTORING_EXAMPLE.md" target="_blank">REFACTORING_EXAMPLE.md</a> for a complete refactoring example.
          </p>
        </div>
      </template>
    </Card>
  </div>
</template>
