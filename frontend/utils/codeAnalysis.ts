/**
 * Code Analysis Utility
 * Detects and reports repetitive code patterns in the frontend application
 */

export interface RepetitivePattern {
  name: string
  description: string
  occurrences: number
  locations: string[]
  impact: 'high' | 'medium' | 'low'
  solution: string
}

export interface CodeAnalysisReport {
  totalPatterns: number
  highImpactPatterns: number
  estimatedLinesToReduce: number
  patterns: RepetitivePattern[]
}

/**
 * Analyzes the codebase for repetitive patterns
 * This is a demonstration utility that documents known patterns
 */
export function analyzeCodePatterns(): CodeAnalysisReport {
  const patterns: RepetitivePattern[] = [
    {
      name: 'CRUD Page Setup',
      description: 'Repetitive page metadata, imports, and composable initialization across index pages',
      occurrences: 12,
      locations: [
        'pages/inventory/warehouses/index.vue',
        'pages/inventory/products/index.vue',
        'pages/inventory/stock-movements/index.vue',
        'pages/billing/customers/index.vue',
        'pages/billing/invoices/index.vue',
        'pages/settings/roles/index.vue',
      ],
      impact: 'high',
      solution: 'Create useCrudPage() composable that encapsulates common setup',
    },
    {
      name: 'Data Loading Pattern',
      description: 'Repeated async data loading with loading state, error handling, and toast notifications',
      occurrences: 15,
      locations: [
        'pages/inventory/warehouses/index.vue (loadWarehouses)',
        'pages/inventory/products/index.vue (loadProducts)',
        'pages/inventory/stock-movements/index.vue (loadData)',
      ],
      impact: 'high',
      solution: 'Create useDataLoader() composable with built-in error handling',
    },
    {
      name: 'Delete Confirmation Dialog',
      description: 'Identical delete confirmation dialog structure across all CRUD pages',
      occurrences: 12,
      locations: [
        'pages/inventory/warehouses/index.vue (lines 189-216)',
        'pages/inventory/products/index.vue (lines 395-422)',
        'pages/inventory/stock-movements/index.vue (lines 296-320)',
      ],
      impact: 'high',
      solution: 'Create DeleteConfirmDialog component',
    },
    {
      name: 'Status Label Functions',
      description: 'Duplicate getStatusLabel() and getStatusSeverity() functions',
      occurrences: 10,
      locations: [
        'pages/inventory/warehouses/index.vue (lines 67-73)',
        'pages/inventory/products/index.vue (lines 124-130)',
      ],
      impact: 'medium',
      solution: 'Move to useFormatters() composable or create useStatus() composable',
    },
    {
      name: 'Export Dialog Pattern',
      description: 'Similar export dialog with format selection and filters',
      occurrences: 3,
      locations: [
        'pages/inventory/warehouses/index.vue (lines 218-261)',
        'pages/inventory/stock-movements/index.vue (lines 322-431)',
      ],
      impact: 'high',
      solution: 'Create ExportDialog component with configurable filters',
    },
    {
      name: 'DataTable Configuration',
      description: 'Repeated DataTable setup with pagination, empty states, and action columns',
      occurrences: 12,
      locations: [
        'pages/inventory/warehouses/index.vue (lines 129-187)',
        'pages/inventory/products/index.vue (lines 320-393)',
        'pages/inventory/stock-movements/index.vue (lines 195-293)',
      ],
      impact: 'medium',
      solution: 'Create CrudDataTable component or useCrudTable() composable',
    },
    {
      name: 'Navigation Functions',
      description: 'Repetitive create/edit/view navigation functions',
      occurrences: 18,
      locations: [
        'pages/inventory/warehouses/index.vue (createWarehouse, confirmDelete)',
        'pages/inventory/products/index.vue (createProduct, confirmDelete)',
      ],
      impact: 'low',
      solution: 'Include in useCrudPage() composable',
    },
    {
      name: 'Filter Management',
      description: 'Reactive filter objects and reset/apply filter functions',
      occurrences: 5,
      locations: [
        'pages/inventory/products/index.vue (lines 22-87)',
      ],
      impact: 'medium',
      solution: 'Create useFilters() composable',
    },
  ]

  const highImpactCount = patterns.filter(p => p.impact === 'high').length
  const estimatedReduction = patterns.reduce((sum, p) => {
    const linesPerOccurrence = p.impact === 'high' ? 50 : p.impact === 'medium' ? 30 : 10
    return sum + (p.occurrences * linesPerOccurrence)
  }, 0)

  return {
    totalPatterns: patterns.length,
    highImpactPatterns: highImpactCount,
    estimatedLinesToReduce: estimatedReduction,
    patterns,
  }
}

/**
 * Generates a formatted report of code analysis
 */
export function generateAnalysisReport(): string {
  const analysis = analyzeCodePatterns()
  
  let report = '# Code Repetition Analysis Report\n\n'
  report += `**Total Patterns Detected:** ${analysis.totalPatterns}\n`
  report += `**High Impact Patterns:** ${analysis.highImpactPatterns}\n`
  report += `**Estimated Lines to Reduce:** ~${analysis.estimatedLinesToReduce} lines\n\n`
  report += '---\n\n'
  
  report += '## Patterns by Impact\n\n'
  
  const byImpact = {
    high: analysis.patterns.filter(p => p.impact === 'high'),
    medium: analysis.patterns.filter(p => p.impact === 'medium'),
    low: analysis.patterns.filter(p => p.impact === 'low'),
  }
  
  for (const [impact, patterns] of Object.entries(byImpact)) {
    if (patterns.length === 0) continue
    
    report += `### ${impact.toUpperCase()} Impact (${patterns.length} patterns)\n\n`
    
    patterns.forEach((pattern, index) => {
      report += `#### ${index + 1}. ${pattern.name}\n\n`
      report += `**Description:** ${pattern.description}\n\n`
      report += `**Occurrences:** ${pattern.occurrences}\n\n`
      report += `**Solution:** ${pattern.solution}\n\n`
      report += '**Sample Locations:**\n'
      pattern.locations.slice(0, 3).forEach((loc) => {
        report += `- ${loc}\n`
      })
      report += '\n'
    })
  }
  
  return report
}

/**
 * Console-friendly analysis summary
 */
export function printAnalysisSummary(): void {
  const analysis = analyzeCodePatterns()
  
  console.group('🔍 Code Repetition Analysis')
  console.log(`📊 Total Patterns: ${analysis.totalPatterns}`)
  console.log(`⚠️  High Impact: ${analysis.highImpactPatterns}`)
  console.log(`📉 Potential Reduction: ~${analysis.estimatedLinesToReduce} lines`)
  console.groupEnd()
  
  console.group('🎯 Top 3 High-Impact Patterns')
  analysis.patterns
    .filter(p => p.impact === 'high')
    .slice(0, 3)
    .forEach((pattern, i) => {
      console.log(`${i + 1}. ${pattern.name} (${pattern.occurrences} occurrences)`)
      console.log(`   Solution: ${pattern.solution}`)
    })
  console.groupEnd()
}
